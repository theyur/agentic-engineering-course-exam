using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Reports;

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
    /// <summary>How many ended results the goal report gives at most (FR-005).</summary>
    private const int GoalReportRecentCount = 30;

    /// <summary>The ended states, each counted in <see cref="RecentResults.Counts"/>, 0 included (FR-005).</summary>
    private static readonly EvaluationState[] EndedStates =
        [EvaluationState.Met, EvaluationState.Failed, EvaluationState.Missed, EvaluationState.Pending];

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
        HasBegunEntry = evaluation.Entries.Any(entry => entry.State != EvaluationState.Upcoming),
        ReadAt = evaluation.Now,
    };

    /// <summary>The goal's figures with their texts (FR-002 to FR-004, HP-005).</summary>
    private static GoalStatsView StatsOf(GoalEvaluation evaluation) => new()
    {
        Consistency = evaluation.Consistency,
        ConsistencyText = ReportText.Consistency(evaluation.Consistency),
        Unit = evaluation.Unit,
        CurrentStreak = evaluation.Streaks.Current,
        CurrentStreakText = ReportText.Streak(evaluation.Streaks.Current, evaluation.Unit),
        LongestStreak = evaluation.Streaks.Longest,
        LongestStreakText = ReportText.Streak(evaluation.Streaks.Longest, evaluation.Unit),
    };

    /// <summary>The latest <paramref name="count"/> ended results, oldest first, with the count of each ended state (FR-005).</summary>
    private static RecentResults RecentOf(GoalEvaluation evaluation, int count)
    {
        var results = evaluation.Ended
            .TakeLast(count)
            .Select(entry => new ResultView { FirstDate = entry.FirstDate, LastDate = entry.LastDate, State = entry.State, Progress = entry.Progress })
            .ToList();
        var counts = EndedStates.ToDictionary(state => state, state => results.Count(result => result.State == state));
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
