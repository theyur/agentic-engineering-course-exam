using System.Globalization;
using System.Text.RegularExpressions;
using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;

namespace HT.AcceptanceTests.Support.F001;

/// <summary>
/// Reads values written in the F001 steps, and names the outcome that each result text of the spec
/// belongs to (spec Clarifications: every refusal and operator result has a named outcome and its text).
/// </summary>
public static partial class F001Text
{
    private static readonly Dictionary<(ResultKind Kind, string Text), Enum> OutcomesByText = new()
    {
        [(ResultKind.Registration, "Enter a valid email address.")] = RegistrationOutcome.InvalidEmail,
        [(ResultKind.Registration, "At least 8 characters, with a number.")] = RegistrationOutcome.PasswordRuleNotMet,
        [(ResultKind.Registration, "An account with this email already exists.")] = RegistrationOutcome.EmailTaken,
        [(ResultKind.SignIn, "Email or password is not correct.")] = SignInOutcome.Failed,
        [(ResultKind.AccountDetails, "You are not signed in.")] = AccountDetailsOutcome.NotSignedIn,
        [(ResultKind.PasswordChange, "You are not signed in.")] = PasswordChangeOutcome.NotSignedIn,
        [(ResultKind.PasswordChange, "The current password is not correct.")] = PasswordChangeOutcome.CurrentPasswordWrong,
        [(ResultKind.PasswordChange, "At least 8 characters, with a number.")] = PasswordChangeOutcome.PasswordRuleNotMet,
        [(ResultKind.AdministratorCreation, "no administrator configured")] = AdministratorCreationOutcome.NotConfigured,
        [(ResultKind.AdministratorCreation, "configured email is not valid")] = AdministratorCreationOutcome.InvalidEmail,
        [(ResultKind.AdministratorCreation, "administrator already exists")] = AdministratorCreationOutcome.AlreadyExists,
        [(ResultKind.AdministratorCreation, "email belongs to a registered user")] = AdministratorCreationOutcome.BelongsToRegisteredUser,
        [(ResultKind.AdministratorCreation, "configured password does not meet the password rule")] = AdministratorCreationOutcome.PasswordRuleNotMet,
        [(ResultKind.AdministratorCreation, "administrator created")] = AdministratorCreationOutcome.Created,
        [(ResultKind.DemoLoading, "no demo password configured")] = DemoLoadingOutcome.NoPassword,
        [(ResultKind.DemoLoading, "demo password does not meet the password rule")] = DemoLoadingOutcome.PasswordRuleNotMet,
    };

    /// <summary>The named outcome that carries <paramref name="text"/> for a result of <paramref name="kind"/>.</summary>
    public static Enum OutcomeFor(ResultKind kind, string text) =>
        OutcomesByText.TryGetValue((kind, text), out var outcome)
            ? outcome
            : throw new InvalidOperationException($"The spec gives no {kind} result with the text \"{text}\".");

    /// <summary>The per-email outcome of demo loading that carries <paramref name="text"/> (FR-016).</summary>
    public static DemoAccountOutcome DemoAccountOutcomeFor(string text) => text switch
    {
        "created" => DemoAccountOutcome.Created,
        "already exists" => DemoAccountOutcome.AlreadyExists,
        _ => throw new InvalidOperationException($"FR-016 gives no demo account result with the text \"{text}\"."),
    };

    public static Role ParseRole(string text) => text switch
    {
        "registered user" => Role.RegisteredUser,
        "administrator" => Role.Administrator,
        _ => throw new InvalidOperationException($"\"{text}\" is not a role."),
    };

    /// <summary>An instant written in UTC, for example <c>2026-11-11T10:00:00Z</c>.</summary>
    public static DateTimeOffset ParseInstant(string text) =>
        DateTimeOffset.ParseExact(text, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

    /// <summary>The instants of a list such as <c>"t1", "t2" and "t3"</c>, in the order written.</summary>
    public static IReadOnlyList<DateTimeOffset> ParseInstantList(string text) =>
        QuotedValue().Matches(text).Select(match => ParseInstant(match.Groups[1].Value)).ToList();

    /// <summary><c>absent</c> means no value is supplied; otherwise the quoted text is the value.</summary>
    public static string? ParseOptional(string token) =>
        token == "absent" ? null : QuotedValue().Match(token).Groups[1].Value;

    /// <summary>
    /// The per-email results of a text such as <c>"created" for "a", for "b" and "already exists" for "c"</c>:
    /// an email without its own result text takes the one before it.
    /// </summary>
    public static IReadOnlyList<(string Email, string OutcomeText)> ParseDemoAccountResults(string text)
    {
        var results = new List<(string Email, string OutcomeText)>();
        string? outcomeText = null;
        foreach (Match match in DemoAccountResult().Matches(text))
        {
            if (match.Groups["outcome"].Success)
            {
                outcomeText = match.Groups["outcome"].Value;
            }

            results.Add((match.Groups["email"].Value,
                outcomeText ?? throw new InvalidOperationException($"No result text before \"{match.Groups["email"].Value}\".")));
        }

        return results;
    }

    [GeneratedRegex("\"([^\"]*)\"")]
    private static partial Regex QuotedValue();

    [GeneratedRegex("(?:\"(?<outcome>[^\"]*)\" )?for \"(?<email>[^\"]*)\"")]
    private static partial Regex DemoAccountResult();
}
