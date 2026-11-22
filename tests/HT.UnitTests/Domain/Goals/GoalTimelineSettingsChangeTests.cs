using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// Slice 8 FR-002, SC-002, SC-005 (research R-03): <see cref="GoalTimeline.SettingsChangeOf"/> on both sides of the
/// day and week boundaries, with a waiting version, on a waiting attempt, and never changing a begun date's version.
/// US1's goal is "Reading, 15 min daily" from Monday 2 November 2026; Kyiv is UTC+2 and New York UTC−5.
/// </summary>
public class GoalTimelineSettingsChangeTests
{
    private static readonly DateOnly Nov2 = Date(2026, 11, 2);
    private static readonly DateOnly Nov12 = Date(2026, 11, 12);
    private static readonly DateOnly Nov13 = Date(2026, 11, 13);
    private static readonly DateOnly Nov23 = Date(2026, 11, 23);
    private static readonly DateOnly Nov30 = Date(2026, 11, 30);

    private static GoalTarget Minutes(int amount) => Target(DurationTimes(amount, DurationUnit.Minutes, 1, SchedulePeriod.Day));

    private static GoalTarget TimesAWeek(int times) => Target(YesNoTimes(times, SchedulePeriod.Week));

    private static Goal Reading(NamedTimeZone zone, DateOnly? start = null) =>
        GoalOf(AttemptFrom(1, start ?? Nov2, null, SettingsFrom(Minutes(15), start ?? Nov2, zone)));

    /// <summary>The goal with the change written as the store writes it.</summary>
    private static Goal Written(Goal goal, GoalSettingsChange change) =>
        GoalOf(goal.OwnName, [.. goal.Attempts.SkipLast(1), new GoalAttemptVersions(goal.Attempt, change.Versions)]);

    public static TheoryData<string, string, DateOnly> AroundMidnight => new()
    {
        { "UTC", "2026-11-11T10:00:00Z", Nov12 },
        { "UTC", "2026-11-11T23:59:59Z", Nov12 },
        { "UTC", "2026-11-12T00:00:00Z", Nov13 },
        { "Europe/Kyiv", "2026-11-11T21:59:59Z", Nov12 }, // 23:59:59 on 11 November in Kyiv
        { "Europe/Kyiv", "2026-11-11T22:00:00Z", Nov13 }, // 00:00 on 12 November in Kyiv
    };

    [Theory]
    [MemberData(nameof(AroundMidnight))]
    public void A_change_applies_from_the_day_after_the_current_date(string zoneId, string at, DateOnly appliesFrom)
    {
        var zone = Zone(zoneId);
        var goal = Reading(zone);

        var change = GoalTimeline.SettingsChangeOf(goal, Utc(at), Minutes(20), zone);

        Assert.Equal(appliesFrom, change.AppliesFrom);
        Assert.Equal([goal.Versions[0], SettingsFrom(Minutes(20), appliesFrom, zone)], change.Versions);
    }

    public static TheoryData<string, DateOnly> PerWeekBoundaries => new()
    {
        { "2026-11-22T23:59:59Z", Nov23 }, // Sunday 22 November
        { "2026-11-23T00:00:00Z", Nov30 }, // Monday 23 November
    };

    [Theory]
    [MemberData(nameof(PerWeekBoundaries))]
    public void A_per_week_change_applies_from_the_Monday_after_the_current_dates_week(string at, DateOnly appliesFrom)
    {
        var goal = GoalOf(AttemptFrom(1, Nov2, null, SettingsFrom(TimesAWeek(3), Nov2, NamedTimeZone.Utc)));

        var change = GoalTimeline.SettingsChangeOf(goal, Utc(at), TimesAWeek(4), NamedTimeZone.Utc);

        Assert.Equal(appliesFrom, change.AppliesFrom);
        Assert.Equal(appliesFrom, change.Versions[^1].AppliesFrom);
        Assert.Equal(DayOfWeek.Monday, change.AppliesFrom.DayOfWeek);
    }

