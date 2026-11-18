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

    /// <summary>The one definition of the name refusal, for an account's name and a goal's own name (FR-009, FR-028).</summary>
    public const string NameTooLong = "The name can have at most 60 characters.";
    public const string NotANamedTimeZone = "Choose a time zone from the list.";
    public const string GoodMorning = "Good morning";
    public const string GoodAfternoon = "Good afternoon";
    public const string GoodEvening = "Good evening";

    /// <summary>A greeting, with ", " and the account's name when it has one (FR-029): "Good morning, Yuri".</summary>
    public static string Greeting(string greeting, string? name)
    {
        ArgumentNullException.ThrowIfNull(greeting);
        return string.IsNullOrEmpty(name) ? greeting : greeting + ", " + name;
    }

    /// <summary>"member since " and the month of <paramref name="year"/> and <paramref name="month"/>, as <see cref="DateTexts.MonthYear"/> writes it (FR-030).</summary>
    public static string MemberSince(int year, int month) => "member since " + DateTexts.MonthYear(new DateOnly(year, month, 1));
}
