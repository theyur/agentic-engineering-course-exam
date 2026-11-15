using System.Diagnostics;
using System.Globalization;
using System.Net;
using HT.Application.Operator;
using HT.Domain.Habits;

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
    /// Check 5 <c>demo &lt;email&gt;</c>: the demo account signs in on a new client with the demo password and
    /// <c>GET /custom-habits</c> shows the name of every habit of <see cref="DemoCustomHabits.For"/>. With
    /// <paramref name="measureLater"/>, also measures the <c>later</c> timing (contracts/smoke-check.md "Timings"):
    /// that request together with every stylesheet and script the page links, fetched one after another with the
    /// account's cookie.
    /// </summary>
    private async Task<(string Line, LaterTiming? Later)> CheckDemoAccountAsync(string email, bool measureLater)
    {
        var name = $"demo {email}";
        var expected = DemoCustomHabits.For(email).Select(habit => habit.Name.Value).ToList();
        LaterTiming? later = null;

        var line = await GuardAsync(name, async () =>
        {
            using var client = _createClient();
            if (!await SignInAsync(client, email, _options.DemoPassword).ConfigureAwait(false))
            {
                return $"{FailPrefix}{name}: the sign-in was refused";
            }

            var clock = Stopwatch.StartNew();
            using var response = await client.GetAsync(new Uri(CustomHabitsPath, UriKind.Relative)).ConfigureAwait(false);
            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (measureLater)
            {
                var assets = await FetchAssetsAsync(client, html).ConfigureAwait(false);
                later = new LaterTiming((long)clock.Elapsed.TotalMilliseconds, 1 + assets);
            }

            return response.StatusCode == HttpStatusCode.OK
                ? NamesLine(name, PageReader.ReadHabitNames(html), expected, "custom habits")
                : $"{FailPrefix}{name}: GET {CustomHabitsPath} gave {StatusText(response.StatusCode)}";
        }).ConfigureAwait(false);

        return (line, later);
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
    /// to <c>/sign-in</c>; the client then holds the session cookie. Used by checks 4 and 5.
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
}
