using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.UnitTests.Application.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Slice 8 (T116): the read-path edges the mutation run showed untested, on <see cref="AttemptWorld"/> in UTC: the
/// Due group of the goal order, the Today row of an entry from an older version or an earlier attempt, an ended goal
/// with a stored pause over today, the trend texts after a change on an unscheduled day, an earlier attempt that ended
/// before it began, and the details of an ended attempt with no date but two versions. Every instant is a UTC literal
/// worked out by hand.
/// </summary>
public sealed class AttemptReadEdgeTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Tue3 = Date(2026, 11, 3);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Fri6 = Date(2026, 11, 6);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);

    private readonly AttemptWorld _world = new();

    [Fact]
    public async Task A_goal_with_a_Due_entry_and_none_Pending_comes_before_a_goal_with_nothing_to_record_whatever_the_names()
    {
        await _world.CreateNamedGoalAsync("Alpha", YesNoTimes(1, SchedulePeriod.Day), Mon23, Utc("2026-11-17T08:00:00Z"));
        await _world.CreateNamedGoalAsync("Beta", YesNoTimes(1, SchedulePeriod.Day), Tue17, Utc("2026-11-17T08:00:00Z"));
        _world.At(Utc("2026-11-17T12:00:00Z"));

        var overview = await _world.OverviewAsync();

        Assert.Equal(["Beta", "Alpha"], overview.Goals.Select(g => g.Item.Name));
        Assert.All(overview.Goals, g => Assert.False(g.HasPending));
    }

    [Fact]
    public async Task A_Today_row_of_the_latest_attempt_takes_the_settings_in_effect_also_for_an_entry_of_an_older_version()
    {
        var goal = await _world.CreateGoalAsync(YesNoTimes(1, SchedulePeriod.Day), Mon16, Utc("2026-11-16T08:00:00Z"));
        _world.At(Utc("2026-11-17T10:00:00Z"));
        var changed = YesNoTimes(2, SchedulePeriod.Day, BuildOrReduce.Reduce);
        Assert.Equal(Wed18, await _world.ChangeSettingsAsync(goal, changed));
        var inEffect = GoalText.ScheduleText(Target(changed));
        Assert.NotEqual(GoalText.ScheduleText(PerDay()), inEffect);
        _world.At(Utc("2026-11-18T12:00:00Z"));

        var dashboard = await _world.DashboardAsync();

        var row = dashboard.Today.Single(r => r.Entry.FirstDate == Tue17);
        Assert.Equal(EvaluationState.Pending, row.Entry.State);
        Assert.True(row.Entry.IsLatestAttempt);
        Assert.Equal(BuildOrReduce.Reduce, row.BuildOrReduce);
        Assert.Equal(inEffect, row.ScheduleText);
    }

    [Fact]
    public async Task A_Today_row_of_an_earlier_attempt_takes_that_attempts_schedule_text()
    {
        var twiceDaily = Target(YesNoTimes(2, SchedulePeriod.Day));
        var goal = _world.Seed(
            AttemptFrom(1, Mon2, EndedOn(Mon16), SettingsFrom(PerDay(), Mon2, NamedTimeZone.Utc)),
            AttemptFrom(2, Tue17, null, SettingsFrom(twiceDaily, Tue17, NamedTimeZone.Utc)));
        Assert.NotEqual(GoalText.ScheduleText(PerDay()), GoalText.ScheduleText(twiceDaily));
        _world.At(Utc("2026-11-17T12:00:00Z"));

        var dashboard = await _world.DashboardAsync();

        var row = dashboard.Today.Single(r => r.GoalId == goal && r.Entry.FirstDate == Sun15);
        Assert.Equal(EvaluationState.Pending, row.Entry.State);
        Assert.False(row.Entry.IsLatestAttempt);
        Assert.Equal(GoalText.ScheduleText(PerDay()), row.ScheduleText);
    }

    [Fact]
    public async Task An_ended_goal_is_never_paused_even_with_a_stored_pause_over_today()
    {
        var goal = _world.Seed(AttemptFrom(1, Mon2, EndedOn(Mon16), SettingsFrom(PerDay(), Mon2, NamedTimeZone.Utc)));
        _world.Records.SeedPause(goal, Mon16, Fri20);
        _world.At(Utc("2026-11-17T12:00:00Z"));

        var overview = await _world.OverviewAsync();

        Assert.Null(Assert.Single(overview.Goals).PausedUntil);
    }

    [Fact]
    public async Task The_trend_texts_follow_the_latest_bars_version_when_the_change_applies_from_an_unscheduled_day()
    {
        var goal = await _world.CreateGoalAsync(CountDays(10, "pages", BuildOrReduce.Build, MonWedFri), Mon2, Utc("2026-11-02T08:00:00Z"));
        _world.At(Utc("2026-11-18T10:00:00Z"));
        Assert.Equal(Thu19, await _world.ChangeSettingsAsync(goal, CountDays(2, "chapters", BuildOrReduce.Build, MonWedFri)));
        _world.At(Utc("2026-11-19T12:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal(Wed18, report.Trend.Bars[^1].FirstDate);
        Assert.Equal("pages", report.Trend.CountUnit);
        Assert.StartsWith("Pages", report.Trend.Title, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_earlier_attempt_that_ended_before_it_began_adds_nothing_to_the_reads_of_the_latest_attempt()
    {
        var goal = _world.Seed(
            AttemptFrom(1, Mon23, EndedOn(Tue17), SettingsFrom(PerDay(), Mon23, NamedTimeZone.Utc)),
            AttemptFrom(2, Wed18, null, SettingsFrom(PerDay(), Wed18, NamedTimeZone.Utc)));
        _world.At(Utc("2026-11-19T12:00:00Z"));

        var recordable = await _world.RecordableAsync(goal);
        var overview = await _world.OverviewAsync();
        var dashboard = await _world.DashboardAsync();

        Assert.Equal([Thu19, Wed18], recordable.Entries.Select(e => e.FirstDate));
        Assert.All(recordable.Entries, e => Assert.True(e.IsLatestAttempt));
        Assert.True(Assert.Single(overview.Goals).HasPending);
        Assert.Equal(2, dashboard.Today.Count(r => r.GoalId == goal));
    }

    [Fact]
    public async Task The_details_of_an_ended_attempt_with_no_date_show_no_later_version()
    {
        var goal = _world.Seed(AttemptFrom(1, Tue3, EndedOn(Fri6),
            SettingsFrom(OnDays(DayOfWeek.Monday), Tue3, NamedTimeZone.Utc),
            SettingsFrom(Target(CountDays(2, "pages", BuildOrReduce.Build, DayOfWeek.Monday)), Wed4, NamedTimeZone.Utc)));
        _world.At(Utc("2026-11-20T12:00:00Z"));

        var details = await _world.DetailsAsync(goal);

        Assert.Null(details.Later);
        Assert.Equal(Measurement.YesNo, details.Measurement);
    }
}
