namespace HT.Application.Reports;

/// <summary>The outcomes of reading a goal's attempts, in FR-014's check order (contracts/application-api.md "AttemptService").</summary>
public enum AttemptListOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    Found,
}

/// <summary>
/// Every attempt of one of the acting user's goals, the latest first (slice 8 FR-014), or the refusal. An unknown
/// identifier and another account's goal give the same <see cref="AttemptListOutcome.NotFound"/> (FR-022).
/// </summary>
public sealed class AttemptListResult
{
    private AttemptListResult(AttemptListOutcome outcome, string? message, IReadOnlyList<AttemptView> attempts)
    {
        Outcome = outcome;
        Message = message;
        Attempts = attempts;
    }

    public AttemptListOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="AttemptListOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Every attempt, the latest first; empty for a refusal.</summary>
    public IReadOnlyList<AttemptView> Attempts { get; }

    internal static AttemptListResult Refused(AttemptListOutcome outcome, string message) => new(outcome, message, []);

    internal static AttemptListResult Found(IReadOnlyList<AttemptView> attempts) => new(AttemptListOutcome.Found, null, attempts);
}

/// <summary>The outcomes of reading one attempt, in FR-014's check order (contracts/application-api.md "AttemptService").</summary>
public enum AttemptReadOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    AttemptNotFound,
    Found,
}

/// <summary>
/// One attempt of one of the acting user's goals with its recent results and History (slice 8 FR-014), or the refusal. A
/// number that is not one of the goal's attempts gives <see cref="AttemptReadOutcome.AttemptNotFound"/>, "This attempt
/// does not exist.".
/// </summary>
public sealed class AttemptReadResult
{
    private AttemptReadResult(AttemptReadOutcome outcome, string? message, AttemptDetails? details)
    {
        Outcome = outcome;
        Message = message;
        Details = details;
    }

    public AttemptReadOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="AttemptReadOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="AttemptReadOutcome.Found"/>.</summary>
    public AttemptDetails? Details { get; }

    internal static AttemptReadResult Refused(AttemptReadOutcome outcome, string message) => new(outcome, message, null);

    internal static AttemptReadResult Found(AttemptDetails details) => new(AttemptReadOutcome.Found, null, details);
}
