using HT.Domain.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// Slice 5 FR-002, US1-AS5, US1-AS6, US4-AS5, research R-03: the entry a date belongs to, whether its day has begun,
/// and the entries of a range. Kyiv is UTC+2 in November 2026, so its midnight of day d is d − 1 at 22:00 UTC; New
/// York is UTC−5 then.
/// </summary>
public class GoalTimelineEntryTests
{
    private static readonly DateOnly Monday = Date(2026, 11, 16);
    private static readonly DateOnly Tuesday = Date(2026, 11, 17);
    private static readonly DateOnly Wednesday = Date(2026, 11, 18);
    private static readonly DateOnly Thursday = Date(2026, 11, 19);
    private static readonly DateOnly Friday = Date(2026, 11, 20);
    private static readonly DateOnly Sunday = Date(2026, 11, 22);
    private static readonly DateOnly NextMonday = Date(2026, 11, 23);
    private static readonly DateOnly NextSunday = Date(2026, 11, 29);
    private static readonly DateOnly MondayAfter = Date(2026, 11, 30);

    [Fact]
    public void US1_AS6_a_Mon_Wed_Fri_goal_has_no_entry_on_a_Tuesday_and_its_commitment_on_a_Wednesday()
    {
        var goal = Goal(OnDays(MonWedFri), Monday, Kyiv);

        Assert.Null(GoalTimeline.EntryOf(goal, Tuesday));
        var wednesday = GoalTimeline.EntryOf(goal, Wednesday);

        Assert.Equal(new CalendarEntry(Wednesday, Wednesday, Utc("2026-11-17T22:00:00Z"), Utc("2026-11-18T22:00:00Z")), wednesday);
        Assert.Equal(Wednesday, wednesday!.Key);
    }

    [Fact]
    public void A_date_before_the_start_date_has_no_entry()
    {
        Assert.Null(GoalTimeline.EntryOf(Goal(OnDays(AllDays), Wednesday, Kyiv), Tuesday));
        Assert.Null(GoalTimeline.EntryOf(Goal(PerDay(), Wednesday, Kyiv), Tuesday));
        Assert.Null(GoalTimeline.EntryOf(Goal(PerWeek(), Monday, Kyiv), Sunday.AddDays(-7)));
        Assert.Null(GoalTimeline.EntryOf(Goal(PerDay(), Wednesday, Kyiv), Date(2025, 1, 1)));
    }

    [Fact]
    public void A_per_day_goal_gives_the_day()
    {
        var goal = Goal(PerDay(), Monday, NewYork);

        Assert.Equal(
            new CalendarEntry(Wednesday, Wednesday, Utc("2026-11-18T05:00:00Z"), Utc("2026-11-19T05:00:00Z")),
            GoalTimeline.EntryOf(goal, Wednesday));
    }

    [Fact]
    public void A_per_week_goal_started_on_a_Wednesday_has_no_entry_until_its_first_Monday()
    {
        var goal = Goal(PerWeek(), Wednesday, Kyiv);

        for (var date = Wednesday; date <= Sunday; date = date.AddDays(1))
        {
            Assert.Null(GoalTimeline.EntryOf(goal, date));
        }

        var week = GoalTimeline.EntryOf(goal, NextMonday);
        Assert.Equal(new CalendarEntry(NextMonday, NextSunday, Utc("2026-11-22T22:00:00Z"), Utc("2026-11-29T22:00:00Z")), week);
        Assert.Equal(NextMonday, week!.Key);
    }

    [Fact]
    public void A_Sunday_gives_the_week_ending_on_it_and_the_next_Monday_the_next_week()
    {
        var goal = Goal(PerWeek(), Wednesday, Kyiv);

        Assert.Equal(
            new CalendarEntry(NextMonday, NextSunday, Utc("2026-11-22T22:00:00Z"), Utc("2026-11-29T22:00:00Z")),
            GoalTimeline.EntryOf(goal, NextSunday));
        Assert.Equal(
            new CalendarEntry(MondayAfter, Date(2026, 12, 6), Utc("2026-11-29T22:00:00Z"), Utc("2026-12-06T22:00:00Z")),
            GoalTimeline.EntryOf(goal, MondayAfter));
    }

    [Fact]
    public void Every_date_of_a_week_names_that_week()
    {
        var goal = Goal(PerWeek(), Wednesday, Kyiv);

        for (var date = NextMonday; date <= NextSunday; date = date.AddDays(1))
        {
            Assert.Equal(NextMonday, GoalTimeline.EntryOf(goal, date)?.Key);
        }
    }

    public static TheoryData<string, bool, bool, bool> AroundThursdayMidnight => new()
    {
        // Thursday 19 November begins at 22:00 UTC on Wednesday (Kyiv midnight).
        { "2026-11-18T10:00:00Z", true, false, false }, // noon on Wednesday
        { "2026-11-18T21:59:59Z", true, false, false }, // a second before Thursday
        { "2026-11-18T22:00:00Z", true, true, false }, // Thursday midnight
        { "2026-11-18T22:00:01Z", true, true, false }, // a second after Thursday midnight
    };

