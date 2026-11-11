using System.Globalization;
using System.Net;
using HT.Domain.Accounts;

namespace HT.UiTests.Host;

[Collection(HostCollection.Name)]
public sealed class SignInPageTests(HostFixture host)
{
    private const string CookieName = ".AspNetCore.Identity.Application";
    private const string Password = "walking42";
    private static readonly TimeSpan Window = TimeSpan.FromDays(14);

    [Fact]
    public async Task With_keep_me_signed_in_the_cookie_expires_14_days_ahead()
    {
        await host.CreateAccountAsync("page.keep@example.com", Password);
        using var browser = host.CreateBrowser();

        using var response = await SignInAsync(browser, "page.keep@example.com", keepSignedIn: true);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        AssertExpiresAbout(response, host.Clock.GetUtcNow() + Window);
    }

    [Fact]
    public async Task Without_keep_me_signed_in_the_cookie_is_a_session_cookie()
    {
        await host.CreateAccountAsync("page.session@example.com", Password);
        using var browser = host.CreateBrowser();

        using var response = await SignInAsync(browser, "page.session@example.com", keepSignedIn: false);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var cookie = response.SetCookieAttributes(CookieName);
        Assert.NotNull(cookie);
        Assert.Contains("secure", cookie, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain(cookie, part => part.StartsWith("expires=", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(cookie, part => part.StartsWith("max-age=", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task HP_004_an_authenticated_request_re_issues_the_kept_cookie_with_a_full_window()
    {
        await host.CreateAccountAsync("page.sliding@example.com", Password);
        using var browser = host.CreateBrowser();
        using (var signedIn = await SignInAsync(browser, "page.sliding@example.com", keepSignedIn: true))
        {
            Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
        }

        host.Clock.Advance(TimeSpan.FromDays(8));
        using var response = await browser.GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertExpiresAbout(response, host.Clock.GetUtcNow() + Window);
    }

    [Theory]
    [InlineData("page.return1@example.com", "/profile", "https://localhost/profile")]
    [InlineData("page.return2@example.com", "//evil.example", "https://localhost/")]
    [InlineData("page.return3@example.com", "/\\evil.example", "https://localhost/")]
    [InlineData("page.return4@example.com", "https://evil.example", "https://localhost/")]
    [InlineData("page.return5@example.com", "", "https://localhost/")]
    public async Task HP_005_only_a_local_return_url_is_followed(string email, string returnUrl, string expected)
    {
        await host.CreateAccountAsync(email, Password);
        using var browser = host.CreateBrowser();

        using var response = await SignInAsync(browser, email, keepSignedIn: false, returnUrl);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expected, RedirectTarget(response, browser));
    }

    [Fact]
    public async Task The_form_posts_back_to_its_own_address_so_the_return_url_survives()
    {
        using var browser = host.CreateBrowser();

        var form = await browser.GetFormAsync("/sign-in?returnUrl=%2Fprofile", "sign-in");

        var start = form.Html.IndexOf("<form", StringComparison.Ordinal);
        var tag = form.Html[start..form.Html.IndexOf('>', start)];
        Assert.Contains("method=\"post\"", tag, StringComparison.Ordinal);
        Assert.Contains("action=\"/sign-in?returnUrl=%2Fprofile\"", tag, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_failed_sign_in_shows_the_message_and_keeps_the_email()
    {
        await host.CreateAccountAsync("page.fail@example.com", Password);
        using var browser = host.CreateBrowser();

        var form = await browser.GetFormAsync("/sign-in", "sign-in");
        using var response = await browser.PostFormAsync("/sign-in", form, Fields("page.fail@example.com", "walking43", false));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Email or password is not correct.", html, StringComparison.Ordinal);
        Assert.Contains("value=\"page.fail@example.com\"", html, StringComparison.Ordinal);
        Assert.Null(response.SetCookieAttributes(CookieName));
    }

    [Fact]
    public async Task A_signed_in_user_who_opens_sign_in_is_redirected_home()
    {
        await host.CreateAccountAsync("page.again@example.com", Password);
        using var browser = host.CreateBrowser();
        using (var signedIn = await SignInAsync(browser, "page.again@example.com", keepSignedIn: false))
        {
            Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
        }

        using var response = await browser.GetAsync(new Uri("/sign-in", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/", RedirectTarget(response, browser));
    }

    [Fact]
    public async Task HP_006_sign_out_ends_the_cookie_in_that_browser_only()
    {
        var account = await host.CreateAccountAsync("page.sign-out@example.com", Password);
        using var first = host.CreateBrowser();
        using var second = host.CreateBrowser();
        string firstCookie;
        using (var signedIn = await SignInAsync(first, "page.sign-out@example.com", keepSignedIn: false))
        {
            firstCookie = signedIn.SetCookieAttributes(CookieName)![0];
        }

        using (var signedIn = await SignInAsync(second, "page.sign-out@example.com", keepSignedIn: false))
        {
            Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
        }

        using var response = await SignOutAsync(firstCookie, host.SignedInAntiforgery(account.Id));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        var cleared = response.SetCookieAttributes(CookieName);
        Assert.NotNull(cleared);
        Assert.Equal(CookieName + "=", cleared[0]);
        Assert.Contains(cleared, part => part.StartsWith("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase));

        using var stillSignedIn = await second.GetAsync(new Uri("/sign-in", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Redirect, stillSignedIn.StatusCode);
    }

    [Fact]
    public async Task A_sign_out_without_an_antiforgery_token_is_refused()
    {
        await host.CreateAccountAsync("page.no-token@example.com", Password);
        using var browser = host.CreateBrowser();
        using (var signedIn = await SignInAsync(browser, "page.no-token@example.com", keepSignedIn: false))
        {
            Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
        }

        using var content = new FormUrlEncodedContent([]);
        using var response = await browser.PostAsync(new Uri("/account/sign-out", UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        // An authenticated request may re-issue the cookie (HP-004); it must not clear it.
        Assert.DoesNotContain(response.SetCookieAttributes(CookieName) ?? [], part => part == CookieName + "=");
        using var stillSignedIn = await browser.GetAsync(new Uri("/sign-in", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Redirect, stillSignedIn.StatusCode);
    }

    private static void AssertExpiresAbout(HttpResponseMessage response, DateTimeOffset expected)
    {
        var cookie = response.SetCookieAttributes(CookieName);
        Assert.NotNull(cookie);
        var expires = cookie.Single(part => part.StartsWith("expires=", StringComparison.OrdinalIgnoreCase));
        var value = DateTimeOffset.Parse(expires["expires=".Length..], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        Assert.InRange(value, expected - TimeSpan.FromMinutes(1), expected + TimeSpan.FromMinutes(1));
    }

    private static string? RedirectTarget(HttpResponseMessage response, HttpClient browser) =>
        response.Headers.Location is { } location ? new Uri(browser.BaseAddress!, location).AbsoluteUri : null;

    private static async Task<HttpResponseMessage> SignInAsync(
        HttpClient browser, string email, bool keepSignedIn, string? returnUrl = null)
    {
        var path = returnUrl is null ? "/sign-in" : "/sign-in?returnUrl=" + Uri.EscapeDataString(returnUrl);
        var form = await browser.GetFormAsync(path, "sign-in");
        return await browser.PostFormAsync(path, form, Fields(email, Password, keepSignedIn));
    }

    private async Task<HttpResponseMessage> SignOutAsync(string authCookie, AntiforgeryTokens tokens)
    {
        // A client without a cookie jar, so the request carries exactly the cookies given here.
        using var client = host.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = false,
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/account/sign-out", UriKind.Relative))
        {
            Content = new FormUrlEncodedContent([new KeyValuePair<string, string>("__RequestVerificationToken", tokens.RequestToken)]),
        };
        request.Headers.Add("Cookie", $"{authCookie}; {tokens.CookieName}={tokens.CookieToken}");
        return await client.SendAsync(request);
    }

    private static KeyValuePair<string, string>[] Fields(string email, string password, bool keepSignedIn) =>
        keepSignedIn
            ? [new("Input.Email", email), new("Input.Password", password), new("Input.KeepSignedIn", "true")]
            : [new("Input.Email", email), new("Input.Password", password)];
}
