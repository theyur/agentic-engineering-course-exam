using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Reports;

namespace HT.AcceptanceTests.Support.F008;

/// <summary>The assertions of the F008 Then steps: each checks exactly what the item writes.</summary>
public static class F008Checks
{
    // ------------------------------------------------------------ history entries

    /// <summary>Every entry the list names is in the entries as written; no entry covers a date the list says has none.</summary>
    public static void Entries(IReadOnlyList<HistoryEntry> entries, F008Text.EntryList expected)
    {
        foreach (var spec in expected.Entries)
        {
            Entry(entries, spec);
        }

        foreach (var (first, last) in expected.NoEntry)
        {
            Assert.DoesNotContain(entries, entry => entry.FirstDate <= last && entry.LastDate >= first);
        }
    }

    public static void Entry(IReadOnlyList<HistoryEntry> entries, F008Text.EntrySpec spec)
    {
        foreach (var (first, last) in spec.Spans)
        {
            var entry = Assert.Single(entries, candidate => candidate.FirstDate == first && candidate.LastDate == last);
            Entry(entry, spec);
        }
    }

    public static void Entry(HistoryEntry entry, F008Text.EntrySpec spec)
    {
        var label = $"{spec.Text} ({entry.FirstDate:yyyy-MM-dd})";
        Assert.True(spec.State == entry.State, $"{label}: the state is {entry.State}, expected {spec.State}.");
        if (spec.Records is not null)
        {
            var actual = entry.Records.Select(record => (record.Answer, record.Amount)).ToList();
            Assert.True(
                spec.Records.Count == actual.Count,
                $"{label}: {actual.Count} records, expected {spec.Records.Count}.");
            Assert.Equal(
                spec.Records.OrderBy(value => value.Answer).ThenBy(value => value.Amount).ToList(),
                actual.OrderBy(value => value.Answer).ThenBy(value => value.Amount).ToList());
        }

        if (spec.NoGraceEnd == true)
        {
            Assert.True(entry.GraceEnds is null, $"{label}: a grace end {entry.GraceEnds} is given, expected none.");
        }

        if (spec.GraceEnds is { } grace)
        {
            Assert.Equal(grace, entry.GraceEnds);
        }

        if (spec.Correction is { } correction)
        {
            Assert.True(correction == entry.IsCorrection, $"{label}: correction is {entry.IsCorrection}, expected {correction}.");
        }

        if (spec.ReasonGiven)
        {
            Assert.Equal(spec.Reason, entry.ExcusalReason);
        }

        if (spec.Removable is { } removable)
        {
            Assert.True(removable == entry.CanRemoveExcusal, $"{label}: removable is {entry.CanRemoveExcusal}, expected {removable}.");
        }
    }

    // ------------------------------------------------------------ pauses

    /// <summary>
    /// The pauses read are exactly the pauses written, in that order, each with its days, reason and timing; a pause a
    /// "paused" result of this item gave carries the identifier that result gave (acceptance.md, "Use cases").
    /// </summary>
    public static void Pauses(IReadOnlyList<PauseView> actual, IReadOnlyList<F008Text.PauseSpec> expected, IEnumerable<F008Pause> created)
    {
        Assert.Equal(
            expected.Select(pause => (pause.FirstDay, pause.LastDay, pause.Reason, pause.Timing)).ToList(),
            actual.Select(pause => (pause.FirstDay, pause.LastDay, pause.Reason, pause.Timing)).ToList());

        var createdList = created.ToList();
        foreach (var view in actual)
        {
            var origin = createdList.Where(pause => pause.FirstDay == view.FirstDay).ToList();
            if (origin.Count == 1)
            {
                Assert.Equal(origin[0].Id, view.Id);
            }
        }
    }

    // ------------------------------------------------------------ trend

    public static void Bar(TrendBar bar, F008Text.BarSpec spec, int? target)
    {
        Assert.Equal((spec.First, spec.Last), (bar.FirstDate, bar.LastDate));
        Assert.True(spec.Value == bar.Value, $"{spec.Text}: the value is {bar.Value?.ToString() ?? "none"}, expected {spec.Value?.ToString() ?? "none"}.");
        Assert.True(spec.Mark == bar.Mark, $"{spec.Text}: the mark is {bar.Mark?.ToString() ?? "none"}, expected {spec.Mark?.ToString() ?? "none"}.");
        if ((spec.Target ?? target) is { } expectedTarget)
        {
            Assert.True(expectedTarget == bar.Target, $"{spec.Text}: the target is {bar.Target}, expected {expectedTarget}.");
        }
    }

