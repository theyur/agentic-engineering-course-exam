using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T063; US1-AS3 to AS5, AS7, FR-005, FR-006, SC-002, SC-005): what a settings change made through
/// <see cref="GoalChangeService.ChangeSettingsAsync"/> does to the history and the report, on <see cref="AttemptWorld"/>
/// in UTC. "Reading, 15 min daily" starts on Monday 2 November 2026 with 15 minutes recorded on each date from 2 to 15
/// November; the change to 20 minutes on Wednesday 11 November at 10:00Z applies from Thursday 12 November. A change
/// never alters the result or the target of a date whose day had begun (SPEC §5.4): the boundary tests read every entry
/// before and after the change at the same instant and compare them. Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class ChangeSettingsEffectTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Mon30 = Date(2026, 11, 30);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private async Task<SettingsChangeResult> ChangeAsync(GoalId goal, GoalTargetFields target)
    {
        var result = await _world.Changes.ChangeSettingsAsync(_world.User, goal, new GoalSettingsInput { Target = target }, None);
        Assert.Equal(SettingsChangeOutcome.Changed, result.Outcome);
        return result;
    }

    /// <summary>"Reading, 15 min daily" with <paramref name="minutes"/> recorded on each date from 2 to 15 November; returns the goal and its record ids by date.</summary>
    private async Task<(GoalId Goal, Dictionary<DateOnly, RecordId> Records)> ReadingAsync(Func<DateOnly, int>? minutes = null)
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        var records = new Dictionary<DateOnly, RecordId>();
        for (var day = Mon2; day <= Sun15; day = day.AddDays(1))
        {
            records[day] = _world.SeedAmount(goal, day, minutes?.Invoke(day) ?? 15);
        }

        return (goal, records);
    }

    private static EvaluationState StateOn(GoalHistory history, DateOnly date) => history.Entries.Single(e => e.FirstDate == date).State;

    /// <summary>Each entry that begins before <paramref name="appliesFrom"/>: its dates, state, progress and the target of its own version.</summary>
    private static List<(DateOnly First, DateOnly Last, EvaluationState State, string Progress, GoalTarget Target)> Begun(GoalHistory history, DateOnly appliesFrom) =>
        history.Entries
            .Where(e => e.FirstDate < appliesFrom)
            .Select(e => (e.FirstDate, e.LastDate, e.State, ReportText.ProgressText(history.SettingsOf(e), e), history.SettingsOf(e).Target))
            .ToList();

    [Fact]
    public async Task US1_AS3_the_dates_before_the_change_keep_15_minutes_and_later_dates_are_judged_by_20()
    {
        var (goal, _) = await ReadingAsync();
        _world.At(Utc("2026-11-11T10:00:00Z"));
        Assert.Equal((DateOnly?)Thu12, (await ChangeAsync(goal, MinutesDaily(20))).AppliesFrom);

        _world.At(Utc("2026-11-16T12:00:00Z"));
        var history = await _world.HistoryAsync(goal, Mon2, Sun15);

        for (var day = Mon2; day <= Wed11; day = day.AddDays(1))
        {
            Assert.Equal(EvaluationState.Met, StateOn(history, day));
            Assert.Equal(15, history.SettingsOf(history.Entries.Single(e => e.FirstDate == day)).Amount);
        }

        Assert.Equal([EvaluationState.Failed, EvaluationState.Failed, EvaluationState.Pending, EvaluationState.Pending],
            new[] { Thu12, Fri13, Sat14, Sun15 }.Select(day => StateOn(history, day)));
        Assert.All(history.Entries.Where(e => e.FirstDate >= Thu12), e => Assert.Equal(20, history.SettingsOf(e).Amount));
    }

    [Fact]
    public async Task US1_AS4_the_report_counts_10_of_12_and_the_longest_streak_is_10_commitments()
    {
        var (goal, _) = await ReadingAsync();
        _world.At(Utc("2026-11-11T10:00:00Z"));
        await ChangeAsync(goal, MinutesDaily(20));

        _world.At(Utc("2026-11-16T12:00:00Z"));
        var report = await _world.ReportAsync(goal);

        Assert.Equal("83% (10 of 12)", report.Stats.ConsistencyText);
        Assert.Equal((StreakUnit.Commitments, "0 commitments", "10 commitments"),
            (report.Stats.Unit, report.Stats.CurrentStreakText, report.Stats.LongestStreakText));
    }

    [Fact]
    public async Task US1_AS5_a_correction_of_9_November_is_judged_by_the_settings_of_its_date()
    {
        var (goal, records) = await ReadingAsync();
        _world.At(Utc("2026-11-11T10:00:00Z"));
        await ChangeAsync(goal, MinutesDaily(20));

        _world.At(Utc("2026-11-16T12:00:00Z"));
        var corrected = await _world.Recording.ChangeRecordAsync(_world.User, goal, records[Mon9], new RecordInput { Amount = 12 }, None);
        Assert.Equal(RecordWriteOutcome.Changed, corrected.Outcome);

        var history = await _world.HistoryAsync(goal, Mon2, Sun15);
        Assert.Equal(EvaluationState.Failed, StateOn(history, Mon9));
        Assert.Equal(15, history.SettingsOf(history.Entries.Single(e => e.FirstDate == Mon9)).Amount);
        Assert.Equal("75% (9 of 12)", (await _world.ReportAsync(goal)).Stats.ConsistencyText);
    }

    [Fact]
    public async Task US1_AS7_the_week_16_to_22_November_keeps_its_target_of_3_after_a_change_to_4_on_Wednesday()
    {
        var goal = await _world.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Mon2, Created);
        foreach (var day in new[] { Mon16, Date(2026, 11, 17), Date(2026, 11, 18) })
        {
            _world.SeedAnswer(goal, day, YesNoAnswer.Yes);
        }

        _world.At(Utc("2026-11-18T10:00:00Z"));
        Assert.Equal((DateOnly?)Mon23, (await ChangeAsync(goal, YesNoTimes(4, SchedulePeriod.Week))).AppliesFrom);

        _world.At(Utc("2026-11-25T12:00:00Z"));
        var history = await _world.HistoryAsync(goal, Mon16, Mon30);
        var week = history.Entries.Single(e => e.FirstDate == Mon16);
        Assert.Equal((Sun22, EvaluationState.Met, 3), (week.LastDate, week.State, history.SettingsOf(week).Times!.Value));
        Assert.Equal(4, history.SettingsOf(history.Entries.Single(e => e.FirstDate == Mon23)).Times);
    }

    [Fact]
    public async Task FR_006_a_streak_runs_across_two_versions_in_one_unit()
    {
        var (goal, _) = await ReadingAsync(day => day < Thu12 ? 15 : 20);
        _world.At(Utc("2026-11-11T10:00:00Z"));
        await ChangeAsync(goal, MinutesDaily(20));

        _world.At(Utc("2026-11-18T12:00:00Z"));
        var report = await _world.ReportAsync(goal);

        Assert.Equal("100% (14 of 14)", report.Stats.ConsistencyText);
        Assert.Equal((StreakUnit.Commitments, "14 commitments", "14 commitments"),
            (report.Stats.Unit, report.Stats.CurrentStreakText, report.Stats.LongestStreakText));
    }

    [Theory]
    [InlineData("2026-11-11T23:59:59Z", 12)]
    [InlineData("2026-11-12T00:00:00Z", 13)]
    public async Task SC_002_SC_005_a_change_just_before_or_after_midnight_leaves_every_begun_date_unchanged(string instant, int appliesFromDay)
    {
        var (goal, _) = await ReadingAsync(day => day.Day % 3 == 0 ? 10 : 25);
        var at = Utc(instant);
        _world.At(at);
        var before = await _world.HistoryAsync(goal, Mon2, Sun15);

        var result = await ChangeAsync(goal, DurationDays(30, DurationUnit.Minutes, BuildOrReduce.Reduce, AllDays));

        var appliesFrom = Date(2026, 11, appliesFromDay);
        Assert.Equal((DateOnly?)appliesFrom, result.AppliesFrom);
        var after = await _world.HistoryAsync(goal, Mon2, Sun15);
        Assert.Equal(Begun(before, appliesFrom), Begun(after, appliesFrom));
        Assert.Contains(after.Entries, e => e.FirstDate == appliesFrom.AddDays(-1));
        Assert.All(after.Entries.Where(e => e.FirstDate >= appliesFrom), e => Assert.Equal(BuildOrReduce.Reduce, after.SettingsOf(e).BuildOrReduce));
    }

    [Theory]
    [InlineData("2026-11-22T23:59:59Z", 23)]
    [InlineData("2026-11-23T00:00:00Z", 30)]
    public async Task SC_002_SC_005_a_per_week_change_on_a_Sunday_or_a_Monday_leaves_every_begun_week_unchanged(string instant, int appliesFromDay)
    {
        var goal = await _world.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Mon2, Created);
        for (var day = Mon2; day <= Mon23; day = day.AddDays(2))
        {
            _world.SeedAnswer(goal, day, YesNoAnswer.Yes);
        }

        _world.At(Utc(instant));
        var before = await _world.HistoryAsync(goal, Mon2, Date(2026, 12, 6));

        var result = await ChangeAsync(goal, YesNoTimes(2, SchedulePeriod.Week));

        var appliesFrom = Date(2026, 11, appliesFromDay);
        Assert.Equal((DateOnly?)appliesFrom, result.AppliesFrom);
        Assert.Equal(DayOfWeek.Monday, appliesFrom.DayOfWeek);
        var after = await _world.HistoryAsync(goal, Mon2, Date(2026, 12, 6));
        Assert.Equal(Begun(before, appliesFrom), Begun(after, appliesFrom));
        Assert.All(after.Entries.Where(e => e.FirstDate >= appliesFrom), e => Assert.Equal(2, after.SettingsOf(e).Times));
    }

    [Fact]
    public async Task SC_005_a_change_before_a_waiting_version_takes_effect_leaves_every_begun_date_unchanged()
    {
        var (goal, _) = await ReadingAsync();
        _world.At(Utc("2026-11-11T10:00:00Z"));
        await ChangeAsync(goal, MinutesDaily(20));

        _world.At(Utc("2026-11-11T23:59:59Z"));
        var before = await _world.HistoryAsync(goal, Mon2, Sun15);
        var result = await ChangeAsync(goal, MinutesDaily(10));

        Assert.Equal((DateOnly?)Thu12, result.AppliesFrom);
        var after = await _world.HistoryAsync(goal, Mon2, Sun15);
        Assert.Equal(Begun(before, Thu12), Begun(after, Thu12));
        Assert.All(after.Entries.Where(e => e.FirstDate >= Thu12), e => Assert.Equal(10, after.SettingsOf(e).Amount));
    }
}
