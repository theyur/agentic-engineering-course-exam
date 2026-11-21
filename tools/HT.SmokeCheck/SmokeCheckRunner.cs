using System.Diagnostics;
using System.Globalization;
using System.Net;
using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Reports;

namespace HT.SmokeCheck;

/// <summary>
/// The checks of contracts/smoke-check.md, in order, over a client factory: each call gives a new client with its own
/// cookies (the tool's handler, or the in-process host's <c>CreateBrowser</c>). Writes one line per check, the
/// <c>TIMING</c> line and the <c>RESULT</c> line. A <c>FAIL</c> reason never contains a password, the administrator
/// email or page content.
/// </summary>
public sealed class SmokeCheckRunner(Func<HttpClient> createClient, SmokeCheckOptions options, TextWriter output)
{
    /// <summary>The first <c>h1</c> of the visitor Home (<c>src/HT.Ui/Components/Pages/Home.razor</c>).</summary>
    public const string VisitorHomeHeading = "Keep the habits you choose";

    /// <summary>How every failed check line starts.</summary>
    public const string FailPrefix = "FAIL ";

    private const string SignInPath = "/sign-in";
    private const string SignInHandler = "sign-in";
    private const string CatalogPath = "/catalog";
    private const string AdminPath = "/admin";
    private const string CustomHabitsPath = "/custom-habits";
    private const string GoalsPath = "/goals";
    private const string ProfilePath = "/profile";

    /// <summary>The title of the goal page's History card (<c>HistoryCard.razor</c>), whose text check 6 reads.</summary>
    private const string HistoryTitle = "History";

    /// <summary>The GoalStats card's consistency label (<c>GoalStats.razor</c>), which check 6 finds (slice 6 HP-008).</summary>
    private const string ConsistencyLabel = "Consistency";

    /// <summary>The Home's StatTile labels (<c>TodayPanel.razor</c>) that checks 6 and 7 find (slice 6 HP-008).</summary>
    private const string DueNowLabel = "Due now";

    private const string MissedThisWeekLabel = "Missed this week";

    /// <summary>The position of "Strength training, 2 times a week" in <see cref="DemoReport.GoalNames"/>, whose page check 7 reads.</summary>
    private const int StrengthTrainingIndex = 1;

    private const string HomePath = "/";

    /// <summary>The Goals page's Paused tab (slice 7 HP-004), which check 8 reads.</summary>
    private const string PausedTabPath = "/goals?tab=paused";

    /// <summary>The position of "Meditation, 10 min daily" in <see cref="DemoPauses.GoalNames"/>, whose page check 8 reads.</summary>
    private const int MeditationIndex = 0;

    /// <summary>The position of "Drinking water, 8 glasses a day" in <see cref="DemoPauses.GoalNames"/>, which check 8 finds on the Paused tab.</summary>
    private const int DrinkingWaterIndex = 1;

    /// <summary>
    /// The goal pages of check 6 and their expected texts, all from <c>HT.Application</c> (slice 5 research R-10): the
    /// History card of "Reading" (<see cref="DemoHistory.GoalNames"/>[0], a daily commitment) holds Met, Failed, Pending,
    /// Due today, the partial amount and the note; the one of "Yoga" ([2], a per-week period) holds Failed and Missed.
    /// On any weekday these lie in the card's first 10 rows. Slice 6 (HP-008, research R-11): "Reading"'s page also has
    /// the stat labels "Consistency" and "Streak, commitments", matched only among its stat labels.
    /// </summary>
    private static readonly IReadOnlyList<HistoryPage> HistoryPages =
    [
        new(0,
        [
            RecordingText.State(EvaluationState.Met, GoalScheduleKind.CommitmentBased, null),
            RecordingText.State(EvaluationState.Failed, GoalScheduleKind.CommitmentBased, null),
            RecordingText.State(EvaluationState.Pending, GoalScheduleKind.CommitmentBased, null),
            RecordingText.State(EvaluationState.Due, GoalScheduleKind.CommitmentBased, null),
            RecordingText.Amount(Measurement.Duration, null, DemoHistory.PartialMinutes),
            DemoHistory.ReadingNote,
        ],
        [ConsistencyLabel, ReportText.StreakLabel(StreakUnit.Commitments)]),
        new(2,
        [
            RecordingText.State(EvaluationState.Failed, GoalScheduleKind.PeriodBased, SchedulePeriod.Week),
            RecordingText.State(EvaluationState.Missed, GoalScheduleKind.PeriodBased, SchedulePeriod.Week),
        ],
        []),
    ];

    /// <summary>The stat label check 7 finds on "Strength training"'s page: "Streak, weeks" (slice 6 HP-008, research R-11).</summary>
    private static readonly string WeeksStreakLabel = ReportText.StreakLabel(StreakUnit.Weeks);

