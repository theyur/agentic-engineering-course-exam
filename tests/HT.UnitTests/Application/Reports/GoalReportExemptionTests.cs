using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using HT.UnitTests.Application.Exemptions;
using static HT.UnitTests.Application.Exemptions.ExemptionWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The figures and recent results of a goal with excusals and pauses through <see cref="ReportService.GetGoalReportAsync"/>
/// (slice 7 US3-AS1 to AS3, AS5 to AS7; FR-013 to FR-015, FR-024, SC-002; T066), over the fakes of
/// <see cref="ExemptionWorld"/>. Excusals and pauses are made through <see cref="ExcusalService"/> and
/// <see cref="PauseService"/> while they are current, then the clock is moved forward and the report is read. The users
/// have no stored time zone, so the goals are in UTC; every goal starts on Monday 2 November 2026 and is created at 08:00Z
/// that day. A day runs from 00:00Z to 00:00Z the next day and its grace period ends 48 hours later; a week runs from
/// Monday 00:00Z and its grace period ends on the Wednesday after it, at 00:00Z. Every instant is worked out by hand.
/// </summary>
public sealed class GoalReportExemptionTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Tue3 = Date(2026, 11, 3);
    private static readonly DateOnly Sun8 = Date(2026, 11, 8);
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);

    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>Tuesday 10 November at 08:00Z: Monday 9 November is Pending (its grace period ends on Thursday 12 at 00:00Z).</summary>
    private static readonly DateTimeOffset Tue10Morning = Utc("2026-11-10T08:00:00Z");

    /// <summary>Friday 13 November at 08:00Z, when US3's goal is paused from that day until Sunday 15 November.</summary>
    private static readonly DateTimeOffset Fri13Morning = Utc("2026-11-13T08:00:00Z");

    /// <summary>
    /// Thursday 19 November at 12:00Z, when US3's report is read: 2 to 16 November are final (16 November's grace period
    /// ended at 00:00Z), 17 and 18 November are Pending and 19 November is Due.
    /// </summary>
    private static readonly DateTimeOffset Thu19Noon = Utc("2026-11-19T12:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Meditation, 10 min daily": Build, duration at least 10 minutes, all seven days (commitment-based).</summary>
    private static GoalTargetFields Meditation10MinDaily => DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>"Yoga, 3 times a week": Build, yes/no, 3 times per week (period-based).</summary>
    private static GoalTargetFields Yoga3TimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    /// <summary>
    /// US3's goal, created on Monday 2 November at 08:00Z with that start date: 10 minutes on each date from 2 to 16
    /// November except 9, 11 and 13 to 15 November.
    /// </summary>
    private async Task<GoalId> Us3GoalAsync()
    {
        var goal = await _world.CreateGoalAsync(Meditation10MinDaily, Mon2, Mon2Morning);
        for (var date = Mon2; date <= Date(2026, 11, 16); date = date.AddDays(1))
        {
            if (date != Mon9 && date != Date(2026, 11, 11) && (date < Fri13 || date > Sun15))
            {
                _world.SeedAmount(goal, date, 10);
            }
        }

        return goal;
    }

    /// <summary>
    /// US3's goal with its exemptions: Monday 9 November excused on Tuesday 10 November at 08:00Z, and the goal paused
    /// on Friday 13 November at 08:00Z from that day until Sunday 15 November; then read on Thursday 19 November at
    /// 12:00Z.
    /// </summary>
    private async Task<GoalReport> Us3ReportAsync()
    {
        var goal = await Us3GoalAsync();
        await ExcuseAsync(goal, Tue10Morning, Mon9);
        await PauseAsync(goal, Fri13Morning, Fri13, Sun15);
        _world.At(Thu19Noon);
        return await _world.ReportAsync(goal);
    }

    private async Task ExcuseAsync(GoalId goal, DateTimeOffset at, DateOnly date, ActingUser? user = null)
    {
        _world.At(at);
        Assert.Equal(ExcuseOutcome.Excused, (await _world.ExcuseAsync(goal, date, user: user)).Outcome);
    }

    private async Task PauseAsync(GoalId goal, DateTimeOffset at, DateOnly firstDay, DateOnly lastDay, ActingUser? user = null)
    {
        _world.At(at);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, firstDay, lastDay, user: user)).Outcome);
    }

    /// <summary>
    /// SC-002: the figures equal those of the same ended results with the exempt ones removed from the sequence. Every
    /// test goal has fewer than 30 ended results, so the recent results hold all of them.
    /// </summary>
    private static void AssertFiguresOfTheSequenceWithoutExemptResults(GoalReport report)
    {
        var counted = report.Recent.Results
            .Select(result => result.State)
            .Where(state => state is not (EvaluationState.Excused or EvaluationState.Paused))
            .ToList();
        var streaks = Streaks.Of(counted);

        Assert.Equal(Consistency.Of(counted), report.Stats.Consistency);
        Assert.Equal((streaks.Current, streaks.Longest), (report.Stats.CurrentStreak, report.Stats.LongestStreak));
    }

    [Fact]
    public async Task US3_AS1_AS2_the_excused_and_paused_days_are_not_counted_and_neither_extend_nor_reset_a_streak()
    {
        var report = await Us3ReportAsync();

        Assert.Equal("90% (10 of 11)", report.Stats.ConsistencyText);
        Assert.Equal(new Consistency(10, 11), report.Stats.Consistency);
        Assert.Equal(StreakUnit.Commitments, report.Stats.Unit);
        Assert.Equal((2, "2 commitments"), (report.Stats.CurrentStreak, report.Stats.CurrentStreakText));
        Assert.Equal((8, "8 commitments"), (report.Stats.LongestStreak, report.Stats.LongestStreakText));
        AssertFiguresOfTheSequenceWithoutExemptResults(report);
    }

    [Fact]
    public async Task US3_AS3_the_recent_results_are_the_17_ended_results_oldest_first_with_the_number_of_each_state()
    {
        var recent = (await Us3ReportAsync()).Recent;

        var met = EvaluationState.Met;
        Assert.Equal(
            [
                met, met, met, met, met, met, met, EvaluationState.Excused, met, EvaluationState.Missed, met,
                EvaluationState.Paused, EvaluationState.Paused, EvaluationState.Paused, met, EvaluationState.Pending, EvaluationState.Pending,
            ],
            recent.Results.Select(result => result.State));
        Assert.Equal(Enumerable.Range(0, 17).Select(day => Mon2.AddDays(day)), recent.Results.Select(result => result.FirstDate));
        Assert.Equal(
            new ResultView { FirstDate = Mon9, LastDate = Mon9, State = EvaluationState.Excused, Progress = new Progress(ProgressKind.RecordedValue, null, 10) },
            recent.Results[7]);
        Assert.Equal(
            new ResultView { FirstDate = Fri13, LastDate = Fri13, State = EvaluationState.Paused, Progress = new Progress(ProgressKind.RecordedValue, null, 10) },
            recent.Results[11]);
        Assert.Equal(
            new Dictionary<EvaluationState, int>
            {
                [EvaluationState.Met] = 10, [EvaluationState.Failed] = 0, [EvaluationState.Missed] = 1,
                [EvaluationState.Excused] = 1, [EvaluationState.Paused] = 3, [EvaluationState.Pending] = 2,
            },
            recent.Counts);
    }

    [Fact]
    public async Task US3_AS5_an_excusal_removed_and_the_date_recorded_late_gives_the_figures_of_the_record_entered_on_time()
    {
        // The excusal of Monday 9 November is removed on Tuesday 10 November at 12:00Z, and 4 minutes are recorded for
        // that date then. The twin goal has the 4 minutes recorded on Monday 9 November at 20:00Z and no excusal; both
        // are paused on Friday 13 November at 08:00Z from that day until Sunday 15 November.
        var goal = await Us3GoalAsync();
        var twin = await Us3GoalAsync();
        _world.At(Utc("2026-11-09T20:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(twin, Mon9, Minutes(4))).Outcome);
        await ExcuseAsync(goal, Tue10Morning, Mon9);
        _world.At(Utc("2026-11-10T12:00:00Z"));
        Assert.Equal(RemoveExcusalOutcome.Removed, (await _world.RemoveExcusalAsync(goal, Mon9)).Outcome);
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Mon9, Minutes(4))).Outcome);
        await PauseAsync(goal, Fri13Morning, Fri13, Sun15);
        await PauseAsync(twin, Fri13Morning, Fri13, Sun15);
        _world.At(Thu19Noon);

        var report = await _world.ReportAsync(goal);
        var onTime = await _world.ReportAsync(twin);

        Assert.Equal("83% (10 of 12)", report.Stats.ConsistencyText);
        Assert.Equal((2, 7), (report.Stats.CurrentStreak, report.Stats.LongestStreak));
        Assert.Equal("7 commitments", report.Stats.LongestStreakText);
        Assert.Equal(EvaluationState.Failed, report.Recent.Results[7].State);
        Assert.Equal(onTime.Stats, report.Stats);
        Assert.Equal(onTime.Recent.Results, report.Recent.Results);
        Assert.Equal(onTime.Recent.Counts, report.Recent.Counts);
        Assert.Equal(onTime.Trend.Bars, report.Trend.Bars);
        AssertFiguresOfTheSequenceWithoutExemptResults(report);
    }

    [Fact]
    public async Task US3_AS6_a_goal_whose_ended_results_are_all_exempt_gives_N_A_and_0_commitments()
    {
        // The other account's goal, nothing recorded: Monday 2 November excused on Tuesday 3 November at 08:00Z, and the
        // goal paused at that time from Tuesday 3 until Sunday 8 November. Read on Monday 9 November at 12:00Z, when 2 to
        // 8 November have ended and Monday 9 is Due.
        var user = _world.OtherUser;
        var goal = await _world.CreateGoalAsync(Meditation10MinDaily, Mon2, Mon2Morning, user);
        var tue3Morning = Utc("2026-11-03T08:00:00Z");
        await ExcuseAsync(goal, tue3Morning, Mon2, user);
        await PauseAsync(goal, tue3Morning, Tue3, Sun8, user);
        _world.At(Utc("2026-11-09T12:00:00Z"));

        var report = await _world.ReportAsync(goal, user);

        Assert.Equal("N/A", report.Stats.ConsistencyText);
        Assert.Equal(new Consistency(0, 0), report.Stats.Consistency);
        Assert.Equal(("0 commitments", "0 commitments"), (report.Stats.CurrentStreakText, report.Stats.LongestStreakText));
        Assert.Equal((0, 0), (report.Stats.CurrentStreak, report.Stats.LongestStreak));
        Assert.Equal(
            [EvaluationState.Excused, .. Enumerable.Repeat(EvaluationState.Paused, 6)],
            report.Recent.Results.Select(result => result.State));
        AssertFiguresOfTheSequenceWithoutExemptResults(report);
    }

    [Fact]
    public async Task US3_AS7_a_paused_week_of_a_per_week_goal_is_not_counted_and_does_not_break_the_streak()
    {
        // Three "Yes" in the week of 2 November and in the week of 16 November; paused on Monday 9 November at 08:00Z from
        // that day until Sunday 15 November. Read on Wednesday 25 November at 12:00Z, when the week of 16 November is
        // final (its grace period ended at 00:00Z) and the week of 23 November is Due.
        var goal = await _world.CreateGoalAsync(Yoga3TimesAWeek, Mon2, Mon2Morning);
        foreach (var day in new[] { 2, 4, 6, 16, 18, 20 })
        {
            _world.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        await PauseAsync(goal, Utc("2026-11-09T08:00:00Z"), Mon9, Sun15);
        _world.At(Utc("2026-11-25T12:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal("100% (2 of 2)", report.Stats.ConsistencyText);
        Assert.Equal(StreakUnit.Weeks, report.Stats.Unit);
        Assert.Equal((2, "2 weeks"), (report.Stats.CurrentStreak, report.Stats.CurrentStreakText));
        Assert.Equal(2, report.Stats.LongestStreak);
        Assert.Equal(
            [(Mon2, Sun8, EvaluationState.Met), (Mon9, Sun15, EvaluationState.Paused), (Date(2026, 11, 16), Date(2026, 11, 22), EvaluationState.Met)],
            report.Recent.Results.Select(result => (result.FirstDate, result.LastDate, result.State)));
        AssertFiguresOfTheSequenceWithoutExemptResults(report);
    }
}
