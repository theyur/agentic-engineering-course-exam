using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// Slice 8 FR-008, FR-009, FR-011, SC-003, SC-005 (research R-04): the last date, an ending on, before and after the
/// start date, the versions an ending keeps, and the earliest start of a new attempt. Every goal is in UTC.
/// </summary>
public class GoalTimelineEndingTests
{
    private static readonly DateOnly Nov2 = Date(2026, 11, 2);
    private static readonly DateOnly Nov15 = Date(2026, 11, 15);
    private static readonly DateOnly Nov16 = Date(2026, 11, 16);
    private static readonly DateOnly Nov17 = Date(2026, 11, 17);
    private static readonly DateOnly Nov18 = Date(2026, 11, 18);
    private static readonly DateOnly Nov22 = Date(2026, 11, 22);
    private static readonly DateOnly Nov23 = Date(2026, 11, 23);
    private static readonly NamedTimeZone Utc0 = NamedTimeZone.Utc;

    private static Goal Ended(GoalTarget target, DateOnly start, DateOnly ending) =>
        GoalOf(AttemptFrom(1, start, EndedOn(ending), SettingsFrom(target, start, Utc0)));

    private static Goal Running(params GoalSettingsVersion[] versions) =>
        GoalOf(AttemptFrom(1, versions[0].AppliesFrom, null, versions));

    public static TheoryData<string, DateOnly, DateOnly> LastDates => new()
    {
        { "commitments", Nov16, Nov15 }, // ended on a Monday
        { "commitments", Nov18, Nov17 }, // a Wednesday
        { "commitments", Nov22, Date(2026, 11, 21) }, // a Sunday
        { "days", Nov16, Nov15 },
        { "days", Nov18, Nov17 },
        { "days", Nov22, Date(2026, 11, 21) },
        { "weeks", Nov16, Nov15 }, // Monday 16: the Sunday before its week
        { "weeks", Nov18, Nov15 }, // Wednesday 18: the same week
        { "weeks", Nov22, Nov15 }, // Sunday 22: still the same week
        { "weeks", Nov23, Nov22 }, // Monday 23: the week before is whole
    };

    private static GoalTarget TargetOf(string kind) => kind switch
    {
        "commitments" => OnDays(MonWedFri),
        "days" => PerDay(),
        _ => PerWeek(),
    };

    [Theory]
    [MemberData(nameof(LastDates))]
    public void The_last_date_is_the_day_before_the_ending_date_or_for_per_week_the_Sunday_before_its_week(string kind, DateOnly ending, DateOnly last)
    {
        Assert.Equal(last, GoalTimeline.LastDate(Ended(TargetOf(kind), Nov2, ending), 1));
    }

