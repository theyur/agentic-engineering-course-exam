using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// Slice 8 (T116): the edges of the attempt timeline that the mutation run showed untested: the calendar's kind when a
/// later attempt has no entry in the range, a date in no attempt that is the goal's current date, the next commitment
/// later today, a waiting attempt with versions in two zones, an ended attempt with no date but two versions, the
/// entries of an earlier attempt read only partly, and an ended attempt whose only entry ends on its last date. Every
/// goal is in UTC unless said otherwise; in November 2026 Kyiv is UTC+2 and New York UTC−5.
/// </summary>
public class GoalTimelineEdgeTests
{
    private static readonly DateOnly Nov2 = Date(2026, 11, 2);
    private static readonly DateOnly Nov3 = Date(2026, 11, 3);
    private static readonly DateOnly Nov4 = Date(2026, 11, 4);
    private static readonly DateOnly Nov6 = Date(2026, 11, 6);
    private static readonly DateOnly Nov9 = Date(2026, 11, 9);
    private static readonly DateOnly Nov10 = Date(2026, 11, 10);
    private static readonly DateOnly Nov11 = Date(2026, 11, 11);
    private static readonly DateOnly Nov12 = Date(2026, 11, 12);
    private static readonly DateOnly Nov13 = Date(2026, 11, 13);
    private static readonly DateOnly Nov14 = Date(2026, 11, 14);
    private static readonly DateOnly Nov16 = Date(2026, 11, 16);
    private static readonly DateOnly Nov17 = Date(2026, 11, 17);
    private static readonly DateOnly Nov18 = Date(2026, 11, 18);
    private static readonly DateOnly Nov22 = Date(2026, 11, 22);
    private static readonly DateOnly Nov23 = Date(2026, 11, 23);
    private static readonly DateOnly Nov30 = Date(2026, 11, 30);
    private static readonly DateOnly Dec1 = Date(2026, 12, 1);

    /// <summary>
    /// Attempt 1 on Mon/Wed/Fri from Monday 2 November, ended on Thursday 12 November (last date Wednesday 11 November);
    /// attempt 2 three times a week from Friday 13 November, whose first week starts on Monday 16 November.
    /// </summary>
    private static Goal FixedDaysThenPerWeek() =>
        GoalOf(
            AttemptFrom(1, Nov2, EndedOn(Nov12), SettingsFrom(OnDays(MonWedFri), Nov2, NamedTimeZone.Utc)),
            AttemptFrom(2, Nov13, null, SettingsFrom(PerWeek(), Nov13, NamedTimeZone.Utc)));

    /// <summary>Daily from Monday 2 November, ended on Monday 16 November (last date 15 November); attempt 2 daily from 23 November.</summary>
    private static Goal DailyRestarted() =>
        GoalOf(
            AttemptFrom(1, Nov2, EndedOn(Nov16), SettingsFrom(PerDay(), Nov2, NamedTimeZone.Utc)),
            AttemptFrom(2, Nov23, null, SettingsFrom(PerDay(), Nov23, NamedTimeZone.Utc)));

    /// <summary>
    /// Mondays only, from Tuesday 3 November, changed on Wednesday 4 November to two pages on Mondays, ended on Friday
    /// 6 November (last date Thursday 5 November): no Monday up to the last date, so the attempt has no date, yet it
    /// keeps both versions.
    /// </summary>
    private static readonly GoalTarget TwoPagesOnMondays = Target(CountDays(2, "pages", BuildOrReduce.Build, DayOfWeek.Monday));

    private static Goal EndedWithNoDateAndTwoVersions() =>
        GoalOf(AttemptFrom(1, Nov3, EndedOn(Nov6),
            SettingsFrom(OnDays(DayOfWeek.Monday), Nov3, NamedTimeZone.Utc),
            SettingsFrom(TwoPagesOnMondays, Nov4, NamedTimeZone.Utc)));

    private static EvaluationCalendar Calendar(Goal goal, DateOnly first, DateOnly last)
    {
        Assert.True(GoalTimeline.TryCalendar(goal, first, last, out var calendar));
        return calendar;
    }

    [Fact]
    public void The_calendar_kind_is_the_attempt_with_an_entry_in_the_range_even_when_a_later_attempt_has_started()
    {
        var goal = FixedDaysThenPerWeek();

        var calendar = Calendar(goal, Nov9, Nov14);

        Assert.Equal([Nov9, Nov11], calendar.Commitments.Select(c => c.Date));
        Assert.Empty(calendar.Periods);
        Assert.Equal(GoalScheduleKind.CommitmentBased, calendar.Kind);
    }

    [Fact]
    public void A_range_with_no_entry_takes_the_kind_of_the_attempt_timed_from_its_last_day()
    {
        var goal = FixedDaysThenPerWeek();

        var onTheTimingDate = Calendar(goal, Nov13, Nov13);
        var theDayBefore = Calendar(goal, Nov12, Nov12);

        Assert.Empty(onTheTimingDate.Commitments);
        Assert.Empty(onTheTimingDate.Periods);
        Assert.Equal(GoalScheduleKind.PeriodBased, onTheTimingDate.Kind);
        Assert.Equal(GoalScheduleKind.CommitmentBased, theDayBefore.Kind);
    }

    [Theory]
    [InlineData("2026-11-15T12:00:00Z", false)]
    [InlineData("2026-11-16T00:00:00Z", true)]
    [InlineData("2026-11-16T12:00:00Z", true)]
    [InlineData("2026-11-17T12:00:00Z", true)]
    public void A_date_in_no_attempt_has_begun_once_it_is_the_goals_current_date_or_earlier(string instant, bool expected)
    {
        var goal = GoalOf(AttemptFrom(1, Nov2, EndedOn(Nov16), SettingsFrom(PerDay(), Nov2, NamedTimeZone.Utc)));

        Assert.Null(GoalTimeline.AttemptOf(goal, Nov16));
        Assert.Equal(expected, GoalTimeline.HasBegun(goal, Nov16, Utc(instant)));
    }

