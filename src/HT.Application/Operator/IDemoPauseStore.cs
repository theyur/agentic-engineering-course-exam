using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// The demo pause port (FR-031, research R-08, ADR 0013, ADR 0023): one member, one database transaction, so the demo
/// account with excusals and pauses is created with its profile, goals, records, notes, excusals and pauses as a whole
/// or not at all. It has the contract of <see cref="IDemoReportStore"/>, which stays unchanged, plus each goal's
/// excusals and pauses, and is a separate port so that each is fenced to its own service. Its only read is the state of
/// the named catalog habits, which is public catalog data; it reads no private data. Public for the AC reference export;
/// architecture rules keep every HT.Ui type, and every HT.Application type other than <see cref="DemoPauseService"/>,
/// away from it.
/// </summary>
public interface IDemoPauseStore
{
    /// <summary>
    /// Creates a registered-user account with this email, password and profile (name, time zone, creation time), then
    /// these goals with their records, notes, excusals and pauses, in one transaction. An email that already has an
    /// account gives <see cref="CreateDemoAccountOutcome.EmailTaken"/> and creates nothing. Each goal finds its catalog
    /// habit by name key and is written only when that habit exists in one of the goal's accepted states, with settings
    /// version 1 in the profile's time zone and attempt 1, both from its start date, then its records in list order,
    /// its notes, its excusals and its pauses, stored directly as facts (FR-025); otherwise it is left out with all of
    /// them, with no error (FR-031). A goal, record, note, excusal or pause that cannot be written is a fault that rolls
    /// everything back; its exception names no goal, habit, note, reason or value (HP-009).
    /// </summary>
    Task<CreateDemoAccountOutcome> CreateDemoPauseAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoPauseGoal> goals,
        CancellationToken cancellationToken);
}