    [Fact]
    public void A_second_change_replaces_the_waiting_version()
    {
        var goal = Reading(NamedTimeZone.Utc);
        var once = Written(goal, GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-11T10:00:00Z"), Minutes(20), NamedTimeZone.Utc));

        var twice = GoalTimeline.SettingsChangeOf(once, Utc("2026-11-11T12:00:00Z"), Minutes(25), NamedTimeZone.Utc);

        Assert.Equal(Nov12, twice.AppliesFrom);
        Assert.Equal([goal.Versions[0], SettingsFrom(Minutes(25), Nov12, NamedTimeZone.Utc)], twice.Versions);
    }

    [Fact]
    public void A_change_after_the_waiting_version_took_effect_adds_one()
    {
        var goal = Reading(NamedTimeZone.Utc);
        var once = Written(goal, GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-11T10:00:00Z"), Minutes(20), NamedTimeZone.Utc));

        var twice = GoalTimeline.SettingsChangeOf(once, Utc("2026-11-12T10:00:00Z"), Minutes(25), NamedTimeZone.Utc);

        Assert.Equal(Nov13, twice.AppliesFrom);
        Assert.Equal(3, twice.Versions.Count);
    }

    [Fact]
    public void US1_AS12_a_change_on_a_waiting_attempt_replaces_its_first_version_and_keeps_its_date()
    {
        var goal = Reading(NamedTimeZone.Utc, Nov23);

        var change = GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-20T10:00:00Z"), Minutes(20), NamedTimeZone.Utc);

        Assert.Equal(Nov23, change.AppliesFrom);
        Assert.Equal([SettingsFrom(Minutes(20), Nov23, NamedTimeZone.Utc)], change.Versions);
    }

    [Fact]
    public void A_change_on_a_waiting_attempt_2_after_a_begun_attempt_1_replaces_attempt_2s_first_version()
    {
        var goal = GoalOf(
            AttemptFrom(1, Nov2, EndedOn(Date(2026, 11, 16)), SettingsFrom(Minutes(10), Nov2, NamedTimeZone.Utc)),
            AttemptFrom(2, Nov23, null, SettingsFrom(Minutes(5), Nov23, NamedTimeZone.Utc)));

        var change = GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-20T10:00:00Z"), Minutes(8), Kyiv);

        Assert.Equal(Nov23, change.AppliesFrom);
        Assert.Equal([SettingsFrom(Minutes(8), Nov23, Kyiv)], change.Versions);
        Assert.Equal(goal.VersionsOf(1), Written(goal, change).VersionsOf(1));
    }

    [Fact]
    public void The_written_version_takes_the_given_zone()
    {
        var goal = Reading(Kyiv);

        var change = GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-11T10:00:00Z"), Minutes(20), NewYork);

        Assert.Equal(NewYork, change.Versions[^1].Settings.TimeZone);
        Assert.Equal(Kyiv, change.Versions[0].Settings.TimeZone);
    }

    [Fact]
    public void No_begun_date_changes_its_version()
    {
        var goal = Reading(Kyiv);
        var at = Utc("2026-11-11T21:59:59Z");
        var current = GoalTimeline.CurrentDate(goal, at)!.Value;

        var changed = Written(goal, GoalTimeline.SettingsChangeOf(goal, at, Minutes(20), NewYork));

        for (var date = Nov2; date <= current; date = date.AddDays(1))
        {
            Assert.Equal(GoalTimeline.VersionOf(goal, date), GoalTimeline.VersionOf(changed, date));
        }

        Assert.Equal(Minutes(20), GoalTimeline.VersionOf(changed, current.AddDays(1)).Settings.Target);
        Assert.Equal(current, GoalTimeline.CurrentDate(changed, at));
    }

    [Fact]
    public void An_ended_goal_throws()
    {
        var goal = GoalOf(AttemptFrom(1, Nov2, EndedOn(Date(2026, 11, 16)), SettingsFrom(Minutes(15), Nov2, NamedTimeZone.Utc)));

        Assert.Throws<InvalidOperationException>(() => GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-17T10:00:00Z"), Minutes(20), NamedTimeZone.Utc));
    }

    [Fact]
    public void Another_schedule_kind_throws()
    {
        var goal = Reading(NamedTimeZone.Utc);

        Assert.Throws<ArgumentException>(() => GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-11T10:00:00Z"), TimesAWeek(3), NamedTimeZone.Utc));
        Assert.Throws<ArgumentException>(() => GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-11T10:00:00Z"), OnDays(AllDays), NamedTimeZone.Utc));
        Assert.Throws<ArgumentException>(() => GoalTimeline.SettingsChangeOf(goal, Utc("2026-11-11T10:00:00Z"), Minutes(20), default));
    }
}
