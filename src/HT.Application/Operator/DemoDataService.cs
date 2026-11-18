using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// Demo loading (FR-016, FR-030, FR-031, FR-040): the seed catalog, then the synthetic demo accounts with their
/// profiles, demo custom habits and demo goals (ADR 0007, ADR 0013). The host runs it at every start, right after
/// administrator creation (HP-021, HP-017); no page offers it, and an architecture rule keeps every HT.Ui type outside
/// <c>HT.Ui.Operator</c> away from this service.
/// </summary>
public sealed class DemoDataService
{
    private readonly IAccountStore _accounts;
    private readonly IDemoDataStore _demoData;
    private readonly TimeProvider _timeProvider;
    private readonly InstallationTimeZone _defaultTimeZone;

    public DemoDataService(
        IAccountStore accounts, IDemoDataStore demoData, TimeProvider timeProvider, InstallationTimeZone defaultTimeZone)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(demoData);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(defaultTimeZone);
        _accounts = accounts;
        _demoData = demoData;
        _timeProvider = timeProvider;
        _defaultTimeZone = defaultTimeZone;
    }

    /// <summary>The demo emails, in loading order; all in the reserved <c>example.com</c> domain.</summary>
    public static IReadOnlyList<string> DemoEmails { get; } =
        ["demo1@example.com", "demo2@example.com", "demo3@example.com"];

    /// <summary>
    /// Checks the demo password, then creates the seed catalog when the catalog is empty, then creates each demo email
    /// that has no account as a registered user with the demo password, together with its profile, its demo custom
    /// habits and its demo goals (FR-040: the catalog comes first, so the catalog habits the goals name exist). The
    /// supplied time is read once: it is every created account's creation time, and its date in the installation's
    /// default time zone is the day of the run. An existing account, of either role, is left unchanged and its private
    /// data is neither read nor changed, so a second run changes nothing (SC-006, SC-007, ADR 0013).
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

        var seeded = await _demoData.TryCreateSeedCatalogAsync(SeedCatalog.Habits, cancellationToken).ConfigureAwait(false);
        var catalog = seeded ? DemoCatalogResult.SeedCatalogLoaded : DemoCatalogResult.CatalogNotEmpty;

        var now = _timeProvider.GetUtcNow();
        var zone = _defaultTimeZone.Value;
        var today = zone.DateAt(now);

        var results = new List<DemoAccountResult>(DemoEmails.Count);
        foreach (var text in DemoEmails)
        {
            if (!Email.TryParse(text, out var email))
            {
                throw new InvalidOperationException($"The demo email {text} is not valid.");
            }

            var profile = DemoAccounts.ProfileFor(email.Value, zone, now);
            results.Add(await LoadAccountAsync(email, demoPassword, profile, today, cancellationToken).ConfigureAwait(false));
        }

        return DemoLoadingResult.Loaded(results, catalog);
    }

    private async Task<DemoAccountResult> LoadAccountAsync(
        Email email, string password, DemoAccountProfile profile, DateOnly today, CancellationToken cancellationToken)
    {
        if (await _accounts.FindByEmailAsync(email, cancellationToken).ConfigureAwait(false) is not null)
        {
            return DemoAccountResult.AlreadyExists(email.Value);
        }

        // A registration or another start that takes the email in the meantime leaves its account as it is.
        var created = await _demoData
            .CreateDemoAccountAsync(
                email,
                password,
                DemoCustomHabits.For(email.Value),
                profile,
                DemoGoals.For(email.Value, today),
                cancellationToken)
            .ConfigureAwait(false);
        return created == CreateDemoAccountOutcome.EmailTaken
            ? DemoAccountResult.AlreadyExists(email.Value)
            : DemoAccountResult.Created(email.Value);
    }
}
