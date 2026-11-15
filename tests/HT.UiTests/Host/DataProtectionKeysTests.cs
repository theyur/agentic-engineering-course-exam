using System.Net;
using HT.Application.Operator;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HT.UiTests.Host;

/// <summary>
/// HP-018, US1-AS6 (research R-07, R-11): with <c>DataProtection:KeysDirectory</c> set, the host keeps its session
/// keys in that directory, so a session signed in on one host is accepted by the next host over the same directory,
/// as after a restart of the hosted app. Without the setting nothing is written there. Database <c>HT-UI-KEYS</c>,
/// reset once; the second host runs without a reset, so the account still exists.
/// </summary>
[Collection(DerivedDatabaseHostCollection.Name)]
public sealed class DataProtectionKeysTests : IDisposable
{
    private const string DatabaseName = "HT-UI-KEYS";
    private const string KeysDirectoryKey = "DataProtection:KeysDirectory";
    private const string CookieName = ".AspNetCore.Identity.Application";

    private readonly List<string> _directories = [];

    [Fact]
    public async Task HP_018_a_session_survives_a_restart_over_the_same_key_directory()
    {
        await DerivedDatabaseHost.ResetAsync(DatabaseName);
        var keys = NewDirectory();
        var demoEmail = DemoDataService.DemoEmails[0];

        string sessionCookie;
        await using (var first = new DerivedDatabaseHost(DatabaseName, Settings(keys)))
        {
            using var browser = first.CreateBrowser();
            sessionCookie = await SignInAsync(browser, demoEmail);

            Assert.NotEmpty(Directory.GetFiles(keys, "key-*.xml"));
        }

        await using (var second = new DerivedDatabaseHost(DatabaseName, Settings(keys)))
        {
            Assert.Equal(HttpStatusCode.OK, await GetProfileStatusAsync(second, sessionCookie));
        }

        // The keys come from the directory: a host over another, empty directory does not accept the session.
        await using var elsewhere = new DerivedDatabaseHost(DatabaseName, Settings(NewDirectory()));
        Assert.Equal(HttpStatusCode.Redirect, await GetProfileStatusAsync(elsewhere, sessionCookie));
    }

    [Fact]
    public async Task Without_the_setting_nothing_is_written_to_the_directory()
    {
        await DerivedDatabaseHost.ResetAsync(DatabaseName);
        var unused = NewDirectory();

        await using var host = new DerivedDatabaseHost(DatabaseName, Settings(keysDirectory: null));
        using var browser = host.CreateBrowser();
        await SignInAsync(browser, DemoDataService.DemoEmails[0]);

        Assert.Empty(Directory.EnumerateFileSystemEntries(unused));
    }

    public void Dispose()
    {
        foreach (var directory in _directories.Where(Directory.Exists))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private string NewDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ht-ui-keys-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        _directories.Add(directory);
        return directory;
    }

    private static Dictionary<string, string?> Settings(string? keysDirectory)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Operator:AdministratorEmail"] = HostFixture.AdministratorEmail,
            ["Operator:AdministratorPassword"] = HostFixture.AdministratorPassword,
            ["Operator:DemoPassword"] = HostFixture.DemoPassword,
        };
        if (keysDirectory is not null)
        {
            settings[KeysDirectoryKey] = keysDirectory;
        }

        return settings;
    }

    /// <summary><c>GET /profile</c> on the host with exactly this session cookie: 200, or a redirect to Sign in.</summary>
    private static async Task<HttpStatusCode> GetProfileStatusAsync(DerivedDatabaseHost host, string sessionCookie)
    {
        // A client without a cookie jar, so the request carries exactly the given cookie.
        using var client = host.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = false,
        });
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/profile", UriKind.Relative));
        request.Headers.Add("Cookie", sessionCookie);

        using var response = await client.SendAsync(request);
        return response.StatusCode;
    }

    /// <summary>Signs in through the sign-in page with "Keep me signed in"; returns the session cookie (name=value).</summary>
    private static async Task<string> SignInAsync(HttpClient browser, string email)
    {
        var form = await browser.GetFormAsync("/sign-in", "sign-in");
        using var response = await browser.PostFormAsync("/sign-in", form,
            [new("Input.Email", email), new("Input.Password", HostFixture.DemoPassword), new("Input.KeepSignedIn", "true")]);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var cookie = response.SetCookieAttributes(CookieName);
        Assert.NotNull(cookie);
        return cookie[0];
    }
}
