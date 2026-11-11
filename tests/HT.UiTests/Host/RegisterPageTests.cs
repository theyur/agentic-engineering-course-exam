using System.Net;

namespace HT.UiTests.Host;

[Collection(HostCollection.Name)]
public sealed class RegisterPageTests(HostFixture host)
{
    private const string CookieName = ".AspNetCore.Identity.Application";

    [Fact]
    public async Task A_valid_registration_redirects_home_with_a_secure_session_cookie()
    {
        using var browser = host.CreateBrowser();

        using var response = await RegisterAsync(browser, "page.register@example.com", "walking42", "walking42");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/", RedirectTarget(response, browser));
        var cookie = response.SetCookieAttributes(CookieName);
        Assert.NotNull(cookie);
        Assert.Contains("secure", cookie, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain(cookie, part => part.StartsWith("expires=", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(cookie, part => part.StartsWith("max-age=", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(1, await HostClient.CountUsersAsync(host.ConnectionString, "page.register@example.com"));
    }

    [Fact]
    public async Task A_signed_in_user_who_opens_register_is_redirected_home()
    {
        using var browser = host.CreateBrowser();
        using (var registered = await RegisterAsync(browser, "page.signed-in@example.com", "walking42", "walking42"))
        {
            Assert.Equal(HttpStatusCode.Redirect, registered.StatusCode);
        }

        using var response = await browser.GetAsync(new Uri("/register", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/", RedirectTarget(response, browser));
    }

    [Fact]
    public async Task An_invalid_registration_shows_the_message_and_keeps_the_email()
    {
        using var browser = host.CreateBrowser();

        using var response = await RegisterAsync(browser, "ana@example", "walking42", "walking42");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Enter a valid email address.", html, StringComparison.Ordinal);
        Assert.Contains("value=\"ana@example\"", html, StringComparison.Ordinal);
        Assert.Null(response.SetCookieAttributes(CookieName));
    }

    [Fact]
    public async Task HP_020_two_parallel_registrations_of_one_email_leave_exactly_one_account()
    {
        using var one = host.CreateBrowser();
        using var two = host.CreateBrowser();
        var formOne = await one.GetFormAsync("/register", "register");
        var formTwo = await two.GetFormAsync("/register", "register");

        var responses = await Task.WhenAll(
            one.PostFormAsync("/register", formOne, Fields("page.parallel@example.com", "walking42", "walking42")),
            two.PostFormAsync("/register", formTwo, Fields("page.parallel@example.com", "walking42", "walking42")));
        foreach (var response in responses)
        {
            response.Dispose();
        }

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Redirect);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Equal(1, await HostClient.CountUsersAsync(host.ConnectionString, "page.parallel@example.com"));
    }

    private static string? RedirectTarget(HttpResponseMessage response, HttpClient browser) =>
        response.Headers.Location is { } location ? new Uri(browser.BaseAddress!, location).AbsoluteUri : null;

    private static async Task<HttpResponseMessage> RegisterAsync(HttpClient browser, string email, string password, string repeat)
    {
        var form = await browser.GetFormAsync("/register", "register");
        return await browser.PostFormAsync("/register", form, Fields(email, password, repeat));
    }

    private static KeyValuePair<string, string>[] Fields(string email, string password, string repeat) =>
    [
        new("Input.Email", email),
        new("Input.Password", password),
        new("Input.RepeatPassword", repeat),
    ];
}
