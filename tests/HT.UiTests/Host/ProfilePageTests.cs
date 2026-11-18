using System.Net;
using System.Text.RegularExpressions;
using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>
/// US3-AS6, US6-AS8, US6-AS11, HP-007, HP-008, HP-014 (slice 1); slice 2's HP-008 and US4-AS14 (the Tabs for
/// registered users); slice 4's HP-015 (the time zone) and HP-017, US11 (the name, the header and the avatar letter):
/// Profile through the real host. The name tests use emails "ui-pr-…" and names "Ui-Pr …".
/// </summary>
[Collection(HostCollection.Name)]
public sealed partial class ProfilePageTests(HostFixture host)
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
    public async Task US6_AS11_profile_shows_the_avatar_letter_and_the_email_and_since_slice_4_the_member_since_line()
    {
        await host.CreateAccountAsync("ana.profile@example.com", Password);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, "ana.profile@example.com", Password, keepSignedIn: false);

        using var response = await browser.GetAsync(new Uri("/profile", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("aria-label=\"Account of ana.profile@example.com\">A</span>", html, StringComparison.Ordinal);
        Assert.Contains(
            "<h1>ana.profile@example.com</h1><span class=\"ht-muted\">member since November 2026</span>", html, StringComparison.Ordinal);
        AssertEmailRow(html, "ana.profile@example.com");
        Assert.Contains("Change password", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Goal history", html, StringComparison.Ordinal);
    }

    // ---- The name (HP-017, US11) -----------------------------------------------------------------------------------

    private const string NameNotice = "Your name is changed.";

    private static string? FirstHeadingOf(string html) =>
        FirstHeadingPattern().Match(html) is { Success: true } match ? WebUtility.HtmlDecode(match.Groups["text"].Value) : null;

    private static string? NavLetterOf(string html) =>
        NavLetterPattern().Match(html) is { Success: true } match ? match.Groups["letter"].Value : null;

    private static string? NameValueOf(string html) =>
        NameValuePattern().Match(html) is { Success: true } match ? WebUtility.HtmlDecode(match.Groups["value"].Value) : null;

    /// <summary>The Profile header's h1 and muted line, decoded (the encoder writes "·" as a character reference).</summary>
    private static (string Heading, string Line) HeaderOf(string html)
    {
        var match = HeaderPattern().Match(html);
        Assert.True(match.Success, "The Profile header has an h1 and a muted line.");
        return (WebUtility.HtmlDecode(match.Groups["heading"].Value), WebUtility.HtmlDecode(match.Groups["line"].Value));
    }

    /// <summary>The KeyValue row "Email" with the account's email (T120: KeyValue writes it, with whitespace between rows).</summary>
    private static void AssertEmailRow(string html, string email) =>
        Assert.Matches(@"<dl class=""ht-kv"">\s*<dt>Email</dt>\s*<dd>" + Regex.Escape(email) + @"</dd>\s*</dl>", html);

    private async Task<string> NamedAccountAsync(string prefix, string? name, Role role = Role.RegisteredUser)
    {
        var email = $"{prefix}.{Guid.NewGuid():N}@example.com";
        await host.CreateAccountAsync(email, Password, role, name: name);
        return email;
    }

    private async Task ClearCreatedAtAsync(string email)
    {
        await using var connection = new SqlConnection(host.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("UPDATE [AspNetUsers] SET [CreatedAt] = NULL WHERE [UserName] = @email", connection);
        command.Parameters.AddWithValue("@email", email);
        Assert.Equal(1, await command.ExecuteNonQueryAsync());
    }

    private async Task<string?> StoredNameAsync(string email)
    {
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = host.Services.CreateAsyncScope();
        return (await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByEmailAsync(parsed, CancellationToken.None))?.Name;
    }

    [Theory]
    [InlineData(Role.RegisteredUser)]
    [InlineData(Role.Administrator)]
    public async Task HP_017_with_a_name_the_header_has_the_name_as_its_first_heading_then_the_email_and_member_since(Role role)
    {
        var email = await NamedAccountAsync("ui-pr-named", "yuri Ui-Pr", role);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);

        var (status, html) = await browser.GetPageAsync("/profile");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal("yuri Ui-Pr", FirstHeadingOf(html));
        Assert.Equal(("yuri Ui-Pr", $"{email} · member since November 2026"), HeaderOf(html));
        Assert.Contains($"aria-label=\"Account of {email}\">Y</span>", html, StringComparison.Ordinal);
        Assert.Equal("Y", NavLetterOf(html));
        AssertEmailRow(html, email);
    }

    [Fact]
    public async Task HP_017_without_a_name_the_email_stays_the_heading_with_member_since()
    {
        var email = await NamedAccountAsync("ui-pr-unnamed", null);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);

        var (_, html) = await browser.GetPageAsync("/profile");

        Assert.Equal(email, FirstHeadingOf(html));
        Assert.Contains($"<h1>{email}</h1><span class=\"ht-muted\">member since November 2026</span>", html, StringComparison.Ordinal);
        Assert.Equal("U", NavLetterOf(html));
    }

    [Theory]
    [InlineData("Olena Ui-Pr", "Olena Ui-Pr", true)]
    [InlineData(null, null, false)]
    public async Task HP_017_an_account_created_before_slice_4_has_no_member_since(string? name, string? heading, bool withName)
    {
        var email = await NamedAccountAsync("ui-pr-pre-slice", name);
        await ClearCreatedAtAsync(email);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);

        var (_, html) = await browser.GetPageAsync("/profile");

        var expected = withName
            ? $"<h1>{heading}</h1><span class=\"ht-muted\">{email}</span>"
            : $"<h1>{email}</h1><span class=\"ht-muted\">Profile</span>";
        Assert.Contains(expected, html, StringComparison.Ordinal);
        Assert.DoesNotContain("member since", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HP_017_the_Account_tab_has_the_Name_form_before_the_Time_zone_form_with_the_current_name()
    {
        var email = await NamedAccountAsync("ui-pr-form", "Ui-Pr Form");
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);

        var form = await browser.GetFormAsync("/profile", "name");
        var html = form.Html;

        Assert.Contains("<span class=\"ht-field__label\">Name <span class=\"ht-muted\">(optional)</span></span>", html, StringComparison.Ordinal);
        Assert.Contains("<span class=\"ht-field__help\" id=\"profile-name-help\">Only you can see it. Home uses it to greet you.</span>", html, StringComparison.Ordinal);
        Assert.Equal("Ui-Pr Form", NameValueOf(html));
        var nameForm = html.IndexOf("id=\"profile-name\"", StringComparison.Ordinal);
        var emailRow = html.IndexOf("<dt>Email</dt>", StringComparison.Ordinal);
        var zoneForm = html.IndexOf("Time zone</span>", StringComparison.Ordinal);
        Assert.True(emailRow >= 0 && nameForm > emailRow && zoneForm > nameForm, "The email, then Name, then Time zone.");
        Assert.DoesNotContain(NameNotice, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HP_017_a_name_change_shows_the_Notice_the_new_header_and_after_the_cookie_refresh_the_new_letter()
    {
        var email = await NamedAccountAsync("ui-pr-change", null);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);
        var form = await browser.GetFormAsync("/profile", "name");
        Assert.Equal("U", NavLetterOf(form.Html));

        using (var response = await browser.PostFormAsync("/profile", form, [new("Input.Name", "  bohdan Ui-Pr  ")]))
        {
            var html = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains($"<div class=\"ht-notice\" role=\"status\"><span>{NameNotice}</span></div>", html, StringComparison.Ordinal);
            Assert.Equal("bohdan Ui-Pr", FirstHeadingOf(html));
            Assert.Contains($"aria-label=\"Account of {email}\">B</span>", html, StringComparison.Ordinal);
            Assert.Equal("bohdan Ui-Pr", NameValueOf(html));
            Assert.NotNull(response.SetCookieAttributes(".AspNetCore.Identity.Application"));
        }

        Assert.Equal("bohdan Ui-Pr", await StoredNameAsync(email));
        var (_, after) = await browser.GetPageAsync("/profile");
        Assert.Equal("B", NavLetterOf(after));
        Assert.DoesNotContain(NameNotice, after, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HP_017_an_empty_name_removes_the_name_and_the_email_is_the_heading_again()
    {
        var email = await NamedAccountAsync("ui-pr-remove", "Ui-Pr Remove");
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);
        var form = await browser.GetFormAsync("/profile", "name");

        using var response = await browser.PostFormAsync("/profile", form, [new("Input.Name", "   ")]);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(NameNotice, html, StringComparison.Ordinal);
        Assert.Equal(email, FirstHeadingOf(html));
        Assert.Null(await StoredNameAsync(email));
        var (_, after) = await browser.GetPageAsync("/profile");
        Assert.Equal("U", NavLetterOf(after));
    }

    [Fact]
    public async Task US11_AS2_a_name_of_61_characters_shows_the_refusal_under_the_field_and_changes_nothing()
    {
        var email = await NamedAccountAsync("ui-pr-long", "Ui-Pr Long");
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);
        var form = await browser.GetFormAsync("/profile", "name");
        var name = new string('n', 61);

        using var response = await browser.PostFormAsync("/profile", form, [new("Input.Name", name)]);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "<span class=\"ht-field__error\" id=\"profile-name-error\">The name can have at most 60 characters.</span>", html, StringComparison.Ordinal);
        Assert.DoesNotContain(NameNotice, html, StringComparison.Ordinal);
        Assert.Equal(name, NameValueOf(html));
        Assert.Equal("Ui-Pr Long", FirstHeadingOf(html));
        Assert.Equal("Ui-Pr Long", await StoredNameAsync(email));
    }

    [Fact]
    public async Task HP_008_a_registered_user_gets_the_Tabs_with_Account_first_and_current()
    {
        await host.CreateAccountAsync("tabs.profile@example.com", Password);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, "tabs.profile@example.com", Password, keepSignedIn: false);

        var (status, html) = await browser.GetPageAsync("/profile");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<nav class=\"ht-tabs\" aria-label=\"Profile sections\">"
            + "<a class=\"ht-tab\" href=\"/profile\" aria-current=\"page\">Account</a>"
            + "<a class=\"ht-tab\" href=\"/profile?tab=custom-habits\">Custom habits</a></nav>", html, StringComparison.Ordinal);
        AssertEmailRow(html, "tabs.profile@example.com");
        Assert.Contains("Change password", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Private habits for your own goals.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US4_AS14_the_Custom_habits_tab_lists_the_users_habits_with_create_and_edit()
    {
        var account = await host.CreateAccountAsync("list.profile@example.com", Password);
        var shower = await host.ArrangeCustomHabitAsync(account.Id, Definition("Ui-Ch Profile shower", Category.Body, icon: "droplet"));
        await host.ArrangeCustomHabitAsync(account.Id, Definition("Ui-Ch Profile bed", Category.Home, Intent.Reduce, "smartphone"));
        var other = await host.CreateAccountAsync("list.profile.other@example.com", Password);
        await host.ArrangeCustomHabitAsync(other.Id, Definition("Ui-Ch Profile other"));
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, "list.profile@example.com", Password, keepSignedIn: false);

        var (status, html) = await browser.GetPageAsync("/profile?tab=custom-habits");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<a class=\"ht-tab\" href=\"/profile?tab=custom-habits\" aria-current=\"page\">Custom habits</a>", html, StringComparison.Ordinal);
        Assert.Contains("<h2 class=\"ht-card__title\">Custom habits</h2>", html, StringComparison.Ordinal);
        Assert.Contains("Private habits for your own goals. Only you can see them.", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/custom-habits/new\"", html, StringComparison.Ordinal);
        Assert.Contains("Create custom habit</a>", html, StringComparison.Ordinal);
        var bed = html.IndexOf("<div class=\"ht-row__name\">Ui-Ch Profile bed</div>", StringComparison.Ordinal);
        var showerRow = html.IndexOf("<div class=\"ht-row__name\">Ui-Ch Profile shower</div>", StringComparison.Ordinal);
        Assert.True(bed >= 0 && showerRow > bed, "The rows are in name order.");
        Assert.Contains("<div class=\"ht-row__meta\">Home · Reduce</div>", html, StringComparison.Ordinal);
        Assert.Contains($"href=\"/custom-habits/{shower.Value}\" aria-label=\"Edit Ui-Ch Profile shower\">Edit</a>", html, StringComparison.Ordinal);
        Assert.Contains("<span class=\"ht-chip ht-chip--strong\">Private</span>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Ch Profile other", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Change password", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US4_AS14_without_custom_habits_the_tab_shows_the_EmptyState()
    {
        await host.CreateAccountAsync("empty.profile@example.com", Password);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, "empty.profile@example.com", Password, keepSignedIn: false);

        var (status, html) = await browser.GetPageAsync("/profile?tab=custom-habits");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h3>No custom habits yet</h3>", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/custom-habits/new\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/profile")]
    [InlineData("/profile?tab=custom-habits")]
    public async Task HP_008_an_administrators_Profile_stays_Account_only_without_Tabs(string path)
    {
        var email = $"admin.profile.{Guid.NewGuid():N}@example.com";
        await host.CreateAccountAsync(email, Password, Role.Administrator);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);

        var (status, html) = await browser.GetPageAsync(path);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"<h1>{email}</h1>", html, StringComparison.Ordinal);
        AssertEmailRow(html, email);
        Assert.Contains("Change password", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ht-tabs", html, StringComparison.Ordinal);
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

    private const string TimeZoneNotice =
        "Your time zone is changed. Your goals use it from their next day or week; earlier days keep their time zone.";

    private static List<string> OptionsOf(string html) =>
        OptionPattern().Matches(html).Select(m => WebUtility.HtmlDecode(m.Groups["value"].Value)).ToList();

    private static string? SelectedOf(string html) =>
        SelectedPattern().Match(html) is { Success: true } match ? WebUtility.HtmlDecode(match.Groups["value"].Value) : null;

    [Theory]
    [InlineData("zone.profile@example.com", "Europe/Kyiv", Role.RegisteredUser)]
    [InlineData("zone.admin.profile@example.com", "America/New_York", Role.Administrator)]
    public async Task HP_015_the_Account_tab_has_the_Time_zone_list_with_UTC_first_and_the_current_value_selected(string email, string zone, Role role)
    {
        await HostClient.DeleteUserAsync(host.ConnectionString, email);
        await host.CreateAccountAsync(email, Password, role, zone);
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);

        var (status, html) = await browser.GetPageAsync("/profile");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<span class=\"ht-field__label\">Time zone</span>", html, StringComparison.Ordinal);
        Assert.Contains("Your goals&#x27; days and weeks start at midnight in this time zone.", html, StringComparison.Ordinal);
        var options = OptionsOf(html);
        Assert.Equal("UTC", options[0]);
        Assert.Contains("Europe/Kyiv", options);
        Assert.Contains("America/New_York", options);
        Assert.DoesNotContain("Europe/Kiev", options);
        Assert.Equal(zone, SelectedOf(html));
        Assert.Contains(">Save</button>", html, StringComparison.Ordinal);
        var timeZoneForm = html.IndexOf("Time zone</span>", StringComparison.Ordinal);
        Assert.True(timeZoneForm >= 0 && timeZoneForm < html.IndexOf("Change password", StringComparison.Ordinal), "Time zone comes before Change password.");
        Assert.DoesNotContain(TimeZoneNotice, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HP_015_a_stored_link_that_is_not_in_the_list_is_added_and_selected()
    {
        const string email = "kiev.profile@example.com";
        await HostClient.DeleteUserAsync(host.ConnectionString, email);
        await host.CreateAccountAsync(email, Password, timeZone: "Europe/Kiev");
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);

        var (_, html) = await browser.GetPageAsync("/profile");

        var options = OptionsOf(html);
        Assert.Single(options, o => o == "Europe/Kiev");
        Assert.Contains("Europe/Kyiv", options);
        Assert.Equal("Europe/Kiev", SelectedOf(html));
    }

    [Fact]
    public async Task HP_015_a_change_shows_the_Notice_selects_the_new_zone_and_stores_it()
    {
        const string email = "change.zone.profile@example.com";
        await HostClient.DeleteUserAsync(host.ConnectionString, email);
        var account = await host.CreateAccountAsync(email, Password, timeZone: "Europe/Kyiv");
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);
        var form = await browser.GetFormAsync("/profile", "time-zone");

        using var response = await browser.PostFormAsync("/profile", form, [new("Input.TimeZone", "America/New_York")]);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<div class=\"ht-notice\" role=\"status\">", html, StringComparison.Ordinal);
        Assert.Contains(TimeZoneNotice, html, StringComparison.Ordinal);
        Assert.Equal("America/New_York", SelectedOf(html));
        Assert.Equal("America/New_York", await StoredZoneAsync(account.Id));
    }

    [Fact]
    public async Task HP_015_a_value_that_is_not_a_named_time_zone_shows_the_refusal_under_the_field_and_keeps_the_zone()
    {
        const string email = "refused.zone.profile@example.com";
        await HostClient.DeleteUserAsync(host.ConnectionString, email);
        var account = await host.CreateAccountAsync(email, Password, timeZone: "Europe/Kyiv");
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, email, Password, keepSignedIn: false);
        var form = await browser.GetFormAsync("/profile", "time-zone");

        using var response = await browser.PostFormAsync("/profile", form, [new("Input.TimeZone", "Mars/Olympus")]);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("<span class=\"ht-field__error\" id=\"profile-time-zone-error\">Choose a time zone from the list.</span>", html, StringComparison.Ordinal);
        Assert.Contains("aria-invalid=\"true\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(TimeZoneNotice, html, StringComparison.Ordinal);
        Assert.DoesNotContain("Mars/Olympus", html, StringComparison.Ordinal);
        Assert.Equal("Europe/Kyiv", SelectedOf(html));
        Assert.Equal("Europe/Kyiv", await StoredZoneAsync(account.Id));
    }

    private async Task<string?> StoredZoneAsync(AccountId id)
    {
        await using var scope = host.Services.CreateAsyncScope();
        return (await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByIdAsync(id, CancellationToken.None))?.StoredTimeZone;
    }

    [GeneratedRegex("""<option value="(?<value>[^"]*)"[^>]*>""")]
    private static partial Regex OptionPattern();

    [GeneratedRegex("""<option value="(?<value>[^"]*)" selected[^>]*>""")]
    private static partial Regex SelectedPattern();

    [GeneratedRegex("""<h1[^>]*>(?<text>[^<]*)</h1>""")]
    private static partial Regex FirstHeadingPattern();

    [GeneratedRegex("""<div class="ht-page-head ht-profile-head">.*?<h1>(?<heading>[^<]*)</h1><span class="ht-muted">(?<line>[^<]*)</span>""", RegexOptions.Singleline)]
    private static partial Regex HeaderPattern();

    [GeneratedRegex("""id="account-menu-button"[^>]*>(?<letter>[^<]*)</button>""")]
    private static partial Regex NavLetterPattern();

    [GeneratedRegex("""<input[^>]*id="profile-name"[^>]*value="(?<value>[^"]*)"|<input[^>]*value="(?<value>[^"]*)"[^>]*id="profile-name"[^>]*>""")]
    private static partial Regex NameValuePattern();

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
