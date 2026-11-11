namespace HT.Application.Accounts;

/// <summary>The texts of the account use cases, worded as in the spec (ADR 0005).</summary>
public static class AccountTexts
{
    public const string InvalidEmail = "Enter a valid email address.";
    public const string PasswordRule = "At least 8 characters, with a number.";
    public const string EmailTaken = "An account with this email already exists.";
    public const string SignInFailed = "Email or password is not correct.";
    public const string NotSignedIn = "You are not signed in.";
    public const string CurrentPasswordWrong = "The current password is not correct.";
}
