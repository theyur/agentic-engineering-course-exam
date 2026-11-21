using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;

namespace HT.Application.Goals;

/// <summary>
/// Personal goals for registered users (FR-001 to FR-016, FR-018, FR-020 to FR-023, FR-027, FR-034; research R-06 to R-08). Every use
/// case takes the acting user first and checks in FR-023's order: signed in, not an administrator, the acting user's
/// account, the identifiers, then the fields. Every store call passes the acting user's account as the owner, so a
/// use case acts only on that user's own goals (constitution III). "Today" is the date in the account's time zone at
/// <see cref="TimeProvider.GetUtcNow"/>; an account with no stored zone has the installation's default (FR-012,
/// FR-025). Results carry texts and fields only, never an input value (HP-011).
/// </summary>
public sealed class GoalService
{
    /// <summary>How many times a creation is tried while the account's time zone keeps changing (research R-08).</summary>
    public const int MaxCreateAttempts = 10;

    private readonly IGoalStore _goals;
    private readonly IAccountStore _accounts;
    private readonly ICatalogHabitStore _catalogHabits;
    private readonly ICustomHabitStore _customHabits;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public GoalService(
        IGoalStore goals,
        IAccountStore accounts,
        ICatalogHabitStore catalogHabits,
        ICustomHabitStore customHabits,
        TimeProvider clock,
        InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(catalogHabits);
        ArgumentNullException.ThrowIfNull(customHabits);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _goals = goals;
        _accounts = accounts;
        _catalogHabits = catalogHabits;
        _customHabits = customHabits;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// The starting values of the New goal form (FR-007), without creating anything:
    /// <list type="bullet">
    ///   <item>a catalog habit with a suggested starting goal position (from 1): that suggestion's fields;</item>
    ///   <item>a catalog habit without a position: its marked suggestion's fields, or only Build or Reduce from its intent when it has none;</item>
    ///   <item>a custom habit: only Build or Reduce from its intent (it has no suggestions, so any position does not exist);</item>
    ///   <item>no habit: only the start date; a position is then ignored.</item>
    /// </list>
    /// The start date is today in the acting user's time zone, there is no name, and a used suggestion gives its
    /// target text under the goal rules as <see cref="GoalStartingValues.CatalogSuggestionText"/>. The habit is looked
    /// up as <see cref="CreateAsync"/> looks it up (FR-008).
    /// </summary>
    public async Task<GoalStartResult> StartNewGoalAsync(
        ActingUser actingUser, GoalHabit? habit, int? suggestionPosition, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return GoalStartResult.Refused(refusal == AccessRefusal.NotSignedIn ? GoalStartOutcome.NotSignedIn : GoalStartOutcome.NotForAdministrators, access.Message!);
        }

        var today = ZoneOf(access.Account!).DateAt(_clock.GetUtcNow());
        if (GoalDraft.CheckHabit(habit) != GoalProblem.None)
        {
            return GoalStartResult.Started(new GoalStartingValues { StartDate = today });
        }

        var chosen = habit!.Value;
        var found = await LookUpAsync(access.Owner, chosen, cancellationToken).ConfigureAwait(false);
        if (found is null)
        {
            return chosen.Kind == HabitKind.Catalog
                ? GoalStartResult.Refused(GoalStartOutcome.NotInCatalog, CatalogTexts.NotInCatalog)
                : GoalStartResult.Refused(GoalStartOutcome.CustomHabitNotFound, CustomHabitTexts.NotFound);
        }

        var suggestion = SuggestionOf(found.Suggestions, suggestionPosition, out var missing);
        if (missing)
        {
            return GoalStartResult.Refused(GoalStartOutcome.SuggestionNotFound, GoalTexts.SuggestionNotFound);
        }

        var habitView = found.View;
        if (suggestion is null)
        {
            return GoalStartResult.Started(new GoalStartingValues
            {
                Habit = habitView,
                BuildOrReduce = found.Intent == Intent.Reduce ? BuildOrReduce.Reduce : BuildOrReduce.Build,
                StartDate = today,
            });
        }

        var fields = FieldsOf(suggestion);
        return GoalStartResult.Started(new GoalStartingValues
        {
            Habit = habitView,
            BuildOrReduce = fields.BuildOrReduce,
            Measurement = fields.Measurement,
            Amount = fields.Amount,
            CountUnit = fields.CountUnit,
            DurationUnit = fields.DurationUnit,
            ScheduleKind = fields.ScheduleKind,
            Days = fields.Days ?? new HashSet<DayOfWeek>(),
            Times = fields.Times,
            Period = fields.Period,
            StartDate = today,
            CatalogSuggestionText = SuggestionText(suggestion, fields),
        });
    }

