using HT.Domain.Goals;
using HT.Domain.Time;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// Slice 8 FR-024, research R-12: the slice 4 to 7 timeline cases (<see cref="GoalSamples.TimelineGoal"/>) give identical
/// results for a one-attempt goal built through the kept constructor and through the attempts constructor, for every
/// <see cref="GoalTimeline"/> member; and the settings in effect equal slice 7's
/// <c>VersionOf(goal, CurrentDate(goal, instant) ?? goal.StartDate)</c>.
/// </summary>
public class GoalTimelineEquivalenceTests
{
    private static readonly NamedTimeZone[] Zones = [Kyiv, NewYork, Tokyo, NamedTimeZone.Utc];

    /// <summary>The dates around a goal's start: ten days before to six weeks after.</summary>
    private static IEnumerable<DateOnly> DatesAround(Goal goal) =>
        Enumerable.Range(-10, 52).Select(offset => goal.StartDate.AddDays(offset));

    private static (bool Ok, GoalScheduleKind? Kind, Commitment[] Commitments, EvaluationPeriod[] Periods) CalendarOf(Goal goal, DateOnly first, DateOnly last) =>
        GoalTimeline.TryCalendar(goal, first, last, out var calendar)
            ? (true, calendar.Kind, [.. calendar.Commitments], [.. calendar.Periods])
            : (false, null, [], []);

    private static CalendarEntry[]? EntriesOf(Goal goal, DateOnly first, DateOnly last) =>
        GoalTimeline.TryEntries(goal, first, last, out var entries) ? [.. entries] : null;

    [Theory]
    [MemberData(nameof(TimelineGoals), MemberType = typeof(GoalSamples))]
    public void Every_date_member_gives_the_same_result_for_both_constructors(string name)
    {
        var kept = TimelineGoal(name);
        var built = ViaAttempts(kept);

        Assert.Equal(kept.StartDate, built.StartDate);
        Assert.Equal(GoalTimeline.FirstWeekStart(kept), GoalTimeline.FirstWeekStart(built));
        foreach (var date in DatesAround(kept))
        {
            Assert.Equal(GoalTimeline.VersionOf(kept, date), GoalTimeline.VersionOf(built, date));
            Assert.Equal(GoalTimeline.EntryOf(kept, date), GoalTimeline.EntryOf(built, date));
            Assert.Equal(GoalTimeline.AttemptOf(kept, date), GoalTimeline.AttemptOf(built, date));
            Assert.Equal(GoalTimeline.EntryOf(kept, date) is null ? null : 1, GoalTimeline.AttemptOf(kept, date));
        }
    }

    [Theory]
    [MemberData(nameof(TimelineGoals), MemberType = typeof(GoalSamples))]
    public void The_calendar_and_the_entries_are_the_same_for_both_constructors(string name)
    {
        var kept = TimelineGoal(name);
        var built = ViaAttempts(kept);
        var start = kept.StartDate;
        (DateOnly First, DateOnly Last)[] ranges =
        [
            (start.AddDays(-10), start.AddDays(40)), (start, start), (start.AddDays(-30), start.AddDays(-1)),
            (start, start.AddDays(365)), (start, start.AddDays(366)), (start.AddDays(5), start.AddDays(4)),
        ];

        foreach (var (first, last) in ranges)
        {
            var expected = CalendarOf(kept, first, last);
            var actual = CalendarOf(built, first, last);
            Assert.Equal((expected.Ok, expected.Kind), (actual.Ok, actual.Kind));
            Assert.Equal(expected.Commitments, actual.Commitments);
            Assert.Equal(expected.Periods, actual.Periods);
            Assert.Equal(EntriesOf(kept, first, last), EntriesOf(built, first, last));
        }

        foreach (var last in new[] { start.AddDays(-1), start.AddDays(20), start.AddDays(400), start.AddDays(800) })
        {
            Assert.Equal(GoalTimeline.EntriesThrough(kept, last), GoalTimeline.EntriesThrough(built, last));
            Assert.Equal(GoalTimeline.EntriesThrough(kept, last), GoalTimeline.EntriesThrough(built, 1, last));
        }
    }

    [Theory]
    [MemberData(nameof(TimelineGoals), MemberType = typeof(GoalSamples))]
    public void Every_instant_member_gives_the_same_result_for_both_constructors(string name)
    {
        var kept = TimelineGoal(name);
        var built = ViaAttempts(kept);

        foreach (var instant in TimelineInstants)
        {
            var current = GoalTimeline.CurrentDate(kept, instant);
            Assert.Equal(current, GoalTimeline.CurrentDate(built, instant));
            Assert.Equal(GoalTimeline.NextCommitment(kept, instant), GoalTimeline.NextCommitment(built, instant));
            Assert.Equal(GoalTimeline.LatestAttemptHasBegun(kept, instant), GoalTimeline.LatestAttemptHasBegun(built, instant));
            Assert.Equal(current is not null, GoalTimeline.LatestAttemptHasBegun(kept, instant));
            foreach (var zone in Zones)
            {
                Assert.Equal(GoalTimeline.ChangeOf(kept, instant, zone), GoalTimeline.ChangeOf(built, instant, zone));
            }

            foreach (var date in DatesAround(kept).Where((_, index) => index % 3 == 0))
            {
                Assert.Equal(GoalTimeline.HasBegun(kept, date, instant), GoalTimeline.HasBegun(built, date, instant));
            }
        }
    }

    [Theory]
    [MemberData(nameof(TimelineGoals), MemberType = typeof(GoalSamples))]
    public void The_settings_in_effect_are_the_version_of_the_current_date_or_the_first(string name)
    {
        var kept = TimelineGoal(name);
        var built = ViaAttempts(kept);

        foreach (var instant in TimelineInstants)
        {
            var slice7 = GoalTimeline.VersionOf(kept, GoalTimeline.CurrentDate(kept, instant) ?? kept.StartDate);
            Assert.Equal(slice7, GoalTimeline.VersionInEffect(kept, instant));
            Assert.Equal(slice7, GoalTimeline.VersionInEffect(built, instant));
            Assert.Equal(slice7, GoalTimeline.VersionInEffect(built, 1, instant));
        }

        Assert.Null(GoalTimeline.LastDate(kept, 1));
        Assert.False(kept.IsEnded);
    }
}
