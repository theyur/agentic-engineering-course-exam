using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// Demo loading (FR-016): the synthetic demo accounts that later slices attach their demo data to (ADR 0007).
/// The host runs it at every start, right after administrator creation (HP-021); no page offers it, and an
/// architecture rule keeps every HT.Ui type outside <c>HT.Ui.Operator</c> away from this service.
/// </summary>
public sealed class DemoDataService
{
    private readonly IAccountStore _store;

    public DemoDataService(IAccountStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    /// <summary>The demo emails, in loading order; all in the reserved <c>example.com</c> domain.</summary>
    public static IReadOnlyList<string> DemoEmails { get; } =
        ["demo1@example.com", "demo2@example.com", "demo3@example.com"];

    /// <summary>
    /// Creates each demo email that has no account as a registered user with the demo password. An existing
    /// account, of either role, is left unchanged, so a second run changes nothing (SC-006).
    /// </summary>
    public async Task<DemoLoadingResult> LoadAsync(string? demoPassword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(demoPassword))
        {
            return DemoLoadingResult.NoPassword;
        }

        if (!PasswordRule.IsMet(demoPassword))
        {
            return DemoLoadingResult.PasswordRuleNotMet;
        }

        var results = new List<DemoAccountResult>(DemoEmails.Count);
        foreach (var text in DemoEmails)
        {
            if (!Email.TryParse(text, out var email))
            {
                throw new InvalidOperationException($"The demo email {text} is not valid.");
            }

            results.Add(await LoadAccountAsync(email, demoPassword, cancellationToken));
        }

        return DemoLoadingResult.Loaded(results);
    }

    private async Task<DemoAccountResult> LoadAccountAsync(Email email, string password, CancellationToken cancellationToken)
    {
        if (await _store.FindByEmailAsync(email, cancellationToken) is not null)
        {
            return DemoAccountResult.AlreadyExists(email.Value);
        }

        // A registration or another start that takes the email in the meantime leaves its account as it is.
        var created = await _store.CreateAsync(email, Role.RegisteredUser, password, cancellationToken);
        return created.IsEmailTaken
            ? DemoAccountResult.AlreadyExists(email.Value)
            : DemoAccountResult.Created(email.Value);
    }
}
