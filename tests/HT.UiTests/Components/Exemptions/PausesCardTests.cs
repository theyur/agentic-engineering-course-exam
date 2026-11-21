using AngleSharp.Dom;
using Bunit;
using HT.Application.Exemptions;
using HT.Domain.Exemptions;
using HT.Ui.Components.Exemptions;
using Microsoft.AspNetCore.Components.Web;

namespace HT.UiTests.Components.Exemptions;

/// <summary>
/// The goal page's Pauses card (HP-001, FR-011, FR-012; design/pages/PageGoalDetail, slice 7 state; T061), rendered from
/// the pause views the page reads: one item per pause, latest first day first as given, its range and reason, "Resume
/// now" on the current pause, "Cancel pause" on an upcoming one, no action on a past one, and no card with no pause.
/// </summary>
public sealed class PausesCardTests : BunitContext
{
    private static readonly PauseView Upcoming = View(new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 29), PauseTiming.Upcoming, "Family visit.");
    private static readonly PauseView Current = View(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 22), PauseTiming.Current, null);
    private static readonly PauseView Past = View(new DateOnly(2026, 11, 13), new DateOnly(2026, 11, 15), PauseTiming.Past, "Ill");

    private static PauseView View(DateOnly first, DateOnly last, PauseTiming timing, string? reason) => new()
    {
        Id = new PauseId(Guid.NewGuid()),
        FirstDay = first,
        LastDay = last,
        Reason = reason,
        Timing = timing,
    };

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private IRenderedComponent<PausesCard> RenderCard(
        IReadOnlyList<PauseView> pauses, Action<PauseView>? resume = null, Action<PauseView>? cancel = null, string? refusal = null) =>
        Render<PausesCard>(p => p
            .Add(x => x.Pauses, pauses)
            .Add(x => x.OnResume, resume ?? (_ => { }))
            .Add(x => x.OnCancel, cancel ?? (_ => { }))
            .Add(x => x.Refusal, refusal));

    [Fact]
    public void HP_001_one_item_per_pause_with_its_range_its_reason_and_its_action()
    {
        var cut = RenderCard([Upcoming, Current, Past]);

        var card = cut.Find("section.ht-card");
        Assert.Equal("Pauses", card.QuerySelector("h2.ht-card__title")!.TextContent);
        var items = cut.FindAll(".ht-goal");
        Assert.Equal(3, items.Count);
        Assert.Equal(
            ["Mon 23 Nov – Sun 29 Nov", "Mon 16 Nov – Sun 22 Nov", "Fri 13 Nov – Sun 15 Nov"],
            items.Select(item => Text(item.QuerySelector(".ht-goal__top .ht-goal__name")!)));
        Assert.Equal("Family visit.", Text(items[0].QuerySelector(".ht-goal__meta.ht-muted")!));
        Assert.Null(items[1].QuerySelector(".ht-goal__meta"));
        Assert.Equal("Ill", Text(items[2].QuerySelector(".ht-goal__meta.ht-muted")!));

        var cancel = Assert.Single(items[0].QuerySelectorAll("button"));
        Assert.Equal("Cancel pause", cancel.TextContent);
        Assert.Equal("Cancel pause, Mon 23 Nov – Sun 29 Nov", cancel.GetAttribute("aria-label"));
        Assert.Equal(["ht-btn", "ht-btn--ghost", "ht-btn--sm"], cancel.ClassList.ToArray());
        Assert.Null(cancel.GetAttribute("aria-haspopup"));
        Assert.Equal("ht-spacer", cancel.PreviousElementSibling!.ClassName);

        var resume = Assert.Single(items[1].QuerySelectorAll("button"));
        Assert.Equal("Resume now", resume.TextContent);
        Assert.Equal("Resume now, Mon 16 Nov – Sun 22 Nov", resume.GetAttribute("aria-label"));
        Assert.Equal(["ht-btn", "ht-btn--ghost", "ht-btn--sm"], resume.ClassList.ToArray());
        Assert.Null(resume.GetAttribute("aria-haspopup"));

        Assert.Empty(items[2].QuerySelectorAll("button"));
        Assert.Empty(items[2].QuerySelectorAll(".ht-spacer"));
        Assert.Empty(cut.FindAll(".ht-status"));
        Assert.Empty(cut.FindAll("[role=alert]"));
    }

    [Fact]
    public void HP_001_no_card_with_no_pause()
    {
        var cut = RenderCard([]);

        Assert.Empty(cut.FindAll("section"));
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }

    [Fact]
    public async Task FR_010_Resume_now_raises_the_resume_callback_with_its_pause()
    {
        PauseView? resumed = null;
        var cut = RenderCard([Upcoming, Current, Past], resume: pause => resumed = pause);

        await cut.Find("button[aria-label^='Resume now']").ClickAsync(new MouseEventArgs());

        Assert.Same(Current, resumed);
    }

    [Fact]
    public async Task FR_011_Cancel_pause_raises_the_cancel_callback_with_its_pause()
    {
        PauseView? cancelled = null;
        var cut = RenderCard([Upcoming, Current, Past], cancel: pause => cancelled = pause);

        await cut.Find("button[aria-label^='Cancel pause']").ClickAsync(new MouseEventArgs());

        Assert.Same(Upcoming, cancelled);
    }

    [Fact]
    public void HP_001_a_refusal_shows_as_an_alert_under_the_card_title()
    {
        var cut = RenderCard([Past], refusal: "This pause does not exist.");

        var alert = cut.Find("p.ht-field__error[role=alert]");
        Assert.Equal("This pause does not exist.", alert.TextContent);
        Assert.Equal("ht-card__title", alert.PreviousElementSibling!.ClassName);
    }
}
