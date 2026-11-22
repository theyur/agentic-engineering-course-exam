using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Time;

namespace HT.Application.Reports;

/// <summary>
/// The personal reports of registered users (FR-007, FR-011, FR-012; research R-03). Every use case takes the acting
/// user first and checks in FR-015's order: signed in, not an administrator, the acting user's account, then (for the
/// goal report) the goal among the acting user's own. Every store call passes the acting user's account as the owner
/// (FR-014, constitution III). The time is <see cref="TimeProvider.GetUtcNow"/>, read once per call, and each goal is
/// evaluated once per call by one <see cref="GoalEvaluation"/>; nothing derived is stored (HP-009, ADR 0020). It logs
/// nothing (HP-007).
/// </summary>
public sealed class ReportService
{
    /// <summary>How many ended results the goal report gives at most (FR-005); slice 8: also an attempt's (FR-014).</summary>
    internal const int GoalReportRecentCount = 30;

    /// <summary>How many ended results each goal of the overview gives at most, the Goals page's mini strip (FR-011).</summary>
    private const int OverviewRecentCount = 15;

    /// <summary>How many recent notes the dashboard gives at most (FR-012).</summary>
    private const int DashboardNoteCount = 5;

    /// <summary>The ended states, each counted in <see cref="RecentResults.Counts"/>, 0 included (FR-005).</summary>
    private static readonly EvaluationState[] EndedStates =
        [EvaluationState.Met, EvaluationState.Failed, EvaluationState.Missed, EvaluationState.Pending];

    /// <summary>Slice 7's exempt states, counted in <see cref="RecentResults.Counts"/> only when above 0 (FR-015, FR-023).</summary>
    private static readonly EvaluationState[] ExemptStates = [EvaluationState.Excused, EvaluationState.Paused];

    private readonly IGoalStore _goals;
    private readonly IRecordStore _records;
    private readonly IAccountStore _accounts;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public ReportService(
        IGoalStore goals, IRecordStore records, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _goals = goals;
        _records = records;
        _accounts = accounts;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// The report of one of the acting user's goals at the supplied time (FR-007): its consistency (FR-001, FR-002), its
    /// streaks in their unit (FR-003, FR-004), its latest 30 ended results with the count of each state (FR-005) and
    /// its trend (FR-006). The goal is looked up only among the acting user's goals, so another account's goal and no
    /// goal both give <see cref="GoalReportOutcome.NotFound"/> (FR-014, FR-015). A goal that has not started gets "N/A",
    /// streaks of 0, no recent result and an empty trend, with <see cref="GoalReport.HasBegunEntry"/> false.
    /// </summary>
    public async Task<GoalReportResult> GetGoalReportAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccountAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return GoalReportResult.Refused(GoalReportOutcomeOf(refusal), access.Message!);
        }

