using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using HT.Ui.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Components.Layout;

/// <summary>US5-AS1 to AS5, HP-010, HP-014: the items of each audience and the behaviour of both menus.</summary>
public sealed class SiteNavTests : BunitContext
{
    private const string Email = "ana@example.com";

    // Links of later slices (R7, ADR 0008); none of them may appear in slice 1.
    private static readonly string[] LaterLinks = ["Admin", "Catalog", "Community", "Goals", "New goal", "Send feedback"];

    private readonly BunitAuthorizationContext _authorization;

    public SiteNavTests()
    {
        _authorization = AddAuthorization();
        Services.AddSingleton<AntiforgeryStateProvider>(new FixedAntiforgery());
    }

    [Fact]
    public void US5_AS1_a_visitor_sees_the_product_name_sign_in_and_register_only()
    {
        var cut = Render<SiteNav>();

        var brand = cut.Find("a.ht-nav__brand");
        Assert.Equal("Habit Tracker", brand.TextContent);
        Assert.Equal("/", brand.GetAttribute("href"));
        Assert.Equal(["Sign in", "Register"], Texts(cut.FindAll(".ht-nav__end > a")));
        Assert.Equal("/sign-in", cut.Find(".ht-nav__end > a.ht-btn--ghost").GetAttribute("href"));
        Assert.Equal("/register", cut.Find(".ht-nav__end > a.ht-btn--primary").GetAttribute("href"));
        Assert.Empty(cut.FindAll(".ht-nav__links"));
        Assert.Empty(cut.FindAll(".ht-avatar"));
        AssertAbsent(cut, [.. LaterLinks, "Today", "Profile", "Sign out"]);
    }

    [Theory]
    [InlineData("RegisteredUser")]
    [InlineData("Administrator")]
    public void US5_AS2_AS3_a_signed_in_user_sees_today_and_the_avatar_menu_with_profile_and_sign_out(string role)
    {
        SignIn(role);
        var cut = Render<SiteNav>();

        Assert.Equal("Habit Tracker", cut.Find("a.ht-nav__brand").TextContent);
        var today = Assert.Single(cut.FindAll(".ht-nav__links a"));
        Assert.Equal("Today", today.TextContent);
        Assert.Equal("/", today.GetAttribute("href"));

        cut.Find(AccountButton).Click();

        Assert.Equal(["Profile", "Sign out"], Texts(cut.FindAll(".ht-popover a, .ht-popover button")));
        Assert.Equal("/profile", cut.Find(".ht-popover a").GetAttribute("href"));
        AssertAbsent(cut, [.. LaterLinks, "Sign in", "Register"]);
    }

    [Fact]
    public void HP_014_the_avatar_shows_the_first_letter_of_the_email_in_upper_case()
    {
        SignIn("RegisteredUser");
        var cut = Render<SiteNav>();

        var avatar = cut.Find(AccountButton);
        Assert.Equal("A", avatar.TextContent);
        Assert.Equal("Account menu for ana@example.com", avatar.GetAttribute("aria-label"));
        Assert.Equal("true", avatar.GetAttribute("aria-haspopup"));
        Assert.Equal(SiteNav.AccountMenuId, avatar.GetAttribute("aria-controls"));
    }

    [Fact]
    public void US5_AS4_the_narrow_menu_of_a_visitor_lists_sign_in_and_register()
    {
        var cut = Render<SiteNav>();
        var wide = Texts(cut.FindAll(".ht-nav__end > a"));

        cut.Find(MobileButton).Click();

        Assert.Equal(wide, Texts(cut.FindAll(MobileItems)));
        Assert.Equal(["/sign-in", "/register"], cut.FindAll($"#{SiteNav.MobileMenuId} a").Select(a => a.GetAttribute("href")));
    }

