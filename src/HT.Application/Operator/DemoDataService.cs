using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// Demo loading (FR-016, FR-030, FR-031): the synthetic demo accounts with their demo custom habits, and the seed
/// catalog (ADR 0007, ADR 0013). The host runs it at every start, right after administrator creation (HP-021,
/// HP-017); no page offers it, and an architecture rule keeps every HT.Ui type outside <c>HT.Ui.Operator</c> away
/// from this service.
/// </summary>
public sealed class DemoDataService
{
    private readonly IAccountStore _accounts;
    private readonly IDemoDataStore _demoData;

    public DemoDataService(IAccountStore accounts, IDemoDataStore demoData)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(demoData);
        _accounts = accounts;
        _demoData = demoData;
    }

    /// <summary>The demo emails, in loading order; all in the reserved <c>example.com</c> domain.</summary>
    public static IReadOnlyList<string> DemoEmails { get; } =
        ["demo1@example.com", "demo2@example.com", "demo3@example.com"];

    /// <summary>
    /// Checks the demo password, then creates each demo email that has no account as a registered user with the demo
    /// password, together with its demo custom habits, and then creates the seed catalog when the catalog is empty.
    /// An existing account, of either role, is left unchanged and its private data is neither read nor changed, so a
    /// second run changes nothing (SC-006, SC-007, ADR 0013).
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

            results.Add(await LoadAccountAsync(email, demoPassword, cancellationToken).ConfigureAwait(false));
        }

        var seeded = await _demoData.TryCreateSeedCatalogAsync(SeedCatalog.Habits, cancellationToken).ConfigureAwait(false);
        return DemoLoadingResult.Loaded(results, seeded ? DemoCatalogResult.SeedCatalogLoaded : DemoCatalogResult.CatalogNotEmpty);
    }

    private async Task<DemoAccountResult> LoadAccountAsync(Email email, string password, CancellationToken cancellationToken)
    {
        if (await _accounts.FindByEmailAsync(email, cancellationToken).ConfigureAwait(false) is not null)
        {
            return DemoAccountResult.AlreadyExists(email.Value);
        }

        // A registration or another start that takes the email in the meantime leaves its account as it is.
        var created = await _demoData
            .CreateDemoAccountAsync(email, password, DemoCustomHabits.For(email.Value), cancellationToken)
            .ConfigureAwait(false);
        return created == CreateDemoAccountOutcome.EmailTaken
            ? DemoAccountResult.AlreadyExists(email.Value)
            : DemoAccountResult.Created(email.Value);
    }
}