    /// <summary>
    /// Creates a goal of the acting user (FR-001, FR-002, FR-005, FR-006, FR-008, FR-009, FR-011, FR-018), checking in
    /// FR-006's order: a habit, the habit lookup (FR-008), the target fields, the Reduce N rule, the start date against
    /// today in the account's time zone, and the name. The store writes version 1 with that zone and attempt 1. When the
    /// account's zone changed in the meantime, the account is read again and the start date checked again in the new
    /// zone, at most <see cref="MaxCreateAttempts"/> times (research R-08).
    /// </summary>
    /// <exception cref="InvalidOperationException">The account's time zone changed during every attempt. The message names no value.</exception>
    public async Task<GoalCreateResult> CreateAsync(ActingUser actingUser, GoalInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return GoalCreateResult.Refused(refusal == AccessRefusal.NotSignedIn ? GoalCreateOutcome.NotSignedIn : GoalCreateOutcome.NotForAdministrators, access.Message!, null);
        }

        if (GoalDraft.CheckHabit(input.Habit) != GoalProblem.None)
        {
            return Refused(GoalProblem.HabitMissing);
        }

        var habit = input.Habit!.Value;
        if (await LookUpAsync(access.Owner, habit, cancellationToken).ConfigureAwait(false) is null)
        {
            return HabitNotFound(habit.Kind);
        }

