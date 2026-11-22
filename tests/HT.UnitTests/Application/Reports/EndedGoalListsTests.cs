using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.UnitTests.Application.Goals;
using static HT.UnitTests.Application.Reports.EndedGoalsArrangement;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Slice 8 (T095; US4-AS1, AS2, AS4, AS5, FR-017 to FR-020): the lists after an ending and a restart, on spec User Story
/// 4's account (<see cref="EndedGoalsArrangement"/>), whose goals are made through <see cref="GoalService"/>
/// and <see cref="GoalChangeService"/>. Everything is in UTC. Meditation's 14 November grace period ends
/// at 2026-11-17T00:00:00Z and its 15 November one at 2026-11-18T00:00:00Z, so at 2026-11-16T12:00:00Z both are Pending and
/// at 2026-11-18T12:00:00Z both are Missed. Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class EndedGoalListsTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    /// <summary>Wednesday 18 November at 12:00Z, after both of Meditation's grace periods.</summary>
    private static readonly DateTimeOffset Wed18At12 = Utc("2026-11-18T12:00:00Z");

    private readonly AttemptWorld _world = new();

    [Fact]
    public async Task US4_AS1_the_overview_flags_Meditation_ended_on_16_November_with_Abandoned_in_slice_6s_order()
    {
        var goals = await MakeAsync(_world);

        var overview = await _world.OverviewAsync();

        // Slice 6's order: the goals with a Pending result (Meditation and Reading, by name), then Sugar, Due today.
        Assert.Equal(
            [
                (goals.Meditation, MeditationName, true, (DateOnly?)Mon16, (Outcome?)Outcome.Abandoned),
                (goals.Reading, ReadingName, false, null, null),
                (goals.Sugar, SugarName, false, null, null),
            ],
            overview.Goals.Select(g => (g.Item.Id, g.Item.Name, g.Item.IsEnded, g.Item.EndingDate, g.Item.Outcome)));
        var meditation = overview.Goals[0];
        Assert.Null(meditation.PausedUntil);
        Assert.True(meditation.HasPending);
        Assert.DoesNotContain(meditation.Recordable.Entries, e => e.State == EvaluationState.Due);
        Assert.Equal("100% (12 of 12)", meditation.Stats.ConsistencyText);
    }

    [Fact]
    public async Task US4_AS2_the_dashboard_at_12_00_on_16_November_keeps_Meditations_Pending_entries_marked_as_ended()
    {
        var goals = await MakeAsync(_world);

        var dashboard = await _world.DashboardAsync();

        var meditation = dashboard.Today.Where(e => e.GoalId == goals.Meditation).ToList();
        Assert.Equal(
            [(Sat14, EvaluationState.Pending), (Sun15, EvaluationState.Pending)],
            meditation.Select(e => (e.Entry.FirstDate, e.Entry.State)).OrderBy(e => e.FirstDate));
        Assert.All(meditation, e => Assert.Equal((MeditationName, true, 1, true), (e.GoalName, e.GoalEnded, e.Entry.AttemptNumber, e.Entry.IsLatestAttempt)));
        Assert.All(dashboard.Today.Where(e => e.GoalId != goals.Meditation), e => Assert.False(e.GoalEnded));
        // Pending: Reading's and Meditation's 14 and 15 November; Due: Reading's and Sugar's 16 November.
        Assert.Equal((2, 4, 2), (dashboard.Counts.Due, dashboard.Counts.Pending, dashboard.Counts.Goals));
        Assert.Equal([ReadingName, SugarName], dashboard.Goals.Select(g => g.Name));
        Assert.DoesNotContain(dashboard.Notes, n => n.GoalId == goals.Meditation);
    }

    [Fact]
    public async Task US4_AS2_the_dashboard_on_18_November_has_no_entry_of_Meditation_and_does_not_count_its_Missed_results()
    {
        var goals = await MakeAsync(_world);
        _world.At(Wed18At12);

        var dashboard = await _world.DashboardAsync();

        Assert.DoesNotContain(dashboard.Today, e => e.GoalId == goals.Meditation);
        Assert.DoesNotContain(dashboard.Goals, g => g.GoalId == goals.Meditation);
        // Only Reading's Missed 14 and 15 November, whose grace periods ended this week; Meditation's are neither listed
        // nor counted.
        Assert.Equal(2, dashboard.Counts.MissedThisWeek);
        Assert.Equal(2, dashboard.Counts.Goals);
    }

    [Fact]
    public async Task US4_AS4_Sugar_is_on_attempt_2_from_16_November_and_its_report_counts_only_attempt_2()
    {
        var goals = await MakeAsync(_world);

        var details = await _world.DetailsAsync(goals.Sugar);
        var report = await _world.ReportAsync(goals.Sugar);

        Assert.Equal((2, Mon16, false), (details.AttemptNumber, details.AttemptStartDate, details.IsEnded));
        Assert.Equal("N/A", report.Stats.ConsistencyText);
        var overview = await _world.OverviewAsync();
        Assert.Equal("N/A", overview.Goals.Single(g => g.Item.Id == goals.Sugar).Stats.ConsistencyText);
    }

    [Fact]
    public async Task US4_AS5_the_catalog_leaves_the_ended_goal_out_of_tracked_habits_counts_and_details()
    {
        var goals = await MakeAsync(_world);

        var tracked = await _world.Catalog.BrowseAsync(_world.User, new CatalogFilter(), true, None);
        var all = await _world.Catalog.BrowseAsync(_world.User, new CatalogFilter(), false, None);
        var details = await _world.Catalog.GetDetailsAsync(_world.User, goals.MeditationHabit.Id, None);

        Assert.Equal(CatalogBrowseOutcome.Found, tracked.Outcome);
        Assert.Equal(["Reading", "Sugar"], tracked.Habits.Select(h => h.Name));
        Assert.Equal(
            [("Meditation", (int?)0), ("Reading", 1), ("Sugar", 1)],
            all.Habits.Select(h => (h.Name, h.GoalCount)).OrderBy(h => h.Name, StringComparer.Ordinal));
        Assert.Equal(CatalogHabitDetailsOutcome.Found, details.Outcome);
        Assert.Equal(0, details.Details!.GoalCount);
        Assert.Empty(details.Details.Goals!);
    }
}
