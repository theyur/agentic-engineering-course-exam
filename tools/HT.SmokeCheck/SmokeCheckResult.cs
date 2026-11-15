namespace HT.SmokeCheck;

/// <summary>
/// The outcome of one <see cref="SmokeCheckRunner.RunAsync"/>: every check line in order (the lines before
/// <c>TIMING</c>), the <c>TIMING</c> line and the timings it shows (contracts/smoke-check.md "Timings"). Timings never
/// fail a check.
/// </summary>
/// <param name="CheckLines">The check lines: <c>PASS …</c>, <c>FAIL …: …</c> or <c>SKIP …</c>.</param>
/// <param name="TimingLine">The <c>TIMING</c> line.</param>
/// <param name="FirstMilliseconds">The duration of check 1's first request.</param>
/// <param name="FirstRetried">True when check 1's first request did not get the visitor Home.</param>
/// <param name="LaterMilliseconds">The <c>later</c> timing, or null (<c>later n/a</c>) when no demo account signed in.</param>
/// <param name="LaterRequests">How many requests the <c>later</c> timing covers; 0 when it is null.</param>
public sealed record SmokeCheckResult(
    IReadOnlyList<string> CheckLines,
    string TimingLine,
    long FirstMilliseconds,
    bool FirstRetried,
    long? LaterMilliseconds,
    int LaterRequests)
{
    /// <summary>True when no check line is a <c>FAIL</c>.</summary>
    public bool Passed => CheckLines.All(line => !line.StartsWith(SmokeCheckRunner.FailPrefix, StringComparison.Ordinal));
}
