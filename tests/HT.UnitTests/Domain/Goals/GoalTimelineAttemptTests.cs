using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// Slice 8 research R-01, R-02 (FR-010, FR-013, FR-016, AC-035): the attempt cuts, <see cref="GoalTimeline.AttemptOf"/>,
/// the extended current date, <see cref="GoalTimeline.HasBegun"/> for a waiting attempt, the settings in effect and when
/// the latest attempt has begun. In November 2026 Kyiv is UTC+2 and New York UTC−5.
/// </summary>
public class GoalTimelineAttemptTests
{
    private static readonly DateOnly Nov2 = Date(2026, 11, 2);
    private static readonly DateOnly Nov15 = Date(2026, 11, 15);
    private static readonly DateOnly Nov16 = Date(2026, 11, 16);
    private static readonly DateOnly Nov17 = Date(2026, 11, 17);
    private static readonly DateOnly Nov18 = Date(2026, 11, 18);
    private static readonly DateOnly Nov22 = Date(2026, 11, 22);
    private static readonly DateOnly Nov23 = Date(2026, 11, 23);

    private static GoalTarget Minutes(int amount) => Target(DurationDays(amount, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));

    /// <summary>US2: "Meditation, 10 min daily" from Monday 2 November, ended on Monday 16 November (last date 15 November).</summary>
    private static GoalAttemptVersions Meditation(NamedTimeZone zone) =>
        AttemptFrom(1, Nov2, EndedOn(Nov16), SettingsFrom(Minutes(10), Nov2, zone));

    /// <summary>US3: the same goal restarted as "Meditation, 5 min daily" from Monday 23 November.</summary>
    private static Goal Restarted(NamedTimeZone first, NamedTimeZone second) =>
        GoalOf(Meditation(first), AttemptFrom(2, Nov23, null, SettingsFrom(Minutes(5), Nov23, second)));

    /// <summary>AC-035: attempt 1 from 23 November ended on 17 November before it began; attempt 2 from 18 November.</summary>
    private static Goal EndedBeforeItBegan() =>
        GoalOf(
            AttemptFrom(1, Nov23, EndedOn(Nov17), SettingsFrom(Minutes(10), Nov23, NamedTimeZone.Utc)),
            AttemptFrom(2, Nov18, null, SettingsFrom(Minutes(5), Nov18, NamedTimeZone.Utc)));

    private static IReadOnlyList<CalendarEntry> Entries(Goal goal, DateOnly first, DateOnly last)
    {
        Assert.True(GoalTimeline.TryEntries(goal, first, last, out var entries));
        return entries;
    }

    [Fact]
    public void US2_a_goal_ended_on_16_November_has_no_entry_from_16_November()
    {
        var goal = GoalOf(Meditation(NamedTimeZone.Utc));

        var entries = Entries(goal, Nov2, Date(2026, 11, 30));

        Assert.Equal(14, entries.Count);
        Assert.Equal(Nov2, entries[0].FirstDate);
        Assert.Equal(Nov15, entries[^1].LastDate);
        Assert.NotNull(GoalTimeline.EntryOf(goal, Nov15));
        Assert.Null(GoalTimeline.EntryOf(goal, Nov16));
        Assert.DoesNotContain(GoalTimeline.EntriesThrough(goal, Date(2026, 12, 31)), entry => entry.FirstDate >= Nov16);
    }

    [Fact]
    public void US3_attempt_2_from_23_November_leaves_16_to_22_November_in_no_attempt()
    {
        var goal = Restarted(NamedTimeZone.Utc, NamedTimeZone.Utc);

        var dates = Entries(goal, Nov2, Date(2026, 11, 29)).Select(entry => entry.FirstDate).ToArray();

        Assert.Equal(14 + 7, dates.Length);
        Assert.DoesNotContain(dates, date => date >= Nov16 && date <= Nov22);
        Assert.Empty(Entries(goal, Nov16, Nov22));
        Assert.Equal(Nov23, GoalTimeline.EntriesThrough(goal, Date(2026, 11, 29))[0].FirstDate);
    }

    public static TheoryData<DateOnly, int?> AttemptOfDates => new()
    {
        { Date(2026, 11, 1), null },
        { Nov2, 1 },
        { Nov15, 1 },
        { Nov16, null },
        { Nov22, null },
        { Nov23, 2 },
        { Date(2027, 1, 4), 2 },
    };

