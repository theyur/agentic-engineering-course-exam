using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// The demo report port (FR-032, research R-09, ADR 0013, ADR 0021): one member, one database transaction, so the demo
/// account with the longer history is created with its profile, goals, records and notes as a whole or not at all. It
/// has the same contract as <see cref="IDemoHistoryStore"/>, which stays unchanged, and is a separate port so that each
/// is fenced to its own service. Its only read is the state of the named catalog habits, which is public catalog data;
/// it reads no private data. Public for the AC reference export; architecture rules keep every HT.Ui type, and every
/// HT.Application type other than <see cref="DemoReportService"/>, away from it (research R-14).
/// </summary>
public interface IDemoReportStore
{
    /// <summary>
    /// Creates a registered-user account with this email, password and profile (name, time zone, creation time), then
    /// these goals with their records and notes, in one transaction. An email that already has an account gives
    /// <see cref="CreateDemoAccountOutcome.EmailTaken"/> and creates nothing. Each goal finds its catalog habit by name
    /// key and is written only when that habit exists in one of the goal's accepted states, with settings version 1 in
    /// the profile's time zone and attempt 1, both from its start date, then its records in list order and its notes;
    /// otherwise it is left out with its records and notes, with no error (FR-032). A goal, record or note that cannot
    /// be written is a fault that rolls everything back; its exception names no goal, habit, note or value (HP-007).
    /// </summary>
    Task<CreateDemoAccountOutcome> CreateDemoReportAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoHistoryGoal> goals,
        CancellationToken cancellationToken);
}
