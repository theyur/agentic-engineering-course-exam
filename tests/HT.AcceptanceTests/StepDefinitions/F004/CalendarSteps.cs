using System.Reflection;
using HT.AcceptanceTests.Support.F004;
using HT.Domain.Goals;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>The Then steps of F004 about the evaluation calendar (FR-012, FR-013, FR-026, FR-027).</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class CalendarSteps(F004Context context)
{
    private const string Date = @"(\d{4}-\d{2}-\d{2})";

    [Then(@"^(?:it|the calendar) has (\d+) (commitments?|evaluation periods?), in this order:$")]
    public void ThenItHasEntriesInThisOrder(int count, string kind)
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        F004Assert.CalendarKind(calendar, kind);
        Assert.Equal(count, F004Assert.Entries(calendar).Count);
        context.CalendarCursor = 0;
    }

    [Then(@"^(\d{4}-\d{2}-\d{2}(?: to \d{4}-\d{2}-\d{2})? from `[^`]+` to `[^`]+`)$")]
    public void ThenTheNextEntryIs(string entry)
    {
        var expected = Assert.Single(F004Text.CalendarEntries(entry));
        var entries = F004Assert.Entries(F004Assert.Calendar(context.LastCalendar));
        Assert.True(context.CalendarCursor < entries.Count, $"The calendar has no entry {context.CalendarCursor + 1}.");
        F004Assert.Entry(expected, entries[context.CalendarCursor++]);
    }

    [Then(@"^(?:it|the calendar) has (\d+) (commitments?|evaluation periods?), in this order: (.+)$")]
    public void ThenItHasTheseEntries(int count, string kind, string entries)
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        F004Assert.CalendarKind(calendar, kind);
        var expected = F004Text.CalendarEntries(entries);
        Assert.Equal(count, expected.Count);
        F004Assert.ExactEntries(calendar, expected);
    }

    [Then(@"^(?:it|the calendar) has, in this order, (.+)$")]
    public void ThenItHasInThisOrder(string entries)
    {
        F004Assert.ExactEntries(F004Assert.Calendar(context.LastCalendar), F004Text.CalendarEntries(entries));
    }

    [Then(@"^it has (\d+) periods?: (.+)$")]
    public void ThenItHasPeriods(int count, string entries)
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        Assert.Equal(GoalScheduleKind.PeriodBased, calendar.Kind);
        var expected = F004Text.CalendarEntries(entries);
        Assert.Equal(count, expected.Count);
        F004Assert.ExactEntries(calendar, expected);
    }

    [Then(@"^it has no period$")]
    public void ThenItHasNoPeriod()
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        Assert.Empty(calendar.Periods);
        Assert.Empty(calendar.Commitments);
    }

    [Then(@"^it has exactly one entry: (.+)$")]
    public void ThenItHasExactlyOneEntry(string entry)
    {
        var expected = F004Text.CalendarEntries(entry);
        Assert.Single(expected);
        F004Assert.ExactEntries(F004Assert.Calendar(context.LastCalendar), expected);
    }

    [Then(@"^it has (\d+) evaluation periods, one for each date from " + Date + " to " + Date + ", in date order$")]
    public void ThenItHasOnePeriodPerDate(int count, string first, string last)
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        Assert.Equal(GoalScheduleKind.PeriodBased, calendar.Kind);
        var from = F004Text.Date(first);
        var to = F004Text.Date(last);
        Assert.Equal(count, calendar.Periods.Count);
        Assert.Equal(count, to.DayNumber - from.DayNumber + 1);
        for (var index = 0; index < count; index++)
        {
            Assert.Equal(from.AddDays(index), calendar.Periods[index].FirstDate);
            Assert.Equal(from.AddDays(index), calendar.Periods[index].LastDate);
        }
    }

    [Then(@"^each runs from (\d{2}:\d{2}:\d{2})Z of its date to (\d{2}:\d{2}:\d{2})Z of the next date: the first from `([^`]+)` to `([^`]+)`, the last from `([^`]+)` to `([^`]+)`$")]
    public void ThenEachRunsFromTo(string startTime, string endTime, string firstStarts, string firstEnds, string lastStarts, string lastEnds)
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        Assert.NotEmpty(calendar.Periods);
        var starts = TimeOnly.Parse(startTime, System.Globalization.CultureInfo.InvariantCulture);
        var ends = TimeOnly.Parse(endTime, System.Globalization.CultureInfo.InvariantCulture);
        foreach (var period in calendar.Periods)
        {
            Assert.Equal(new DateTimeOffset(period.FirstDate.ToDateTime(starts), TimeSpan.Zero), period.Starts);
            Assert.Equal(new DateTimeOffset(period.FirstDate.AddDays(1).ToDateTime(ends), TimeSpan.Zero), period.Ends);
        }

        Assert.Equal(F004Text.Instant(firstStarts), calendar.Periods[0].Starts);
        Assert.Equal(F004Text.Instant(firstEnds), calendar.Periods[0].Ends);
        Assert.Equal(F004Text.Instant(lastStarts), calendar.Periods[^1].Starts);
        Assert.Equal(F004Text.Instant(lastEnds), calendar.Periods[^1].Ends);
    }

    [Then(@"^no entry has a status$")]
    public void ThenNoEntryHasAStatus()
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        var types = new List<Type> { calendar.GetType() };
        types.AddRange(calendar.Commitments.Select(entry => entry.GetType()));
        types.AddRange(calendar.Periods.Select(entry => entry.GetType()));
        foreach (var type in types.Distinct())
        {
            var statuses = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.Name.Contains("Status", StringComparison.OrdinalIgnoreCase))
                .Select(property => property.Name)
                .ToList();
            Assert.True(statuses.Count == 0, $"{type.Name} gives a status: {string.Join(", ", statuses)}.");
        }
    }

    [Then(@"^the calendar has no entry$")]
    public void ThenTheCalendarHasNoEntry()
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        Assert.Empty(calendar.Commitments);
        Assert.Empty(calendar.Periods);
    }

    [Then(@"^it has no commitment on " + Date + ", before the start date$")]
    public void ThenItHasNoCommitmentOn(string date)
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        Assert.DoesNotContain(calendar.Commitments, commitment => commitment.Date == F004Text.Date(date));
    }

    [Then(@"^the result is a calendar of (\d+) commitments?, the first on " + Date + " and the last on " + Date + "$")]
    public void ThenTheResultIsACalendarOfFirstAndLast(int count, string first, string last)
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        Assert.Equal(GoalScheduleKind.CommitmentBased, calendar.Kind);
        Assert.Equal(count, calendar.Commitments.Count);
        Assert.Equal(F004Text.Date(first), calendar.Commitments[0].Date);
        Assert.Equal(F004Text.Date(last), calendar.Commitments[^1].Date);
    }

    [Then(@"^the result is a calendar of (\d+) commitments?, on " + Date + "$")]
    public void ThenTheResultIsACalendarOn(int count, string only)
    {
        var calendar = F004Assert.Calendar(context.LastCalendar);
        Assert.Equal(GoalScheduleKind.CommitmentBased, calendar.Kind);
        Assert.Equal(count, calendar.Commitments.Count);
        Assert.Equal(F004Text.Date(only), calendar.Commitments[0].Date);
    }
}