        if (await _goals.FindAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false) is not { } stored)
        {
            return GoalReportResult.Refused(GoalReportOutcome.NotFound, GoalTexts.NotFound);
        }

        var now = _clock.GetUtcNow();
        var evaluation = await GoalEvaluation.OfAsync(_records, access.Owner, stored, now, cancellationToken).ConfigureAwait(false);
        return GoalReportResult.Found(ReportOf(evaluation));
    }

    /// <summary>
    /// Every goal of the acting user with its figures at the supplied time (FR-011, research R-07): slice 4's list item,
    /// built by the helper <see cref="GoalService.ListAsync"/> uses, so both give the same item; the consistency and
    /// streaks; the last 15 ended results; whether a Pending result exists; whether something can be recorded now; and
    /// what can be recorded, as <see cref="RecordingService.GetRecordableAsync"/> gives it at the same instant. The
    /// goals come in <see cref="GoalOrder"/>'s order: Pending, then Due, then the others, each group by name and then in
    /// creation order. A user with no goals is found with none (FR-013).
    /// </summary>
    public async Task<GoalsOverviewResult> GetGoalsOverviewAsync(ActingUser actingUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccountAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return GoalsOverviewResult.Refused(
                refusal == AccessRefusal.NotSignedIn ? GoalsOverviewOutcome.NotSignedIn : GoalsOverviewOutcome.NotForAdministrators,
                access.Message!);
        }

        var now = _clock.GetUtcNow();
        var stored = await _goals.ListAsync(access.Owner, cancellationToken).ConfigureAwait(false);
        var evaluated = new List<(StoredGoal Stored, GoalEvaluation Evaluation)>(stored.Count);
        foreach (var goal in stored)
        {
            evaluated.Add((goal, await GoalEvaluation.OfAsync(_records, access.Owner, goal, now, cancellationToken).ConfigureAwait(false)));
        }

        var today = ZoneOf(access.Account!).DateAt(now);
        var goals = GoalOrder.Of(evaluated, pair => pair.Evaluation)
            .Select(pair => OverviewGoalOf(pair.Stored, pair.Evaluation, today))
            .ToList();
        return GoalsOverviewResult.Found(new GoalsOverview { Goals = goals, Today = today, ReadAt = now });
    }

    /// <summary>
    /// The signed-in Home's figures at the supplied time (FR-012, FR-013, research R-06), with "this week" the
    /// Monday-to-Sunday week that contains the supplied time in the account's time zone (the stored one, else the
    /// installation default): the counts of Due, Pending and Missed-this-week entries and of goals; the Today list
    /// (Pending by grace end, then Due in goal order, then Missed this week by latest grace end, ties in goal order, then
    /// one Upcoming entry per goal with no Due entry that starts within 6 days, earliest start first, then goal order);
    /// every goal with its figures in goal order; and at most 5 notes, latest first date first, ties in goal order. A user
    /// with no goals gets counts of 0 and empty lists.
    /// </summary>
    public async Task<DashboardResult> GetDashboardAsync(ActingUser actingUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccountAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return DashboardResult.Refused(
                refusal == AccessRefusal.NotSignedIn ? DashboardOutcome.NotSignedIn : DashboardOutcome.NotForAdministrators,
                access.Message!);
        }

        var now = _clock.GetUtcNow();
        var zone = ZoneOf(access.Account!);
        var today = zone.DateAt(now);
        var weekStarts = zone.StartOfDay(today.AddDays(-DaysSinceMonday(today)));
        var stored = await _goals.ListAsync(access.Owner, cancellationToken).ConfigureAwait(false);
        var evaluated = new List<GoalEvaluation>(stored.Count);
        foreach (var goal in stored)
        {
            evaluated.Add(await GoalEvaluation.OfAsync(_records, access.Owner, goal, now, cancellationToken).ConfigureAwait(false));
        }

        // Slice 8 (FR-019, research R-07): an ended goal keeps only its Pending entries; earlier attempts give their
        // open Pending entries; every other list and count leaves ended goals out.
        var ordered = GoalOrder.Of(evaluated, evaluation => evaluation);
        var missedThisWeek = ordered
            .SelectMany((evaluation, rank) => evaluation.Entries
                .Where(entry => entry.State == EvaluationState.Missed && !evaluation.Goal.IsEnded)
                .Select(entry => (Evaluation: evaluation, Rank: rank, Entry: entry, GraceEnds: GracePeriod.EndOf(entry.Ends))))
            .Where(missed => missed.GraceEnds >= weekStarts && missed.GraceEnds <= now)
            .OrderByDescending(missed => missed.GraceEnds)
            .ThenBy(missed => missed.Rank)
            .Select(missed => TodayEntryOf(missed.Evaluation, missed.Entry))
            .ToList();
        var pending = ordered
            .SelectMany((evaluation, rank) => evaluation.Pending.Concat(evaluation.EarlierPending)
                .Select(entry => (Evaluation: evaluation, Rank: rank, Entry: entry)))
            .OrderBy(item => item.Entry.GraceEnds)
            .ThenBy(item => item.Rank)
            .Select(item => TodayEntryOf(item.Evaluation, item.Entry))
            .ToList();
        var due = ordered
            .Where(evaluation => !evaluation.Goal.IsEnded)
            .SelectMany(evaluation => evaluation.Due.Select(entry => TodayEntryOf(evaluation, entry)))
            .ToList();
        var upcoming = ordered
            .Select((evaluation, rank) => (Evaluation: evaluation, Rank: rank))
            .Where(item => item.Evaluation.Due.Count == 0 && !item.Evaluation.Goal.IsEnded)
            .Select(item => (item.Evaluation, item.Rank, Entry: item.Evaluation.NextUpcoming(item.Evaluation.CurrentDate ?? today)))
            .Where(item => item.Entry is not null)
            .OrderBy(item => item.Entry!.Starts)
            .ThenBy(item => item.Rank)
            .Select(item => TodayEntryOf(item.Evaluation, item.Entry!))
            .ToList();
        var notes = ordered
            .Select((evaluation, rank) => (Evaluation: evaluation, Rank: rank))
            .Where(item => !item.Evaluation.Goal.IsEnded)
            .SelectMany(item => item.Evaluation.Notes.Select(entry => (item.Evaluation, item.Rank, Entry: entry)))
            .OrderByDescending(item => item.Entry.FirstDate)
            .ThenBy(item => item.Rank)
            .Take(DashboardNoteCount)
            .Select(item => new DashboardNote
            {
                GoalId = item.Evaluation.Goal.Id,
                GoalName = item.Evaluation.Name,
                FirstDate = item.Entry.FirstDate,
                LastDate = item.Entry.LastDate,
                Note = item.Entry.Note!,
                State = item.Entry.State,
            })
            .ToList();

        var goals = ordered
            .Where(evaluation => !evaluation.Goal.IsEnded)
            .Select(evaluation => new DashboardGoal
            {
                GoalId = evaluation.Goal.Id,
                Name = evaluation.Name,
                Stats = StatsOf(evaluation),
                PausedUntil = evaluation.PausedUntil(today),
            })
            .ToList();

        return DashboardResult.Found(new Dashboard
        {
            Counts = new DashboardCounts
            {
                Due = due.Count,
                Pending = pending.Count,
                MissedThisWeek = missedThisWeek.Count,
                Goals = goals.Count(goal => goal.PausedUntil is null),
            },
            Today = [.. pending, .. due, .. missedThisWeek, .. upcoming],
            Goals = goals,
            Notes = notes,
            AccountToday = today,
            WeekStarts = weekStarts,
            ReadAt = now,
        });
    }

    /// <summary>The days from this week's Monday to <paramref name="date"/>: 0 on a Monday, 6 on a Sunday.</summary>
    private static int DaysSinceMonday(DateOnly date) => ((int)date.DayOfWeek + 6) % 7;

    /// <summary>
    /// One row of the Today list (FR-012, research R-06): its goal, the goal's settings and what can be recorded now, and
    /// the entry. Slice 8 (FR-019, research R-07): <see cref="TodayEntry.GoalEnded"/> for an ended goal's entry; an entry
    /// of an attempt that is not the latest has that attempt's name, and its Build or Reduce and schedule text from its
    /// own settings.
    /// </summary>
    private static TodayEntry TodayEntryOf(GoalEvaluation evaluation, HistoryEntry entry)
    {
        var recordable = evaluation.Recordable;
        var settings = recordable.SettingsOf(entry);
        return new TodayEntry
        {
            GoalId = evaluation.Goal.Id,
            // Stryker disable once Conditional : an entry of the latest attempt has no AttemptName, so the fallback gives evaluation.Name as well
            GoalName = entry.IsLatestAttempt ? evaluation.Name : entry.AttemptName ?? evaluation.Name,
            BuildOrReduce = entry.IsLatestAttempt ? evaluation.BuildOrReduce : settings.BuildOrReduce,
            ScheduleText = GoalText.ScheduleText(entry.IsLatestAttempt ? evaluation.LatestTarget : settings.Target),
            Goal = recordable,
            Entry = entry,
            GoalEnded = evaluation.Goal.IsEnded,
        };
    }

    /// <summary>
    /// One goal of the overview (FR-011, research R-07), with slice 7's <see cref="OverviewGoal.PausedUntil"/> at the
    /// pause date (<paramref name="accountToday"/> for a goal that has not started; research R-03, FR-018). Slice 8
    /// (FR-018, research R-07): <see cref="OverviewGoal.HasPending"/>, <see cref="OverviewGoal.CanRecord"/> and
    /// <see cref="OverviewGoal.Recordable"/> include the earlier attempts' open Pending entries; an ended goal is never
    /// paused.
    /// </summary>
    private static OverviewGoal OverviewGoalOf(StoredGoal stored, GoalEvaluation evaluation, DateOnly accountToday) => new()
    {
        Item = GoalListItems.Of(stored, evaluation.Now),
        Stats = StatsOf(evaluation),
        Recent = RecentOf(evaluation, OverviewRecentCount),
        HasPending = evaluation.HasPending,
        CanRecord = evaluation.RecordableEntries.Count > 0,
        Recordable = evaluation.Recordable,
        PausedUntil = evaluation.Goal.IsEnded ? null : evaluation.PausedUntil(accountToday),
    };

    private static GoalReport ReportOf(GoalEvaluation evaluation) => new()
    {
        GoalId = evaluation.Goal.Id,
        Name = evaluation.Name,
        BuildOrReduce = evaluation.BuildOrReduce,
        ScheduleKind = evaluation.LatestTarget.ScheduleKindOf,
        Period = evaluation.LatestTarget.PeriodLength,
        Stats = StatsOf(evaluation),
        Recent = RecentOf(evaluation, GoalReportRecentCount),
        Trend = TrendOf(evaluation),
        HasBegunEntry = evaluation.Entries.Any(entry => entry.Starts <= evaluation.Now),
        ReadAt = evaluation.Now,
    };

    /// <summary>The goal's figures with their texts (FR-002 to FR-004, HP-005); slice 8: also an attempt's (<see cref="AttemptService"/>).</summary>
    internal static GoalStatsView StatsOf(GoalEvaluation evaluation) => new()
    {
        Consistency = evaluation.Consistency,
        ConsistencyText = ReportText.Consistency(evaluation.Consistency),
        Unit = evaluation.Unit,
        CurrentStreak = evaluation.Streaks.Current,
        CurrentStreakText = ReportText.Streak(evaluation.Streaks.Current, evaluation.Unit),
        LongestStreak = evaluation.Streaks.Longest,
        LongestStreakText = ReportText.Streak(evaluation.Streaks.Longest, evaluation.Unit),
    };

    /// <summary>
    /// The latest <paramref name="count"/> ended results, oldest first, with the count of each ended state (FR-005).
    /// Slice 7 (FR-015, FR-023): the exempt results are among them, and the keys Excused and Paused are added only when
    /// their count is above 0, so a goal without exemptions keeps slice 6's four keys. Slice 8: also an attempt's
    /// (<see cref="AttemptService"/>).
    /// </summary>
    internal static RecentResults RecentOf(GoalEvaluation evaluation, int count)
    {
        var results = evaluation.Ended
            .TakeLast(count)
            .Select(entry => new ResultView { FirstDate = entry.FirstDate, LastDate = entry.LastDate, State = entry.State, Progress = entry.Progress })
            .ToList();
        var counts = EndedStates.ToDictionary(state => state, state => results.Count(result => result.State == state));
        foreach (var exempt in ExemptStates)
        {
            var number = results.Count(result => result.State == exempt);
            if (number > 0)
            {
                counts[exempt] = number;
            }
        }

        return new RecentResults { Results = results, Counts = counts };
    }

    /// <summary>
    /// The trend as the goal page draws it (FR-006): its texts from <see cref="ReportText"/>, decided by the target of
    /// the latest bar's settings version (research R-05, R-13).
    /// </summary>
    private static TrendView TrendOf(GoalEvaluation evaluation)
    {
        var trend = evaluation.Trend;
        var target = evaluation.TrendTarget;
        var view = new TrendView
        {
            Kind = trend.Kind,
            Span = trend.Span,
            Measurement = target.Measurement,
            CountUnit = target.CountUnit,
            Bars = trend.Bars,
            Target = trend.Target,
            Title = ReportText.TrendTitle(target, trend.Kind, trend.Span),
            SubLine = ReportText.TrendSubLine(target, trend.Span, trend.MaxBars),
            TargetLine = ReportText.TargetLine(trend.Kind, trend.Target),
            Label = string.Empty,
        };
        return view with { Label = ReportText.TrendLabel(view) };
    }

    /// <summary>
    /// FR-015's steps 1 to 3: not signed in, an administrator, an account that is not stored (deleted after sign-in, or
    /// a stale cookie, which also gives "You are not signed in." and reveals nothing).
    /// </summary>
    private async Task<Access> CheckAccountAsync(ActingUser actingUser, CancellationToken cancellationToken)
    {
        if (actingUser.AccountId is not { } owner)
        {
            return Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn);
        }

        if (actingUser.Role != Role.RegisteredUser)
        {
            return Access.Refused(AccessRefusal.NotForAdministrators, GoalTexts.NotForAdministrators);
        }

        return await _accounts.FindByIdAsync(owner, cancellationToken).ConfigureAwait(false) is { } account
            ? new Access(owner, account, null, null)
            : Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn);
    }

    /// <summary>The account's stored time zone, or the installation default when none is stored (as <see cref="GoalService"/> reads it).</summary>
    private NamedTimeZone ZoneOf(Account account) =>
        NamedTimeZone.TryParse(account.StoredTimeZone, out var zone) ? zone : _defaultZone.Value;

    private static GoalReportOutcome GoalReportOutcomeOf(AccessRefusal refusal) => refusal switch
    {
        AccessRefusal.NotSignedIn => GoalReportOutcome.NotSignedIn,
        _ => GoalReportOutcome.NotForAdministrators,
    };

    private enum AccessRefusal
    {
        NotSignedIn,
        NotForAdministrators,
    }

    /// <summary>The acting user's stored account, or why the use case refuses before anything else is looked at.</summary>
    private sealed record Access(AccountId Owner, Account? Account, AccessRefusal? Refusal, string? Message)
    {
        public static Access Refused(AccessRefusal refusal, string message) => new(default, null, refusal, message);
    }
}
