using Bunit;
using HT.Ui.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace HT.UiTests.Components.Shared;

/// <summary>StatTile (HP-003, HP-006, design/components/StatTile "Slice 6 state"): the filter button and the link form.</summary>
public sealed class StatTileTests : BunitContext
{
    private IRenderedComponent<StatTile> Button(bool pressed, Action? onToggle = null, string? modifier = null) =>
        Render<StatTile>(p => p
            .Add(x => x.Label, "Pending · still can record")
            .Add(x => x.Value, 5)
            .Add(x => x.Modifier, modifier)
            .Add(x => x.Pressed, pressed)
            .Add(x => x.OnToggle, () => onToggle?.Invoke()));

    [Fact]
    public void A_filter_tile_is_a_native_button_with_its_value_its_label_and_aria_pressed()
    {
        var cut = Button(pressed: false, modifier: "ht-tile--pending");

        var button = cut.Find("button");
        Assert.Equal(("button", "false"), (button.GetAttribute("type"), button.GetAttribute("aria-pressed")));
        Assert.Equal(["ht-card", "ht-tile", "ht-tile--pending"], button.ClassList);
        Assert.Equal(
            [("SPAN", "ht-tile__value", "5"), ("SPAN", "ht-tile__label", "Pending · still can record")],
            button.Children.Select(c => (c.TagName, c.ClassName, c.TextContent)));
        Assert.Empty(cut.FindAll("a, div"));
    }

    [Fact]
    public void A_pressed_tile_says_so_and_a_neutral_tile_has_no_modifier()
    {
        var cut = Button(pressed: true);

        Assert.Equal("true", cut.Find("button").GetAttribute("aria-pressed"));
        Assert.Equal(["ht-card", "ht-tile"], cut.Find("button").ClassList);
    }

    [Fact]
    public void Choosing_the_tile_raises_OnToggle()
    {
        // A native button: Enter and Space activate it as a click, so the click is the keyboard's path too.
        var toggled = 0;
        var cut = Button(pressed: false, () => toggled++);

        cut.Find("button").Click();
        cut.Find("button").Click();

        Assert.Equal(2, toggled);
    }

    [Fact]
    public void A_link_tile_is_an_anchor_with_the_same_two_spans()
    {
        var cut = Render<StatTile>(p => p.Add(x => x.Label, "Active goals").Add(x => x.Value, 3).Add(x => x.Href, "/goals"));

        var link = cut.Find("a");
        Assert.Equal("/goals", link.GetAttribute("href"));
        Assert.Equal(["ht-card", "ht-tile"], link.ClassList);
        Assert.Null(link.GetAttribute("aria-pressed"));
        Assert.Equal(["3", "Active goals"], link.Children.Select(c => c.TextContent));
        Assert.Empty(cut.FindAll("button"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FocusAsync_focuses_the_button_or_the_link(bool link)
    {
        var cut = link
            ? Render<StatTile>(p => p.Add(x => x.Label, "Active goals").Add(x => x.Value, 3).Add(x => x.Href, "/goals"))
            : Button(pressed: false);

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        var focused = Assert.IsType<ElementReference>(JSInterop.VerifyFocusAsyncInvoke().Arguments[0]);
        Assert.Equal(cut.Find(link ? "a" : "button").GetAttribute("blazor:elementReference"), focused.Id);
    }
}