    [Theory]
    [MemberData(nameof(AttemptOfDates))]
    public void AttemptOf_on_both_sides_of_each_cut(DateOnly date, int? number)
    {
        Assert.Equal(number, GoalTimeline.AttemptOf(Restarted(NamedTimeZone.Utc, NamedTimeZone.Utc), date));
    }

    [Fact]
    public void AttemptOf_a_date_inside_an_attempt_without_an_entry_is_null()
    {
        var weekly = GoalOf(AttemptFrom(1, Nov18, null, SettingsFrom(PerWeek(), Nov18, NamedTimeZone.Utc)));
        var monWedFri = GoalOf(AttemptFrom(1, Nov16, null, SettingsFrom(OnDays(MonWedFri), Nov16, NamedTimeZone.Utc)));

        Assert.Null(GoalTimeline.AttemptOf(weekly, Nov22));
        Assert.Equal(1, GoalTimeline.AttemptOf(weekly, Nov23));
        Assert.Null(GoalTimeline.AttemptOf(monWedFri, Nov17));
        Assert.Equal(1, GoalTimeline.AttemptOf(monWedFri, Nov18));
    }

    [Fact]
    public void The_current_date_of_a_running_goal_is_its_attempts()
    {
        var goal = GoalOf(AttemptFrom(1, Nov2, null, SettingsFrom(Minutes(10), Nov2, Kyiv)));

        Assert.Equal(Date(2026, 11, 11), GoalTimeline.CurrentDate(goal, Utc("2026-11-11T21:59:59Z")));
        Assert.Equal(Date(2026, 11, 12), GoalTimeline.CurrentDate(goal, Utc("2026-11-11T22:00:00Z")));
    }

    [Fact]
    public void The_current_date_of_an_ended_goal_runs_on_in_its_last_versions_zone()
    {
        var goal = GoalOf(Meditation(Kyiv));

        Assert.Equal(Nov16, GoalTimeline.CurrentDate(goal, Utc("2026-11-16T10:00:00Z")));
        Assert.Equal(Date(2026, 11, 19), GoalTimeline.CurrentDate(goal, Utc("2026-11-18T22:00:00Z")));
        Assert.Equal(Nov18, GoalTimeline.CurrentDate(goal, Utc("2026-11-18T21:59:59Z")));
    }

    [Fact]
    public void A_waiting_attempt_after_a_begun_attempt_1_takes_attempt_1s_zone_for_the_current_date()
    {
        var goal = Restarted(Kyiv, NewYork);

        // 23:00 UTC on Friday 20 November is 01:00 Saturday in Kyiv and 18:00 Friday in New York.
        Assert.Equal(Date(2026, 11, 21), GoalTimeline.CurrentDate(goal, Utc("2026-11-20T23:00:00Z")));

        // Monday 23 November begins in New York at 05:00 UTC; from then on attempt 2's own date counts.
        Assert.Equal(Nov23, GoalTimeline.CurrentDate(goal, Utc("2026-11-23T04:59:59Z")));
        Assert.Equal(Nov23, GoalTimeline.CurrentDate(goal, Utc("2026-11-23T05:00:00Z")));
        Assert.Equal(Nov23, GoalTimeline.CurrentDate(goal, Utc("2026-11-24T04:59:59Z")));
        Assert.Equal(Date(2026, 11, 24), GoalTimeline.CurrentDate(goal, Utc("2026-11-24T05:00:00Z")));
    }

    [Fact]
    public void A_goal_whose_attempts_have_never_begun_has_no_current_date()
    {
        var waiting = GoalOf(AttemptFrom(1, Nov23, null, SettingsFrom(Minutes(10), Nov23, NamedTimeZone.Utc)));
        var acc035 = EndedBeforeItBegan();

        Assert.Null(GoalTimeline.CurrentDate(waiting, Utc("2026-11-22T23:59:59Z")));
        Assert.Null(GoalTimeline.CurrentDate(acc035, Utc("2026-11-17T23:59:59Z")));
        Assert.Equal(Nov18, GoalTimeline.CurrentDate(acc035, Utc("2026-11-18T00:00:00Z")));
    }

