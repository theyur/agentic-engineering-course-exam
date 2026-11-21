using HT.Application.Accounts;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Report privacy (US7-AS1 to AS4, FR-014, FR-015, SC-005) over the fakes of <see cref="ReportWorld"/>: for each of the
/// three <see cref="ReportService"/> use cases, an anonymous visitor, an administrator and an account the store does not
/// have each get their refusal and no view, before any goal lookup and with no record store call. B reading A's goal
/// gets the same refusal as for a goal that does not exist, and B's overview and dashboard hold only B's goal. User A's
/// goal "A's private reading" (20 min daily, in UTC) and B's goal "B's walk" start on Monday 9 November 2026, created
/// at 08:00Z that day. Their records and notes are stored directly, so the arrangement makes no goal lookup and no record
/// store call; the reads are made on Thursday 12 November at 10:00Z.
/// </summary>
public sealed class ReportAccessTests
{
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string NotSignedIn = "You are not signed in.";

    private const string AsGoalName = "A's private reading";
    private const string AsNote = "A's private evening note.";
    private const string BsGoalName = "B's walk";
    private const string BsNote = "B's own note.";

    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Tue10 = Date(2026, 11, 10);
    private static readonly DateTimeOffset Mon9Morning = Utc("2026-11-09T08:00:00Z");
    private static readonly DateTimeOffset Thu12TenOClock = Utc("2026-11-12T10:00:00Z");

    private static readonly string[] ThreeUseCases =
    [
        nameof(ReportService.GetGoalReportAsync),
        nameof(ReportService.GetGoalsOverviewAsync),
        nameof(ReportService.GetDashboardAsync),
    ];

    private readonly ReportWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>One call of a use case: its name, its outcome's name, its text and whether it returned a view.</summary>
    private sealed record Call(string UseCase, string Outcome, string? Message, bool HasView);

    /// <summary>
    /// A's goal with 25 minutes on Monday 9, 12 minutes on Tuesday 10 and a note on Tuesday 10; B's goal with 20 minutes
    /// on Monday 9 and a note on it. The clock is left at Thursday 12, 10:00Z.
    /// </summary>
    private async Task<(GoalId A, GoalId B)> TwoUsersGoalsAsync()
    {
        var a = await _world.CreateNamedGoalAsync(AsGoalName, Reading20MinDaily, Mon9, Mon9Morning);
        var b = await _world.CreateNamedGoalAsync(BsGoalName, Reading20MinDaily, Mon9, Mon9Morning, _world.OtherUser);
        _world.SeedAmount(a, Mon9, 25);
        _world.SeedAmount(a, Tue10, 12);
        _world.SeedNote(a, Tue10, AsNote);
        _world.SeedAmount(b, Mon9, 20);
        _world.SeedNote(b, Mon9, BsNote);
        _world.At(Thu12TenOClock);
        return (a, b);
    }

    /// <summary>The three use cases by <paramref name="user"/>, the goal report on <paramref name="goal"/>.</summary>
    private async Task<IReadOnlyList<Call>> EveryUseCaseAsync(ActingUser user, GoalId goal)
    {
        var service = _world.Service;
        var none = CancellationToken.None;
        var report = await service.GetGoalReportAsync(user, goal, none);
        var overview = await service.GetGoalsOverviewAsync(user, none);
        var dashboard = await service.GetDashboardAsync(user, none);
        return
        [
            new(nameof(service.GetGoalReportAsync), report.Outcome.ToString(), report.Message, report.Report is not null),
            new(nameof(service.GetGoalsOverviewAsync), overview.Outcome.ToString(), overview.Message, overview.Overview is not null),
            new(nameof(service.GetDashboardAsync), dashboard.Outcome.ToString(), dashboard.Message, dashboard.Dashboard is not null),
        ];
    }

