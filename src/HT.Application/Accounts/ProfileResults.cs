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

/// <summary>The outcomes of <see cref="ProfileService.ChangeNameAsync"/>, in FR-023's check order.</summary>
public enum NameChangeOutcome
{
    NotSignedIn,
    NameTooLong,

    /// <summary>The name is set, or removed when it was empty after trimming (FR-028).</summary>
    Changed,
}

/// <summary>
/// The result of changing the acting user's own name (FR-028). It carries no input value (HP-011). No refusal is an
/// exception (ADR 0005).
/// </summary>
public sealed class NameChangeResult
{
    private NameChangeResult(NameChangeOutcome outcome, string? message)
    {
        Outcome = outcome;
        Message = message;
    }

    public NameChangeOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="NameChangeOutcome.Changed"/>.</summary>
    public string? Message { get; }

    internal static NameChangeResult NotSignedIn { get; } = new(NameChangeOutcome.NotSignedIn, AccountTexts.NotSignedIn);

    internal static NameChangeResult NameTooLong { get; } = new(NameChangeOutcome.NameTooLong, AccountTexts.NameTooLong);

    internal static NameChangeResult Changed { get; } = new(NameChangeOutcome.Changed, null);
}

/// <summary>The acting user's own profile (FR-028, FR-030). Private account data, given only to its own account.</summary>
/// <param name="Name">The account's name, trimmed; null when it has none.</param>
/// <param name="Email">The account's email, in its normal form.</param>
/// <param name="MemberSince">"member since November 2026", in the account's time zone; null for an account with no recorded creation time.</param>
public sealed record ProfileView(string? Name, string Email, string? MemberSince);

/// <summary>The outcomes of <see cref="ProfileService.GetProfileAsync"/>.</summary>
public enum ProfileReadOutcome
{
    NotSignedIn,
    Found,
}

/// <summary>The result of reading the acting user's own profile (FR-028, FR-030). No refusal is an exception (ADR 0005).</summary>
public sealed class ProfileReadResult
{
    private ProfileReadResult(ProfileReadOutcome outcome, string? message, ProfileView? profile)
    {
        Outcome = outcome;
        Message = message;
        Profile = profile;
    }

    public ProfileReadOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="ProfileReadOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>The profile; set only for <see cref="ProfileReadOutcome.Found"/>.</summary>
    public ProfileView? Profile { get; }

    internal static ProfileReadResult NotSignedIn { get; } = new(ProfileReadOutcome.NotSignedIn, AccountTexts.NotSignedIn, null);

    internal static ProfileReadResult Found(ProfileView profile) => new(ProfileReadOutcome.Found, null, profile);
}

/// <summary>The Home heading (FR-029).</summary>
/// <param name="Greeting">"Good evening, Yuri", or "Good evening" without a name, by the local time in the account's zone.</param>
/// <param name="Date">Today in the account's zone, in English: "Tuesday, 17 November".</param>
public sealed record HomeHeading(string Greeting, string Date);

/// <summary>The outcomes of <see cref="ProfileService.GetHomeHeadingAsync"/>.</summary>
public enum HomeHeadingOutcome
{
    NotSignedIn,
    Found,
}

/// <summary>The result of reading the acting user's Home heading (FR-029). No refusal is an exception (ADR 0005).</summary>
public sealed class HomeHeadingResult
{
    private HomeHeadingResult(HomeHeadingOutcome outcome, string? message, HomeHeading? heading)
    {
        Outcome = outcome;
        Message = message;
        Heading = heading;
    }

    public HomeHeadingOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="HomeHeadingOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>The heading; set only for <see cref="HomeHeadingOutcome.Found"/>.</summary>
    public HomeHeading? Heading { get; }

    internal static HomeHeadingResult NotSignedIn { get; } = new(HomeHeadingOutcome.NotSignedIn, AccountTexts.NotSignedIn, null);

    internal static HomeHeadingResult Found(HomeHeading heading) => new(HomeHeadingOutcome.Found, null, heading);
}
