using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// Demo history loading (FR-030, research R-08, ADR 0018): the demo account with history, <see cref="DemoEmail"/>, with
/// the goals, records and notes of <see cref="DemoHistory"/> relative to the run date, created as a whole with the
/// account (ADR 0013). The host runs it at every start, right after demo loading (HP-007); no page offers it, and an
/// architecture rule keeps every HT.Ui type outside <c>HT.Ui.Operator</c> away from this service. Demo loading
/// (<see cref="DemoDataService"/>) is not changed by it (FR-031).
/// </summary>
public sealed class DemoHistoryService
{
    /// <summary>The demo email of the account with history, in the reserved <c>example.com</c> domain.</summary>
    public const string DemoEmail = "demo4@example.com";

    private readonly IAccountStore _accounts;
    private readonly IDemoHistoryStore _demoHistory;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public DemoHistoryService(
        IAccountStore accounts, IDemoHistoryStore demoHistory, TimeProvider clock, InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(demoHistory);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _accounts = accounts;
        _demoHistory = demoHistory;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// Checks the demo password as demo loading does, then creates <see cref="DemoEmail"/> when it has no account (looked
    /// up ignoring case), as a registered user with the demo password, no account name, the installation's default
    /// time zone and the supplied time as its creation time, together with the goals, records and notes of
    /// <see cref="DemoHistory.For"/> for the run date R: the date of the supplied time, read once, in the default zone.
    /// An existing account, of either role, is reported as "already exists" and its data is neither read nor changed
    /// (ADR 0013). The results name the email and the result texts only (HP-006).
    /// </summary>
    public async Task<DemoHistoryLoadingResult> LoadAsync(string? demoPassword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(demoPassword))
        {
            return DemoHistoryLoadingResult.NoPassword;
        }

        if (!PasswordRule.IsMet(demoPassword))
        {
            return DemoHistoryLoadingResult.PasswordRuleNotMet;
        }

        var now = _clock.GetUtcNow();
        var zone = _defaultZone.Value;
        var runDate = zone.DateAt(now);

        if (!Email.TryParse(DemoEmail, out var email))
        {
            throw new InvalidOperationException($"The demo email {DemoEmail} is not valid.");
        }

        if (await _accounts.FindByEmailAsync(email, cancellationToken).ConfigureAwait(false) is not null)
        {
            return DemoHistoryLoadingResult.Loaded(DemoHistoryAccountResult.AlreadyExists(email.Value));
        }

        // A registration or another start that takes the email in the meantime leaves its account as it is.
        var created = await _demoHistory
            .CreateDemoHistoryAccountAsync(
                email, demoPassword, new DemoAccountProfile(null, zone, now), DemoHistory.For(runDate), cancellationToken)
            .ConfigureAwait(false);
        return DemoHistoryLoadingResult.Loaded(created == CreateDemoAccountOutcome.EmailTaken
            ? DemoHistoryAccountResult.AlreadyExists(email.Value)
            : DemoHistoryAccountResult.Created(email.Value));
    }
}
