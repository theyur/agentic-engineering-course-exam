using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using HT.Application.Operator;
using HT.Domain.Habits;
using HT.SmokeCheck;

namespace HT.UiTests.SmokeChecks;

/// <summary>
/// T008, T024, T025: the smoke check against a scripted site (<see cref="ScriptedHandler"/>), through
/// <see cref="SmokeCheckCommand.RunAsync"/> with no network: check 1 <c>home</c>, check 2 <c>https-redirect</c>,
/// check 3 <c>catalog</c>, check 4 <c>administrator</c>, check 5 <c>demo &lt;email&gt;</c>, the <c>TIMING</c> line
/// and the result.
/// </summary>
public sealed class RunnerBehaviourTests
{
    private const string Host = "ht-demo.example.net";
    private const string Https = "https://" + Host + "/";
    private const string Http = "http://" + Host + "/";

    private const string DemoPassword = "sentinel-pw-1";
    private const string AdministratorEmail = "sentinel-admin-2@example.com";
    private const string AdministratorPassword = "sentinel-pw-3";

    private const string VisitorHome = """
        <!DOCTYPE html><html><body>
        <section class="ht-hero"><div class="ht-hero__inner"><div><h1>Keep the habits you choose</h1></div></div></section>
        </body></html>
        """;

    private const string SignedInHome = "<!DOCTYPE html><html><body><h1>Today</h1></body></html>";

    private const string SignInPage = """
        <!DOCTYPE html><html><body><form method="post" action="/sign-in">
        <input type="hidden" name="_handler" value="sign-in" />
        <input type="hidden" name="__RequestVerificationToken" value="scripted-token" />
        </form></body></html>
        """;

    private const string Stylesheet = Https + "app.css";
    private const string Script = Https + "_framework/blazor.web.js";

    private static readonly Regex FirstTiming =
        new(@"^TIMING first \d+ ms, later \d+ ms \(3 requests\)$", RegexOptions.None, TimeSpan.FromSeconds(1));

    private static readonly Regex RetriedTiming =
        new(@"^TIMING first \d+ ms \(retried\), later \d+ ms \(3 requests\)$", RegexOptions.None, TimeSpan.FromSeconds(1));

    private static readonly Regex NoLaterTiming =
        new(@"^TIMING first \d+ ms, later n/a$", RegexOptions.None, TimeSpan.FromSeconds(1));

    /// <summary>The names of the Published habits of the seed catalog: what check 3 expects.</summary>
    private static readonly IReadOnlyList<string> PublishedNames =
        [.. SeedCatalog.Habits.Where(h => h.State == CatalogHabitState.Published).Select(h => h.Definition.Name.Value)];

    /// <summary>The demo custom-habit names of every demo account: what check 5 expects, in all.</summary>
    private static readonly IReadOnlyList<string> DemoHabitNames =
        [.. DemoDataService.DemoEmails.SelectMany(DemoCustomHabits.For).Select(h => h.Name.Value)];

    /// <summary>
    /// The good site: the visitor Home, the redirect, the catalog with every Published seed habit, the sign-in form
    /// (every sign-in is accepted), the admin page, and a custom-habits page with every demo custom habit that links
    /// one stylesheet and one script.
    /// </summary>
    private static ScriptedHandler GoodSite() => new ScriptedHandler()
        .On(HttpMethod.Get, Https, ScriptedHandler.Html(VisitorHome))
        .On(HttpMethod.Get, Http, ScriptedHandler.Redirect(HttpStatusCode.MovedPermanently, Https))
        .On(HttpMethod.Get, Https + "catalog", ScriptedHandler.Html(CardsPage([.. PublishedNames, "A reviewer's habit"])))
        .On(HttpMethod.Get, Https + "sign-in", ScriptedHandler.Html(SignInPage))
        .On(HttpMethod.Post, Https + "sign-in", ScriptedHandler.Redirect(HttpStatusCode.Found, "/"))
        .On(HttpMethod.Get, Https + "admin", ScriptedHandler.Html("<!DOCTYPE html><html><body><h1>Admin</h1></body></html>"))
        .On(HttpMethod.Get, Https + "custom-habits", ScriptedHandler.Html(CardsPage(DemoHabitNames)))
        .On(HttpMethod.Get, Stylesheet, ScriptedHandler.Html("body {}"))
        .On(HttpMethod.Get, Script, ScriptedHandler.Html("// script"));

