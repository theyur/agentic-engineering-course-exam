using Bunit;
using HT.Ui.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace HT.UiTests.Components.Shared;

/// <summary>design/components/Dialog, HP-016: HtDialog's parts, its title ids and its focus handling.</summary>
public sealed class HtDialogTests : BunitContext
{
    private const string FirstButton = "#open-first";
    private const string SecondButton = "#open-second";

    private static string ReferenceOf(AngleSharp.Dom.IElement element)
    {
        // bUnit writes an element reference into the markup of the render that first shows the element.
        var reference = element.GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(reference));
        return reference;
    }

    private IReadOnlyList<string?> FocusedReferences(int times) =>
        JSInterop.VerifyFocusAsyncInvoke(times).Select(i => Assert.IsType<ElementReference>(i.Arguments[0]).Id).ToList();

    [Fact]
    public void A_closed_dialog_renders_nothing()
    {
        var cut = Render<DialogHost>();

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Empty(cut.FindAll(".ht-backdrop"));
    }

    [Fact]
    public void An_open_dialog_has_its_parts_and_is_labelled_by_its_title()
    {
        var cut = Render<DialogHost>();

        cut.Find(FirstButton).Click();

        var dialog = cut.Find(".ht-backdrop > .ht-dialog");
        Assert.Equal("dialog", dialog.GetAttribute("role"));
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        var title = dialog.QuerySelector(".ht-dialog__head h2")!;
        Assert.Equal("Delete this custom habit?", title.TextContent);
        Assert.False(string.IsNullOrEmpty(title.Id));
        Assert.Equal(title.Id, dialog.GetAttribute("aria-labelledby"));
        Assert.Equal("Cold shower · Private", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Equal("Close", dialog.QuerySelector("button.ht-dialog__close")!.GetAttribute("aria-label"));
        Assert.Equal("The habit is deleted for good. This cannot be undone.", dialog.QuerySelector(".ht-dialog__body p")!.TextContent);
        Assert.Equal(["Delete", "Cancel"], dialog.QuerySelectorAll(".ht-dialog__foot button").Select(b => b.TextContent));
    }

    [Fact]
    public void Without_a_subtitle_and_a_footer_neither_is_rendered()
    {
        var cut = Render<DialogHost>();

        cut.Find(SecondButton).Click();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Second", dialog.QuerySelector("h2")!.TextContent);
        Assert.Null(dialog.QuerySelector(".ht-dialog__sub"));
        Assert.Null(dialog.QuerySelector(".ht-dialog__foot"));
    }

    [Fact]
    public void Two_dialogs_on_one_page_have_different_title_ids()
    {
        var cut = Render<DialogHost>();

        cut.Find(FirstButton).Click();
        cut.Find(SecondButton).Click();

        var dialogs = cut.FindAll("[role=dialog]");
        Assert.Equal(2, dialogs.Count);
        var ids = dialogs.Select(d => d.QuerySelector("h2")!.Id).ToList();
        Assert.NotEqual(ids[0], ids[1]);
        Assert.All(dialogs, d => Assert.Equal(d.QuerySelector("h2")!.Id, d.GetAttribute("aria-labelledby")));
        Assert.Equal(2, cut.FindAll("[id]").Select(e => e.Id).Where(id => ids.Contains(id)).Count());
    }

    [Fact]
    public void Focus_moves_into_the_dialog_when_it_opens()
    {
        var cut = Render<DialogHost>();

        cut.Find(FirstButton).Click();

        var dialog = ReferenceOf(cut.Find("[role=dialog]"));
        Assert.Equal([dialog], FocusedReferences(1));
        Assert.Equal("-1", cut.Find("[role=dialog]").GetAttribute("tabindex"));
    }

    [Theory]
    [InlineData("Escape")]
    [InlineData("Close")]
    [InlineData("Cancel")]
    public void Esc_the_close_button_and_Cancel_close_it_and_focus_returns_to_the_opening_control(string way)
    {
        var cut = Render<DialogHost>();
        var opener = ReferenceOf(cut.Find(FirstButton));
        cut.Find(FirstButton).Click();
        var dialog = ReferenceOf(cut.Find("[role=dialog]"));

        switch (way)
        {
            case "Escape":
                cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
                break;
            case "Close":
                cut.Find("button.ht-dialog__close").Click();
                break;
            default:
                cut.Find("#first-cancel").Click();
                break;
        }

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal([dialog, opener], FocusedReferences(2));
    }

    [Fact]
    public void Another_key_leaves_the_dialog_open()
    {
        var cut = Render<DialogHost>();
        cut.Find(FirstButton).Click();

        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Single(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public void Each_dialog_returns_focus_to_its_own_opener()
    {
        var cut = Render<DialogHost>();
        var second = ReferenceOf(cut.Find(SecondButton));
        cut.Find(SecondButton).Click();

        cut.Find("button.ht-dialog__close").Click();

        Assert.Equal(second, FocusedReferences(2)[1]);
    }
}
