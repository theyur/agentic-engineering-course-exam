namespace HT.Application.Operator;

/// <summary>The texts of administrator creation and demo loading, worded as in the spec (ADR 0005).</summary>
public static class OperatorTexts
{
    public const string AdministratorNotConfigured = "no administrator configured";
    public const string AdministratorEmailInvalid = "configured email is not valid";
    public const string AdministratorAlreadyExists = "administrator already exists";
    public const string EmailBelongsToRegisteredUser = "email belongs to a registered user";
    public const string AdministratorPasswordRule = "configured password does not meet the password rule";
    public const string AdministratorCreated = "administrator created";
    public const string DemoPasswordNotConfigured = "no demo password configured";
    public const string DemoPasswordRule = "demo password does not meet the password rule";
    public const string DemoAccountCreated = "created";
    public const string DemoAccountAlreadyExists = "already exists";
}