    [Theory]
    [MemberData(nameof(AroundThursdayMidnight))]
    public void US4_AS5_HasBegun_for_today_tomorrow_and_a_later_date_of_the_current_week(
        string instant, bool wednesday, bool thursday, bool friday)
    {
        var daily = Goal(PerDay(), Monday, Kyiv);
        var weekly = Goal(PerWeek(), Monday, Kyiv);

        foreach (var goal in new[] { daily, weekly })
        {
            Assert.True(GoalTimeline.HasBegun(goal, Monday, Utc(instant)));
            Assert.Equal(wednesday, GoalTimeline.HasBegun(goal, Wednesday, Utc(instant)));
            Assert.Equal(thursday, GoalTimeline.HasBegun(goal, Thursday, Utc(instant)));
            Assert.Equal(friday, GoalTimeline.HasBegun(goal, Friday, Utc(instant)));
        }
    }

    [Fact]
    public void HasBegun_is_false_one_tick_before_the_local_midnight_that_begins_the_date()
    {
        var goal = Goal(PerDay(), Monday, Kyiv);
        var thursdayMidnight = Utc("2026-11-18T22:00:00Z");

        Assert.False(GoalTimeline.HasBegun(goal, Thursday, thursdayMidnight.AddTicks(-1)));
        Assert.True(GoalTimeline.HasBegun(goal, Thursday, thursdayMidnight));
    }

    [Fact]
    public void A_goal_whose_start_date_has_not_begun_has_begun_no_date()
    {
        var goal = Goal(PerDay(), Thursday, Kyiv);

        Assert.False(GoalTimeline.HasBegun(goal, Thursday, Utc("2026-11-18T10:00:00Z")));
        Assert.False(GoalTimeline.HasBegun(goal, Wednesday, Utc("2026-11-18T10:00:00Z")));
    }

    [Fact]
    public void The_overlap_after_a_move_east_gives_two_entries_each_with_its_own_instants()
    {
        // Noon in New York on Wednesday 18 November; the Kyiv version applies from Thursday.
        var goal = GoalVersionChange.Add(Thursday, Kyiv).ApplyTo(Goal(OnDays(AllDays), Monday, NewYork));
        var wednesday = new CalendarEntry(Wednesday, Wednesday, Utc("2026-11-18T05:00:00Z"), Utc("2026-11-19T05:00:00Z"));
        var thursday = new CalendarEntry(Thursday, Thursday, Utc("2026-11-18T22:00:00Z"), Utc("2026-11-19T22:00:00Z"));

        Assert.True(GoalTimeline.TryEntries(goal, Wednesday, Thursday, out var entries));

        Assert.Equal([wednesday, thursday], entries);
        Assert.Equal(wednesday, GoalTimeline.EntryOf(goal, Wednesday));
        Assert.Equal(thursday, GoalTimeline.EntryOf(goal, Thursday));

        // At 23:00 UTC on Wednesday both have begun, and Wednesday has not ended yet.
        var inTheOverlap = Utc("2026-11-18T23:00:00Z");
        Assert.True(GoalTimeline.HasBegun(goal, Wednesday, inTheOverlap));
        Assert.True(GoalTimeline.HasBegun(goal, Thursday, inTheOverlap));
        Assert.True(wednesday.Ends > inTheOverlap && thursday.Starts <= inTheOverlap);
    }

    [Fact]
    public void TryEntries_has_the_range_rule_of_the_calendar()
    {
        var goal = Goal(PerDay(), Wednesday, Kyiv);

        Assert.False(GoalTimeline.TryEntries(goal, Wednesday, Tuesday, out var none));
        Assert.Null(none);
        Assert.False(GoalTimeline.TryEntries(goal, Wednesday, Date(2027, 11, 19), out _));
        Assert.True(GoalTimeline.TryEntries(goal, Wednesday, Date(2027, 11, 18), out var year));
        Assert.Equal(366, year.Count);
    }

    public static TheoryData<string> Kinds => new() { "commitments", "days", "weeks" };

    private static Goal GoalOf(string kind) => kind switch
    {
        "commitments" => Goal(OnDays(MonWedFri), Wednesday, Kyiv),
        "days" => Goal(PerDay(), Wednesday, NewYork),
        "weeks" => Goal(PerWeek(), Wednesday, Kyiv),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static IReadOnlyList<CalendarEntry> CalendarAsEntries(Goal goal, DateOnly first, DateOnly last)
    {
        Assert.True(GoalTimeline.TryCalendar(goal, first, last, out var calendar));
        return calendar.Kind == GoalScheduleKind.CommitmentBased
            ? calendar.Commitments.Select(c => new CalendarEntry(c.Date, c.Date, c.Starts, c.Ends)).ToList()
            : calendar.Periods.Select(p => new CalendarEntry(p.FirstDate, p.LastDate, p.Starts, p.Ends)).ToList();
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void EntryOf_agrees_with_the_calendar_of_its_date_for_every_date_of_28_days(string kind)
    {
        var goal = GoalOf(kind);

        for (var date = Monday; date < Monday.AddDays(28); date = date.AddDays(1))
        {
            var expected = CalendarAsEntries(goal, date, date).SingleOrDefault(e => e.FirstDate <= date && date <= e.LastDate);
            Assert.Equal(expected, GoalTimeline.EntryOf(goal, date));
        }
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void TryEntries_agrees_with_the_calendar_of_the_same_range(string kind)
    {
        var goal = GoalOf(kind);
        var last = Monday.AddDays(27);

        Assert.True(GoalTimeline.TryEntries(goal, Monday, last, out var entries));

        Assert.Equal(CalendarAsEntries(goal, Monday, last), entries);
        Assert.NotEmpty(entries);
        Assert.Equal(entries.OrderBy(e => e.FirstDate), entries);
    }
}
