using System.Collections;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HT.Application.Operator;
using HT.Domain.Habits;
using HT.SmokeCheck;

namespace HT.UiTests.SmokeChecks;

/// <summary>
/// T008, T024, T025: the smoke check against a scripted site (<see cref="ScriptedHandler"/>), through
/// <see cref="SmokeCheckCommand.RunAsync"/> with no network: check 1 <c>home</c>, check 2 <c>https-redirect</c>,
/// check 3 <c>catalog</c>, check 4 <c>administrator</c>, check 5 <c>demo &lt;email&gt;</c> (with slice 4's goal names and
/// account name, HP-018), check 6 <c>demo history demo4@example.com</c> (slice 5 HP-008, research R-10), the
/// <c>TIMING</c> line and the result.
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

    /// <summary>The demo goal names of every demo account: what check 5 expects on Goals, in all (slice 4 HP-018).</summary>
    private static readonly IReadOnlyList<string> DemoGoalNames =
        [.. DemoDataService.DemoEmails.SelectMany(DemoGoals.DisplayNames)];

    /// <summary>The names of demo4's goals: what check 6 expects on Goals (slice 5 research R-10).</summary>
    private static readonly IReadOnlyList<string> HistoryGoalNames = DemoHistory.GoalNames;

    /// <summary>What check 6 expects in the History card of "Reading": Met, Failed, Pending, Due today, the partial amount and the note.</summary>
    private static readonly IReadOnlyList<string> ReadingTexts =
        ["Met", "Failed", "Pending", "Due today", "12 min", DemoHistory.ReadingNote];

    /// <summary>What check 6 expects in the History card of "Yoga".</summary>
    private static readonly IReadOnlyList<string> YogaTexts = ["Failed", "Missed"];

    /// <summary>The good site's Goals page: every demo goal of checks 5 and 6, then a reviewer's goal.</summary>
    private static readonly IReadOnlyList<string> AllGoalNames = [.. DemoGoalNames, .. HistoryGoalNames, "A reviewer's goal"];

    /// <summary>The goal page paths of "Reading" and "Yoga" on the good site, as <see cref="GoalsPage"/> links them.</summary>
    private static readonly string ReadingPath = GoalPath(DemoGoalNames.Count);

    private static readonly string YogaPath = GoalPath(DemoGoalNames.Count + 2);

    /// <summary>
    /// The good site: the visitor Home, the redirect, the catalog with every Published seed habit, the sign-in form
    /// (every sign-in is accepted), the admin page, a custom-habits page with every demo custom habit that links one
    /// stylesheet and one script, a Goals page with every demo goal, demo4's included, and a reviewer's goal, a Profile
    /// page whose first heading is "Demo One", and the goal pages of "Reading" and "Yoga" with their History cards after
    /// a Settings card.
    /// </summary>
    private static ScriptedHandler GoodSite() => new ScriptedHandler()
        .On(HttpMethod.Get, Https, ScriptedHandler.Html(VisitorHome))
        .On(HttpMethod.Get, Http, ScriptedHandler.Redirect(HttpStatusCode.MovedPermanently, Https))
        .On(HttpMethod.Get, Https + "catalog", ScriptedHandler.Html(CardsPage([.. PublishedNames, "A reviewer's habit"])))
        .On(HttpMethod.Get, Https + "sign-in", ScriptedHandler.Html(SignInPage))
        .On(HttpMethod.Post, Https + "sign-in", ScriptedHandler.Redirect(HttpStatusCode.Found, "/"))
        .On(HttpMethod.Get, Https + "admin", ScriptedHandler.Html("<!DOCTYPE html><html><body><h1>Admin</h1></body></html>"))
        .On(HttpMethod.Get, Https + "custom-habits", ScriptedHandler.Html(CardsPage(DemoHabitNames)))
        .On(HttpMethod.Get, Https + "goals", ScriptedHandler.Html(GoalsPage(AllGoalNames)))
        .On(HttpMethod.Get, Https + "profile", ScriptedHandler.Html(ProfilePage("Demo One")))
        .On(HttpMethod.Get, Https + ReadingPath[1..], ScriptedHandler.Html(GoalPage(["Sat 7", "20 min", .. ReadingTexts, "Show all 28 →"])))
        .On(HttpMethod.Get, Https + YogaPath[1..], ScriptedHandler.Html(GoalPage(["Mon 2", "Yes", .. YogaTexts])))
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

                // Slice 4 HP-018: 2 custom habits, 2 goals and the account name; 2 and 2; 2 and 3.
                "PASS demo demo1@example.com (5 of 5)",
                "PASS demo demo2@example.com (4 of 4)",
                "PASS demo demo3@example.com (5 of 5)",

                // Slice 5 HP-008, SC-005: 3 goal names, 6 texts on Reading's page and 2 on Yoga's.
                "PASS demo history demo4@example.com (11 of 11)",
            ],
            lines.Take(8));
        Assert.Matches(FirstTiming, lines[8]);
        Assert.Equal("RESULT PASS", lines[9]);
        Assert.Equal(10, lines.Count);
        Assert.Single(site.Requests, r => r == "GET " + Stylesheet);
        Assert.Single(site.Requests, r => r == "GET " + Script);
        Assert.Equal(4, site.Requests.Count(r => r == "GET " + Https + "goals"));
        Assert.Equal(1, site.Requests.Count(r => r == "GET " + Https + "profile"));
        Assert.Single(site.Requests, r => r == "GET " + Https + ReadingPath[1..]);
        Assert.Single(site.Requests, r => r == "GET " + Https + YogaPath[1..]);
        Assert.All(
            DemoHabitNames.Concat(PublishedNames).Concat(DemoGoalNames).Append("Demo One"),
            name => Assert.DoesNotContain(name, string.Join('\n', lines), StringComparison.Ordinal));
        AssertNoDemoHistoryContent(lines);
    }

    [Fact]
    public async Task Demo_history_fails_when_its_sign_in_is_refused_and_the_other_checks_pass()
    {
        // The administrator and demo1 to demo3 sign in; demo4, the last sign-in, goes back to Sign in.
        var accepted = ScriptedHandler.Redirect(HttpStatusCode.Found, "/");
        var site = GoodSite().On(HttpMethod.Post, Https + "sign-in",
            accepted, accepted, accepted, accepted, ScriptedHandler.Redirect(HttpStatusCode.Found, "/sign-in"));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("PASS demo demo3@example.com (5 of 5)", lines[6]);
        Assert.Equal("FAIL demo history demo4@example.com: the sign-in was refused", lines[7]);
        Assert.Matches(FirstTiming, lines[8]);
        Assert.Equal("RESULT FAIL", lines[^1]);
    }

    [Fact]
    public async Task Demo_history_fails_with_the_status_when_its_goals_page_is_not_shown()
    {
        // Goals is one page on this site, so checks 5 fail too; check 6 names the status, not a goal.
        var site = GoodSite().On(HttpMethod.Get, Https + "goals", ScriptedHandler.Status(HttpStatusCode.InternalServerError));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo history demo4@example.com: GET /goals gave status 500", lines[7]);
        Assert.DoesNotContain(site.Requests, r => r == "GET " + Https + ReadingPath[1..]);
        AssertNoDemoHistoryContent(lines);
    }

    [Fact]
    public async Task Demo_history_fails_when_its_goals_page_misses_a_goal_name_and_still_reads_both_goal_pages()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + "goals",
            ScriptedHandler.Html(GoalsPage([.. AllGoalNames.Select(n => n == HistoryGoalNames[1] ? "Another goal" : n)])));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo history demo4@example.com: 2 of 3 goal names shown", lines[7]);
        Assert.Single(site.Requests, r => r == "GET " + Https + ReadingPath[1..]);
        Assert.Single(site.Requests, r => r == "GET " + Https + YogaPath[1..]);
        AssertNoDemoHistoryContent(lines);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("https://elsewhere.example.net/goals/1")]
    [InlineData("//elsewhere.example.net/goals/1")]
    public async Task Demo_history_fails_when_a_goal_page_has_no_link_on_its_own_site(string href)
    {
        var yogaAnchor = $" href=\"{YogaPath}\"";
        var goals = GoalsPage(AllGoalNames).Replace(yogaAnchor, href == "none" ? string.Empty : $" href=\"{href}\"", StringComparison.Ordinal);
        var site = GoodSite().On(HttpMethod.Get, Https + "goals", ScriptedHandler.Html(goals));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo history demo4@example.com: no link for goal page 3", lines[7]);
        Assert.DoesNotContain(site.Requests, r => r.Contains("elsewhere", StringComparison.Ordinal));
        AssertNoDemoHistoryContent(lines);
    }

    [Fact]
    public async Task Demo_history_fails_with_the_status_of_a_goal_page_that_is_not_shown()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + ReadingPath[1..], ScriptedHandler.Status(HttpStatusCode.NotFound));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo history demo4@example.com: GET goal page 1 gave status 404", lines[7]);
        Assert.Single(site.Requests, r => r == "GET " + Https + YogaPath[1..]);
        AssertNoDemoHistoryContent(lines);
    }

    [Fact]
    public async Task Demo_history_fails_when_the_history_card_misses_the_note()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + ReadingPath[1..],
            ScriptedHandler.Html(GoalPage([.. ReadingTexts.Where(t => t != DemoHistory.ReadingNote)])));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo history demo4@example.com: 5 of 6 texts on goal page 1 shown", lines[7]);
        AssertNoDemoHistoryContent(lines);
    }

    [Fact]
    public async Task Demo_history_does_not_count_texts_outside_the_history_card()
    {
        // Yoga's states only in the Settings card and after the History card: none of them counts.
        var site = GoodSite().On(HttpMethod.Get, Https + YogaPath[1..],
            ScriptedHandler.Html(GoalPage(["Mon 2", "Yes"], settings: YogaTexts, after: YogaTexts)));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo history demo4@example.com: 0 of 2 texts on goal page 3 shown", lines[7]);
        AssertNoDemoHistoryContent(lines);
    }

    [Fact]
    public async Task A_demo_account_fails_when_its_goals_page_misses_a_goal_name()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + "goals", ScriptedHandler.Html(GoalsPage(DemoGoalNames.Skip(1))));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo demo1@example.com: 1 of 2 goals shown", lines[4]);
        Assert.Equal("PASS demo demo2@example.com (4 of 4)", lines[5]);
        Assert.Equal("PASS demo demo3@example.com (5 of 5)", lines[6]);
        Assert.DoesNotContain(DemoGoalNames[0], string.Join('\n', lines), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Demo1_fails_when_its_profile_does_not_show_the_account_name_and_the_others_pass()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + "profile", ScriptedHandler.Html(ProfilePage("demo1@example.com")));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo demo1@example.com: account name not shown", lines[4]);
        Assert.Equal("PASS demo demo2@example.com (4 of 4)", lines[5]);
        Assert.Equal("PASS demo demo3@example.com (5 of 5)", lines[6]);
    }

    [Fact]
    public async Task The_first_part_that_misses_is_named_and_every_page_is_still_read()
    {
        var site = GoodSite()
            .On(HttpMethod.Get, Https + "custom-habits", ScriptedHandler.Html(CardsPage([])))
            .On(HttpMethod.Get, Https + "goals", ScriptedHandler.Html(GoalsPage([])))
            .On(HttpMethod.Get, Https + "profile", ScriptedHandler.Html(ProfilePage("Someone else")));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo demo1@example.com: 0 of 2 custom habits shown", lines[4]);
        Assert.Equal("FAIL demo demo2@example.com: 0 of 2 custom habits shown", lines[5]);
        Assert.Equal(4, site.Requests.Count(r => r == "GET " + Https + "goals"));
        Assert.Equal(1, site.Requests.Count(r => r == "GET " + Https + "profile"));
    }

    [Fact]
    public async Task A_goals_page_that_is_not_shown_fails_with_its_status()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + "goals", ScriptedHandler.Status(HttpStatusCode.NotFound));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo demo1@example.com: GET /goals gave status 404", lines[4]);
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
            [
                .. DemoDataService.DemoEmails.Select(e => $"FAIL demo {e}: the sign-in was refused"),
                "FAIL demo history demo4@example.com: the sign-in was refused",
            ],
            lines.Skip(4).Take(4));
        Assert.Matches(NoLaterTiming, lines[8]);
        Assert.Equal("RESULT FAIL", lines[^1]);
    }

    [Fact]
    public async Task A_demo_account_fails_when_its_custom_habits_page_misses_a_name()
    {
        var site = GoodSite().On(HttpMethod.Get, Https + "custom-habits", ScriptedHandler.Html(CardsPage(DemoHabitNames.Skip(1))));

        var (exitCode, lines) = await RunAsync(site);

        Assert.Equal(1, exitCode);
        Assert.Equal("FAIL demo demo1@example.com: 1 of 2 custom habits shown", lines[4]);
        Assert.Equal("PASS demo demo2@example.com (4 of 4)", lines[5]);
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

    /// <summary>A Goals page with one goal item per name, as <c>GoalItem</c> renders it, the i-th linking <see cref="GoalPath"/>(i).</summary>
    private static string GoalsPage(IEnumerable<string> names) =>
        $"""
         <!DOCTYPE html><html><body><h1>Goals</h1>
         {string.Concat(names.Select((n, i) => $"<div class=\"ht-goalitem ht-goalitem--reduced\"><div><a class=\"ht-goalitem__name\" href=\"{GoalPath(i)}\">{WebUtility.HtmlEncode(n)}</a><div class=\"ht-goalitem__meta\"><span>Every day</span></div></div></div>"))}
         </body></html>
         """;

    private static string GoalPath(int index) => string.Create(CultureInfo.InvariantCulture, $"/goals/{index}");

    /// <summary>
    /// A goal page as <c>GoalDetail</c> renders it: a Settings card with <paramref name="settings"/>, then the History card
    /// (<c>HistoryCard</c>), one cell per text of <paramref name="history"/>, then <paramref name="after"/> outside every card.
    /// </summary>
    private static string GoalPage(IEnumerable<string> history, IEnumerable<string>? settings = null, IEnumerable<string>? after = null) =>
        $"""
         <!DOCTYPE html><html><body><h1>Goal</h1>
         <section class="ht-card"><div class="ht-card__head"><h2 class="ht-card__title">Settings</h2></div>
         {Cells(settings ?? ["Changes apply from the next commitment."])}
         </section>
         <section class="ht-card">
           <div class="ht-card__head"><h2 class="ht-card__title">History</h2><span class="ht-spacer"></span>
           <p class="ht-card__sub">Corrections are evaluated with the rules of that date.</p></div>
           <table><tr>{Cells(history)}</tr></table>
         </section>
         {Cells(after ?? [])}
         </body></html>
         """;

    private static string Cells(IEnumerable<string> texts) =>
        string.Concat(texts.Select(t => $"<td><span class=\"ht-chip\">{WebUtility.HtmlEncode(t)}</span></td>\n"));

    /// <summary>No line holds a demo4 goal name, note, amount or goal path (slice 5 HP-008, research R-10).</summary>
    private static void AssertNoDemoHistoryContent(IReadOnlyList<string> lines)
    {
        var text = string.Join('\n', lines);
        Assert.All(
            [.. HistoryGoalNames, DemoHistory.ReadingNote, "12 min", "20 min", ReadingPath, YogaPath, "Settings", "Show all"],
            value => Assert.DoesNotContain(value, text, StringComparison.Ordinal));
    }

    /// <summary>A Profile page whose header's <c>h1</c> is <paramref name="heading"/>, as <c>Profile</c> renders it.</summary>
    private static string ProfilePage(string heading) =>
        $"""<!DOCTYPE html><html><body><div class="ht-profile__head"><h1>{WebUtility.HtmlEncode(heading)}</h1><span class="ht-muted">member since November 2026</span></div></body></html>""";

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
