using AngleSharp.Dom;
using Bunit;
using HT.Application.Reports;
using HT.Domain.Goals;
using HT.Domain.Reports;
using HT.Ui.Components.Goals;

namespace HT.UiTests.Components.GoalForms;

/// <summary>
/// Slice 8 (T090, T092; HP-001, design/pages/PageGoalDetail "Slice 8 state"): the Earlier attempts card over attempt views
/// as <c>AttemptService.GetAttemptsAsync</c> gives them, the latest first. The texts come from <see cref="ReportText"/>;
/// the figures are the views' own.
/// </summary>
public sealed class EarlierAttemptsCardTests : BunitContext
{
    private static readonly Guid Goal = Guid.Parse("6f9a3c1e-0000-4000-8000-0000000000cc");

    private static GoalStatsView Stats(string consistency) => new()
    {
        Consistency = new Consistency(0, 0),
        ConsistencyText = consistency,
        Unit = StreakUnit.Commitments,
        CurrentStreak = 0,
        CurrentStreakText = "0 commitments",
        LongestStreak = 0,
        LongestStreakText = "0 commitments",
    };

    private static AttemptView Latest(int number, string name) => new()
    {
        Number = number, Name = name, StartDate = new DateOnly(2026, 11, 23), IsLatest = true, HasNoDate = false, Stats = Stats("N/A"),
    };

    private static AttemptView Ended(
        int number, string name, DateOnly start, DateOnly last, Outcome outcome, string consistency, string? note = null, bool hasNoDate = false) => new()
    {
        Number = number,
        Name = name,
        StartDate = start,
        IsLatest = false,
        EndingDate = last.AddDays(1),
        LastDate = last,
        Outcome = outcome,
        EndingNote = note,
        HasNoDate = hasNoDate,
        Stats = Stats(consistency),
    };

    private IRenderedComponent<EarlierAttemptsCard> RenderCard(string goalName, params AttemptView[] attempts) =>
        Render<EarlierAttemptsCard>(p => p
            .Add(x => x.Attempts, attempts)
            .Add(x => x.GoalId, Goal)
            .Add(x => x.GoalName, goalName));

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    [Fact]
    public void HP_001_an_earlier_attempt_has_its_span_as_a_link_its_Tag_its_consistency_its_name_when_it_differs_and_its_note()
    {
        var cut = RenderCard("Meditation, 5 min daily",
            Latest(2, "Meditation, 5 min daily"),
            Ended(1, "Meditation, 10 min daily", new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 15), Outcome.Abandoned, "85% (12 of 14)", "Too busy this month."));

        var card = cut.Find("section.ht-card");
        Assert.Equal("Earlier attempts", card.QuerySelector("h2.ht-card__title")!.TextContent);
        var item = Assert.Single(card.QuerySelectorAll(".ht-goal"));
        var link = item.QuerySelector(".ht-goal__top > a.ht-goal__name")!;
        Assert.Equal(("Attempt 1 · 2 Nov – 15 Nov", $"/goals/{Goal}/attempts/1"), (link.TextContent, link.GetAttribute("href")));
        Assert.Equal(["Attempt 1 · 2 Nov – 15 Nov", "Abandoned", "85% (12 of 14)"], item.QuerySelector(".ht-goal__top")!.Children.Select(Text));
        Assert.Equal("ht-chip", item.QuerySelector(".ht-goal__top > span")!.ClassName);
        Assert.Equal("ht-goal__pct", item.QuerySelector(".ht-goal__top > span:last-child")!.ClassName);
        var metas = item.QuerySelectorAll(".ht-goal__meta");
        Assert.Equal([("ht-goal__meta", "Meditation, 10 min daily"), ("ht-goal__meta ht-muted", "Too busy this month.")],
            metas.Select(m => (m.ClassName!, m.TextContent)));
    }

    [Fact]
    public void HP_001_the_name_is_left_out_when_it_equals_the_goals_name_and_the_note_when_there_is_none()
    {
        var cut = RenderCard("Sugar, at most 1 sweet a day",
            Latest(2, "Sugar, at most 1 sweet a day"),
            Ended(1, "Sugar, at most 1 sweet a day", new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 13), Outcome.Achieved, "100% (12 of 12)"));

        var item = cut.Find(".ht-goal");
        Assert.Equal(["Attempt 1 · 2 Nov – 13 Nov", "Achieved", "100% (12 of 12)"], item.QuerySelector(".ht-goal__top")!.Children.Select(Text));
        Assert.Empty(item.QuerySelectorAll(".ht-goal__meta"));
    }

    [Fact]
    public void HP_001_an_attempt_with_no_date_says_ended_before_it_began_with_its_Tag_and_N_A()
    {
        var cut = RenderCard("Meditation, 10 min daily",
            Latest(2, "Meditation, 10 min daily"),
            Ended(1, "Meditation, 10 min daily", new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 16), Outcome.NoLongerRelevant, "N/A", hasNoDate: true));

        var link = cut.Find(".ht-goal__top > a.ht-goal__name");
        Assert.Equal(("Attempt 1 · ended before it began", $"/goals/{Goal}/attempts/1"), (link.TextContent, link.GetAttribute("href")));
        Assert.Equal(["Attempt 1 · ended before it began", "No longer relevant", "N/A"], cut.Find(".ht-goal__top").Children.Select(Text));
    }

    [Fact]
    public void HP_001_every_earlier_attempt_is_listed_latest_first_and_the_latest_attempt_never()
    {
        var cut = RenderCard("Yoga, 3 days a week",
            Latest(3, "Yoga, 3 days a week"),
            Ended(2, "Yoga, 3 days a week", new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 30), Outcome.Abandoned, "50% (6 of 12)"),
            Ended(1, "Yoga, 3 days a week", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 25), Outcome.Achieved, "100% (11 of 11)"));

        Assert.Equal(["Attempt 2 · 5 Oct – 30 Oct", "Attempt 1 · 1 Sep – 25 Sep"], cut.FindAll("a.ht-goal__name").Select(a => a.TextContent));
        Assert.DoesNotContain("Attempt 3", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_001_the_card_is_absent_with_one_attempt_whether_it_is_ended_or_not()
    {
        var running = RenderCard("Yoga, 3 days a week", Latest(1, "Yoga, 3 days a week"));
        var ended = RenderCard("Yoga, 3 days a week",
            Ended(1, "Yoga, 3 days a week", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 25), Outcome.Achieved, "100% (11 of 11)") with { IsLatest = true });

        Assert.Equal(string.Empty, running.Markup.Trim());
        Assert.Equal(string.Empty, ended.Markup.Trim());
        Assert.False(EarlierAttemptsCard.HasEarlier([Latest(1, "Yoga, 3 days a week")]));
    }
}