    /// <summary>The StatusChip word check 7 finds on demo5's Home: "Missed" (slice 6 HP-008).</summary>
    private static readonly string MissedChip = ReportText.State(EvaluationState.Missed, GoalScheduleKind.PeriodBased, SchedulePeriod.Day);

    /// <summary>The StatusChip word check 8 finds on "Meditation"'s page: "Excused" (slice 7 HP-011).</summary>
    private static readonly string ExcusedChip = ReportText.State(EvaluationState.Excused, GoalScheduleKind.CommitmentBased, null);

    /// <summary>
    /// How the meta text of a paused goal starts, "Paused until " (slice 7 HP-004, HP-011): <see cref="ReportText.PausedUntil"/>
    /// without its date, so the text is not typed in the tool and the check holds whatever the hosted run date is.
    /// </summary>
    private static readonly string PausedUntilPrefix = PausedUntilWithoutDate(new DateOnly(2026, 11, 22));

    /// <summary>The pause between two tries of check 1, shortened to the warm-up time that is left.</summary>
    private static readonly TimeSpan RetryPause = TimeSpan.FromSeconds(5);

    private static readonly HashSet<HttpStatusCode> RedirectStatuses =
    [
        HttpStatusCode.MovedPermanently,
        HttpStatusCode.Found,
        HttpStatusCode.TemporaryRedirect,
        HttpStatusCode.PermanentRedirect,
    ];

    private readonly Func<HttpClient> _createClient = createClient ?? throw new ArgumentNullException(nameof(createClient));
    private readonly SmokeCheckOptions _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly TextWriter _output = output ?? throw new ArgumentNullException(nameof(output));

    /// <summary>Runs every check, writes the lines and returns the outcome.</summary>
    public async Task<SmokeCheckResult> RunAsync()
    {
        var lines = new List<string>();

        var home = await CheckHomeAsync().ConfigureAwait(false);
        await WriteCheckAsync(lines, home.Line).ConfigureAwait(false);
        await WriteCheckAsync(lines, await CheckHttpsRedirectAsync().ConfigureAwait(false)).ConfigureAwait(false);

        await WriteCheckAsync(lines, await CheckCatalogAsync().ConfigureAwait(false)).ConfigureAwait(false);
        await WriteCheckAsync(lines, await CheckAdministratorAsync().ConfigureAwait(false)).ConfigureAwait(false);

        // The "later" timing comes from the first demo account that signs in.
        LaterTiming? later = null;
        foreach (var email in DemoDataService.DemoEmails)
        {
            var demo = await CheckDemoAccountAsync(email, measureLater: later is null).ConfigureAwait(false);
            await WriteCheckAsync(lines, demo.Line).ConfigureAwait(false);
            later ??= demo.Later;
        }

        await WriteCheckAsync(lines, await CheckDemoHistoryAsync().ConfigureAwait(false)).ConfigureAwait(false);
        await WriteCheckAsync(lines, await CheckDemoReportAsync().ConfigureAwait(false)).ConfigureAwait(false);
        await WriteCheckAsync(lines, await CheckDemoPauseAsync().ConfigureAwait(false)).ConfigureAwait(false);

        var laterMilliseconds = later?.Milliseconds;
        var laterRequests = later?.Requests ?? 0;
        var timing = TimingLine(home.FirstMilliseconds, home.FirstRetried, laterMilliseconds, laterRequests);
        var result = new SmokeCheckResult(lines, timing, home.FirstMilliseconds, home.FirstRetried, laterMilliseconds, laterRequests);
        await _output.WriteLineAsync(timing).ConfigureAwait(false);
        await _output.WriteLineAsync(result.Passed ? "RESULT PASS" : "RESULT FAIL").ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// The <c>TIMING</c> line (contracts/smoke-check.md "Timings"): <c>first</c> with <c>(retried)</c> when check 1's
    /// first request did not get the visitor Home, and <c>later</c>, or <c>later n/a</c> when no demo account signed in.
    /// </summary>
    internal static string TimingLine(long firstMilliseconds, bool firstRetried, long? laterMilliseconds, int laterRequests)
    {
        var first = string.Create(CultureInfo.InvariantCulture, $"first {firstMilliseconds} ms{(firstRetried ? " (retried)" : string.Empty)}");
        var later = laterMilliseconds is { } milliseconds
            ? string.Create(CultureInfo.InvariantCulture, $"later {milliseconds} ms ({laterRequests} requests)")
            : "later n/a";
        return $"TIMING {first}, {later}";
    }

    private async Task WriteCheckAsync(List<string> lines, string line)
    {
        lines.Add(line);
        await _output.WriteLineAsync(line).ConfigureAwait(false);
    }

    /// <summary>
    /// Check 1 <c>home</c>: <c>GET /</c> until it returns 200 with the visitor Home, or until the warm-up time ends.
    /// Records the duration of the first request, from sending to the full response, whether it succeeded or not.
    /// </summary>
    private async Task<(string Line, long FirstMilliseconds, bool FirstRetried)> CheckHomeAsync()
    {
        using var client = _createClient();
        var clock = Stopwatch.StartNew();
        long? firstMilliseconds = null;
        var firstSucceeded = false;

        while (true)
        {
            var started = clock.Elapsed;
            var problem = await TryHomeAsync(client).ConfigureAwait(false);
            if (firstMilliseconds is null)
            {
                firstMilliseconds = (long)(clock.Elapsed - started).TotalMilliseconds;
                firstSucceeded = problem is null;
            }

            if (problem is null)
            {
                return ("PASS home", firstMilliseconds.Value, !firstSucceeded);
            }

            var remaining = _options.WarmUp - clock.Elapsed;
            if (remaining <= TimeSpan.Zero)
            {
                var seconds = (long)_options.WarmUp.TotalSeconds;
                return (string.Create(CultureInfo.InvariantCulture, $"{FailPrefix}home: no visitor Home within {seconds} s (last try: {problem})"),
                    firstMilliseconds.Value, !firstSucceeded);
            }

            await Task.Delay(remaining < RetryPause ? remaining : RetryPause).ConfigureAwait(false);
        }
    }

    /// <summary>One try of check 1: null when <c>GET /</c> gave the visitor Home, otherwise what it gave instead.</summary>
    private static async Task<string?> TryHomeAsync(HttpClient client)
    {
        try
        {
            using var response = await client.GetAsync(new Uri("/", UriKind.Relative)).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return StatusText(response.StatusCode);
            }

            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return PageReader.ReadFirstHeading(html) == VisitorHomeHeading ? null : "200 without the visitor Home";
        }
        catch (HttpRequestException exception)
        {
            return $"no response ({exception.HttpRequestError})";
        }
        catch (TaskCanceledException)
        {
            return "no response in time";
        }
    }

