using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// Demo report loading (FR-032, research R-09, ADR 0021): the demo account with the longer history,
/// <see cref="DemoEmail"/>, with the goals, records and notes of <see cref="DemoReport"/> relative to the run date,
/// created as a whole with the account (ADR 0013). The host runs it at every start, right after demo history loading
/// (HP-011); no page offers it, and an architecture rule keeps every HT.Ui type outside <c>HT.Ui.Operator</c> away from
/// this service. Demo loading and demo history loading are not changed by it (FR-030, FR-031).
/// </summary>
public sealed class DemoReportService
{
    /// <summary>The demo email of the account with the longer history, in the reserved <c>example.com</c> domain.</summary>
    public const string DemoEmail = "demo5@example.com";

    private readonly IAccountStore _accounts;
    private readonly IDemoReportStore _demoReport;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public DemoReportService(
        IAccountStore accounts, IDemoReportStore demoReport, TimeProvider clock, InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(demoReport);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _accounts = accounts;
        _demoReport = demoReport;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// Checks the demo password as demo loading does, then creates <see cref="DemoEmail"/> when it has no account (looked
    /// up ignoring case), as a registered user with the demo password, no account name, the installation's default
    /// time zone and the supplied time as its creation time, together with the goals, records and notes of
    /// <see cref="DemoReport.For"/> for the run date R: the date of the supplied time, read once, in the default zone.
    /// An existing account, of either role, is reported as "already exists" and its data is neither read nor changed
    /// (ADR 0013). The results name the email and the result texts only (FR-032).
    /// </summary>
    public async Task<DemoReportLoadingResult> LoadAsync(string? demoPassword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(demoPassword))
        {
            return DemoReportLoadingResult.NoPassword;
        }

        if (!PasswordRule.IsMet(demoPassword))
        {
            return DemoReportLoadingResult.PasswordRuleNotMet;
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
            return DemoReportLoadingResult.Loaded(DemoReportAccountResult.AlreadyExists(email.Value));
        }

        // A registration or another start that takes the email in the meantime leaves its account as it is.
        var created = await _demoReport
            .CreateDemoReportAccountAsync(
                email, demoPassword, new DemoAccountProfile(null, zone, now), DemoReport.For(runDate), cancellationToken)
            .ConfigureAwait(false);
        return DemoReportLoadingResult.Loaded(created == CreateDemoAccountOutcome.EmailTaken
            ? DemoReportAccountResult.AlreadyExists(email.Value)
            : DemoReportAccountResult.Created(email.Value));
    }
}
