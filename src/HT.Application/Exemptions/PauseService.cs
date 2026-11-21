using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Time;

namespace HT.Application.Exemptions;

/// <summary>
/// Pausing a goal, resuming it early, cancelling an upcoming pause and reading its pauses, for registered users
/// (FR-006 to FR-012, FR-021, FR-022; research R-03, R-05). Every use case takes the acting user first and checks in
/// the order: signed in, not an administrator, the acting user's account, the goal among the acting user's own, then
/// the steps of contracts/application-api.md. Every store call passes the acting user's account as the owner and the
/// goal named in the call, and a pause is looked up only among that goal's pauses (constitution III). The time is
/// <see cref="TimeProvider.GetUtcNow"/>, read once per call, and every "today" is the pause date of that instant
/// (<see cref="PauseDateOf"/>). A refusal carries no input value, and nothing is logged (HP-009).
/// </summary>
public sealed class PauseService
{
    private readonly IRecordStore _records;
    private readonly IGoalStore _goals;
    private readonly IAccountStore _accounts;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public PauseService(
        IRecordStore records, IGoalStore goals, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _records = records;
        _goals = goals;
        _accounts = accounts;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// Pauses one of the acting user's goals from <paramref name="firstDay"/> to <paramref name="lastDay"/> (FR-006),
    /// checking in FR-006's order after access: (4) a missing day; (5) a first day before the pause date; (6) a last day
    /// before the first day; (7) a shared date with another pause of the goal (steps 4 to 7 from
    /// <see cref="PauseRules.Check"/>); (8) a reason over 200 characters after trimming. The store re-checks step 7
    /// under the goal-row lock, and a concurrent pause there gives step 7's refusal.
    /// </summary>
    public async Task<PauseResult> PauseAsync(
        ActingUser actingUser, GoalId goal, DateOnly? firstDay, DateOnly? lastDay, string? reason,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return PauseResult.Refused(OutcomeOf(refusal, PauseOutcome.NotSignedIn, PauseOutcome.NotForAdministrators, PauseOutcome.NotFound), access.Message!);
        }

        var pauseDate = PauseDateOf(access);
        var exemptions = await _records.ReadExemptionsAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false);
        switch (PauseRules.Check(firstDay, lastDay, pauseDate, exemptions))
        {
            case PauseProblem.DaysMissing:
                return PauseResult.Refused(PauseOutcome.DaysMissing, PauseTexts.DaysMissing);
            case PauseProblem.StartsBeforeToday:
                return PauseResult.Refused(PauseOutcome.StartsBeforeToday, PauseTexts.StartsBeforeToday);
            case PauseProblem.LastBeforeFirst:
                return PauseResult.Refused(PauseOutcome.LastBeforeFirst, PauseTexts.LastBeforeFirst);
            case PauseProblem.Overlaps:
                return PauseResult.Refused(PauseOutcome.Overlaps, PauseTexts.Overlaps);
        }

        if (!ExemptionReason.TryCreate(reason, out var text, out _))
        {
            return PauseResult.Refused(PauseOutcome.ReasonTooLong, PauseTexts.ReasonTooLong);
        }