    [Fact]
    public void The_next_commitment_of_a_daily_goal_begun_weeks_ago_is_today_while_today_lasts()
    {
        var goal = Goal(OnDays(AllDays), Nov2, NamedTimeZone.Utc);

        Assert.Equal(Nov18, GoalTimeline.NextCommitment(goal, Utc("2026-11-18T12:00:00Z")));
        Assert.Equal(Date(2026, 11, 19), GoalTimeline.NextCommitment(goal, Utc("2026-11-19T00:00:00Z")));
    }

    /// <summary>
    /// Pago Pago is UTC−11 and Kiritimati UTC+14. From Monday 16 November every day in Pago Pago, from Tuesday 17
    /// November in Kiritimati: at 10:30Z on 16 November, 17 November has begun in Kiritimati (at 10:00Z) but 16 November
    /// not yet in Pago Pago (at 11:00Z), so the current date is 17 November, and 15 November, the day before the attempt,
    /// has not ended yet in Pago Pago.
    /// </summary>
    [Fact]
    public void The_next_commitment_never_lies_before_the_timing_date_even_when_the_day_before_it_has_not_ended()
    {
        var goal = GoalOf(AttemptFrom(1, Nov16, null,
            SettingsFrom(OnDays(AllDays), Nov16, Zone("Pacific/Pago_Pago")),
            SettingsFrom(OnDays(AllDays), Nov17, Zone("Pacific/Kiritimati"))));
        var instant = Utc("2026-11-16T10:30:00Z");

        Assert.Equal(Nov17, GoalTimeline.CurrentDate(goal, instant));
        Assert.Equal(Nov16, GoalTimeline.NextCommitment(goal, instant));
    }

    [Fact]
    public void A_waiting_attempt_with_a_version_in_another_zone_gets_its_first_version_replaced_by_that_zone()
    {
        var kyiv = SettingsFrom(OnDays(AllDays), Nov30, Kyiv);
        var newYork = SettingsFrom(OnDays(AllDays), Dec1, NewYork);
        var goal = GoalOf(AttemptFrom(1, Nov30, null, kyiv, newYork));
        var instant = Utc("2026-11-20T12:00:00Z");

        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, instant));
        Assert.Equal(GoalVersionChange.ReplaceFirst(Kyiv), GoalTimeline.ChangeOf(goal, instant, Kyiv));
    }

    [Fact]
    public void An_ended_attempt_with_no_date_is_named_and_in_effect_by_its_first_version_not_its_latest()
    {
        var goal = EndedWithNoDateAndTwoVersions();
        var first = TargetText(OnDays(DayOfWeek.Monday));
        Assert.NotEqual(first, TargetText(TwoPagesOnMondays));

        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-20T12:00:00Z")));
        Assert.Equal("Reading, " + first, GoalText.AttemptName(goal, 1, "Reading"));
        Assert.Equal(Nov3, GoalTimeline.VersionInEffect(goal, 1, Utc("2026-11-20T12:00:00Z")).AppliesFrom);
        Assert.Equal(Nov3, GoalTimeline.VersionInEffect(goal, Utc("2026-11-20T12:00:00Z")).AppliesFrom);
    }

    [Fact]
    public void An_attempt_that_is_not_ended_is_named_by_its_latest_version()
    {
        var goal = GoalOf(AttemptFrom(1, Nov3, null,
            SettingsFrom(OnDays(DayOfWeek.Monday), Nov3, NamedTimeZone.Utc),
            SettingsFrom(TwoPagesOnMondays, Nov4, NamedTimeZone.Utc)));

        Assert.Equal("Reading, " + TargetText(TwoPagesOnMondays), GoalText.AttemptName(goal, 1, "Reading"));
    }

    [Fact]
    public void An_earlier_attempts_entries_through_a_date_before_its_last_date_stop_at_that_date()
    {
        var goal = DailyRestarted();

        var entries = GoalTimeline.EntriesThrough(goal, 1, Nov10);

        Assert.Equal(9, entries.Count);
        Assert.Equal(Nov2, entries[0].FirstDate);
        Assert.Equal(Nov10, entries[^1].LastDate);
    }

    [Fact]
    public void A_per_week_attempt_ended_the_Monday_after_its_first_week_has_that_week_as_its_date()
    {
        var goal = GoalOf(AttemptFrom(1, Nov16, EndedOn(Nov23), SettingsFrom(PerWeek(), Nov16, NamedTimeZone.Utc)));

        Assert.Equal(Nov22, GoalTimeline.LastDate(goal, 1));
        Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-25T12:00:00Z")));
        Assert.Equal(Nov16, Assert.Single(GoalTimeline.EntriesThrough(goal, 1, Nov30)).FirstDate);
    }

    [Fact]
    public void A_fixed_days_attempt_whose_only_commitment_is_its_last_date_has_a_date()
    {
        var goal = GoalOf(AttemptFrom(1, Nov16, EndedOn(Nov17), SettingsFrom(OnDays(MonWedFri), Nov16, NamedTimeZone.Utc)));

        Assert.Equal(Nov16, GoalTimeline.LastDate(goal, 1));
        Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-20T12:00:00Z")));
        Assert.Equal(Nov16, GoalTimeline.CurrentDate(goal, Utc("2026-11-16T12:00:00Z")));
    }

    private static string TargetText(GoalTarget target) => GoalText.TargetText(target);
}
