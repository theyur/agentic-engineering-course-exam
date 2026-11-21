using HT.Domain.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// Slice 6 FR-001, research R-02: every entry from the start date to the entry that contains the last date, read in
/// Monday-aligned windows of 364 days, with no 366-day cap. Monday 5 January 2026 starts a window that ends on Sunday
/// 3 January 2027, so Monday 4 January 2027 starts the next one. Kyiv is UTC+2 in winter.
/// </summary>
public class GoalTimelineEntriesThroughTests
{
    private static void AssertConsecutiveDays(IReadOnlyList<CalendarEntry> entries)
    {
        for (var i = 1; i < entries.Count; i++)
        {
            Assert.Equal(entries[i - 1].FirstDate.AddDays(1), entries[i].FirstDate);
        }

        Assert.All(entries, entry => Assert.Equal(entry.FirstDate, entry.LastDate));
    }

    [Fact]
    public void A_per_week_goal_across_a_window_edge_has_every_week_once()
    {
        var goal = Goal(PerWeek(), Date(2026, 1, 5), Kyiv);

        var entries = GoalTimeline.EntriesThrough(goal, Date(2027, 2, 7));

        // Monday 5 January 2026 to Monday 1 February 2027: 392 days, 56 weeks apart, so 57 weeks.
        Assert.Equal(57, entries.Count);
        Assert.Equal(Date(2026, 1, 5), entries[0].FirstDate);
        Assert.Equal(Date(2027, 2, 1), entries[^1].FirstDate);
        Assert.Equal(Date(2027, 2, 7), entries[^1].LastDate);
        for (var i = 1; i < entries.Count; i++)
        {
            Assert.Equal(entries[i - 1].FirstDate.AddDays(7), entries[i].FirstDate);
            Assert.Equal(entries[i - 1].Ends, entries[i].Starts);
        }

        Assert.All(entries, entry => Assert.Equal(entry.FirstDate.AddDays(6), entry.LastDate));
        Assert.Single(entries, entry => entry.FirstDate == Date(2026, 12, 28));
        Assert.Equal(
            new CalendarEntry(Date(2027, 1, 4), Date(2027, 1, 10), Utc("2027-01-03T22:00:00Z"), Utc("2027-01-10T22:00:00Z")),
            Assert.Single(entries, entry => entry.FirstDate == Date(2027, 1, 4)));
    }

    [Fact]
    public void A_per_day_goal_across_a_window_edge_has_every_day_once()
    {
        // Started on Thursday 8 January 2026: its first window still starts on Monday 5 January.
        var goal = Goal(PerDay(), Date(2026, 1, 8), Kyiv);

        var entries = GoalTimeline.EntriesThrough(goal, Date(2027, 1, 10));

        // 8 January 2026 to 10 January 2027, both included: 368 days, more than the 366 of one calendar.
        Assert.Equal(368, entries.Count);
        Assert.Equal(Date(2026, 1, 8), entries[0].FirstDate);
        Assert.Equal(Date(2027, 1, 10), entries[^1].FirstDate);
        AssertConsecutiveDays(entries);
        Assert.Equal(
            new CalendarEntry(Date(2027, 1, 3), Date(2027, 1, 3), Utc("2027-01-02T22:00:00Z"), Utc("2027-01-03T22:00:00Z")),
            Assert.Single(entries, entry => entry.FirstDate == Date(2027, 1, 3)));
        Assert.Equal(
            new CalendarEntry(Date(2027, 1, 4), Date(2027, 1, 4), Utc("2027-01-03T22:00:00Z"), Utc("2027-01-04T22:00:00Z")),
            Assert.Single(entries, entry => entry.FirstDate == Date(2027, 1, 4)));
    }

    [Fact]
    public void A_commitment_based_goal_800_days_old_has_every_commitment_from_its_start()
    {
        // Monday 2 September 2024 to Tuesday 10 November 2026 is 800 days: 114 weeks of 3 commitments, then a Monday.
        var goal = Goal(OnDays(MonWedFri), Date(2024, 9, 2), Kyiv);

        var entries = GoalTimeline.EntriesThrough(goal, Date(2026, 11, 10));

        Assert.Equal(343, entries.Count);
        Assert.Equal(Date(2024, 9, 2), entries[0].FirstDate);
        Assert.Equal(Date(2026, 11, 9), entries[^1].FirstDate);
        Assert.All(entries, entry => Assert.Contains(entry.FirstDate.DayOfWeek, MonWedFri));
        for (var i = 1; i < entries.Count; i++)
        {
            Assert.InRange(entries[i].FirstDate.DayNumber - entries[i - 1].FirstDate.DayNumber, 2, 3);
        }
    }

