using System.Collections;
using HT.SmokeCheck;

namespace HT.UiTests.SmokeChecks;

/// <summary>
/// T007: the smoke check's arguments and environment (contracts/smoke-check.md "Invocation"). Exit code 2 is a
/// broken invocation; no value of the three variables ever reaches the output.
/// </summary>
public sealed class InvocationTests
{
    private const string DemoPassword = "sentinel-pw-1";
    private const string AdministratorEmail = "sentinel-admin-2@example.com";
    private const string AdministratorPassword = "sentinel-pw-3";

    private const string BaseUrl = "https://ht-demo.example.net";

    private static readonly string[] Sentinels = [DemoPassword, AdministratorEmail, AdministratorPassword];

    private static Hashtable FullEnvironment() => new()
    {
        ["HT_SMOKE_DEMO_PASSWORD"] = DemoPassword,
        ["HT_SMOKE_ADMIN_EMAIL"] = AdministratorEmail,
        ["HT_SMOKE_ADMIN_PASSWORD"] = AdministratorPassword,
    };

    public static TheoryData<string[]> BrokenArguments => new()
    {
        Array.Empty<string>(),
        new[] { "--skip-http-redirect" },
        new[] { "ht-demo.example.net" },
        new[] { "/relative" },
        new[] { "http://ht-demo.example.net" },
        new[] { "ftp://ht-demo.example.net" },
        new[] { BaseUrl, "--unknown" },
        new[] { BaseUrl, "-x" },
        new[] { BaseUrl, "https://second.example.net" },
        new[] { BaseUrl, "--warm-up-seconds" },
        new[] { BaseUrl, "--warm-up-seconds", "five" },
        new[] { BaseUrl, "--warm-up-seconds", "-1" },
    };

    public static TheoryData<string, string?> BrokenEnvironment => new()
    {
        { "HT_SMOKE_DEMO_PASSWORD", null },
        { "HT_SMOKE_DEMO_PASSWORD", "   " },
        { "HT_SMOKE_ADMIN_EMAIL", null },
        { "HT_SMOKE_ADMIN_EMAIL", " " },
        { "HT_SMOKE_ADMIN_PASSWORD", null },
        { "HT_SMOKE_ADMIN_PASSWORD", "\t" },
    };

    [Theory]
    [MemberData(nameof(BrokenArguments))]
    public async Task A_broken_argument_list_exits_with_2_and_a_usage_line(string[] args)
    {
        using var output = new StringWriter();

        var exitCode = await SmokeCheckCommand.RunAsync(args, FullEnvironment(), output, NoNetwork);

        Assert.Equal(2, exitCode);
        Assert.Contains("usage:", output.ToString(), StringComparison.Ordinal);
        AssertNoSentinel(output.ToString());
    }

    [Theory]
    [MemberData(nameof(BrokenEnvironment))]
    public async Task A_missing_or_blank_variable_exits_with_2_and_names_only_the_variable(string name, string? value)
    {
        var environment = FullEnvironment();
        if (value is null)
        {
            environment.Remove(name);
        }
        else
        {
            environment[name] = value;
        }

        using var output = new StringWriter();

        var exitCode = await SmokeCheckCommand.RunAsync([BaseUrl, "--skip-http-redirect"], environment, output, NoNetwork);

        Assert.Equal(2, exitCode);
        Assert.Contains("usage:", output.ToString(), StringComparison.Ordinal);
        Assert.Contains(name, output.ToString(), StringComparison.Ordinal);
        AssertNoSentinel(output.ToString());
    }

    [Fact]
    public void Parse_reads_the_base_url_the_variables_and_the_defaults()
    {
        var parsed = SmokeCheckCommand.Parse([BaseUrl], FullEnvironment());

        Assert.Null(parsed.Error);
        var options = Assert.IsType<SmokeCheckOptions>(parsed.Options);
        Assert.Equal(new Uri("https://ht-demo.example.net/"), options.BaseUrl);
        Assert.Equal(DemoPassword, options.DemoPassword);
        Assert.Equal(AdministratorEmail, options.AdministratorEmail);
        Assert.Equal(AdministratorPassword, options.AdministratorPassword);
        Assert.False(options.SkipHttpRedirect);
        Assert.Equal(TimeSpan.FromSeconds(300), options.WarmUp);
    }

    [Fact]
    public void Parse_reads_skip_http_redirect_and_the_warm_up_seconds_in_any_order()
    {
        var parsed = SmokeCheckCommand.Parse(["--warm-up-seconds", "5", BaseUrl, "--skip-http-redirect"], FullEnvironment());

        var options = Assert.IsType<SmokeCheckOptions>(parsed.Options);
        Assert.True(options.SkipHttpRedirect);
        Assert.Equal(TimeSpan.FromSeconds(5), options.WarmUp);
    }

    [Fact]
    public void Parse_gives_an_error_and_no_options_for_an_unknown_option()
    {
        var parsed = SmokeCheckCommand.Parse([BaseUrl, "--verbose"], FullEnvironment());

        Assert.Null(parsed.Options);
        Assert.Contains("--verbose", parsed.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void The_options_text_shows_no_secret_value()
    {
        var options = SmokeCheckCommand.Parse([BaseUrl], FullEnvironment()).Options!;

        AssertNoSentinel(options.ToString());
    }

    private static HttpMessageHandler NoNetwork() =>
        throw new InvalidOperationException("A broken invocation must not create a client.");

    private static void AssertNoSentinel(string text)
    {
        foreach (var sentinel in Sentinels)
        {
            Assert.DoesNotContain(sentinel, text, StringComparison.Ordinal);
        }
    }
}