        // PauseRules.Check returned no problem, so both days are set.
        var first = firstDay!.Value;
        var last = lastDay!.Value;
        var write = await _records.AddPauseAsync(access.Owner, goal, first, last, text, cancellationToken).ConfigureAwait(false);
        return write.Outcome switch
        {
            ExemptionStoreWriteOutcome.Written => PauseResult.Paused(
                PauseView.Of(new Pause(write.PauseId!.Value, first, last, text), pauseDate)),

            // A concurrent pause on these dates between the check above and the write (research R-04).
            ExemptionStoreWriteOutcome.Overlaps => PauseResult.Refused(PauseOutcome.Overlaps, PauseTexts.Overlaps),

            // The goal was removed between the lookup and the write.
            ExemptionStoreWriteOutcome.GoalNotFound => PauseResult.Refused(PauseOutcome.NotFound, GoalTexts.NotFound),
            _ => throw new InvalidOperationException("The record store gave an outcome that pausing does not have."),
        };
    }

    /// <summary>
    /// Resumes one of the acting user's goals (FR-010), checking after access: (4) no pause is current at the pause
    /// date. The current pause is then removed when it began on the pause date, else ends on the day before it
    /// (<see cref="PauseRules.Resume"/>).
    /// </summary>
    public async Task<ResumeResult> ResumeAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return ResumeResult.Refused(OutcomeOf(refusal, ResumeOutcome.NotSignedIn, ResumeOutcome.NotForAdministrators, ResumeOutcome.NotFound), access.Message!);
        }

        var pauseDate = PauseDateOf(access);
        var exemptions = await _records.ReadExemptionsAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false);
        if (exemptions.CurrentPause(pauseDate) is not { } current)
        {
            return ResumeResult.Refused(ResumeOutcome.NotPaused, PauseTexts.NotPaused);
        }

        var resumption = PauseRules.Resume(current, pauseDate);
        var write = resumption.Removes
            ? await _records.DeletePauseAsync(access.Owner, goal, current.Id, cancellationToken).ConfigureAwait(false)
            : await _records.SetPauseLastDayAsync(access.Owner, goal, current.Id, resumption.NewLastDay!.Value, cancellationToken)
                .ConfigureAwait(false);
        return write.Outcome switch
        {
            ExemptionStoreWriteOutcome.Written => ResumeResult.Resumed(),

            // The pause was removed (resumed or cancelled elsewhere) between the check above and the write.
            ExemptionStoreWriteOutcome.PauseNotFound => ResumeResult.Refused(ResumeOutcome.NotPaused, PauseTexts.NotPaused),

            // The goal was removed between the lookup and the write.
            ExemptionStoreWriteOutcome.GoalNotFound => ResumeResult.Refused(ResumeOutcome.NotFound, GoalTexts.NotFound),
            _ => throw new InvalidOperationException("The record store gave an outcome that resuming does not have."),
        };
    }

    /// <summary>
    /// Cancels an upcoming pause of one of the acting user's goals (FR-011), checking after access: (4) the pause is not
    /// among this goal's pauses (another goal's, another account's, or none); (5) it is current or past at the pause
    /// date.
    /// </summary>
    public async Task<CancelPauseResult> CancelPauseAsync(
        ActingUser actingUser, GoalId goal, PauseId pause, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return CancelPauseResult.Refused(
                OutcomeOf(refusal, CancelPauseOutcome.NotSignedIn, CancelPauseOutcome.NotForAdministrators, CancelPauseOutcome.NotFound),
                access.Message!);
        }

        var pauseDate = PauseDateOf(access);
        var exemptions = await _records.ReadExemptionsAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false);
        if (exemptions.Pauses.FirstOrDefault(candidate => candidate.Id == pause) is not { } found)
        {
            return CancelPauseResult.Refused(CancelPauseOutcome.PauseNotFound, PauseTexts.PauseNotFound);
        }

        if (found.TimingAt(pauseDate) != PauseTiming.Upcoming)
        {
            return CancelPauseResult.Refused(CancelPauseOutcome.HasBegun, PauseTexts.HasBegun);
        }

        var write = await _records.DeletePauseAsync(access.Owner, goal, pause, cancellationToken).ConfigureAwait(false);
        return write.Outcome switch
        {
            ExemptionStoreWriteOutcome.Written => CancelPauseResult.Cancelled(),

            // The pause was removed between the check above and the write.
            ExemptionStoreWriteOutcome.PauseNotFound => CancelPauseResult.Refused(CancelPauseOutcome.PauseNotFound, PauseTexts.PauseNotFound),

            // The goal was removed between the lookup and the write.
            ExemptionStoreWriteOutcome.GoalNotFound => CancelPauseResult.Refused(CancelPauseOutcome.NotFound, GoalTexts.NotFound),
            _ => throw new InvalidOperationException("The record store gave an outcome that cancelling a pause does not have."),
        };
    }

    /// <summary>
    /// Every pause of one of the acting user's goals (FR-012), the latest first day first, each with its timing at the
    /// pause date.
    /// </summary>
    public async Task<PausesResult> GetPausesAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return PausesResult.Refused(OutcomeOf(refusal, PausesOutcome.NotSignedIn, PausesOutcome.NotForAdministrators, PausesOutcome.NotFound), access.Message!);
        }

        var pauseDate = PauseDateOf(access);
        var exemptions = await _records.ReadExemptionsAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false);
        return PausesResult.Found(exemptions.Pauses.Select(pause => PauseView.Of(pause, pauseDate)).ToList(), pauseDate);
    }

    /// <summary>
    /// The pause date of this call (research R-03), the one "today" of FR-006, FR-010, FR-011 and FR-012: the goal's
    /// current date at the supplied time, or, for a goal that has not started, the account's today in its stored time
    /// zone, else the installation default. The time is read here, once per call.
    /// </summary>
    private DateOnly PauseDateOf(Access access)
    {
        var now = _clock.GetUtcNow();
        return GoalTimeline.CurrentDate(access.Goal!, now) ?? ZoneOf(access.Account!).DateAt(now);
    }

    /// <summary>The account's stored time zone, or the installation default when none is stored (as ReportService reads it).</summary>
    private NamedTimeZone ZoneOf(Account account) =>
        NamedTimeZone.TryParse(account.StoredTimeZone, out var zone) ? zone : _defaultZone.Value;

    /// <summary>
    /// FR-006 steps 1 to 3 (as FR-002's): not signed in, an administrator, an account that is not stored (which also
    /// gives "You are not signed in." and reveals nothing), then the goal through the owner-scoped
    /// <see cref="IGoalStore.FindAsync"/>, so another account's goal and no goal look the same (FR-021, FR-022).
    /// </summary>
    private async Task<Access> CheckAccessAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        if (actingUser.AccountId is not { } owner)
        {
            return Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn);
        }

        if (actingUser.Role != Role.RegisteredUser)
        {
            return Access.Refused(AccessRefusal.NotForAdministrators, GoalTexts.NotForAdministrators);
        }

        if (await _accounts.FindByIdAsync(owner, cancellationToken).ConfigureAwait(false) is not { } account)
        {
            return Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn);
        }

        return await _goals.FindAsync(owner, goal, cancellationToken).ConfigureAwait(false) is { } stored
            ? new Access(owner, account, stored.Goal, null, null)
            : Access.Refused(AccessRefusal.NotFound, GoalTexts.NotFound);
    }

    private static TOutcome OutcomeOf<TOutcome>(AccessRefusal refusal, TOutcome notSignedIn, TOutcome notForAdministrators, TOutcome notFound) =>
        refusal switch
        {
            AccessRefusal.NotSignedIn => notSignedIn,
            AccessRefusal.NotForAdministrators => notForAdministrators,
            _ => notFound,
        };

    private enum AccessRefusal
    {
        NotSignedIn,
        NotForAdministrators,
        NotFound,
    }

    /// <summary>The acting user's account and goal, or why the use case refuses before anything else is looked at.</summary>
    private sealed record Access(AccountId Owner, Account? Account, Goal? Goal, AccessRefusal? Refusal, string? Message)
    {
        public static Access Refused(AccessRefusal refusal, string message) => new(default, null, null, refusal, message);
    }
}
