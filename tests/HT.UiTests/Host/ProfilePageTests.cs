using System.Net;

namespace HT.UiTests.Host;

/// <summary>US3-AS6, US6-AS8, US6-AS11, HP-007, HP-008, HP-014: Profile through the real host.</summary>
[Collection(HostCollection.Name)]
public sealed class ProfilePageTests(HostFixture host)
{
    private const string Password = "walking42";

    [Fact]
    public async Task HP_008_an_anonymous_visitor_is_sent_to_sign_in_without_profile_content()
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri("/profile", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=%2Fprofile", RedirectTarget(response, browser));
        Assert.DoesNotContain("Change password", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US6_AS11_profile_shows_the_avatar_letter_and_the_email_and_no_member_since()
    {
        await host.CreateAccountAsync("ana.profile@example.com", Password);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, "ana.profile@example.com", Password, keepSignedIn: false);

        using var response = await browser.GetAsync(new Uri("/profile", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("aria-label=\"Account of ana.profile@example.com\">A</span>", html, StringComparison.Ordinal);
        Assert.Contains("<h1>ana.profile@example.com</h1>", html, StringComparison.Ordinal);
        Assert.Contains("<dt>Email</dt><dd>ana.profile@example.com</dd>", html, StringComparison.Ordinal);
        Assert.Contains("Change password", html, StringComparison.Ordinal);
        Assert.DoesNotContain("member since", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ht-tabs", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Goal history", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Custom habits", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/profile", "no-store.profile@example.com")]
    [InlineData("/", "no-store.home@example.com")]
    public async Task HP_008_signed_in_pages_are_not_stored_by_the_browser(string path, string email)
    {
        await host.CreateAccountAsync(email, Password);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: true);

        using var response = await browser.GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore, $"Cache-Control of {path}: {response.Headers.CacheControl}");
    }

    [Fact]
    public async Task HP_007_a_change_keeps_this_browser_signed_in_and_ends_the_kept_session_of_another()
    {
        await host.CreateAccountAsync("two.browsers@example.com", Password);
        using var first = host.CreateBrowser();
        using var second = host.CreateBrowser();
        await SignInAsync(first, "two.browsers@example.com", Password, keepSignedIn: false);
        await SignInAsync(second, "two.browsers@example.com", Password, keepSignedIn: true);

        var form = await first.GetFormAsync("/profile", "change-password");
        using (var changed = await first.PostFormAsync("/profile", form,
                   [new("Input.CurrentPassword", Password), new("Input.NewPassword", "running42"), new("Input.RepeatNewPassword", "running42")]))
        {
            var html = await changed.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
            Assert.Contains("Your password is changed.", html, StringComparison.Ordinal);
        }

        using (var stillSignedIn = await first.GetAsync(new Uri("/profile", UriKind.Relative)))
        {
            Assert.Equal(HttpStatusCode.OK, stillSignedIn.StatusCode);
        }

        using var ended = await second.GetAsync(new Uri("/profile", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Redirect, ended.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=%2Fprofile", RedirectTarget(ended, second));
    }

    [Fact]
    public async Task A_refused_change_shows_the_message_and_keeps_the_password()
    {
        await host.CreateAccountAsync("refused.change@example.com", Password);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, "refused.change@example.com", Password, keepSignedIn: false);

        var form = await browser.GetFormAsync("/profile", "change-password");
        using var response = await browser.PostFormAsync("/profile", form,
            [new("Input.CurrentPassword", "wrong4242"), new("Input.NewPassword", "running42"), new("Input.RepeatNewPassword", "running42")]);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("The current password is not correct.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Your password is changed.", html, StringComparison.Ordinal);
        using var other = host.CreateBrowser();
        await SignInAsync(other, "refused.change@example.com", Password, keepSignedIn: false);
    }

    [Fact]
    public async Task HP_008_after_sign_out_profile_is_redirected_again()
    {
        await host.CreateAccountAsync("profile.sign-out@example.com", Password);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, "profile.sign-out@example.com", Password, keepSignedIn: false);
        // Any request token of this user and antiforgery cookie is valid for the sign-out form.
        var page = await browser.GetFormAsync("/profile", "change-password");

        using (var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("__RequestVerificationToken", page.Token)]))
        using (var signedOut = await browser.PostAsync(new Uri("/account/sign-out", UriKind.Relative), content))
        {
            Assert.Equal(HttpStatusCode.Redirect, signedOut.StatusCode);
        }

        using var response = await browser.GetAsync(new Uri("/profile", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=%2Fprofile", RedirectTarget(response, browser));
    }

    private static string? RedirectTarget(HttpResponseMessage response, HttpClient browser) =>
        response.Headers.Location is { } location ? new Uri(browser.BaseAddress!, location).AbsoluteUri : null;

    private static async Task SignInAsync(HttpClient browser, string email, string password, bool keepSignedIn)
    {
        var form = await browser.GetFormAsync("/sign-in", "sign-in");
        KeyValuePair<string, string>[] fields = keepSignedIn
            ? [new("Input.Email", email), new("Input.Password", password), new("Input.KeepSignedIn", "true")]
            : [new("Input.Email", email), new("Input.Password", password)];
        using var response = await browser.PostFormAsync("/sign-in", form, fields);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
