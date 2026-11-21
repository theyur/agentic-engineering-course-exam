using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// Demo pause loading (FR-031, research R-08, ADR 0023): the demo account with excusals and pauses,
/// <see cref="DemoEmail"/>, with the goals, records, notes, excusals and pauses of <see cref="DemoPauses"/> relative to
/// the run date, created as a whole with the account (ADR 0013). The host runs it at every start, right after demo
/// report loading (HP-010); no page offers it, and an architecture rule keeps every HT.Ui type outside
/// <c>HT.Ui.Operator</c> away from this service. Demo loading, demo history loading and demo report loading are not
/// changed by it (FR-030).
/// </summary>
public sealed class DemoPauseService
{
    /// <summary>The demo email of the account with excusals and pauses, in the reserved <c>example.com</c> domain.</summary>
    public const string DemoEmail = "demo6@example.com";

    private readonly IDemoPauseStore _demoPause;
    private readonly IAccountStore _accounts;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public DemoPauseService(
        IDemoPauseStore demoPause, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(demoPause);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _demoPause = demoPause;
        _accounts = accounts;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// Checks the demo password as demo loading does, then creates <see cref="DemoEmail"/> when it has no account (looked
    /// up ignoring case), as a registered user with the demo password, no account name, the installation's default
    /// time zone and the supplied time as its creation time, together with the goals, records, notes, excusals and
    /// pauses of <see cref="DemoPauses.For"/> for the run date R: the date of the supplied time, read once, in the
    /// default zone. An existing account, of either role, is reported as "already exists" and its data is neither read
    /// nor changed (ADR 0013). The results name the email and the result texts only (FR-031).
    /// </summary>
    public async Task<DemoPauseLoadingResult> LoadAsync(string? demoPassword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(demoPassword))
        {
            return DemoPauseLoadingResult.NoPassword;
        }

        if (!PasswordRule.IsMet(demoPassword))
        {
            return DemoPauseLoadingResult.PasswordRuleNotMet;
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
            return DemoPauseLoadingResult.Loaded(DemoPauseAccountResult.AlreadyExists(email.Value));
        }

        // A registration or another start that takes the email in the meantime leaves its account as it is.
        var created = await _demoPause
            .CreateDemoPauseAccountAsync(
                email, demoPassword, new DemoAccountProfile(null, zone, now), DemoPauses.For(runDate), cancellationToken)
            .ConfigureAwait(false);
        return DemoPauseLoadingResult.Loaded(created == CreateDemoAccountOutcome.EmailTaken
            ? DemoPauseAccountResult.AlreadyExists(email.Value)
            : DemoPauseAccountResult.Created(email.Value));
    }
}