    [Theory]
    [InlineData("RegisteredUser")]
    [InlineData("Administrator")]
    public void US5_AS4_the_narrow_menu_of_a_signed_in_user_lists_the_same_items_as_the_wide_navigation(string role)
    {
        SignIn(role);
        var cut = Render<SiteNav>();
        cut.Find(AccountButton).Click();
        var wide = Texts(cut.FindAll(".ht-nav__links a")).Concat(Texts(cut.FindAll(".ht-popover a, .ht-popover button"))).ToList();

        cut.Find(MobileButton).Click();

        Assert.Equal(["Today", "Profile", "Sign out"], wide);
        Assert.Equal(wide, Texts(cut.FindAll(MobileItems)));
        AssertAbsent(cut, [.. LaterLinks, "Sign in", "Register"]);
    }

    [Theory]
    [InlineData(AccountButton, "Escape")]
    [InlineData(AccountButton, "Outside")]
    [InlineData(AccountButton, "Item")]
    [InlineData(MobileButton, "Escape")]
    [InlineData(MobileButton, "Outside")]
    [InlineData(MobileButton, "Item")]
    public void US5_AS5_a_menu_closes_on_esc_an_outside_click_or_an_item_and_focus_returns_to_its_button(
        string button, string way)
    {
        SignIn("RegisteredUser");
        var cut = Render<SiteNav>();
        var menu = button == AccountButton ? "#" + SiteNav.AccountMenuId : "#" + SiteNav.MobileMenuId;
        // bUnit writes the element reference into the markup of the first render only.
        var buttonReference = cut.Find(button).GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(buttonReference));
        cut.Find(button).Click();
        Assert.Single(cut.FindAll(menu));

        switch (way)
        {
            case "Escape":
                cut.Find(menu).KeyDown(new KeyboardEventArgs { Key = "Escape" });
                break;
            case "Outside":
                cut.Find(".ht-clickaway").Click();
                break;
            default:
                cut.Find($"{menu} a").Click();
                break;
        }

