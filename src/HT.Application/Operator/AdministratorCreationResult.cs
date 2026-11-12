namespace HT.Application.Operator;

/// <summary>The outcomes of <see cref="OperatorService.CreateAdministratorAsync"/>, in FR-014's check order.</summary>
public enum AdministratorCreationOutcome
{
    NotConfigured,
    InvalidEmail,
    AlreadyExists,
    BelongsToRegisteredUser,
    PasswordRuleNotMet,
    Created,
}

/// <summary>The result of administrator creation; every outcome has its text (FR-014, ADR 0005).</summary>
public sealed class AdministratorCreationResult
{
    private AdministratorCreationResult(AdministratorCreationOutcome outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    public AdministratorCreationOutcome Outcome { get; }

    /// <summary>The spec's text, written to the operator log.</summary>
    public string Message { get; }

    internal static AdministratorCreationResult NotConfigured { get; } =
        new(AdministratorCreationOutcome.NotConfigured, OperatorTexts.AdministratorNotConfigured);

    internal static AdministratorCreationResult InvalidEmail { get; } =
        new(AdministratorCreationOutcome.InvalidEmail, OperatorTexts.AdministratorEmailInvalid);

    internal static AdministratorCreationResult AlreadyExists { get; } =
        new(AdministratorCreationOutcome.AlreadyExists, OperatorTexts.AdministratorAlreadyExists);

    internal static AdministratorCreationResult BelongsToRegisteredUser { get; } =
        new(AdministratorCreationOutcome.BelongsToRegisteredUser, OperatorTexts.EmailBelongsToRegisteredUser);

    internal static AdministratorCreationResult PasswordRuleNotMet { get; } =
        new(AdministratorCreationOutcome.PasswordRuleNotMet, OperatorTexts.AdministratorPasswordRule);

    internal static AdministratorCreationResult Created { get; } =
        new(AdministratorCreationOutcome.Created, OperatorTexts.AdministratorCreated);
}