    /// <summary>
    /// Check 2 <c>https-redirect</c>: <c>GET http://&lt;host&gt;/</c> returns 301, 302, 307 or 308 with
    /// <c>Location</c> <c>https://&lt;host&gt;/</c>. Skipped with <c>--skip-http-redirect</c>.
    /// </summary>
    private async Task<string> CheckHttpsRedirectAsync()
    {
        const string name = "https-redirect";
        if (_options.SkipHttpRedirect)
        {
            return $"SKIP {name}";
        }

        var httpAddress = new UriBuilder(_options.BaseUrl) { Scheme = Uri.UriSchemeHttp, Port = -1, Path = "/" }.Uri;
        try
        {
            using var client = _createClient();
            using var response = await client.GetAsync(httpAddress).ConfigureAwait(false);
            if (!RedirectStatuses.Contains(response.StatusCode))
            {
                return $"{FailPrefix}{name}: {StatusText(response.StatusCode)}, not a redirect";
            }

            return IsHttpsRoot(response.Headers.Location)
                ? $"PASS {name}"
                : $"{FailPrefix}{name}: the redirect does not go to the https:// address";
        }
        catch (HttpRequestException exception)
        {
            return $"{FailPrefix}{name}: no response ({exception.HttpRequestError})";
        }
        catch (TaskCanceledException)
        {
            return $"{FailPrefix}{name}: no response in time";
        }
    }

    /// <summary>True when the location is the root of the published https:// address.</summary>
    private bool IsHttpsRoot(Uri? location) =>
        location is { IsAbsoluteUri: true }
        && location.Scheme == Uri.UriSchemeHttps
        && string.Equals(location.Host, _options.BaseUrl.Host, StringComparison.OrdinalIgnoreCase)
        && location.Port == _options.BaseUrl.Port
        && location.AbsolutePath == "/";

