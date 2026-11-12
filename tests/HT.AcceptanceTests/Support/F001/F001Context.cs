using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;

namespace HT.AcceptanceTests.Support.F001;

/// <summary>The kind of use case whose result a "the result is ..." step reads.</summary>
public enum ResultKind
{
    Registration,
    SignIn,
    AccountDetails,
    PasswordChange,
    AdministratorCreation,
    DemoLoading,
}

/// <summary>The named outcome and the text of the result of the last When step.</summary>
public sealed record RecordedResult(ResultKind Kind, Enum Outcome, string? Message);

/// <summary>Per-scenario state shared by the F001 step definitions (Reqnroll context injection).</summary>
public sealed class F001Context
{
    /// <summary>The acting user for the steps that follow ("they ...").</summary>
    public ActingUser ActingUser { get; set; } = ActingUser.Anonymous;

    /// <summary>Acting users from successful sign-ins, by the email the step named.</summary>
    public Dictionary<string, ActingUser> SignedInAccounts { get; } = new(StringComparer.Ordinal);

    public RecordedResult? LastResult { get; private set; }

    public RegistrationResult? LastRegistration { get; private set; }

    public SignInResult? LastSignIn { get; private set; }

    public IReadOnlyList<SignInResult> LastSignIns { get; private set; } = [];

    public AccountDetailsResult? LastAccountDetails { get; private set; }

    public PasswordChangeResult? LastPasswordChange { get; private set; }

    public AdministratorCreationResult? LastAdministratorCreation { get; private set; }

    public DemoLoadingResult? LastDemoLoading { get; private set; }

    /// <summary>Administrator credentials given by a Given step; null means absent.</summary>
    public string? ConfiguredAdministratorEmail { get; set; }

    public string? ConfiguredAdministratorPassword { get; set; }

    public bool AdministratorConfigurationGiven { get; set; }

    public void Record(RegistrationResult result)
    {
        LastRegistration = result;
        LastResult = new RecordedResult(ResultKind.Registration, result.Outcome, result.Message);
    }

    public void Record(SignInResult result)
    {
        LastSignIn = result;
        LastResult = new RecordedResult(ResultKind.SignIn, result.Outcome, result.Message);
    }

    public void Record(IReadOnlyList<SignInResult> results)
    {
        LastSignIns = results;
    }

    public void Record(AccountDetailsResult result)
    {
        LastAccountDetails = result;
        LastResult = new RecordedResult(ResultKind.AccountDetails, result.Outcome, result.Message);
    }

    public void Record(PasswordChangeResult result)
    {
        LastPasswordChange = result;
        LastResult = new RecordedResult(ResultKind.PasswordChange, result.Outcome, result.Message);
    }

    public void Record(AdministratorCreationResult result)
    {
        LastAdministratorCreation = result;
        LastResult = new RecordedResult(ResultKind.AdministratorCreation, result.Outcome, result.Message);
    }

    public void Record(DemoLoadingResult result)
    {
        LastDemoLoading = result;
        LastResult = new RecordedResult(ResultKind.DemoLoading, result.Outcome, result.Message);
    }
}
