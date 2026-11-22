using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// Demo goal-change loading (slice 8 FR-031, research R-09, ADR 0026): the demo account with goal changes,
/// <see cref="DemoEmail"/>, with the goals, settings versions, attempts, endings, records and notes of
/// <see cref="DemoGoalChanges"/> relative to the run date, created as a whole with the account (ADR 0013). The host runs
/// it at every start, right after demo pause loading (HP-011); no page offers it, and an architecture rule keeps every
/// HT.Ui type outside <c>HT.Ui.Operator</c> away from this service. Demo loading, demo history loading, demo report
/// loading and demo pause loading are not changed by it (FR-030).
/// </summary>
public sealed class DemoGoalChangeService
{
    /// <summary>The demo email of the account with goal changes, in the reserved <c>example.com</c> domain.</summary>
    public const string DemoEmail = "demo7@example.com";

    private readonly IDemoGoalChangeStore _demoGoalChange;
    private readonly IAccountStore _accounts;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public DemoGoalChangeService(
        IDemoGoalChangeStore demoGoalChange, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(demoGoalChange);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _demoGoalChange = demoGoalChange;
        _accounts = accounts;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// Checks the demo password as demo loading does, then creates <see cref="DemoEmail"/> when it has no account (looked
    /// up ignoring case), as a registered user with the demo password, no account name, the installation's default
    /// time zone and the supplied time as its creation time, together with the goals of <see cref="DemoGoalChanges.For"/>
    /// for the run date R: the date of the supplied time, read once, in the default zone. An existing account, of either
    /// role, is reported as "already exists" and its data is neither read nor changed (ADR 0013). The results name the
    /// email and the result texts only (FR-031).
    /// </summary>
    public async Task<DemoGoalChangeLoadingResult> LoadAsync(string? demoPassword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(demoPassword))
        {
            return DemoGoalChangeLoadingResult.NoPassword;
        }

        if (!PasswordRule.IsMet(demoPassword))
        {
            return DemoGoalChangeLoadingResult.PasswordRuleNotMet;
        }

        var now = _clock.GetUtcNow();
        var zone = _defaultZone.Value;
        var runDate = zone.DateAt(now);

        // Stryker disable once Block,Statement : DemoEmail is a valid constant, so no test can reach the throw
        if (!Email.TryParse(DemoEmail, out var email))
        {
            throw new InvalidOperationException($"The demo email {DemoEmail} is not valid.");
        }

        if (await _accounts.FindByEmailAsync(email, cancellationToken).ConfigureAwait(false) is not null)
        {
            return DemoGoalChangeLoadingResult.Loaded(DemoGoalChangeAccountResult.AlreadyExists(email.Value));
        }

        // A registration or another start that takes the email in the meantime leaves its account as it is.
        var created = await _demoGoalChange
            .CreateDemoGoalChangeAccountAsync(
                email, demoPassword, new DemoAccountProfile(null, zone, now), DemoGoalChanges.For(runDate), cancellationToken)
            .ConfigureAwait(false);
        return DemoGoalChangeLoadingResult.Loaded(created == CreateDemoAccountOutcome.EmailTaken
            ? DemoGoalChangeAccountResult.AlreadyExists(email.Value)
            : DemoGoalChangeAccountResult.Created(email.Value));
    }
}
