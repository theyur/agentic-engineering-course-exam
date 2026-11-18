using System.Net;
using HT.Application.Accounts;
using HT.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public async Task HP_016_the_page_has_the_hidden_browser_time_zone_input_and_no_visible_time_zone_field()
    {
        using var browser = host.CreateBrowser();

        var form = await browser.GetFormAsync("/register", "register");

        var marked = System.Text.RegularExpressions.Regex.Matches(form.Html, "<input[^>]*data-ht-browser-time-zone[^>]*>").Select(m => m.Value).ToList();
        Assert.Equal(["<input type=\"hidden\" name=\"TimeZone\" data-ht-browser-time-zone>"], marked);
        Assert.DoesNotContain("<select", form.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("Time zone", form.Html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("page.zone.kyiv@example.com", "Europe/Kyiv")]
    [InlineData("page.zone.empty@example.com", "")]
    [InlineData("page.zone.windows@example.com", "FLE Standard Time")]
    public async Task HP_016_a_posted_named_time_zone_is_stored_and_an_empty_or_other_value_stores_the_default(string email, string posted)
    {
        await HostClient.DeleteUserAsync(host.ConnectionString, email);
        using var browser = host.CreateBrowser();
        var form = await browser.GetFormAsync("/register", "register");

        using var response = await browser.PostFormAsync("/register", form, [.. Fields(email, "walking42", "walking42"), new("TimeZone", posted)]);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var scope = host.Services.CreateAsyncScope();
        var expected = posted == "Europe/Kyiv" ? posted : scope.ServiceProvider.GetRequiredService<InstallationTimeZone>().Value.Id;
        Assert.True(Email.TryParse(email, out var parsed));
        var account = await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByEmailAsync(parsed, CancellationToken.None);
        Assert.Equal(expected, account?.StoredTimeZone);
    }

    [Fact]
    public async Task HP_017_a_registration_with_a_name_stores_it_trimmed_and_Home_greets_with_it()
    {
        const string email = "page.named@example.com";
        await HostClient.DeleteUserAsync(host.ConnectionString, email);
        using var browser = host.CreateBrowser();
        var form = await browser.GetFormAsync("/register", "register");
        Assert.Contains("<span class=\"ht-field__label\">Name <span class=\"ht-muted\">(optional)</span></span>", form.Html, StringComparison.Ordinal);
        Assert.True(
            form.Html.IndexOf("id=\"register-name\"", StringComparison.Ordinal) < form.Html.IndexOf("id=\"register-email\"", StringComparison.Ordinal),
            "Name comes before Email.");

        using (var response = await browser.PostFormAsync("/register", form, [new("Input.Name", "  zoe Ui-Rg  "), .. Fields(email, "walking42", "walking42")]))
        {
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        await using (var scope = host.Services.CreateAsyncScope())
        {
            Assert.True(Email.TryParse(email, out var parsed));
            var account = await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByEmailAsync(parsed, CancellationToken.None);
            Assert.Equal("zoe Ui-Rg", account?.Name);
        }

        var (status, home) = await browser.GetPageAsync("/");
        Assert.Equal(HttpStatusCode.OK, status);
        var heading = System.Text.RegularExpressions.Regex.Match(home, "<h1>(?<text>[^<]*)</h1>").Groups["text"].Value;
        Assert.StartsWith("Good ", heading, StringComparison.Ordinal);
        Assert.EndsWith(", zoe Ui-Rg", heading, StringComparison.Ordinal);
        Assert.Matches("id=\"account-menu-button\"[^>]*>Z</button>", home);
    }

    [Fact]
    public async Task US11_AS2_a_name_of_61_characters_is_refused_and_creates_no_account()
    {
        const string email = "page.long-name@example.com";
        await HostClient.DeleteUserAsync(host.ConnectionString, email);
        using var browser = host.CreateBrowser();
        var form = await browser.GetFormAsync("/register", "register");

        using var response = await browser.PostFormAsync("/register", form, [new("Input.Name", new string('n', 61)), .. Fields(email, "walking42", "walking42")]);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<span class=\"ht-field__error\" id=\"register-name-error\">The name can have at most 60 characters.</span>", html, StringComparison.Ordinal);
        Assert.Contains($"value=\"{email}\"", html, StringComparison.Ordinal);
        Assert.Null(response.SetCookieAttributes(CookieName));
        Assert.Equal(0, await HostClient.CountUsersAsync(host.ConnectionString, email));
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