    /// <summary>The trend's bars are exactly the bars written, in that order.</summary>
    public static void Trend(TrendView trend, IReadOnlyList<F008Text.BarSpec> expected, int? target)
    {
        Assert.Equal(
            expected.Select(bar => (bar.First, bar.Last)).ToList(),
            trend.Bars.Select(bar => (bar.FirstDate, bar.LastDate)).ToList());
        for (var index = 0; index < expected.Count; index++)
        {
            Bar(trend.Bars[index], expected[index], target);
        }
    }

    /// <summary>The trend has each bar written (by its dates), as written.</summary>
    public static void TrendContains(TrendView trend, IReadOnlyList<F008Text.BarSpec> expected)
    {
        foreach (var spec in expected)
        {
            var bar = Assert.Single(trend.Bars, candidate => candidate.FirstDate == spec.First && candidate.LastDate == spec.Last);
            Bar(bar, spec, null);
        }
    }

    // ------------------------------------------------------------ dashboard Today list

    public static void Today(IReadOnlyList<TodayEntry> today, F008Text.TodayList expected)
    {
        foreach (var spec in expected.Contains)
        {
            Assert.True(
                today.Any(entry =>
                    entry.GoalName == spec.Goal
                    && entry.Entry.FirstDate == spec.First
                    && entry.Entry.LastDate == spec.Last
                    && entry.Entry.State == spec.State),
                $"The Today list has no entry {spec.Text}; it has: {Describe(today)}.");
        }

        foreach (var (first, last) in expected.NoEntryFor)
        {
            Assert.DoesNotContain(today, entry => entry.Entry.FirstDate <= last && entry.Entry.LastDate >= first);
        }

        foreach (var (goal, first, last) in expected.NoEntryOfGoalFor)
        {
            Assert.DoesNotContain(today, entry => entry.GoalName == goal && entry.Entry.FirstDate <= last && entry.Entry.LastDate >= first);
        }

        foreach (var (goal, states) in expected.NoStatesOfGoal)
        {
            Assert.DoesNotContain(today, entry => entry.GoalName == goal && states.Contains(entry.Entry.State));
        }
    }

    private static string Describe(IReadOnlyList<TodayEntry> today) =>
        string.Join("; ", today.Select(entry => $"({entry.GoalName}, {entry.Entry.FirstDate:yyyy-MM-dd} – {entry.Entry.LastDate:yyyy-MM-dd}, {entry.Entry.State})"));

    // ------------------------------------------------------------ recent notes

    public static void RecentNotes(IReadOnlyList<RecentNote> actual, IReadOnlyList<F008Text.NoteSpec> expected) =>
        Assert.Equal(
            expected.Select(note => (note.First, note.Last, note.Note, note.State)).ToList(),
            actual.Select(note => (note.FirstDate, note.LastDate, note.Note, note.State)).ToList());

    public static void DashboardNotes(IReadOnlyList<DashboardNote> actual, IReadOnlyList<F008Text.NoteSpec> expected) =>
        Assert.Equal(
            expected.Select(note => (note.Goal, note.First, note.Last, note.Note, note.State)).ToList(),
            actual.Select(note => ((string?)note.GoalName, note.FirstDate, note.LastDate, note.Note, note.State)).ToList());

    // ------------------------------------------------------------ overview

    /// <summary>"`G` paused until D" or "`G` not paused" for a goal of the overview, found by its name.</summary>
    public static void OverviewPaused(GoalsOverview overview, string goalName, DateOnly? pausedUntil)
    {
        var goal = Assert.Single(overview.Goals, candidate => candidate.Item.Name == goalName);
        Assert.Equal(pausedUntil, goal.PausedUntil);
    }

    // ------------------------------------------------------------ recent results

    public static void States(IReadOnlyList<ResultView> results, IReadOnlyList<(DateOnly Date, EvaluationState State)> expected) =>
        Assert.Equal(
            expected.Select(result => (result.Date, result.Date, result.State)).ToList(),
            results.Select(result => (result.FirstDate, result.LastDate, result.State)).ToList());
}