    [Fact]
    public void A_per_day_goal_800_days_old_has_every_day_from_its_start()
    {
        var goal = Goal(PerDay(), Date(2024, 9, 2), Kyiv);

        var entries = GoalTimeline.EntriesThrough(goal, Date(2026, 11, 10));

        Assert.Equal(800, entries.Count);
        Assert.Equal(Date(2024, 9, 2), entries[0].FirstDate);
        Assert.Equal(Date(2026, 11, 10), entries[^1].FirstDate);
        AssertConsecutiveDays(entries);
    }

    [Fact]
    public void Edge_case_a_per_week_goal_started_mid_week_has_no_entry_before_its_first_week()
    {
        var goal = Goal(PerWeek(), Date(2026, 11, 18), Kyiv);

        Assert.Empty(GoalTimeline.EntriesThrough(goal, Date(2026, 11, 22)));
        Assert.Equal(
            [new CalendarEntry(Date(2026, 11, 23), Date(2026, 11, 29), Utc("2026-11-22T22:00:00Z"), Utc("2026-11-29T22:00:00Z"))],
            GoalTimeline.EntriesThrough(goal, Date(2026, 11, 23)));
        Assert.Equal(
            [Date(2026, 11, 23), Date(2026, 11, 30)],
            GoalTimeline.EntriesThrough(goal, Date(2026, 12, 2)).Select(entry => entry.FirstDate));
    }

    [Fact]
    public void A_last_date_before_the_first_entry_gives_no_entry()
    {
        Assert.Empty(GoalTimeline.EntriesThrough(Goal(PerDay(), Date(2026, 11, 18), Kyiv), Date(2026, 11, 17)));
        Assert.Empty(GoalTimeline.EntriesThrough(Goal(OnDays(MonWedFri), Date(2026, 11, 17), Kyiv), Date(2026, 11, 17)));
        Assert.Empty(GoalTimeline.EntriesThrough(Goal(PerWeek(), Date(2026, 11, 16), Kyiv), Date(2026, 11, 15)));
        Assert.Empty(GoalTimeline.EntriesThrough(Goal(PerDay(), Date(2026, 11, 18), Kyiv), Date(2025, 1, 1)));
    }

    [Fact]
    public void The_entry_that_contains_the_last_date_is_included()
    {
        var goal = Goal(OnDays(MonWedFri), Date(2026, 11, 16), Kyiv);

        Assert.Equal(
            [Date(2026, 11, 16), Date(2026, 11, 18)],
            GoalTimeline.EntriesThrough(goal, Date(2026, 11, 18)).Select(entry => entry.FirstDate));
        Assert.Equal(
            [Date(2026, 11, 16), Date(2026, 11, 18)],
            GoalTimeline.EntriesThrough(goal, Date(2026, 11, 19)).Select(entry => entry.FirstDate));
    }

    [Fact]
    public void FR_027_the_overlap_after_a_move_east_keeps_both_entries_in_date_order()
    {
        // New York from Monday 16 November, Kyiv from Thursday 19 November: Thursday starts before Wednesday ends.
        var goal = GoalVersionChange.Add(Date(2026, 11, 19), Kyiv).ApplyTo(Goal(OnDays(AllDays), Date(2026, 11, 16), NewYork));

        var entries = GoalTimeline.EntriesThrough(goal, Date(2026, 11, 20));

        Assert.Equal(
            [Date(2026, 11, 16), Date(2026, 11, 17), Date(2026, 11, 18), Date(2026, 11, 19), Date(2026, 11, 20)],
            entries.Select(entry => entry.FirstDate));
        Assert.Equal(
            new CalendarEntry(Date(2026, 11, 18), Date(2026, 11, 18), Utc("2026-11-18T05:00:00Z"), Utc("2026-11-19T05:00:00Z")),
            entries[2]);
        Assert.Equal(
            new CalendarEntry(Date(2026, 11, 19), Date(2026, 11, 19), Utc("2026-11-18T22:00:00Z"), Utc("2026-11-19T22:00:00Z")),
            entries[3]);
        Assert.Equal(
            new CalendarEntry(Date(2026, 11, 20), Date(2026, 11, 20), Utc("2026-11-19T22:00:00Z"), Utc("2026-11-20T22:00:00Z")),
            entries[4]);
    }

    public static TheoryData<string> Kinds => new() { "commitments", "days", "weeks" };

    [Theory]
    [MemberData(nameof(Kinds))]
    public void Within_one_calendar_it_agrees_with_TryEntries_from_the_start_date(string kind)
    {
        var start = Date(2026, 11, 18);
        var goal = kind switch
        {
            "commitments" => Goal(OnDays(MonWedFri), start, Kyiv),
            "days" => Goal(PerDay(), start, NewYork),
            _ => Goal(PerWeek(), start, Kyiv),
        };
        var last = start.AddDays(100);

        Assert.True(GoalTimeline.TryEntries(goal, start, last, out var expected));
        Assert.NotEmpty(expected);
        Assert.Equal(expected, GoalTimeline.EntriesThrough(goal, last));
    }
}
