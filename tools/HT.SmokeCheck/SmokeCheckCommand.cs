using System.Collections;
using System.Globalization;
using System.Net;

namespace HT.SmokeCheck;

/// <summary>
/// The smoke check's entry point (contracts/smoke-check.md "Invocation", "Exit codes"): parses the arguments, reads
/// the three values from the environment it is given, runs the checks and returns the exit code. Secret values are
/// read only from the environment, never from arguments, and never written to the output.
/// </summary>
public static class SmokeCheckCommand
{
    public const string DemoPasswordVariable = "HT_SMOKE_DEMO_PASSWORD";
    public const string AdministratorEmailVariable = "HT_SMOKE_ADMIN_EMAIL";
    public const string AdministratorPasswordVariable = "HT_SMOKE_ADMIN_PASSWORD";

    /// <summary>All checks passed.</summary>
    public const int Passed = 0;

    /// <summary>At least one check failed.</summary>
    public const int Failed = 1;

    /// <summary>A broken invocation: a missing or invalid argument or environment variable.</summary>
    public const int UsageError = 2;

    private const string SkipHttpRedirectOption = "--skip-http-redirect";
    private const string WarmUpSecondsOption = "--warm-up-seconds";

    private const string Usage =
        "usage: dotnet run --project tools/HT.SmokeCheck -- <base-url> [--skip-http-redirect] [--warm-up-seconds <n>]"
        + " (environment: " + DemoPasswordVariable + ", " + AdministratorEmailVariable + ", " + AdministratorPasswordVariable + ")";

    /// <summary>
    /// Runs the smoke check and returns <see cref="Passed"/>, <see cref="Failed"/> or <see cref="UsageError"/>.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="environment">The environment variables, such as <see cref="Environment.GetEnvironmentVariables()"/>.</param>
    /// <param name="output">Where the check lines go.</param>
    /// <param name="createHandler">
    /// Creates the handler of each new client, for tests without a network; by default a new
    /// <see cref="HttpClientHandler"/> with its own cookies that does not follow redirects.
    /// </param>
    public static async Task<int> RunAsync(
        string[] args, IDictionary environment, TextWriter output, Func<HttpMessageHandler>? createHandler = null)
    {
        ArgumentNullException.ThrowIfNull(output);

        var parsed = Parse(args, environment);
        if (parsed.Options is not { } options)
        {
            await output.WriteLineAsync($"usage error: {parsed.Error}").ConfigureAwait(false);
            await output.WriteLineAsync(Usage).ConfigureAwait(false);
            return UsageError;
        }

        var handlerFactory = createHandler ?? DefaultHandler;
        var root = options.BaseUrl;
        HttpClient CreateClient() => new(handlerFactory(), disposeHandler: true) { BaseAddress = root };

        var result = await new SmokeCheckRunner(CreateClient, options, output).RunAsync().ConfigureAwait(false);
        return result.Passed ? Passed : Failed;
    }

    /// <summary>Reads the options from the arguments and the environment, or a usage error that names no value.</summary>
    public static SmokeCheckParseResult Parse(string[] args, IDictionary environment)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(environment);

        string? baseUrlText = null;
        var skipHttpRedirect = false;
        var warmUp = SmokeCheckOptions.DefaultWarmUp;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg == SkipHttpRedirectOption)
            {
                skipHttpRedirect = true;
            }
            else if (arg == WarmUpSecondsOption)
            {
                if (i + 1 >= args.Length
                    || !int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var seconds))
                {
                    return SmokeCheckParseResult.Invalid($"{WarmUpSecondsOption} needs a whole number of seconds, 0 or more.");
                }

                warmUp = TimeSpan.FromSeconds(seconds);
                i++;
            }
            else if (arg.StartsWith('-'))
            {
                return SmokeCheckParseResult.Invalid($"unknown option {arg}.");
            }
            else if (baseUrlText is null)
            {
                baseUrlText = arg;
            }
            else
            {
                return SmokeCheckParseResult.Invalid("more than one base URL.");
            }
        }

        if (baseUrlText is null)
        {
            return SmokeCheckParseResult.Invalid("the base URL is missing.");
        }

        if (!Uri.TryCreate(baseUrlText, UriKind.Absolute, out var baseUrl) || baseUrl.Scheme != Uri.UriSchemeHttps)
        {
            return SmokeCheckParseResult.Invalid("the base URL must be an absolute https:// address.");
        }

        if (Read(environment, DemoPasswordVariable) is not { } demoPassword)
        {
            return Missing(DemoPasswordVariable);
        }

        if (Read(environment, AdministratorEmailVariable) is not { } administratorEmail)
        {
            return Missing(AdministratorEmailVariable);
        }

        if (Read(environment, AdministratorPasswordVariable) is not { } administratorPassword)
        {
            return Missing(AdministratorPasswordVariable);
        }

        var root = new Uri(baseUrl.GetLeftPart(UriPartial.Authority) + "/");
        return SmokeCheckParseResult.Valid(new SmokeCheckOptions(root, demoPassword, administratorEmail, administratorPassword)
        {
            SkipHttpRedirect = skipHttpRedirect,
            WarmUp = warmUp,
        });
    }

    /// <summary>A usage error that names the variable, never a value.</summary>
    private static SmokeCheckParseResult Missing(string name) =>
        SmokeCheckParseResult.Invalid($"the environment variable {name} is missing or empty.");

    /// <summary>The variable's value, or null when it is missing or whitespace.</summary>
    private static string? Read(IDictionary environment, string name) =>
        environment[name] is string value && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static HttpClientHandler DefaultHandler() => new()
    {
        AllowAutoRedirect = false,
        UseCookies = true,
        CookieContainer = new CookieContainer(),
    };
}
