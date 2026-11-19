using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F005;
using HT.Application.Records;
using HT.Domain.Goals;

namespace HT.AcceptanceTests.StepDefinitions.F005;

/// <summary>
/// The Then steps of F005 about what a read gives: history entries (FR-009, FR-010, FR-014, FR-016), what can be
/// recorded now (FR-011), recent notes (FR-008) and slice 4's evaluation calendar (FR-019).
/// </summary>
[Binding]
[Scope(Tag = "F005")]
public sealed class HistorySteps(F005Context context, F005Driver driver, F005Arrange arrange, F005Checks checks)
{
    private const string D = F005Text.D;

    // ------------------------------------------------------------ the When step's read

    [Then(@"^it gives, in this order, (.+)$")]
    public async Task ThenItGivesInThisOrder(string list)
    {
        await arrange.AtThenTimeAsync();
        F005Entries.CheckOrdered(checks.LastEntries(), list, checks.LastReadKind);
    }

    /// <summary>
    /// "it gives N entries, in this order:" and "it gives exactly N notes, in this order:" (the lines after it give
    /// the entries), "it gives N entries, one for each date from D1 to D2, in date order" (the lines after it give
    /// entries by date), and everything else a read gives (<see cref="F005Entries.CheckBody"/>).
    /// </summary>
    [Then(@"^it gives (.+)$")]
    public async Task ThenItGives(string body)
    {
        await arrange.AtThenTimeAsync();
        Match match;
        if ((match = Regex.Match(body, @"^(\d+) entries, in this order:$")).Success)
        {
            Assert.Equal(F005Text.Int(match.Groups[1].Value), checks.LastEntries().Count);
            context.ListMode = F005ListMode.OrderedEntries;
            context.ListCursor = 0;
            return;
        }

        if ((match = Regex.Match(
                body,
                @"^(\d+) entries, one for each date from (" + D + ") to (" + D + @"), in date order(?:, each from (\d{2}:\d{2}:\d{2})Z of its date to (\d{2}:\d{2}:\d{2})Z of the next date)?$")).Success)
        {
            var entries = checks.LastEntries();
            var count = F005Text.Int(match.Groups[1].Value);
            var from = F005Text.Date(match.Groups[2].Value);
            var to = F005Text.Date(match.Groups[3].Value);
            Assert.Equal(count, to.DayNumber - from.DayNumber + 1);
            Assert.Equal(count, entries.Count);
            for (var index = 0; index < count; index++)
            {
                var date = from.AddDays(index);
                Assert.Equal(date, entries[index].FirstDate);
                Assert.Equal(date, entries[index].LastDate);
                if (match.Groups[4].Success)
                {
                    var starts = TimeOnly.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    var ends = TimeOnly.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                    Assert.Equal(new DateTimeOffset(date.ToDateTime(starts), TimeSpan.Zero), entries[index].Starts);
                    Assert.Equal(new DateTimeOffset(date.AddDays(1).ToDateTime(ends), TimeSpan.Zero), entries[index].Ends);
                }
            }

            context.ListMode = F005ListMode.EntriesByDate;
            context.ListCursor = 0;
            return;
        }

        if ((match = Regex.Match(body, @"^exactly (\d+) notes, in this order:$")).Success)
        {
            Assert.Equal(F005Text.Int(match.Groups[1].Value), checks.LastNotes().Count);
            context.ListMode = F005ListMode.OrderedNotes;
            context.ListCursor = 0;
            return;
        }

        F005Entries.CheckBody(checks.LastEntries(), body, checks.LastReadKind);
    }

    /// <summary>One line after a list header: the next entry or note in order, or entries named by date.</summary>
    [Then(@"^(\d{4}-\d{2}-\d{2}.*|each date from .+|each of .+)$")]
    public async Task ThenTheListLine(string line)
    {
        await arrange.AtThenTimeAsync();
        switch (context.ListMode)
        {
            case F005ListMode.OrderedEntries:
            {
                var entries = checks.LastEntries();
                Assert.True(context.ListCursor < entries.Count, $"The read gives no entry {context.ListCursor + 1}.");
                F005Entries.Check(entries[context.ListCursor++], line, checks.LastReadKind);
                break;
            }

            case F005ListMode.OrderedNotes:
            {
                var notes = checks.LastNotes();
                Assert.True(context.ListCursor < notes.Count, $"The read gives no note {context.ListCursor + 1}.");
                F005Entries.CheckNote(notes[context.ListCursor++], line);
                break;
            }

            case F005ListMode.EntriesByDate:
                EntriesByDate(checks.LastEntries(), line, checks.LastReadKind);
                break;
            default:
                throw new InvalidOperationException($"\"{line}\" follows no list header.");
        }
    }