    [Fact]
    public void A_goal_that_is_not_ended_has_no_last_date()
    {
        Assert.Null(GoalTimeline.LastDate(Running(SettingsFrom(PerDay(), Nov2, Utc0)), 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => GoalTimeline.LastDate(Running(SettingsFrom(PerDay(), Nov2, Utc0)), 2));
    }

    [Fact]
    public void A_per_week_attempt_ended_on_a_Monday_keeps_its_last_whole_week()
    {
        var goal = Ended(PerWeek(), Nov2, Nov23);

        Assert.True(GoalTimeline.TryEntries(goal, Nov2, Date(2026, 11, 30), out var entries));
        Assert.Equal([Nov2, Date(2026, 11, 9), Nov16], entries.Select(entry => entry.FirstDate));
        Assert.Equal(Nov22, entries[^1].LastDate);
    }

    [Fact]
    public void An_ending_on_the_start_date_leaves_no_date()
    {
        var goal = Ended(PerDay(), Nov18, Nov18);

        Assert.Equal(Nov17, GoalTimeline.LastDate(goal, 1));
        Assert.Empty(GoalTimeline.EntriesThrough(goal, Nov23));
        Assert.Null(GoalTimeline.AttemptOf(goal, Nov18));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-18T10:00:00Z")));
    }

    [Fact]
    public void An_ending_before_the_start_date_leaves_no_date()
    {
        var goal = Ended(OnDays(MonWedFri), Nov18, Nov17);

        Assert.Equal(Nov16, GoalTimeline.LastDate(goal, 1));
        Assert.Empty(GoalTimeline.EntriesThrough(goal, Nov23));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-20T10:00:00Z")));
    }

    [Fact]
    public void An_ending_the_day_after_the_start_date_keeps_the_start_date()
    {
        var goal = Ended(PerDay(), Nov18, Date(2026, 11, 19));

        Assert.Equal(Nov18, GoalTimeline.LastDate(goal, 1));
        Assert.Equal([Nov18], GoalTimeline.EntriesThrough(goal, Nov23).Select(entry => entry.FirstDate));
        Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-19T10:00:00Z")));
    }

    [Fact]
    public void A_commitment_based_attempt_ended_before_its_first_scheduled_day_has_no_date()
    {
        // Mon/Wed/Fri from Tuesday 17 November, ended on Wednesday 18: the last date 17 November has no commitment.
        var goal = Ended(OnDays(MonWedFri), Nov17, Nov18);

        Assert.Equal(Nov17, GoalTimeline.LastDate(goal, 1));
        Assert.Empty(GoalTimeline.EntriesThrough(goal, Nov23));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-18T10:00:00Z")));
    }

    [Fact]
    public void A_per_week_attempt_from_Wednesday_18_ended_Tuesday_24_has_no_date()
    {
        // Plan "Spec discrepancies" 3: its last date Sunday 22 November is before its first week, 23 to 29 November.
        var goal = Ended(PerWeek(), Nov18, Date(2026, 11, 24));

        Assert.Equal(Nov22, GoalTimeline.LastDate(goal, 1));
        Assert.True(GoalTimeline.TryEntries(goal, Nov16, Date(2026, 11, 30), out var entries));
        Assert.Empty(entries);
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-24T10:00:00Z")));
        Assert.Equal(goal.Versions[0], GoalTimeline.VersionInEffect(goal, Utc("2026-11-24T10:00:00Z")));
    }

    [Fact]
    public void EndingOf_removes_a_waiting_later_version()
    {
        var first = SettingsFrom(PerDay(), Nov2, Utc0);
        var goal = Running(first, SettingsFrom(Target(YesNoTimes(2, SchedulePeriod.Day)), Nov17, Utc0));

        var effect = GoalTimeline.EndingOf(goal, Nov16);

        Assert.Equal(Nov15, effect.LastDate);
        Assert.Equal([first], effect.KeptVersions);
    }

    [Fact]
    public void EndingOf_removes_a_version_that_took_effect_on_the_ending_date()
    {
        var first = SettingsFrom(PerDay(), Nov2, Utc0);
        var goal = Running(first, SettingsFrom(PerDay(), Nov16, NewYork));

        Assert.Equal([first], GoalTimeline.EndingOf(goal, Nov16).KeptVersions);
    }

    [Fact]
    public void EndingOf_keeps_a_version_that_applies_on_the_last_date()
    {
        var first = SettingsFrom(PerDay(), Nov2, Utc0);
        var onLast = SettingsFrom(PerDay(), Nov15, NewYork);

        Assert.Equal([first, onLast], GoalTimeline.EndingOf(Running(first, onLast), Nov16).KeptVersions);
    }

    [Fact]
    public void EndingOf_a_per_week_goal_removes_the_version_of_the_week_in_progress()
    {
        var first = SettingsFrom(PerWeek(), Nov2, Utc0);
        var goal = Running(first, SettingsFrom(PerWeek(), Nov16, NewYork));

        var effect = GoalTimeline.EndingOf(goal, Nov18);

        Assert.Equal(Nov15, effect.LastDate);
        Assert.Equal([first], effect.KeptVersions);
    }

    [Fact]
    public void EndingOf_always_keeps_the_first_version()
    {
        var first = SettingsFrom(PerDay(), Nov23, Utc0);

        var effect = GoalTimeline.EndingOf(Running(first), Nov17);

        Assert.Equal(Nov16, effect.LastDate);
        Assert.Equal([first], effect.KeptVersions);
    }

    [Fact]
    public void The_goal_after_EndingOf_is_valid()
    {
        var first = SettingsFrom(PerDay(), Nov2, Utc0);
        var goal = Running(first, SettingsFrom(PerDay(), Nov17, NewYork));
        var effect = GoalTimeline.EndingOf(goal, Nov16);

        var ended = GoalOf(AttemptFrom(1, Nov2, EndedOn(Nov16), [.. effect.KeptVersions]));

        Assert.Equal(effect.LastDate, GoalTimeline.LastDate(ended, 1));
    }

    [Fact]
    public void EndingOf_an_ended_goal_throws()
    {
        Assert.Throws<InvalidOperationException>(() => GoalTimeline.EndingOf(Ended(PerDay(), Nov2, Nov16), Nov18));
    }

    [Fact]
    public void A_new_attempt_starts_at_the_earliest_the_day_after_the_ending_date()
    {
        var goal = Ended(PerDay(), Nov2, Nov16);

        Assert.Equal(Nov17, GoalTimeline.NewAttemptEarliestStart(goal, Nov16));
        Assert.Equal(Nov17, GoalTimeline.NewAttemptEarliestStart(goal, Nov17));
        Assert.Equal(Nov22, GoalTimeline.NewAttemptEarliestStart(goal, Nov22));
    }

    [Fact]
    public void US3_AS13_the_accounts_today_a_day_behind_the_ending_date_gives_the_day_after_the_ending_date()
    {
        // Ended on Tuesday 17 November on the goal's Kyiv timeline while the account's today in UTC is Monday 16 November.
        var goal = GoalOf(AttemptFrom(1, Nov2, EndedOn(Nov17), SettingsFrom(PerDay(), Nov2, Kyiv)));

        Assert.Equal(Nov18, GoalTimeline.NewAttemptEarliestStart(goal, Nov16));
    }

    [Fact]
    public void The_earliest_start_of_a_goal_that_is_not_ended_throws()
    {
        Assert.Throws<InvalidOperationException>(() => GoalTimeline.NewAttemptEarliestStart(Running(SettingsFrom(PerDay(), Nov2, Utc0)), Nov16));
    }
}
