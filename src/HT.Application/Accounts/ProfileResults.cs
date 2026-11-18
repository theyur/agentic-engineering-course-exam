namespace HT.Application.Accounts;

/// <summary>The outcomes of <see cref="ProfileService.GetTimeZoneAsync"/>.</summary>
public enum TimeZoneReadOutcome
{
    NotSignedIn,
    Found,
}

/// <summary>The result of reading the acting user's own time zone (FR-024, FR-026). No refusal is an exception (ADR 0005).</summary>
public sealed class TimeZoneReadResult
{
    private TimeZoneReadResult(TimeZoneReadOutcome outcome, string? message, string? timeZone)
    {
        Outcome = outcome;
        Message = message;
        TimeZone = timeZone;
    }

    public TimeZoneReadOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="TimeZoneReadOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>
    /// The account's named time zone, exactly as stored, or the installation's default when none is stored (FR-025);
    /// set only for <see cref="TimeZoneReadOutcome.Found"/>.
    /// </summary>
    public string? TimeZone { get; }

    internal static TimeZoneReadResult NotSignedIn { get; } = new(TimeZoneReadOutcome.NotSignedIn, AccountTexts.NotSignedIn, null);

    internal static TimeZoneReadResult Found(string timeZone) => new(TimeZoneReadOutcome.Found, null, timeZone);
}

/// <summary>The outcomes of <see cref="ProfileService.ChangeTimeZoneAsync"/>, in FR-023's check order.</summary>
public enum TimeZoneChangeOutcome
{
    NotSignedIn,
    NotANamedTimeZone,

    /// <summary>The account already has this time zone: a success that changes nothing (FR-026).</summary>
    Unchanged,
    Changed,
}

/// <summary>
/// The result of changing the acting user's own time zone (FR-024, FR-026). It carries no input value (HP-011). No
/// refusal is an exception (ADR 0005).
/// </summary>
public sealed class TimeZoneChangeResult
{
    private TimeZoneChangeResult(TimeZoneChangeOutcome outcome, string? message)
    {
        Outcome = outcome;
        Message = message;
    }

    public TimeZoneChangeOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="TimeZoneChangeOutcome.Unchanged"/> and <see cref="TimeZoneChangeOutcome.Changed"/>.</summary>
    public string? Message { get; }

    internal static TimeZoneChangeResult NotSignedIn { get; } = new(TimeZoneChangeOutcome.NotSignedIn, AccountTexts.NotSignedIn);

    internal static TimeZoneChangeResult NotANamedTimeZone { get; } =
        new(TimeZoneChangeOutcome.NotANamedTimeZone, AccountTexts.NotANamedTimeZone);

    internal static TimeZoneChangeResult Unchanged { get; } = new(TimeZoneChangeOutcome.Unchanged, null);

    internal static TimeZoneChangeResult Changed { get; } = new(TimeZoneChangeOutcome.Changed, null);
}
