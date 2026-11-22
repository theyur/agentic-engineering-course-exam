using HT.Application.Accounts;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Time;

namespace HT.Application.Goals;

/// <summary>
/// Goal changes for registered users (slice 8 FR-001 to FR-012, FR-022, FR-023; contracts/application-api.md
/// "GoalChangeService"). Every use case takes the acting user first and checks in FR-023's order: signed in, not an
/// administrator, the acting user's stored account, then the goal among that account's goals. Every store call passes
/// the acting user's account as the owner and the goal named in the call (constitution III). Each write reads
/// <see cref="IGoalStore.ReadOwnerGoalAsync"/>, decides in the Domain and calls its conditional store member; a
/// <see cref="GoalChangeWriteOutcome.Stale"/> reads again and decides again (research R-05). The time is read once per
/// call. It logs nothing, and no result or exception names a value (HP-010).
/// </summary>
public sealed class GoalChangeService
{
    private readonly IGoalStore _goals;
    private readonly IAccountStore _accounts;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public GoalChangeService(IGoalStore goals, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _goals = goals;
        _accounts = accounts;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// Changes the settings or the own name of one of the acting user's goals that is not ended (FR-001 to FR-006),
    /// checking in FR-003's order after access: an ended goal, the target fields (<see cref="GoalDraft.CheckTarget"/>),
    /// the schedule kind against the latest version's, and the name. Then:
    /// <list type="bullet">
    ///   <item>the latest version's target (a waiting one included) and the goal's own name, both unchanged: "settings
    ///   unchanged", and nothing is written;</item>
    ///   <item>only the name differs: "settings changed" with no date, and the name is written at once;</item>
    ///   <item>otherwise: the versions <see cref="GoalTimeline.SettingsChangeOf"/> gives at the supplied time with the
    ///   account's current zone are written, with the name, and "settings changed" carries the date they apply from.</item>
    /// </list>
    /// Values that do not belong to the chosen measurement or schedule are neither refused nor stored.
    /// </summary>
    /// <exception cref="InvalidOperationException">The goal or the account kept changing during every write. The message names no value.</exception>
    public async Task<SettingsChangeResult> ChangeSettingsAsync(
        ActingUser actingUser, GoalId goal, GoalSettingsInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return SettingsChangeResult.Refused(
                refusal == AccessRefusal.NotSignedIn ? SettingsChangeOutcome.NotSignedIn : SettingsChangeOutcome.NotForAdministrators,
                access.Message!,
                null);
        }

        var now = _clock.GetUtcNow();
        return await DecideAndWriteAsync(
            access.Owner,
            goal,
            static () => SettingsChangeResult.Refused(SettingsChangeOutcome.NotSignedIn, AccountTexts.NotSignedIn, null),
            static () => SettingsChangeResult.Refused(SettingsChangeOutcome.NotFound, GoalTexts.NotFound, null),
            (read, stored) => DecideSettingsChange(access.Owner, read, stored, input, now),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Steps 4 to 7 of FR-003 and the three successes, on one read of the goal.</summary>
    private Decision<SettingsChangeResult> DecideSettingsChange(
        AccountId owner, OwnerGoal read, Goal goal, GoalSettingsInput input, DateTimeOffset now)
    {
        if (goal.IsEnded)
        {
            return Decision<SettingsChangeResult>.Done(
                SettingsChangeResult.Refused(SettingsChangeOutcome.GoalEnded, GoalChangeTexts.GoalEnded, null));
        }

        var problem = GoalDraft.CheckTarget(input.Target, out var target);
        if (problem != GoalProblem.None)
        {
            return Decision<SettingsChangeResult>.Done(SettingsChangeResult.Refused(
                Enum.Parse<SettingsChangeOutcome>(problem.ToString()), GoalService.TextOf(problem), GoalDraft.FieldOf(problem)));
        }

        var latest = goal.Versions[^1].Settings.Target;
        if (!target!.HasSameScheduleKind(latest))
        {
            return Decision<SettingsChangeResult>.Done(SettingsChangeResult.Refused(
                SettingsChangeOutcome.ScheduleKindChanged, GoalChangeTexts.ScheduleKindChanged, GoalField.Schedule));
        }

        if (!GoalName.TryCreate(input.Name, out var name, out _))
        {
            return Decision<SettingsChangeResult>.Done(
                SettingsChangeResult.Refused(SettingsChangeOutcome.NameTooLong, AccountTexts.NameTooLong, GoalField.Name));
        }

        if (target.Equals(latest))
        {
            if (name == goal.OwnName)
            {
                return Decision<SettingsChangeResult>.Done(SettingsChangeResult.Unchanged());
            }

            var renamed = new GoalSettingsWrite(goal.Id, goal.Attempt.Number, name, null);
            return new Decision<SettingsChangeResult>(
                SettingsChangeResult.Changed(null),
                token => _goals.TryChangeSettingsAsync(owner, read.StoredTimeZone, read.GoalsRevision, renamed, token));
        }

        var change = GoalTimeline.SettingsChangeOf(goal, now, target, ZoneOf(read.StoredTimeZone));
        var write = new GoalSettingsWrite(goal.Id, goal.Attempt.Number, name, change.Versions);
        return new Decision<SettingsChangeResult>(
            SettingsChangeResult.Changed(change.AppliesFrom),
            token => _goals.TryChangeSettingsAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, token));
    }

    /// <summary>
    /// Ends one of the acting user's goals that is not ended (FR-007 to FR-009), checking in FR-007's order after access:
    /// an ended goal, no outcome (null or not defined), then a note longer than 500 characters after trimming. The ending
    /// date E is the goal's current date at the supplied time, or the account's today for a goal that has not started;
    /// <see cref="GoalTimeline.EndingOf"/> gives the last date and the versions the attempt keeps, and
    /// <see cref="IGoalStore.TryEndAsync"/> writes them with the ending and cuts the pauses and excusals after the last
    /// date. "goal ended" carries E and the last date. A goal that has not started, or whose attempt has not begun, can be
    /// ended; its attempt then has no commitment or period.
    /// </summary>
    /// <exception cref="InvalidOperationException">The goal or the account kept changing during every write. The message names no value.</exception>
    public async Task<GoalEndResult> EndAsync(
        ActingUser actingUser, GoalId goal, Outcome? outcome, string? note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return GoalEndResult.Refused(
                refusal == AccessRefusal.NotSignedIn ? GoalEndOutcome.NotSignedIn : GoalEndOutcome.NotForAdministrators,
                access.Message!);
        }

        var now = _clock.GetUtcNow();
        return await DecideAndWriteAsync(
            access.Owner,
            goal,
            static () => GoalEndResult.Refused(GoalEndOutcome.NotSignedIn, AccountTexts.NotSignedIn),
            static () => GoalEndResult.Refused(GoalEndOutcome.NotFound, GoalTexts.NotFound),
            (read, stored) => DecideEnding(access.Owner, read, stored, outcome, note, now),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Steps 4 to 6 of FR-007 and the ending, on one read of the goal.</summary>
    private Decision<GoalEndResult> DecideEnding(
        AccountId owner, OwnerGoal read, Goal goal, Outcome? outcome, string? note, DateTimeOffset now)
    {
        if (goal.IsEnded)
        {
            return Decision<GoalEndResult>.Done(GoalEndResult.Refused(GoalEndOutcome.AlreadyEnded, GoalChangeTexts.AlreadyEnded));
        }

        if (outcome is not { } chosen || !Enum.IsDefined(chosen))
        {
            return Decision<GoalEndResult>.Done(GoalEndResult.Refused(GoalEndOutcome.OutcomeMissing, GoalChangeTexts.OutcomeMissing));
        }

        if (!EndingNote.TryCreate(note, out var endingNote, out _))
        {
            return Decision<GoalEndResult>.Done(GoalEndResult.Refused(GoalEndOutcome.NoteTooLong, RecordTexts.NoteTooLong));
        }

        var endingDate = GoalTimeline.CurrentDate(goal, now) ?? ZoneOf(read.StoredTimeZone).DateAt(now);
        var effect = GoalTimeline.EndingOf(goal, endingDate);
        var write = new GoalEndingWrite(
            goal.Id, goal.Attempt.Number, new GoalEnding(endingDate, chosen, endingNote), effect.LastDate, effect.KeptVersions);
        return new Decision<GoalEndResult>(
            GoalEndResult.Ended(endingDate, effect.LastDate),
            token => _goals.TryEndAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, token));
    }

    /// <summary>
    /// The starting values of a new attempt of one of the acting user's ended goals (FR-011), checking after access that
    /// the goal is ended: the target of the ended attempt's last version (<see cref="GoalTimeline.VersionInEffect(Goal, int, DateTimeOffset)"/>,
    /// the version of its last date, or its first when it has no date), the start date
    /// <see cref="GoalTimeline.NewAttemptEarliestStart"/> gives for the account's today at the supplied time, and the
    /// number the attempt will get. Nothing is written.
    /// </summary>
    public async Task<NewAttemptValuesResult> GetNewAttemptValuesAsync(
        ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return NewAttemptValuesResult.Refused(
                refusal == AccessRefusal.NotSignedIn ? NewAttemptValuesOutcome.NotSignedIn : NewAttemptValuesOutcome.NotForAdministrators,
                access.Message!);
        }

        var now = _clock.GetUtcNow();
        return await DecideAndWriteAsync(
            access.Owner,
            goal,
            static () => NewAttemptValuesResult.Refused(NewAttemptValuesOutcome.NotSignedIn, AccountTexts.NotSignedIn),
            static () => NewAttemptValuesResult.Refused(NewAttemptValuesOutcome.NotFound, GoalTexts.NotFound),
            (read, stored) => Decision<NewAttemptValuesResult>.Done(NewAttemptValuesOf(read, stored, now)),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Step 4 of FR-011 and the starting values, on one read of the goal.</summary>
    private NewAttemptValuesResult NewAttemptValuesOf(OwnerGoal read, Goal goal, DateTimeOffset now)
    {
        if (!goal.IsEnded)
        {
            return NewAttemptValuesResult.Refused(NewAttemptValuesOutcome.NotEnded, GoalChangeTexts.NotEnded);
        }

        var target = GoalTimeline.VersionInEffect(goal, goal.Attempt.Number, now).Settings.Target;
        var schedule = target.Schedule;
        return NewAttemptValuesResult.Found(new NewAttemptValues
        {
            BuildOrReduce = target.BuildOrReduce,
            Measurement = target.Measurement,
            Amount = target.Amount,
            CountUnit = target.CountUnit,
            DurationUnit = target.DurationUnit,
            ScheduleKind = schedule.Kind,
            Days = schedule.Days,
            Times = schedule.Times,
            Period = schedule.Period,
            StartDate = GoalTimeline.NewAttemptEarliestStart(goal, ZoneOf(read.StoredTimeZone).DateAt(now)),
            AttemptNumber = goal.Attempt.Number + 1,
        });
    }

    /// <summary>
    /// Starts a new attempt of one of the acting user's ended goals (FR-012), checking in FR-012's order after access: a
    /// goal that is not ended, the target fields (<see cref="GoalDraft.CheckTarget"/>, any schedule kind), then the start
    /// date: missing, before the account's today, on or before the ending date, more than 365 days after the account's
    /// today. <see cref="IGoalStore.TryStartAttemptAsync"/> then writes the attempt numbered the latest number + 1 with its
    /// first version from the start date in the account's current zone; "attempt started" carries the number and the
    /// start date. The habit is not looked up, so an Unpublished catalog habit can start a new attempt (AC-064), and the
    /// goal keeps its habit and name.
    /// </summary>
    /// <exception cref="InvalidOperationException">The goal or the account kept changing during every write. The message names no value.</exception>
    public async Task<AttemptStartResult> StartAttemptAsync(
        ActingUser actingUser, GoalId goal, NewAttemptInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return AttemptStartResult.Refused(
                refusal == AccessRefusal.NotSignedIn ? AttemptStartOutcome.NotSignedIn : AttemptStartOutcome.NotForAdministrators,
                access.Message!,
                null);
        }

        var now = _clock.GetUtcNow();
        return await DecideAndWriteAsync(
            access.Owner,
            goal,
            static () => AttemptStartResult.Refused(AttemptStartOutcome.NotSignedIn, AccountTexts.NotSignedIn, null),
            static () => AttemptStartResult.Refused(AttemptStartOutcome.NotFound, GoalTexts.NotFound, null),
            (read, stored) => DecideAttemptStart(access.Owner, read, stored, input, now),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Steps 4 to 6 of FR-012 and the new attempt, on one read of the goal.</summary>
    private Decision<AttemptStartResult> DecideAttemptStart(
        AccountId owner, OwnerGoal read, Goal goal, NewAttemptInput input, DateTimeOffset now)
    {
        if (goal.Attempt.Ending is not { } ending)
        {
            return Decision<AttemptStartResult>.Done(
                AttemptStartResult.Refused(AttemptStartOutcome.NotEnded, GoalChangeTexts.NotEnded, null));
        }

        var problem = GoalDraft.CheckTarget(input.Target, out var target);
        var zone = ZoneOf(read.StoredTimeZone);
        if (problem == GoalProblem.None)
        {
            problem = StartDateProblem(input.StartDate, zone.DateAt(now), ending.Date);
        }

        if (problem != GoalProblem.None)
        {
            return Decision<AttemptStartResult>.Done(AttemptStartResult.Refused(
                Enum.Parse<AttemptStartOutcome>(problem.ToString()), GoalService.TextOf(problem), GoalDraft.FieldOf(problem)));
        }

        var start = input.StartDate!.Value;
        var number = goal.Attempt.Number + 1;
        var write = new GoalAttemptWrite(goal.Id, number, new GoalSettingsVersion(new GoalSettings(target!, zone), start));
        return new Decision<AttemptStartResult>(
            AttemptStartResult.Started(number, start),
            token => _goals.TryStartAttemptAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, token));
    }

    /// <summary>
    /// FR-012 step 6, in its order: no start date, a date before <paramref name="accountToday"/>, a date on or before
    /// <paramref name="endingDate"/>, a date more than <see cref="GoalDraft.MaxDaysAhead"/> days after
    /// <paramref name="accountToday"/>; otherwise <see cref="GoalProblem.None"/>.
    /// </summary>
    private static GoalProblem StartDateProblem(DateOnly? startDate, DateOnly accountToday, DateOnly endingDate)
    {
        if (startDate is not { } date)
        {
            return GoalProblem.StartDateMissing;
        }

        if (date < accountToday)
        {
            return GoalProblem.StartDateInPast;
        }

        if (date <= endingDate)
        {
            return GoalProblem.StartDateNotAfterEnding;
        }

        return date > accountToday.AddDays(GoalDraft.MaxDaysAhead) ? GoalProblem.StartDateTooFar : GoalProblem.None;
    }

    /// <summary>
    /// The one write-and-retry loop of the goal changes (contracts/application-api.md "Writes", research R-05): it reads
    /// the owner's zone, revision and goal, lets <paramref name="decide"/> check them and choose the write, and makes it.
    /// No account gives <paramref name="notSignedIn"/>, a goal that is not the owner's <paramref name="notFound"/> (also
    /// when the store says so), a decision without a write its result at once, and a written one its result. A
    /// <see cref="GoalChangeWriteOutcome.Stale"/> reads and decides again, so every check is made again, at most
    /// <see cref="GoalService.MaxCreateAttempts"/> times.
    /// </summary>
    /// <exception cref="InvalidOperationException">Every write was stale. The message names no value.</exception>
    private async Task<TResult> DecideAndWriteAsync<TResult>(
        AccountId owner,
        GoalId goal,
        Func<TResult> notSignedIn,
        Func<TResult> notFound,
        Func<OwnerGoal, Goal, Decision<TResult>> decide,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= GoalService.MaxCreateAttempts; attempt++)
        {
            if (await _goals.ReadOwnerGoalAsync(owner, goal, cancellationToken).ConfigureAwait(false) is not { } read)
            {
                return notSignedIn();
            }

            if (read.Goal is not { } stored)
            {
                return notFound();
            }

            var decision = decide(read, stored.Goal);
            if (decision.Write is not { } write)
            {
                return decision.Result;
            }

            switch (await write(cancellationToken).ConfigureAwait(false))
            {
                case GoalChangeWriteOutcome.Written:
                    return decision.Result;
                case GoalChangeWriteOutcome.GoalNotFound:
                    return notFound();
            }
        }

        throw new InvalidOperationException("The goal kept changing while it was changed.");
    }

    /// <summary>
    /// FR-023 steps 1 and 2, then the acting user's stored account: not signed in, an administrator, or an account that
    /// is not stored, which also gives "You are not signed in." and reveals nothing.
    /// </summary>
    private async Task<Access> AccessAsync(ActingUser actingUser, CancellationToken cancellationToken)
    {
        if (actingUser.AccountId is not { } owner)
        {
            return Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn);
        }

        if (actingUser.Role != Role.RegisteredUser)
        {
            return Access.Refused(AccessRefusal.NotForAdministrators, GoalTexts.NotForAdministrators);
        }

        return await _accounts.FindByIdAsync(owner, cancellationToken).ConfigureAwait(false) is null
            ? Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn)
            : new Access(owner, null, null);
    }

    /// <summary>The account's stored time zone as read with the goal, or the installation's default when it has none.</summary>
    private NamedTimeZone ZoneOf(string? storedTimeZone) =>
        NamedTimeZone.TryParse(storedTimeZone, out var zone) ? zone : _defaultZone.Value;

    private enum AccessRefusal
    {
        NotSignedIn,
        NotForAdministrators,
    }

    /// <summary>The acting user's account, or why the use case refuses before the goal is looked at.</summary>
    private sealed record Access(AccountId Owner, AccessRefusal? Refusal, string? Message)
    {
        public static Access Refused(AccessRefusal refusal, string message) => new(default, refusal, message);
    }

    /// <summary>
    /// What one read decided: the result, and the conditional write to make first, or none when the result needs no
    /// write (a refusal, or "settings unchanged").
    /// </summary>
    private sealed record Decision<TResult>(TResult Result, Func<CancellationToken, Task<GoalChangeWriteOutcome>>? Write)
    {
        public static Decision<TResult> Done(TResult result) => new(result, null);
    }
}
