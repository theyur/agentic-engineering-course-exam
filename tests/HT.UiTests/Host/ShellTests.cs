using System.Net;

namespace HT.UiTests.Host;

/// <summary>US5-AS1, AS2, AS6, AS7, AS10, HP-010: every page has the shell; Home and Privacy need no sign-in.</summary>
[Collection(HostCollection.Name)]
public sealed class ShellTests(HostFixture host)
{
    private const string Password = "walking42";

    [Fact]
    public async Task US5_AS1_AS10_an_anonymous_home_has_sign_in_register_and_the_footer_privacy_link()
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri("/", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"/sign-in\">Sign in</a>", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/register\">Register</a>", html, StringComparison.Ordinal);
        Assert.Contains("<a href=\"/privacy\">Privacy</a>", html, StringComparison.Ordinal);
        Assert.Contains("Keep the habits you choose", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Account menu for", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US5_AS7_privacy_opens_without_a_cookie()
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri("/privacy", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<h1>Privacy</h1>", html, StringComparison.Ordinal);
        Assert.Contains("What stays private", html, StringComparison.Ordinal);
        Assert.Contains("Habit Tracker · Agentic Engineering course project", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/sign-in")]
    [InlineData("/register")]
    [InlineData("/privacy")]
    [InlineData("/no-such-page")]
    public async Task HP_010_every_page_has_the_navigation_and_the_footer(string path)
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri(path, UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("<a class=\"ht-nav__brand\" href=\"/\">Habit Tracker</a>", html, StringComparison.Ordinal);
        Assert.Contains("<footer class=\"ht-footer\">", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Send feedback", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US5_AS2_a_signed_in_home_has_today_and_the_avatar_and_no_sign_in_or_register()
    {
        await host.CreateAccountAsync("shell.user@example.com", Password);
        using var browser = host.CreateBrowser();
        var form = await browser.GetFormAsync("/sign-in", "sign-in");
        using (var signedIn = await browser.PostFormAsync("/sign-in", form,
                   [new("Input.Email", "shell.user@example.com"), new("Input.Password", Password)]))
        {
            Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
        }

        using var response = await browser.GetAsync(new Uri("/", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(">Today</a>", html, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Account menu for shell.user@example.com\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"/sign-in\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"/register\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Keep the habits you choose", html, StringComparison.Ordinal);
    }
}