        var account = access.Account!;
        for (var attempt = 1; ; attempt++)
        {
            var zone = ZoneOf(account);
            var today = zone.DateAt(_clock.GetUtcNow());
            var problem = GoalDraft.TryCreate(input.Target, input.StartDate, today, input.Name, out var target, out var start, out var ownName);
            if (problem != GoalProblem.None)
            {
                return Refused(problem);
            }

            var outcome = await _goals.CreateAsync(
                access.Owner, new NewGoal(habit, start, ownName, target!), zone, _defaultZone.Value, cancellationToken).ConfigureAwait(false);
            switch (outcome.Kind)
            {
                case GoalWriteOutcomeKind.Created:
                    return GoalCreateResult.Created(outcome.Id!.Value);
                case GoalWriteOutcomeKind.HabitGone:
                    return HabitNotFound(habit.Kind);
            }

            if (attempt == MaxCreateAttempts)
            {
                throw new InvalidOperationException("The account's time zone kept changing while a goal was created.");
            }

            if (await _accounts.FindByIdAsync(access.Owner, cancellationToken).ConfigureAwait(false) is not { } reread)
            {
                return GoalCreateResult.Refused(GoalCreateOutcome.NotSignedIn, AccountTexts.NotSignedIn, null);
            }

            account = reread;
        }
    }

    /// <summary>
    /// The acting user's goals (FR-014, FR-015): every goal of the account, in name order (the keys of
    /// <see cref="HabitName.KeyOf"/> compared ordinally), then in the store's creation order. Each item carries, at the
    /// supplied time and by the goal's own timeline rather than the account's "today": its start date while it has no
    /// current date, its next commitment (commitment-based goals), and its first week's Monday while that week has not
    /// begun (per-week goals). <see cref="GoalListResult.Today"/> is today in the account's time zone.
    /// </summary>
    public async Task<GoalListResult> ListAsync(ActingUser actingUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return GoalListResult.Refused(refusal == AccessRefusal.NotSignedIn ? GoalListOutcome.NotSignedIn : GoalListOutcome.NotForAdministrators, access.Message!);
        }

        var now = _clock.GetUtcNow();
        var today = ZoneOf(access.Account!).DateAt(now);
        var stored = await _goals.ListAsync(access.Owner, cancellationToken).ConfigureAwait(false);
        var items = stored
            .Select(goal => GoalListItems.Of(goal, now))
            .OrderBy(item => HabitName.KeyOf(item.Name), StringComparer.Ordinal)
            .ToList();
        return GoalListResult.Found(items, today);
    }

    /// <summary>
    /// One of the acting user's goals (FR-016, FR-018, FR-034). An identifier that is not one of the acting user's
    /// goals gives "This goal does not exist.", whether it belongs to nothing or to another account (FR-020). The time
    /// zone is the one of the version of the goal's current date at the supplied time, or of the first version while
    /// the goal has no current date; the later time zone is that of the one version after it, if any.
    /// </summary>
    public async Task<GoalReadResult> GetAsync(ActingUser actingUser, GoalId id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return GoalReadResult.Refused(refusal == AccessRefusal.NotSignedIn ? GoalReadOutcome.NotSignedIn : GoalReadOutcome.NotForAdministrators, access.Message!);
        }

        if (await _goals.FindAsync(access.Owner, id, cancellationToken).ConfigureAwait(false) is not { } stored)
        {
            return GoalReadResult.Refused(GoalReadOutcome.NotFound, GoalTexts.NotFound);
        }

        return GoalReadResult.Found(DetailsOf(stored, _clock.GetUtcNow()));
    }

    /// <summary>
    /// The evaluation calendar of one of the acting user's goals from <paramref name="first"/> to <paramref name="last"/>,
    /// both included (FR-012, FR-013, FR-027), checking in FR-023's order: signed in, not an administrator, the goal
    /// identifier (an unknown goal and another account's goal both give "This goal does not exist.", FR-020), then the
    /// range (1 to 366 days). Each commitment or period starts and ends at midnight in the time zone of its own version,
    /// in UTC instants. It reads no clock: the calendar depends only on the goal and the range.
    /// </summary>
    public async Task<GoalCalendarResult> GetCalendarAsync(
        ActingUser actingUser, GoalId id, DateOnly first, DateOnly last, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return GoalCalendarResult.Refused(refusal == AccessRefusal.NotSignedIn ? GoalCalendarOutcome.NotSignedIn : GoalCalendarOutcome.NotForAdministrators, access.Message!);
        }

        if (await _goals.FindAsync(access.Owner, id, cancellationToken).ConfigureAwait(false) is not { } stored)
        {
            return GoalCalendarResult.Refused(GoalCalendarOutcome.NotFound, GoalTexts.NotFound);
        }

        if (!GoalTimeline.TryCalendar(stored.Goal, first, last, out var calendar))
        {
            return GoalCalendarResult.Refused(GoalCalendarOutcome.RangeOutOfBounds, GoalTexts.RangeOutOfBounds);
        }

        return GoalCalendarResult.Found(new GoalCalendarView(calendar.Kind, calendar.Commitments, calendar.Periods));
    }

    /// <summary>The reading fields of FR-016 and FR-018 at <paramref name="now"/>.</summary>
    private static GoalDetails DetailsOf(StoredGoal stored, DateTimeOffset now)
    {
        var goal = stored.Goal;
        var current = GoalTimeline.CurrentDate(goal, now);
        var version = current is { } date ? GoalTimeline.VersionOf(goal, date) : goal.Versions[0];
        var later = goal.Versions.FirstOrDefault(v => v.AppliesFrom > version.AppliesFrom);
        var target = version.Settings.Target;
        var schedule = target.Schedule;
        return new GoalDetails
        {
            Id = goal.Id,
            Name = GoalText.Name(goal, stored.Habit.Name),
            HasOwnName = goal.OwnName is not null,
            Habit = stored.Habit,
            BuildOrReduce = target.BuildOrReduce,
            Measurement = target.Measurement,
            Amount = target.Amount,
            CountUnit = target.CountUnit,
            DurationUnit = target.DurationUnit,
            ScheduleKind = schedule.Kind,
            Days = schedule.Days,
            Times = schedule.Times,
            Period = schedule.Period,
            GoalScheduleKind = target.ScheduleKindOf,
            TargetText = GoalText.TargetText(target),
            ScheduleText = GoalText.ScheduleText(target),
            Kind = GoalText.Kind(target),
            StartDate = goal.StartDate,
            HasStarted = current is not null,
            FirstWeekStarts = GoalTimeline.FirstWeekStart(goal),
            TimeZone = version.Settings.TimeZone.Id,
            TimeZoneAppliesFrom = version.AppliesFrom,
            LaterTimeZone = later?.Settings.TimeZone.Id,
            LaterTimeZoneAppliesFrom = later?.AppliesFrom,
            AttemptNumber = goal.Attempt.Number,
            AttemptStartDate = goal.Attempt.StartDate,
        };
    }

    /// <summary>
    /// FR-023 steps 1 and 2, then the acting user's account: not signed in, an administrator, or an account that is
    /// not stored (deleted after sign-in, or a stale cookie), which also gives "You are not signed in." and reveals
    /// nothing.
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

        var account = await _accounts.FindByIdAsync(owner, cancellationToken).ConfigureAwait(false);
        return account is null ? Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn) : new Access(owner, account, null, null);
    }

    /// <summary>The account's stored time zone, or the installation's default when it has none (FR-025, research R-04).</summary>
    private NamedTimeZone ZoneOf(Account account) =>
        NamedTimeZone.TryParse(account.StoredTimeZone, out var zone) ? zone : _defaultZone.Value;

    /// <summary>
    /// The one habit lookup of starting and creating (FR-008): a catalog habit only among the Published ones, a custom
    /// habit only among the owner's own. Null when the habit is not found there.
    /// </summary>
    private async Task<FoundHabit?> LookUpAsync(AccountId owner, GoalHabit habit, CancellationToken cancellationToken)
    {
        if (habit.Kind == HabitKind.Catalog)
        {
            var catalog = await _catalogHabits.FindPublishedAsync(habit.Id, cancellationToken).ConfigureAwait(false);
            return catalog is null
                ? null
                : new FoundHabit(ViewOf(catalog.Id, HabitKind.Catalog, catalog.Definition, isInCatalog: true), catalog.Definition.Intent, catalog.SuggestedStartingGoals);
        }

        var custom = await _customHabits.FindAsync(owner, habit.Id, cancellationToken).ConfigureAwait(false);
        return custom is null
            ? null
            : new FoundHabit(ViewOf(custom.Id, HabitKind.Custom, custom.Definition, isInCatalog: null), custom.Definition.Intent, SuggestedStartingGoals.None);
    }

    private static GoalHabitView ViewOf(HabitId id, HabitKind kind, HabitDefinition definition, bool? isInCatalog) =>
        new(id, kind, definition.Name.Value, definition.Category, isInCatalog) { Icon = definition.Icon.Name };

    /// <summary>
    /// The suggestion at <paramref name="position"/> (from 1), or the marked one without a position (null when there is
    /// none). <paramref name="missing"/> is true when a position was given that the list does not have.
    /// </summary>
    private static SuggestedStartingGoal? SuggestionOf(SuggestedStartingGoals suggestions, int? position, out bool missing)
    {
        missing = false;
        if (position is not { } chosen)
        {
            return suggestions.Marked;
        }

        if (chosen < 1 || chosen > suggestions.Count)
        {
            missing = true;
            return null;
        }

        return suggestions.Items[chosen - 1];
    }

    private static GoalTargetFields FieldsOf(SuggestedStartingGoal suggestion) => new()
    {
        BuildOrReduce = suggestion.BuildOrReduce,
        Measurement = suggestion.Measurement,
        Amount = suggestion.Amount,
        CountUnit = suggestion.CountUnit,
        DurationUnit = suggestion.DurationUnit,
        ScheduleKind = suggestion.Schedule.Kind,
        Days = suggestion.Schedule.Kind == ScheduleKind.FixedDays ? suggestion.Schedule.Days : null,
        Times = suggestion.Schedule.Times,
        Period = suggestion.Schedule.Period,
    };

    /// <summary>
    /// The suggestion's target text under the goal rules (FR-007), so a Reduce yes/no suggestion on fixed days reads
    /// "avoided on …". A suggestion that is not a valid goal target (a Reduce amount of several times per period) keeps
    /// slice 2's text, which is the same text for every target the goal exception does not cover.
    /// </summary>
    private static string SuggestionText(SuggestedStartingGoal suggestion, GoalTargetFields fields) =>
        GoalTarget.TryCreate(fields, out var target, out _) ? GoalText.TargetText(target) : SuggestedStartingGoalText.Format(suggestion).Text;

    private static GoalCreateResult HabitNotFound(HabitKind kind) =>
        kind == HabitKind.Catalog
            ? GoalCreateResult.Refused(GoalCreateOutcome.NotInCatalog, CatalogTexts.NotInCatalog, GoalField.Habit)
            : GoalCreateResult.Refused(GoalCreateOutcome.CustomHabitNotFound, CustomHabitTexts.NotFound, GoalField.Habit);

    private static GoalCreateResult Refused(GoalProblem problem) =>
        GoalCreateResult.Refused(Enum.Parse<GoalCreateOutcome>(problem.ToString()), TextOf(problem), GoalDraft.FieldOf(problem));

    /// <summary>The text of a goal problem (FR-006): slice 2's target texts, with the goal's own yes/no text.</summary>
    private static string TextOf(GoalProblem problem) => problem switch
    {
        GoalProblem.HabitMissing => GoalTexts.HabitMissing,
        GoalProblem.BuildOrReduceMissing => HabitTexts.IntentMissing,
        GoalProblem.MeasurementMissing => CatalogTexts.MeasurementMissing,
        GoalProblem.YesNoHasAmountOrUnit => GoalTexts.YesNoHasAmountOrUnit,
        GoalProblem.BuildAmountOutOfRange => CatalogTexts.BuildAmountOutOfRange,
        GoalProblem.ReduceAmountOutOfRange => CatalogTexts.ReduceAmountOutOfRange,
        GoalProblem.CountUnitMissing => CatalogTexts.CountUnitMissing,
        GoalProblem.CountUnitTooLong => CatalogTexts.CountUnitTooLong,
        GoalProblem.DurationUnitMissing => CatalogTexts.DurationUnitMissing,
        GoalProblem.ScheduleMissing => CatalogTexts.ScheduleMissing,
        GoalProblem.DaysMissing => CatalogTexts.DaysMissing,
        GoalProblem.TimesOutOfRange => CatalogTexts.TimesOutOfRange,
        GoalProblem.PeriodMissing => CatalogTexts.PeriodMissing,
        GoalProblem.ReduceAmountNeedsOneTime => GoalTexts.ReduceAmountNeedsOneTime,
        GoalProblem.StartDateMissing => GoalTexts.StartDateMissing,
        GoalProblem.StartDateInPast => GoalTexts.StartDateInPast,
        GoalProblem.StartDateTooFar => GoalTexts.StartDateTooFar,
        GoalProblem.NameTooLong => AccountTexts.NameTooLong,
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, "Not a refusal."),
    };

    private enum AccessRefusal
    {
        NotSignedIn,
        NotForAdministrators,
    }

    /// <summary>The acting user's account, or why the use case refuses before any identifier is looked at.</summary>
    private sealed record Access(AccountId Owner, Account? Account, AccessRefusal? Refusal, string? Message)
    {
        public static Access Refused(AccessRefusal refusal, string message) => new(default, null, refusal, message);
    }

    /// <summary>A habit the lookup found: its view, its intent and its suggested starting goals (none for a custom habit).</summary>
    private sealed record FoundHabit(GoalHabitView View, Intent Intent, SuggestedStartingGoals Suggestions);
}