    [Then(@"^its entry that contains (" + D + ") is (.+)$")]
    public async Task ThenItsEntryThatContainsIs(string date, string facts)
    {
        await arrange.AtThenTimeAsync();
        var day = F005Text.Date(date);
        var entry = Assert.Single(checks.LastEntries(), item => item.FirstDate <= day && day <= item.LastDate);
        var view = F005View.Of(entry, checks.LastReadKind);
        foreach (var fact in F005Entries.Facts(facts))
        {
            fact(view);
        }
    }

    // ------------------------------------------------------------ reads made by the Then step

    /// <summary>
    /// "(reading) the history (of the goal) from D1 to D2 (as `x`) gives …", "the history gives, in this order, N
    /// entries: …" (the When step's read) and "reading the history from D1 to D2 of every goal of `a`, `b` and `c`
    /// gives no record and no note".
    /// </summary>
    [Then(@"^((?:reading )?the history .+)$")]
    public async Task ThenTheHistory(string sentence)
    {
        await arrange.AtThenTimeAsync();
        Match match;
        if ((match = Regex.Match(sentence, @"^the history gives, in this order, (\d+) entries: (.+)$")).Success)
        {
            var entries = checks.LastEntries();
            Assert.Equal(F005Text.Int(match.Groups[1].Value), entries.Count);
            F005Entries.CheckOrdered(entries, match.Groups[2].Value, checks.LastReadKind);
            return;
        }

        if ((match = Regex.Match(sentence, "^reading the history from (" + D + ") to (" + D + ") of every goal of (.+) gives no record and no note$")).Success)
        {
            var first = F005Text.Date(match.Groups[1].Value);
            var last = F005Text.Date(match.Groups[2].Value);
            var emails = Regex.Matches(match.Groups[3].Value, "`([^`]+)`").Select(item => item.Groups[1].Value).ToList();
            Assert.NotEmpty(emails);
            foreach (var email in emails)
            {
                var user = await arrange.AccountAsync(email);
                foreach (var goal in await arrange.ListGoalsAsync(email))
                {
                    var read = await driver.GetHistoryAsync(user, goal.Id, first, last);
                    Assert.Equal(HistoryOutcome.Found, read.Outcome);
                    Assert.NotNull(read.History);
                    Assert.All(read.History.Entries, entry =>
                    {
                        Assert.Empty(entry.Records);
                        Assert.Null(entry.Note);
                    });
                }
            }

            return;
        }

        await checks.HistoryAsync(sentence);
    }

    [Then(@"^at `([^`]+)` `([^`]+)` reads the history of ""([^""]+)"" from (" + D + ") to (" + D + "), which gives, in this order: (.+)$")]
    public async Task ThenTheUserReadsTheHistoryWhichGives(string time, string email, string name, string first, string last, string list)
    {
        await arrange.AtThenTimeAsync();
        driver.SetNow(F005Text.Instant(time));
        var goal = await arrange.DemoGoalAsync(email, name);
        var history = await arrange.HistoryAsync(goal, email, F005Text.Date(first), F005Text.Date(last));
        F005Entries.CheckOrdered(history.Entries, list, goal.Kind);
    }

    // ------------------------------------------------------------ recent notes