    [Fact]
    public void A_date_of_a_waiting_attempt_in_New_York_has_not_begun_until_its_day_begins_there()
    {
        // Attempt 1 in Kyiv ended on 16 November; attempt 2 in New York from Wednesday 18 November, which begins there
        // at 05:00 UTC, while the goal's current date (attempt 1's Kyiv date) is already 18 November.
        var goal = GoalOf(Meditation(Kyiv), AttemptFrom(2, Nov18, null, SettingsFrom(Minutes(5), Nov18, NewYork)));
        var before = Utc("2026-11-18T04:59:59Z");
        var at = Utc("2026-11-18T05:00:00Z");

        Assert.Equal(Nov18, GoalTimeline.CurrentDate(goal, before));
        Assert.False(GoalTimeline.HasBegun(goal, Nov18, before));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, before));
        Assert.True(GoalTimeline.HasBegun(goal, Nov18, at));
        Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, at));
        Assert.True(GoalTimeline.HasBegun(goal, Nov15, before));
        Assert.False(GoalTimeline.HasBegun(goal, Date(2026, 11, 19), at));
    }

    [Fact]
    public void A_waiting_attempt_in_New_York_ended_on_its_start_date_never_begins_and_Kyiv_keeps_the_current_date()
    {
        // Attempt 2 starts on 17 November. At 23:00 UTC on 16 November it is 17 November 01:00 in Kyiv (attempt 1's
        // current date gives the ending date 17 November) but 16 November 18:00 in New York, so its last date
        // 16 November is before its start date: it has no date.
        var goal = GoalOf(Meditation(Kyiv), AttemptFrom(2, Nov17, EndedOn(Nov17), SettingsFrom(Minutes(5), Nov17, NewYork)));

        Assert.Equal(Nov16, GoalTimeline.LastDate(goal, 2));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-17T12:00:00Z")));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-12-01T12:00:00Z")));

        // 23:30 UTC on 17 November is 18 November in Kyiv and still 17 November in New York.
        Assert.Equal(Nov18, GoalTimeline.CurrentDate(goal, Utc("2026-11-17T23:30:00Z")));
        Assert.Equal(Nov17, GoalTimeline.CurrentDate(goal, Utc("2026-11-17T21:59:59Z")));
        Assert.Empty(Entries(goal, Nov16, Nov18));
    }

    [Fact]
    public void A_running_attempt_has_the_version_of_its_current_date_in_effect()
    {
        var v15 = SettingsFrom(Minutes(15), Nov2, NamedTimeZone.Utc);
        var v20 = SettingsFrom(Minutes(20), Date(2026, 11, 12), NamedTimeZone.Utc);
        var goal = GoalOf(AttemptFrom(1, Nov2, null, v15, v20));

        Assert.Equal(v15, GoalTimeline.VersionInEffect(goal, Utc("2026-11-11T23:59:59Z")));
        Assert.Equal(v20, GoalTimeline.VersionInEffect(goal, Utc("2026-11-12T00:00:00Z")));
        Assert.Equal(v15, GoalTimeline.VersionInEffect(goal, 1, Utc("2026-11-11T23:59:59Z")));
        Assert.Equal(v15, GoalTimeline.VersionInEffect(goal, Utc("2026-11-01T12:00:00Z")));
        Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-02T00:00:00Z")));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-01T23:59:59Z")));
    }

    [Fact]
    public void A_waiting_attempt_has_its_first_version_in_effect_and_an_earlier_attempt_its_last()
    {
        var goal = Restarted(NamedTimeZone.Utc, NamedTimeZone.Utc);
        var waiting = Utc("2026-11-20T10:00:00Z");

        Assert.Equal(goal.VersionsOf(2)[0], GoalTimeline.VersionInEffect(goal, waiting));
        Assert.Equal(goal.VersionsOf(2)[0], GoalTimeline.VersionInEffect(goal, 2, waiting));
        Assert.Equal(goal.VersionsOf(1)[^1], GoalTimeline.VersionInEffect(goal, 1, waiting));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-22T23:59:59Z")));
        Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-23T00:00:00Z")));
    }

    [Fact]
    public void An_ended_attempt_has_the_version_of_its_last_date_in_effect()
    {
        var v10 = SettingsFrom(Minutes(10), Nov2, NamedTimeZone.Utc);
        var v12 = SettingsFrom(Minutes(12), Nov15, NamedTimeZone.Utc);
        var goal = GoalOf(AttemptFrom(1, Nov2, EndedOn(Nov16), v10, v12));

        Assert.Equal(v12, GoalTimeline.VersionInEffect(goal, Utc("2026-11-16T10:00:00Z")));
        Assert.Equal(v12, GoalTimeline.VersionInEffect(goal, 1, Utc("2027-01-01T00:00:00Z")));
        Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-16T10:00:00Z")));
    }

    [Fact]
    public void AC_035_attempt_2_starts_before_attempt_1s_start_date_and_attempt_1_never_began()
    {
        var goal = EndedBeforeItBegan();

        Assert.Equal(Nov23, goal.StartDate);
        Assert.Equal(goal.VersionsOf(1)[0], GoalTimeline.VersionInEffect(goal, 1, Utc("2026-11-25T10:00:00Z")));
        Assert.Equal(goal.VersionsOf(2)[0], GoalTimeline.VersionInEffect(goal, Utc("2026-11-17T12:00:00Z")));
        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-17T23:59:59Z")));
        Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-18T00:00:00Z")));
        Assert.Equal(2, GoalTimeline.AttemptOf(goal, Nov23));
        Assert.Equal(goal.VersionsOf(2)[0], GoalTimeline.VersionOf(goal, Nov23));
        Assert.Equal(Nov18, Entries(goal, Nov16, Nov23)[0].FirstDate);
        Assert.Equal(Nov18, GoalTimeline.EntriesThrough(goal, Nov23)[0].FirstDate);
    }

    [Fact]
    public void An_attempt_ended_with_no_date_never_begins()
    {
        var goal = GoalOf(AttemptFrom(1, Nov23, EndedOn(Nov17), SettingsFrom(Minutes(10), Nov23, NamedTimeZone.Utc)));

        Assert.False(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-12-01T00:00:00Z")));
        Assert.Null(GoalTimeline.CurrentDate(goal, Utc("2026-12-01T00:00:00Z")));
        Assert.Equal(goal.Versions[0], GoalTimeline.VersionInEffect(goal, Utc("2026-12-01T00:00:00Z")));
        Assert.Empty(GoalTimeline.EntriesThrough(goal, Date(2026, 12, 31)));
    }

    [Fact]
    public void A_one_attempt_goal_whose_attempt_starts_after_the_start_date_is_timed_from_the_start_date()
    {
        var kept = new Goal(
            new GoalId(Guid.NewGuid()), Owner, Habit, Nov18, null, new GoalAttempt(1, Date(2026, 11, 20)),
            [SettingsFrom(PerDay(), Nov18, NamedTimeZone.Utc)]);

        foreach (var goal in new[] { kept, ViaAttempts(kept) })
        {
            Assert.Equal(Nov18, Entries(goal, Nov16, Nov22)[0].FirstDate);
            Assert.Equal(Nov18, GoalTimeline.CurrentDate(goal, Utc("2026-11-18T10:00:00Z")));
            Assert.True(GoalTimeline.HasBegun(goal, Nov18, Utc("2026-11-18T10:00:00Z")));
            Assert.True(GoalTimeline.LatestAttemptHasBegun(goal, Utc("2026-11-18T00:00:00Z")));
            Assert.Equal(1, GoalTimeline.AttemptOf(goal, Nov18));
        }
    }

    [Fact]
    public void An_ended_goal_has_no_next_commitment_and_no_first_week()
    {
        var commitments = GoalOf(Meditation(NamedTimeZone.Utc));
        var weekly = GoalOf(AttemptFrom(1, Nov18, EndedOn(Date(2026, 12, 1)), SettingsFrom(PerWeek(), Nov18, NamedTimeZone.Utc)));

        Assert.Null(GoalTimeline.NextCommitment(commitments, Utc("2026-11-10T10:00:00Z")));
        Assert.Null(GoalTimeline.FirstWeekStart(weekly));
        Assert.Null(GoalTimeline.NextCommitment(weekly, Utc("2026-11-10T10:00:00Z")));
    }

    [Fact]
    public void A_waiting_attempt_gives_its_own_next_commitment_and_first_week()
    {
        var commitments = Restarted(NamedTimeZone.Utc, NamedTimeZone.Utc);
        var weekly = GoalOf(Meditation(NamedTimeZone.Utc), AttemptFrom(2, Nov18, null, SettingsFrom(PerWeek(), Nov18, NamedTimeZone.Utc)));

        Assert.Equal(Nov23, GoalTimeline.NextCommitment(commitments, Utc("2026-11-20T10:00:00Z")));
        Assert.Equal(Nov23, GoalTimeline.FirstWeekStart(weekly));
    }
}