        Assert.Empty(cut.FindAll(menu));
        Assert.Empty(cut.FindAll(".ht-clickaway"));
        Assert.Equal("false", cut.Find(button).GetAttribute("aria-expanded"));
        var focused = Assert.IsType<ElementReference>(JSInterop.VerifyFocusAsyncInvoke().Arguments[0]);
        Assert.Equal(buttonReference, focused.Id);
    }

    [Fact]
    public void Another_key_leaves_the_menu_open()
    {
        SignIn("RegisteredUser");
        var cut = Render<SiteNav>();
        cut.Find(AccountButton).Click();

        cut.Find("#" + SiteNav.AccountMenuId).KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Single(cut.FindAll("#" + SiteNav.AccountMenuId));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Aria_expanded_follows_the_menu_state(bool signedIn)
    {
        if (signedIn)
        {
            SignIn("RegisteredUser");
        }

        var cut = Render<SiteNav>();
        var button = cut.Find(MobileButton);
        Assert.Equal("false", button.GetAttribute("aria-expanded"));
        Assert.Equal("Menu", button.GetAttribute("aria-label"));
        Assert.Equal(SiteNav.MobileMenuId, button.GetAttribute("aria-controls"));

        cut.Find(MobileButton).Click();
        Assert.Equal("true", cut.Find(MobileButton).GetAttribute("aria-expanded"));
        Assert.Equal("Close menu", cut.Find(MobileButton).GetAttribute("aria-label"));

        cut.Find(MobileButton).Click();
        Assert.Equal("false", cut.Find(MobileButton).GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("#" + SiteNav.MobileMenuId));
    }

    [Fact]
    public void Opening_one_menu_closes_the_other()
    {
        SignIn("RegisteredUser");
        var cut = Render<SiteNav>();

        cut.Find(AccountButton).Click();
        cut.Find(MobileButton).Click();

        Assert.Empty(cut.FindAll("#" + SiteNav.AccountMenuId));
        Assert.Equal("false", cut.Find(AccountButton).GetAttribute("aria-expanded"));
        Assert.Single(cut.FindAll("#" + SiteNav.MobileMenuId));
    }

    [Theory]
    [InlineData(AccountButton, SiteNav.AccountMenuId)]
    [InlineData(MobileButton, SiteNav.MobileMenuId)]
    public void HP_010_sign_out_is_a_form_posting_to_the_sign_out_endpoint_with_an_antiforgery_field(
        string button, string menuId)
    {
        SignIn("RegisteredUser");
        var cut = Render<SiteNav>();

        cut.Find(button).Click();

        var form = cut.Find($"#{menuId} form");
        Assert.Equal("post", form.GetAttribute("method"));
        Assert.Equal("/account/sign-out", form.GetAttribute("action"));
        Assert.Single(form.QuerySelectorAll("input[type=hidden][name=__RequestVerificationToken]"));
        var submit = form.QuerySelector("button[type=submit]");
        Assert.Equal("Sign out", submit!.TextContent);
        Assert.DoesNotContain(cut.FindAll("a"), a => a.TextContent.Trim() == "Sign out");
    }

    [Fact]
    public void Today_is_marked_current_on_home_only_and_navigating_closes_the_menu()
    {
        SignIn("RegisteredUser");
        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = Render<SiteNav>();
        Assert.Equal("page", cut.Find(".ht-nav__links a").GetAttribute("aria-current"));
        cut.Find(MobileButton).Click();

        navigation.NavigateTo("/privacy");

        cut.WaitForAssertion(() =>
        {
            Assert.Null(cut.Find(".ht-nav__links a").GetAttribute("aria-current"));
            Assert.Empty(cut.FindAll("#" + SiteNav.MobileMenuId));
        });
    }

    [Fact]
    public void HP_007_a_session_that_ends_while_the_page_is_open_reloads_the_page()
    {
        SignIn("RegisteredUser");
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        navigation.NavigateTo("/profile");
        var cut = Render<SiteNav>();
        Assert.DoesNotContain(navigation.History, entry => entry.Options.ForceLoad);

        _authorization.SetNotAuthorized();

        cut.WaitForAssertion(() =>
        {
            var reload = navigation.History.First();
            Assert.True(reload.Options.ForceLoad);
            Assert.Equal(navigation.BaseUri + "profile", reload.Uri);
        });
    }

    [Fact]
    public void A_signed_in_page_is_not_reloaded_while_the_session_stays_valid()
    {
        SignIn("RegisteredUser");
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        var cut = Render<SiteNav>();

        cut.Render();
        cut.Find(AccountButton).Click();

        Assert.DoesNotContain(navigation.History, entry => entry.Options.ForceLoad);
    }

    private const string AccountButton = "#" + SiteNav.AccountButtonId;
    private const string MobileButton = "#" + SiteNav.MobileButtonId;
    private const string MobileItems = "#" + SiteNav.MobileMenuId + " a, #" + SiteNav.MobileMenuId + " button";

    private void SignIn(string role)
    {
        _authorization.SetAuthorized(Email);
        _authorization.SetRoles(role);
    }

    private static List<string> Texts(IEnumerable<IElement> elements) =>
        elements.Select(e => e.TextContent.Trim()).ToList();

    private static void AssertAbsent(IRenderedComponent<SiteNav> cut, IEnumerable<string> labels)
    {
        var items = Texts(cut.FindAll("a, button"));
        foreach (var label in labels)
        {
            Assert.DoesNotContain(items, item => item.Contains(label, StringComparison.Ordinal));
        }
    }

    /// <summary>bUnit's provider gives no token; this one gives a fixed token, so the token field renders.</summary>
    private sealed class FixedAntiforgery : AntiforgeryStateProvider
    {
        public override AntiforgeryRequestToken GetAntiforgeryToken() => new("test-token", "__RequestVerificationToken");
    }
}