    /// <summary>
    /// Check 3 <c>catalog</c>: anonymous <c>GET /catalog</c> shows the name of every Published habit of
    /// <see cref="SeedCatalog.Habits"/>, whatever else it shows.
    /// </summary>
    private async Task<string> CheckCatalogAsync()
    {
        const string name = "catalog";
        var expected = SeedCatalog.Habits
            .Where(habit => habit.State == CatalogHabitState.Published)
            .Select(habit => habit.Definition.Name.Value)
            .ToList();

        return await GuardAsync(name, async () =>
        {
            using var client = _createClient();
            using var response = await client.GetAsync(new Uri(CatalogPath, UriKind.Relative)).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return $"{FailPrefix}{name}: GET {CatalogPath} gave {StatusText(response.StatusCode)}";
            }

            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return NamesLine(name, PageReader.ReadHabitNames(html), expected, "Published seed habits");
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Check 4 <c>administrator</c>: the configured administrator signs in on a new client and <c>GET /admin</c>
    /// returns 200. The line names the word <c>administrator</c>, never the email.
    /// </summary>
    private async Task<string> CheckAdministratorAsync()
    {
        const string name = "administrator";
        return await GuardAsync(name, async () =>
        {
            using var client = _createClient();
            if (!await SignInAsync(client, _options.AdministratorEmail, _options.AdministratorPassword).ConfigureAwait(false))
            {
                return $"{FailPrefix}{name}: the sign-in was refused";
            }

            using var response = await client.GetAsync(new Uri(AdminPath, UriKind.Relative)).ConfigureAwait(false);
            return response.StatusCode == HttpStatusCode.OK
                ? $"PASS {name}"
                : $"{FailPrefix}{name}: GET {AdminPath} gave {StatusText(response.StatusCode)}";
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Check 5 <c>demo &lt;email&gt;</c>: the demo account signs in on a new client with the demo password, then
    /// <c>GET /custom-habits</c> shows the name of every habit of <see cref="DemoCustomHabits.For"/>, <c>GET /goals</c>
    /// the name of every goal of <see cref="DemoGoals.DisplayNames"/>, and, for an account with a name
    /// (<see cref="DemoAccounts.NameFor"/>, demo1), <c>GET /profile</c> that name as its first heading (slice 4
    /// HP-018). Every page is read; the line counts the expected names of all of them, and a <c>FAIL</c> names the first
    /// part that misses, in that order. With <paramref name="measureLater"/>, also measures the <c>later</c> timing
    /// (contracts/smoke-check.md "Timings"): <c>GET /custom-habits</c> together with every stylesheet and script the
    /// page links, fetched one after another with the account's cookie.
    /// </summary>
    private async Task<(string Line, LaterTiming? Later)> CheckDemoAccountAsync(string email, bool measureLater)
    {
        var name = $"demo {email}";
        var expectedHabits = DemoCustomHabits.For(email).Select(habit => habit.Name.Value).ToList();
        var expectedGoals = DemoGoals.DisplayNames(email);
        var expectedAccountName = DemoAccounts.NameFor(email);
        LaterTiming? later = null;

        var line = await GuardAsync(name, async () =>
        {
            using var client = _createClient();
            if (!await SignInAsync(client, email, _options.DemoPassword).ConfigureAwait(false))
            {
                return $"{FailPrefix}{name}: the sign-in was refused";
            }

            var clock = Stopwatch.StartNew();
            var (habitsStatus, habitsHtml) = await GetPageAsync(client, CustomHabitsPath).ConfigureAwait(false);
            if (measureLater)
            {
                var assets = await FetchAssetsAsync(client, habitsHtml).ConfigureAwait(false);
                later = new LaterTiming((long)clock.Elapsed.TotalMilliseconds, 1 + assets);
            }

            List<NamesPart> parts =
            [
                NamesPart.Of(CustomHabitsPath, habitsStatus, PageReader.ReadHabitNames(habitsHtml), expectedHabits, "custom habits"),
            ];

            var (goalsStatus, goalsHtml) = await GetPageAsync(client, GoalsPath).ConfigureAwait(false);
            parts.Add(NamesPart.Of(GoalsPath, goalsStatus, PageReader.ReadGoalNames(goalsHtml), expectedGoals, "goals"));

            if (expectedAccountName is not null)
            {
                var (profileStatus, profileHtml) = await GetPageAsync(client, ProfilePath).ConfigureAwait(false);
                parts.Add(NamesPart.OfAccountName(profileStatus, PageReader.ReadFirstHeading(profileHtml), expectedAccountName));
            }

            return PartsLine(name, parts);
        }).ConfigureAwait(false);

        return (line, later);
    }

    /// <summary>
    /// Check 6 <c>demo history &lt;email&gt;</c> (slice 5 HP-008, SC-005, research R-10): <see cref="DemoHistoryService.DemoEmail"/>
    /// signs in on a new client with the demo password, <c>GET /goals</c> shows the 3 names of
    /// <see cref="DemoHistory.GoalNames"/>, and the History card of each goal page of <see cref="HistoryPages"/>, reached
    /// through the goal's link on Goals, contains its expected texts as ordinal substrings: 11 expected texts. Slice 6
    /// (HP-008, research R-11) adds 3: the stat labels "Consistency" and "Streak, commitments" among the stat labels of
    /// "Reading"'s page, and "Due now" among the Home's tile labels, each matched whole: 14 in all. Every page is read; a
    /// <c>FAIL</c> names the first part that misses, in that order. A goal page is named by its position in
    /// <see cref="DemoHistory.GoalNames"/>, never by its name or its path, which holds an identifier read from the page;
    /// no name, note or amount is printed.
    /// </summary>
    private async Task<string> CheckDemoHistoryAsync()
    {
        var name = $"demo history {DemoHistoryService.DemoEmail}";
        return await GuardAsync(name, async () =>
        {
            using var client = _createClient();
            if (!await SignInAsync(client, DemoHistoryService.DemoEmail, _options.DemoPassword).ConfigureAwait(false))
            {
                return $"{FailPrefix}{name}: the sign-in was refused";
            }

            var (goalsStatus, goalsHtml) = await GetPageAsync(client, GoalsPath).ConfigureAwait(false);
            IReadOnlyList<GoalLink> links = goalsStatus == HttpStatusCode.OK ? PageReader.ReadGoalLinks(goalsHtml) : [];
            List<NamesPart> parts =
            [
                NamesPart.Of(GoalsPath, goalsStatus, [.. links.Select(link => link.Name)], DemoHistory.GoalNames, "goal names"),
            ];

            foreach (var page in HistoryPages)
            {
                parts.AddRange(await CheckHistoryPageAsync(client, links, page).ConfigureAwait(false));
            }

            var (homeStatus, homeHtml) = await GetPageAsync(client, HomePath).ConfigureAwait(false);
            parts.Add(LabelsPart(HomePath, homeStatus, () => PageReader.ReadTileLabels(homeHtml), [DueNowLabel], "tile labels on the Home"));
            return PartsLine(name, parts);
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// One goal page of check 6: its link on Goals (the first goal item with the goal's name, whose <c>href</c> is a path
    /// on this site), then its History card's text (<see cref="PageReader.ReadCardText"/>) holding each expected text,
    /// then, when the page has any, its expected stat labels (<see cref="PageReader.ReadStatLabels"/>).
    /// </summary>
    private static async Task<IReadOnlyList<NamesPart>> CheckHistoryPageAsync(HttpClient client, IReadOnlyList<GoalLink> links, HistoryPage page)
    {
        var position = (page.Index + 1).ToString(CultureInfo.InvariantCulture);
        var expected = page.Texts.Count + page.StatLabels.Count;
        var (problem, html) = await GetGoalPageAsync(client, links, DemoHistory.GoalNames[page.Index], position).ConfigureAwait(false);
        if (problem is not null)
        {
            return [new NamesPart(0, expected, problem)];
        }

        var card = PageReader.ReadCardText(html, HistoryTitle) ?? string.Empty;
        var found = page.Texts.Count(text => card.Contains(text, StringComparison.Ordinal));
        List<NamesPart> parts =
        [
            found == page.Texts.Count
                ? new NamesPart(found, page.Texts.Count, null)
                : new NamesPart(found, page.Texts.Count, string.Create(CultureInfo.InvariantCulture, $"{found} of {page.Texts.Count} texts on goal page {position} shown")),
        ];
        if (page.StatLabels.Count > 0)
        {
            parts.Add(LabelsPart("goal page", HttpStatusCode.OK, () => PageReader.ReadStatLabels(html), page.StatLabels, $"stat labels on goal page {position}"));
        }

        return parts;
    }

    /// <summary>
    /// Check 7 <c>demo report &lt;email&gt;</c> (slice 6 HP-008, research R-11): <see cref="DemoReportService.DemoEmail"/>
    /// signs in on a new client with the demo password; <c>GET /goals</c> shows the 3 names of
    /// <see cref="DemoReport.GoalNames"/>; the page of "Strength training, 2 times a week", reached through its link on
    /// Goals, has the stat label "Streak, weeks"; and the Home has "Missed this week" among its tile labels and "Missed"
    /// among its status chips: 6 expected texts, each matched whole. Every page is read; a <c>FAIL</c> names the first
    /// part that misses, in that order. No name, note, figure or goal path is printed.
    /// </summary>
    private async Task<string> CheckDemoReportAsync()
    {
        var name = $"demo report {DemoReportService.DemoEmail}";
        return await GuardAsync(name, async () =>
        {
            using var client = _createClient();
            if (!await SignInAsync(client, DemoReportService.DemoEmail, _options.DemoPassword).ConfigureAwait(false))
            {
                return $"{FailPrefix}{name}: the sign-in was refused";
            }

            var (goalsStatus, goalsHtml) = await GetPageAsync(client, GoalsPath).ConfigureAwait(false);
            IReadOnlyList<GoalLink> links = goalsStatus == HttpStatusCode.OK ? PageReader.ReadGoalLinks(goalsHtml) : [];
            List<NamesPart> parts =
            [
                NamesPart.Of(GoalsPath, goalsStatus, [.. links.Select(link => link.Name)], DemoReport.GoalNames, "goal names"),
            ];

            var position = (StrengthTrainingIndex + 1).ToString(CultureInfo.InvariantCulture);
            var (problem, strengthHtml) = await GetGoalPageAsync(client, links, DemoReport.GoalNames[StrengthTrainingIndex], position).ConfigureAwait(false);
            parts.Add(problem is not null
                ? new NamesPart(0, 1, problem)
                : LabelsPart("goal page", HttpStatusCode.OK, () => PageReader.ReadStatLabels(strengthHtml), [WeeksStreakLabel], $"stat labels on goal page {position}"));

            var (homeStatus, homeHtml) = await GetPageAsync(client, HomePath).ConfigureAwait(false);
            parts.Add(LabelsPart(HomePath, homeStatus, () => PageReader.ReadTileLabels(homeHtml), [MissedThisWeekLabel], "tile labels on the Home"));
            parts.Add(LabelsPart(HomePath, homeStatus, () => PageReader.ReadStatusChips(homeHtml), [MissedChip], "status chips on the Home"));
            return PartsLine(name, parts);
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Check 8 <c>demo pause &lt;email&gt;</c> (slice 7 HP-011): <see cref="DemoPauseService.DemoEmail"/> signs in on a new
    /// client with the demo password; the page of "Meditation, 10 min daily" (<see cref="DemoPauses.GoalNames"/>[0]),
    /// reached through its link on <c>GET /goals</c>, has "Excused" among its status chips; and the Paused tab
    /// <c>GET /goals?tab=paused</c> shows the goal "Drinking water, 8 glasses a day" ([1]) with a meta text that starts
    /// "Paused until" in the same goal item: 3 expected texts, the chip and the name matched whole. Every page is read; a
    /// <c>FAIL</c> names the first part that misses, in that order. No goal name, reason, note, figure, date or goal path
    /// is printed.
    /// </summary>
    private async Task<string> CheckDemoPauseAsync()
    {
        var name = $"demo pause {DemoPauseService.DemoEmail}";
        return await GuardAsync(name, async () =>
        {
            using var client = _createClient();
            if (!await SignInAsync(client, DemoPauseService.DemoEmail, _options.DemoPassword).ConfigureAwait(false))
            {
                return $"{FailPrefix}{name}: the sign-in was refused";
            }

            List<NamesPart> parts = [await CheckExcusedChipAsync(client).ConfigureAwait(false)];

            var pausedGoal = DemoPauses.GoalNames[DrinkingWaterIndex];
            var (pausedStatus, pausedHtml) = await GetPageAsync(client, PausedTabPath).ConfigureAwait(false);
            IReadOnlyList<GoalMeta> metas = pausedStatus == HttpStatusCode.OK ? PageReader.ReadGoalMetas(pausedHtml) : [];
            parts.Add(NamesPart.Of(PausedTabPath, pausedStatus, [.. metas.Select(meta => meta.Name)], [pausedGoal], "goal names on the Paused tab"));
            parts.Add(metas.Any(meta => meta.Name == pausedGoal && meta.Meta is { } text && text.StartsWith(PausedUntilPrefix, StringComparison.Ordinal))
                ? new NamesPart(1, 1, null)
                : new NamesPart(0, 1, "0 of 1 paused goal metas on the Paused tab shown"));
            return PartsLine(name, parts);
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// The first part of check 8: <c>GET /goals</c>, then the page of "Meditation" through its link there, then "Excused"
    /// among that page's status chips (<see cref="PageReader.ReadStatusChips"/>). The page is named by its position only.
    /// </summary>
    private static async Task<NamesPart> CheckExcusedChipAsync(HttpClient client)
    {
        var (goalsStatus, goalsHtml) = await GetPageAsync(client, GoalsPath).ConfigureAwait(false);
        if (goalsStatus != HttpStatusCode.OK)
        {
            return new NamesPart(0, 1, $"GET {GoalsPath} gave {StatusText(goalsStatus)}");
        }

        var position = (MeditationIndex + 1).ToString(CultureInfo.InvariantCulture);
        var (problem, html) = await GetGoalPageAsync(client, PageReader.ReadGoalLinks(goalsHtml), DemoPauses.GoalNames[MeditationIndex], position)
            .ConfigureAwait(false);
        return problem is not null
            ? new NamesPart(0, 1, problem)
            : LabelsPart("goal page", HttpStatusCode.OK, () => PageReader.ReadStatusChips(html), [ExcusedChip], $"status chips on goal page {position}");
    }

    /// <summary>"Paused until Sun 22 Nov" without "Sun 22 Nov": the text before the date, whatever the date is.</summary>
    private static string PausedUntilWithoutDate(DateOnly lastDay)
    {
        var text = ReportText.PausedUntil(lastDay);
        return text[..^DateTexts.ShortDate(lastDay).Length];
    }

    /// <summary>
    /// A goal page reached through its link on Goals: the first goal item with <paramref name="goalName"/> whose
    /// <c>href</c> is a path on this site. Gives a reason that names the page by its <paramref name="position"/> only, or
    /// null and the page's HTML.
    /// </summary>
    private static async Task<(string? Problem, string Html)> GetGoalPageAsync(
        HttpClient client, IReadOnlyList<GoalLink> links, string goalName, string position)
    {
        var href = links.FirstOrDefault(link => string.Equals(link.Name, goalName, StringComparison.Ordinal))?.Href;
        if (href is null || !IsSitePath(href))
        {
            return ($"no link for goal page {position}", string.Empty);
        }

        var (status, html) = await GetPageAsync(client, href).ConfigureAwait(false);
        return status == HttpStatusCode.OK
            ? (null, html)
            : ($"GET goal page {position} gave {StatusText(status)}", string.Empty);
    }

    /// <summary>
    /// The part of a check that finds whole labels or chips (<see cref="PageReader.ContainsAll"/>, ordinal): the page's
    /// status first, then how many of <paramref name="expected"/> are among the texts read. The reason holds counts and
    /// a path only, never a label read from the page.
    /// </summary>
    private static NamesPart LabelsPart(
        string path, HttpStatusCode status, Func<IReadOnlyList<string>> read, IReadOnlyList<string> expected, string what)
    {
        if (status != HttpStatusCode.OK)
        {
            return new NamesPart(0, expected.Count, $"GET {path} gave {StatusText(status)}");
        }

        var found = read();
        var present = found.ToHashSet(StringComparer.Ordinal);
        var count = expected.Count(present.Contains);
        return PageReader.ContainsAll(found, expected)
            ? new NamesPart(count, expected.Count, null)
            : new NamesPart(count, expected.Count, string.Create(CultureInfo.InvariantCulture, $"{count} of {expected.Count} {what} shown"));
    }

    /// <summary>True for a path on this site (<c>/goals/7</c>), never an absolute or protocol-relative address.</summary>
    private static bool IsSitePath(string href) =>
        href.StartsWith('/') && !href.StartsWith("//", StringComparison.Ordinal) && !href.StartsWith("/\\", StringComparison.Ordinal);

    /// <summary>A page's status and its HTML; the HTML of a status other than 200 is not read for names.</summary>
    private static async Task<(HttpStatusCode Status, string Html)> GetPageAsync(HttpClient client, string path)
    {
        using var response = await client.GetAsync(new Uri(path, UriKind.Relative)).ConfigureAwait(false);
        return (response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    /// <summary>
    /// <c>PASS &lt;name&gt; (&lt;found&gt; of &lt;expected&gt;)</c>, counting the expected names of every part together, when
    /// no part misses; otherwise a <c>FAIL</c> with the first part's reason. No name is printed.
    /// </summary>
    private static string PartsLine(string name, IReadOnlyList<NamesPart> parts)
    {
        if (parts.FirstOrDefault(part => part.Problem is not null) is { Problem: { } problem })
        {
            return $"{FailPrefix}{name}: {problem}";
        }

        var found = parts.Sum(part => part.Found);
        var expected = parts.Sum(part => part.Expected);
        return string.Create(CultureInfo.InvariantCulture, $"PASS {name} ({found} of {expected})");
    }

    /// <summary>
    /// Fetches every stylesheet and script the page links, one after another with this client, as a browser without a
    /// cache does; returns how many requests were sent. A failed asset never fails a check: timings only report.
    /// </summary>
    private async Task<int> FetchAssetsAsync(HttpClient client, string html)
    {
        var requests = 0;
        foreach (var url in PageReader.ReadAssetUrls(html))
        {
            if (!Uri.TryCreate(_options.BaseUrl, url, out var address))
            {
                continue;
            }

            requests++;
            try
            {
                using var response = await client.GetAsync(address).ConfigureAwait(false);
                await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                // Reported through the timing only.
            }
            catch (TaskCanceledException)
            {
                // Reported through the timing only.
            }
        }

        return requests;
    }

    /// <summary>
    /// <c>PASS &lt;name&gt; (&lt;found&gt; of &lt;expected&gt;)</c> when every expected name is on the page, otherwise a
    /// <c>FAIL</c> with the counts only: names are compared in memory and never printed.
    /// </summary>
    private static string NamesLine(string name, IReadOnlyList<string> found, IReadOnlyList<string> expected, string what)
    {
        var present = found.ToHashSet(StringComparer.Ordinal);
        var count = expected.Count(present.Contains);
        return PageReader.ContainsAll(found, expected)
            ? string.Create(CultureInfo.InvariantCulture, $"PASS {name} ({count} of {expected.Count})")
            : string.Create(CultureInfo.InvariantCulture, $"{FailPrefix}{name}: {count} of {expected.Count} {what} shown");
    }

    /// <summary>Runs one check; a request that gets no response fails it with a reason that holds no value.</summary>
    private static async Task<string> GuardAsync(string name, Func<Task<string>> check)
    {
        try
        {
            return await check().ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            return $"{FailPrefix}{name}: no response ({exception.HttpRequestError})";
        }
        catch (TaskCanceledException)
        {
            return $"{FailPrefix}{name}: no response in time";
        }
    }

    /// <summary>
    /// Signs in through the sign-in page as a browser does: <c>GET /sign-in</c>, its antiforgery token and the
    /// <c>sign-in</c> form handler, then <c>POST /sign-in</c>. True when the answer is a redirect that does not go back
    /// to <c>/sign-in</c>; the client then holds the session cookie. Used by checks 4, 5, 6, 7 and 8.
    /// </summary>
    private static async Task<bool> SignInAsync(HttpClient client, string email, string password)
    {
        using var page = await client.GetAsync(new Uri(SignInPath, UriKind.Relative)).ConfigureAwait(false);
        if (page.StatusCode != HttpStatusCode.OK)
        {
            return false;
        }

        var html = await page.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!PageReader.ReadHandlers(html).Contains(SignInHandler, StringComparer.Ordinal))
        {
            return false;
        }

        string token;
        try
        {
            token = PageReader.ReadToken(html, SignInPath);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        using var form = new FormUrlEncodedContent(
        [
            new("Input.Email", email),
            new("Input.Password", password),
            new("__RequestVerificationToken", token),
            new("_handler", SignInHandler),
        ]);
        using var response = await client.PostAsync(new Uri(SignInPath, UriKind.Relative), form).ConfigureAwait(false);
        return RedirectStatuses.Contains(response.StatusCode)
            && response.Headers.Location is { } location
            && !IsSignInPage(location);
    }

    private static bool IsSignInPage(Uri location)
    {
        var absolute = location.IsAbsoluteUri ? location : new Uri(new Uri("https://localhost/"), location);
        return string.Equals(absolute.AbsolutePath, SignInPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string StatusText(HttpStatusCode status) =>
        string.Create(CultureInfo.InvariantCulture, $"status {(int)status}");

    /// <summary>The <c>later</c> timing: its duration and how many requests it covers.</summary>
    private readonly record struct LaterTiming(long Milliseconds, int Requests);

    /// <summary>
    /// A goal page of check 6: the goal's index in <see cref="DemoHistory.GoalNames"/>, its History card's expected texts
    /// and its expected stat labels (slice 6).
    /// </summary>
    private sealed record HistoryPage(int Index, IReadOnlyList<string> Texts, IReadOnlyList<string> StatLabels);

    /// <summary>
    /// One page of check 5: how many of its expected names it shows, how many it should, and the reason it misses, or
    /// null. The reason holds counts and a path only, never a name.
    /// </summary>
    private sealed record NamesPart(int Found, int Expected, string? Problem)
    {
        public static NamesPart Of(
            string path, HttpStatusCode status, IReadOnlyList<string> found, IReadOnlyList<string> expected, string what)
        {
            if (status != HttpStatusCode.OK)
            {
                return new NamesPart(0, expected.Count, $"GET {path} gave {StatusText(status)}");
            }

            var present = found.ToHashSet(StringComparer.Ordinal);
            var count = expected.Count(present.Contains);
            return count == expected.Count
                ? new NamesPart(count, expected.Count, null)
                : new NamesPart(count, expected.Count, string.Create(CultureInfo.InvariantCulture, $"{count} of {expected.Count} {what} shown"));
        }

        public static NamesPart OfAccountName(HttpStatusCode status, string? heading, string expected)
        {
            if (status != HttpStatusCode.OK)
            {
                return new NamesPart(0, 1, $"GET {ProfilePath} gave {StatusText(status)}");
            }

            return string.Equals(heading, expected, StringComparison.Ordinal)
                ? new NamesPart(1, 1, null)
                : new NamesPart(0, 1, "account name not shown");
        }
    }
}
