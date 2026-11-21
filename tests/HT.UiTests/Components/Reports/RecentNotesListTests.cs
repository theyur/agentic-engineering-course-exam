using Bunit;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Ui.Components.Reports;

namespace HT.UiTests.Components.Reports;

/// <summary>RecentNotesList (HP-003, design/pages/PageHomeSignedIn "Slice 6 state"): the Home's "Recent notes" card.</summary>
public sealed class RecentNotesListTests : BunitContext
{
    private static DashboardNote NoteOf(string goal, DateOnly first, DateOnly last, string note) => new()
    {
        GoalId = new GoalId(Guid.NewGuid()), GoalName = goal, FirstDate = first, LastDate = last, Note = note, State = EvaluationState.Missed,
    };

    [Fact]
    public void Each_note_is_in_quotes_with_its_goal_and_date_under_it()
    {
        var cut = Render<RecentNotesList>(p => p.Add(x => x.Notes, new[]
        {
            NoteOf("Coffee, at most 2 cups a day", new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 16), "Birthday cake at work."),
            NoteOf("Running, 2 times a week", new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 15), "Knee was sore."),
        }));

        Assert.Equal("Recent notes", cut.Find("section.ht-card > h2.ht-card__title").TextContent);
        var items = cut.FindAll("section.ht-card > .ht-goal");
        Assert.Equal(
            [
                ("\"Birthday cake at work.\"Coffee, at most 2 cups a day · Mon 16 Nov", "Coffee, at most 2 cups a day · Mon 16 Nov"),
                ("\"Knee was sore.\"Running, 2 times a week · 9 – 15 Nov", "Running, 2 times a week · 9 – 15 Nov"),
            ],
            items.Select(i => (i.TextContent, i.QuerySelector(".ht-goal__meta")!.TextContent)));
    }

    [Fact]
    public void Nothing_is_rendered_without_a_note()
    {
        var cut = Render<RecentNotesList>(p => p.Add(x => x.Notes, Array.Empty<DashboardNote>()));

        Assert.Equal(string.Empty, cut.Markup.Trim());
    }
}