    /// <summary>Every use case was refused with <paramref name="outcome"/> and <paramref name="message"/>, with no view, no goal lookup and no record store call.</summary>
    private void AssertRefusedBeforeAnyLookup(IReadOnlyList<Call> calls, string outcome, string message)
    {
        Assert.Equal(ThreeUseCases, calls.Select(c => c.UseCase));
        Assert.All(calls, call =>
        {
            Assert.Equal(outcome, call.Outcome);
            Assert.Equal(message, call.Message);
            Assert.False(call.HasView);
        });
        Assert.Equal(0, _world.Goals.Lookups);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task US7_AS3_an_anonymous_visitor_gets_You_are_not_signed_in_from_every_use_case_before_any_lookup()
    {
        var (a, _) = await TwoUsersGoalsAsync();

        var calls = await EveryUseCaseAsync(ActingUser.Anonymous, a);

        AssertRefusedBeforeAnyLookup(calls, "NotSignedIn", NotSignedIn);
    }

    [Fact]
    public async Task US7_AS2_an_administrator_gets_Goals_are_for_registered_users_from_every_use_case_before_any_lookup()
    {
        var (a, _) = await TwoUsersGoalsAsync();
        var unstored = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

        foreach (var administrator in new[] { _world.Administrator, unstored })
        {
            var calls = await EveryUseCaseAsync(administrator, a);

            AssertRefusedBeforeAnyLookup(calls, "NotForAdministrators", NotForAdministrators);
        }
    }

    [Fact]
    public async Task US7_AS4_an_account_the_store_does_not_have_gets_You_are_not_signed_in_from_every_use_case_before_any_lookup()
    {
        var (a, _) = await TwoUsersGoalsAsync();

        var calls = await EveryUseCaseAsync(ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser), a);

        AssertRefusedBeforeAnyLookup(calls, "NotSignedIn", NotSignedIn);
    }

    [Fact]
    public async Task US7_AS1_B_reading_As_goal_gets_This_goal_does_not_exist_the_same_as_for_an_unknown_goal()
    {
        var (a, _) = await TwoUsersGoalsAsync();

        var onAsGoal = await _world.ReadAsync(a, _world.OtherUser);
        var onNoGoal = await _world.ReadAsync(new GoalId(Guid.NewGuid()), _world.OtherUser);

        foreach (var result in new[] { onAsGoal, onNoGoal })
        {
            Assert.Equal(GoalReportOutcome.NotFound, result.Outcome);
            Assert.Equal(GoalNotFound, result.Message);
            Assert.Null(result.Report);
        }

        // Only the owner-scoped goal lookups ran; no record or note of A's goal was read.
        Assert.Equal(2, _world.Goals.Lookups);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task US7_AS1_Bs_overview_holds_only_Bs_goal_with_its_own_results()
    {
        var (_, b) = await TwoUsersGoalsAsync();

        var overview = await _world.OverviewAsync(_world.OtherUser);

        var goal = Assert.Single(overview.Goals);
        Assert.Equal((b, BsGoalName), (goal.Item.Id, goal.Item.Name));
        // B's only ended results are Monday 9 to Wednesday 11, with B's 20 minutes on Monday 9 alone.
        Assert.Equal([Mon9, Tue10, Date(2026, 11, 11)], goal.Recent.Results.Select(r => r.FirstDate).Order());
        Assert.Equal("100% (1 of 1)", goal.Stats.ConsistencyText);
        Assert.All(goal.Recordable.Entries, entry => Assert.NotEqual(AsNote, entry.Note));
    }

    [Fact]
    public async Task US7_AS1_Bs_dashboard_holds_none_of_As_goals_results_or_notes()
    {
        var (_, b) = await TwoUsersGoalsAsync();

        var dashboard = await _world.DashboardAsync(_world.OtherUser);

        Assert.Equal(1, dashboard.Counts.Goals);
        Assert.Equal([(b, BsGoalName)], dashboard.Goals.Select(g => (g.GoalId, g.Name)));
        Assert.NotEmpty(dashboard.Today);
        Assert.All(dashboard.Today, row =>
        {
            Assert.Equal((b, BsGoalName), (row.GoalId, row.GoalName));
            Assert.NotEqual(AsNote, row.Entry.Note);
        });
        var note = Assert.Single(dashboard.Notes);
        Assert.Equal((b, BsGoalName, BsNote), (note.GoalId, note.GoalName, note.Note));
    }

    [Fact]
    public async Task The_same_reads_succeed_for_A_so_each_refusal_comes_from_the_acting_user()
    {
        var (a, _) = await TwoUsersGoalsAsync();

        var calls = await EveryUseCaseAsync(_world.User, a);

        Assert.Equal(ThreeUseCases, calls.Select(c => c.UseCase));
        Assert.All(calls, call =>
        {
            Assert.Equal("Found", call.Outcome);
            Assert.Null(call.Message);
            Assert.True(call.HasView);
        });
        var dashboard = await _world.DashboardAsync();
        Assert.Equal([(a, AsGoalName)], dashboard.Goals.Select(g => (g.GoalId, g.Name)));
        Assert.Equal(AsNote, Assert.Single(dashboard.Notes).Note);
    }
}