    [Then(@"^the (first|second) goal gives (.+)$")]
    public async Task ThenTheNthGoalGivesNotes(string ordinal, string body)
    {
        await arrange.AtThenTimeAsync();
        var notes = FoundNotes(ordinal + " goal");
        if (body == "no note")
        {
            Assert.Empty(notes);
            return;
        }

        var match = Regex.Match(body, @"^exactly (\d+) notes, in this order: (.+)$");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read \"{body}\".");
        }

        Assert.Equal(F005Text.Int(match.Groups[1].Value), notes.Count);
        F005Entries.CheckNotes(notes, match.Groups[2].Value);
    }

    [Then(@"^""([^""]+)"" gives exactly(?:, in this order)?: (.+)$")]
    public async Task ThenTheDemoGoalGivesExactly(string name, string list)
    {
        await arrange.AtThenTimeAsync();
        F005Entries.CheckNotes(FoundNotes(name), list);
    }

    [Then(@"^`([^`]+)`'s recent notes of ""([^""]+)"" are still exactly (.+)$")]
    public async Task ThenTheRecentNotesAreStill(string email, string name, string list)
    {
        await arrange.AtThenTimeAsync();
        var goal = await arrange.DemoGoalAsync(email, name);
        var read = await driver.GetRecentNotesAsync(await arrange.AccountAsync(email), goal.Id);
        Assert.Equal(RecentNotesOutcome.Found, read.Outcome);
        F005Entries.CheckNotes(read.Notes, list);
    }

    // ------------------------------------------------------------ slice 4's evaluation calendar

    [Then(@"^it has (\d+) commitments, (.+)$")]
    public async Task ThenItHasCommitments(int count, string list)
    {
        await arrange.AtThenTimeAsync();
        var calendar = checks.LastCalendar();
        Assert.Equal(GoalScheduleKind.CommitmentBased, calendar.Kind);
        var items = Regex.Matches(list, "(" + D + ") from `([^`]+)` to `([^`]+)`");
        Assert.Equal(count, items.Count);
        Assert.Equal(count, calendar.Commitments.Count);
        for (var index = 0; index < count; index++)
        {
            Assert.Equal(F005Text.Date(items[index].Groups[1].Value), calendar.Commitments[index].Date);
            Assert.Equal(F005Text.Instant(items[index].Groups[2].Value), calendar.Commitments[index].Starts);
            Assert.Equal(F005Text.Instant(items[index].Groups[3].Value), calendar.Commitments[index].Ends);
        }
    }

    [Then(@"^no entry has a status$")]
    public async Task ThenNoEntryHasAStatus()
    {
        await arrange.AtThenTimeAsync();
        var calendar = checks.LastCalendar();
        Assert.NotEmpty(calendar.Commitments);
        var types = new List<Type> { calendar.GetType() };
        types.AddRange(calendar.Commitments.Select(entry => entry.GetType()));
        types.AddRange(calendar.Periods.Select(entry => entry.GetType()));
        foreach (var type in types.Distinct())
        {
            var statuses = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property =>
                    property.Name.Contains("Status", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Contains("State", StringComparison.OrdinalIgnoreCase))
                .Select(property => property.Name)
                .ToList();
            Assert.True(statuses.Count == 0, $"{type.Name} gives a status: {string.Join(", ", statuses)}.");
        }
    }

    // ------------------------------------------------------------ helpers

    private IReadOnlyList<RecentNote> FoundNotes(string key)
    {
        var read = context.RecentNotes.TryGetValue(key, out var found)
            ? found
            : throw new InvalidOperationException($"The When step read no recent notes of \"{key}\".");
        Assert.Equal(RecentNotesOutcome.Found, read.Outcome);
        return read.Notes;
    }

    /// <summary>
    /// A line after "N entries, one for each date …": "D, facts" or "D: facts" (that entry), "each date from D1 to D2
    /// (except …), facts" or "each date from D1 to D2 (except …): facts", and "each of D1, D2 … and Dn: facts".
    /// </summary>
    private static void EntriesByDate(IReadOnlyList<HistoryEntry> entries, string line, GoalScheduleKind kind)
    {
        Match match;
        List<DateOnly> dates;
        string facts;
        if ((match = Regex.Match(line, "^each date from (" + D + ") to (" + D + ")(?: except (.+?))?(?:: |, )(.+)$")).Success)
        {
            var from = F005Text.Date(match.Groups[1].Value);
            var to = F005Text.Date(match.Groups[2].Value);
            List<DateOnly> except = match.Groups[3].Success ? F005Text.Dates(match.Groups[3].Value) : [];
            dates = Enumerable.Range(0, to.DayNumber - from.DayNumber + 1)
                .Select(from.AddDays)
                .Where(date => !except.Contains(date))
                .ToList();
            facts = match.Groups[4].Value;
        }
        else if ((match = Regex.Match(line, "^each of (.+?): (.+)$")).Success)
        {
            dates = F005Text.Dates(match.Groups[1].Value);
            facts = match.Groups[2].Value;
        }
        else
        {
            F005Entries.CheckFound(entries, line, kind);
            return;
        }

        Assert.NotEmpty(dates);
        var factChecks = F005Entries.Facts(facts);
        foreach (var date in dates)
        {
            var entry = Assert.Single(entries, item => item.FirstDate == date && item.LastDate == date);
            var view = F005View.Of(entry, kind);
            foreach (var check in factChecks)
            {
                check(view);
            }
        }
    }
}