    [Fact]
    public async Task A_good_site_passes_every_check_and_exits_with_0()
    {
        var site = GoodSite();

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(0, exitCode);
        Assert.Equal(
            [
                "PASS home",
                "PASS https-redirect",
                $"PASS catalog ({PublishedNames.Count} of {PublishedNames.Count})",
                "PASS administrator",
                "PASS demo demo1@example.com (2 of 2)",
                "PASS demo demo2@example.com (2 of 2)",
                "PASS demo demo3@example.com (2 of 2)",
            ],
            lines.Take(7));
        Assert.Matches(FirstTiming, lines[7]);
        Assert.Equal("RESULT PASS", lines[8]);
        Assert.Equal(9, lines.Count);
        Assert.Single(site.Requests, r => r == "GET " + Stylesheet);
        Assert.Single(site.Requests, r => r == "GET " + Script);
        Assert.All(DemoHabitNames.Concat(PublishedNames), name => Assert.DoesNotContain(name, string.Join('\n', lines), StringComparison.Ordinal));
    }

    [Fact]
    public async Task The_catalog_fails_when_a_published_seed_habit_is_missing()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + "catalog", ScriptedHandler.Html(CardsPage(PublishedNames.Skip(1))));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal(
            $"FAIL catalog: {PublishedNames.Count - 1} of {PublishedNames.Count} Published seed habits shown",
            Assert.Single(lines, l => l.Contains(" catalog", StringComparison.Ordinal)));
        Assert.DoesNotContain(PublishedNames[0], string.Join('\n', lines), StringComparison.Ordinal);
        Assert.Equal("RESULT FAIL", lines[^1]);
    }

    [Fact]
    public async Task The_administrator_fails_when_the_admin_page_is_not_shown()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + "admin", ScriptedHandler.Status(HttpStatusCode.NotFound));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL administrator: GET /admin gave status 404", lines[3]);
    }

    [Fact]
    public async Task Demo_checks_fail_and_later_is_n_a_when_every_demo_sign_in_returns_to_sign_in()
    {
        // The administrator signs in first (check 4); every later sign-in, the demo accounts', goes back to Sign in.
        var site = GoodSite().On(HttpMethod.Post, Https + "sign-in",
            ScriptedHandler.Redirect(HttpStatusCode.Found, "/"),
            ScriptedHandler.Redirect(HttpStatusCode.Found, "/sign-in"));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("PASS administrator", lines[3]);
        Assert.Equal(
            [.. DemoDataService.DemoEmails.Select(e => $"FAIL demo {e}: the sign-in was refused")],
            lines.Skip(4).Take(3));
        Assert.Matches(NoLaterTiming, lines[7]);
        Assert.Equal("RESULT FAIL", lines[^1]);
    }

    [Fact]
    public async Task A_demo_account_fails_when_its_custom_habits_page_misses_a_name()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + "custom-habits", ScriptedHandler.Html(CardsPage(DemoHabitNames.Skip(1))));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo demo1@example.com: 1 of 2 custom habits shown", lines[4]);
        Assert.Equal("PASS demo demo2@example.com (2 of 2)", lines[5]);
    }

    [Fact]
    public async Task Home_is_retried_after_a_503_and_the_first_timing_says_retried()
    {
        var site = GoodSite()
            .On(HttpMethod.Get, Https, ScriptedHandler.Status(HttpStatusCode.ServiceUnavailable), ScriptedHandler.Html(VisitorHome));

        var (exitCode, lines) = await RunAsync(site, "--warm-up-seconds", "2");

        Assert.Equal(0, exitCode);
        Assert.Equal("PASS home", lines[0]);
        Assert.Matches(RetriedTiming, Assert.Single(lines, l => l.StartsWith("TIMING ", StringComparison.Ordinal)));
        Assert.Equal(2, site.Requests.Count(r => r == "GET " + Https));
    }

    [Fact]
    public async Task Home_fails_when_the_site_never_answers_within_the_warm_up_time()
    {
        var site = GoodSite()
            .On(HttpMethod.Get, Https, ScriptedHandler.Status(HttpStatusCode.ServiceUnavailable));

        var (exitCode, lines) = await RunAsync(site, "--warm-up-seconds", "1");

        Assert.Equal(1, exitCode);
        Assert.StartsWith("FAIL home: ", lines[0], StringComparison.Ordinal);
        Assert.Matches(RetriedTiming, Assert.Single(lines, l => l.StartsWith("TIMING ", StringComparison.Ordinal)));
        Assert.Equal("RESULT FAIL", lines[^1]);
    }

    [Fact]
    public async Task Home_fails_when_the_page_is_not_the_visitor_home()
    {
        var site = GoodSite().On(HttpMethod.Get, Https, ScriptedHandler.Html(SignedInHome));

        var (exitCode, lines) = await RunAsync(site, "--warm-up-seconds", "1");

        Assert.Equal(1, exitCode);
        Assert.StartsWith("FAIL home: ", lines[0], StringComparison.Ordinal);
        Assert.DoesNotContain("Today", lines[0], StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.PermanentRedirect)]
    public async Task The_http_address_passes_when_it_redirects_to_the_https_address(HttpStatusCode status)
    {
        var site = GoodSite().On(HttpMethod.Get, Http, ScriptedHandler.Redirect(status, Https));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(0, exitCode);
        Assert.Equal("PASS https-redirect", lines[1]);
    }

    [Theory]
    [InlineData("200")]
    [InlineData("another host")]
    [InlineData("still http")]
    [InlineData("relative")]
    public async Task The_http_address_fails_without_a_redirect_to_the_https_address(string answer)
    {
        var response = answer switch
        {
            "200" => ScriptedHandler.Html(VisitorHome),
            "another host" => ScriptedHandler.Redirect(HttpStatusCode.MovedPermanently, "https://elsewhere.example.net/"),
            "still http" => ScriptedHandler.Redirect(HttpStatusCode.MovedPermanently, Http),
            _ => ScriptedHandler.Redirect(HttpStatusCode.MovedPermanently, "/"),
        };
        var site = GoodSite().On(HttpMethod.Get, Http, response);

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("PASS home", lines[0]);
        Assert.StartsWith("FAIL https-redirect: ", lines[1], StringComparison.Ordinal);
        Assert.Equal("RESULT FAIL", lines[^1]);
    }

    [Fact]
    public async Task Skip_http_redirect_skips_check_2_without_a_request()
    {
        var site = GoodSite();

        var (exitCode, lines) = await RunAsync(site, "--skip-http-redirect");

        Assert.Equal(0, exitCode);
        Assert.Equal("SKIP https-redirect", lines[1]);
        Assert.DoesNotContain(site.Requests, r => r.Contains("http://", StringComparison.Ordinal));
    }

    /// <summary>A page with one habit card per name, as <c>HabitCard</c> renders it, linking one stylesheet and one script.</summary>
    private static string CardsPage(IEnumerable<string> names) =>
        $"""
         <!DOCTYPE html><html><head><base href="/" /><link rel="stylesheet" href="app.css" /></head><body>
         {string.Concat(names.Select(n => $"<article class=\"ht-habit\"><div class=\"ht-habit__head\"><h3 class=\"ht-habit__name\">{WebUtility.HtmlEncode(n)}</h3></div></article>"))}
         <script src="_framework/blazor.web.js"></script><script>/* inline */</script>
         </body></html>
         """;

    private static async Task<(int ExitCode, IReadOnlyList<string> Lines)> RunAsync(ScriptedHandler site, params string[] options)
    {
        var environment = new Hashtable
        {
            ["HT_SMOKE_DEMO_PASSWORD"] = DemoPassword,
            ["HT_SMOKE_ADMIN_EMAIL"] = AdministratorEmail,
            ["HT_SMOKE_ADMIN_PASSWORD"] = AdministratorPassword,
        };
        using var output = new StringWriter();

        var exitCode = await SmokeCheckCommand.RunAsync(["https://" + Host, .. options], environment, output, () => site);

        var text = output.ToString();
        foreach (var secret in new[] { DemoPassword, AdministratorEmail, AdministratorPassword })
        {
            Assert.DoesNotContain(secret, text, StringComparison.Ordinal);
        }

        return (exitCode, text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }
}
