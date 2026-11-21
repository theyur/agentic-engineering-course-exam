using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.UnitTests.Application.Exemptions;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The instants at which slice 7's report reads change (FR-015, FR-018, research R-03, R-06), over the fakes of
/// <see cref="ExemptionWorld"/>: an exempt entry joins the recent results exactly when it ends, an entry is begun from
/// the instant it starts, and a goal is paused by the pause that covers its own current date, not the account's today.
/// Every goal is "Meditation, 10 min daily" (commitment-based, every day); the user has no stored time zone unless a test
/// says otherwise, so a day runs from 00:00Z to 00:00Z the next day. Every instant is worked out by hand.
/// </summary>
public sealed class ExemptionBoundaryTests
{
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");

    private readonly ExemptionWorld _world = new();

    private static GoalTargetFields Meditation10MinDaily => DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    [Fact]
    public async Task FR_015_an_excused_commitment_joins_the_recent_results_at_the_instant_it_ends_and_not_before()
    {
        // Wednesday 18 is excused; it ends on Thursday 19 at 00:00Z. At 12:00Z on Wednesday it is Excused but has not
        // ended, so the recent results are Monday 16 and Tuesday 17 (Pending); at 00:00Z on Thursday it is one of them.
        var goal = await _world.CreateGoalAsync(Meditation10MinDaily, Mon16, Mon16Morning);
        _world.SeedExcusal(goal, Wed18);

        _world.At(Utc("2026-11-18T12:00:00Z"));
        var before = await _world.ReportAsync(goal);
        Assert.Equal([Mon16, Tue17], before.Recent.Results.Select(r => r.FirstDate));

        _world.At(Utc("2026-11-19T00:00:00Z"));
        var atEnd = await _world.ReportAsync(goal);
        Assert.Equal([Mon16, Tue17, Wed18], atEnd.Recent.Results.Select(r => r.FirstDate));
        Assert.Equal(EvaluationState.Excused, atEnd.Recent.Results[^1].State);
    }

    [Fact]
    public async Task FR_015_a_paused_day_joins_the_recent_results_at_the_instant_it_ends_and_not_before()
    {
        var goal = await _world.CreateGoalAsync(Meditation10MinDaily, Mon16, Mon16Morning);
        _world.SeedPause(goal, Wed18, Wed18);

        _world.At(Utc("2026-11-18T23:59:59Z"));
        Assert.Equal([Mon16, Tue17], (await _world.ReportAsync(goal)).Recent.Results.Select(r => r.FirstDate));

        _world.At(Utc("2026-11-19T00:00:00Z"));
        var atEnd = await _world.ReportAsync(goal);
        Assert.Equal([Mon16, Tue17, Wed18], atEnd.Recent.Results.Select(r => r.FirstDate));
        Assert.Equal(EvaluationState.Paused, atEnd.Recent.Results[^1].State);
    }

    [Fact]
    public async Task R_06_the_first_commitment_is_begun_at_the_instant_it_starts()
    {
        // Created on Monday 16 at 08:00Z with the start date Tuesday 17, read on Tuesday 17 at 00:00Z exactly: Tuesday 17
        // is Due, the goal has a begun entry and the trend has its bar.
        var goal = await _world.CreateGoalAsync(Meditation10MinDaily, Tue17, Mon16Morning);
        _world.At(Utc("2026-11-17T00:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.True(report.HasBegunEntry);
        Assert.Equal([Tue17], report.Trend.Bars.Select(bar => bar.FirstDate));
    }

    [Fact]
    public async Task R_03_a_goal_is_paused_by_the_pause_of_its_own_current_date_not_the_accounts_today()
    {
        // The account is in Kiritimati (UTC+14), the goal's version in UTC. At 12:00Z on Wednesday 18 the goal's current
        // date is Wednesday 18 and the account's today Thursday 19 (02:00 in Kiritimati). The pause ends on Wednesday 18,
        // so it is current for the goal, which is paused until that day.
        var user = _world.UserIn("Pacific/Kiritimati");
        var goal = _world.AddGoal(user, Target(Meditation10MinDaily), (Mon16, NamedTimeZone.Utc));
        _world.SeedPause(goal, Tue17, Wed18);
        _world.At(Utc("2026-11-18T12:00:00Z"));

        var overview = await _world.OverviewAsync(user);

        Assert.Equal(Wed18, Assert.Single(overview.Goals, g => g.Item.Id == goal).PausedUntil);
    }
}
