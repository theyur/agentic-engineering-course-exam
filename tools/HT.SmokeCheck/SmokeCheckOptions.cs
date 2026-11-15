using System.Globalization;
using System.Text;

namespace HT.SmokeCheck;

/// <summary>
/// One smoke check run (contracts/smoke-check.md "Invocation"): the published address, the three values from the
/// environment, and the options. Its text form leaves out the passwords and the administrator email.
/// </summary>
/// <param name="BaseUrl">The published <c>https://</c> address, as its root.</param>
/// <param name="DemoPassword">From <c>HT_SMOKE_DEMO_PASSWORD</c>.</param>
/// <param name="AdministratorEmail">From <c>HT_SMOKE_ADMIN_EMAIL</c>.</param>
/// <param name="AdministratorPassword">From <c>HT_SMOKE_ADMIN_PASSWORD</c>.</param>
public sealed record SmokeCheckOptions(Uri BaseUrl, string DemoPassword, string AdministratorEmail, string AdministratorPassword)
{
    /// <summary>How long check 1 retries when no <c>--warm-up-seconds</c> is given.</summary>
    public static TimeSpan DefaultWarmUp { get; } = TimeSpan.FromSeconds(300);

    /// <summary><c>--skip-http-redirect</c>: skip check 2 (a local run without an HTTP front end).</summary>
    public bool SkipHttpRedirect { get; init; }

    /// <summary><c>--warm-up-seconds</c>: how long check 1 retries.</summary>
    public TimeSpan WarmUp { get; init; } = DefaultWarmUp;

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append(CultureInfo.InvariantCulture, $"BaseUrl = {BaseUrl}, SkipHttpRedirect = {SkipHttpRedirect}, WarmUp = {WarmUp}");
        return true;
    }
}

/// <summary>The outcome of <see cref="SmokeCheckCommand.Parse"/>: options, or a usage error that names no value.</summary>
public sealed record SmokeCheckParseResult(SmokeCheckOptions? Options, string? Error)
{
    public static SmokeCheckParseResult Valid(SmokeCheckOptions options) => new(options, null);

    public static SmokeCheckParseResult Invalid(string error) => new(null, error);
}
