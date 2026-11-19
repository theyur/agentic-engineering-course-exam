using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// The demo history port (FR-030, research R-08, ADR 0013, ADR 0018): one member, one database transaction, so the
/// demo account with history is created with its profile, goals, records and notes as a whole or not at all. Its only
/// read is the state of the named catalog habits, which is public catalog data; it reads no private data. A separate
/// port from <see cref="IDemoDataStore"/>, which stays unchanged (FR-031). Public for the AC reference export;
/// architecture rules keep every HT.Ui type, and every HT.Application type outside <c>HT.Application.Operator</c>, away
/// from it (research R-13).
/// </summary>
public interface IDemoHistoryStore
{
    /// <summary>
    /// Creates a registered-user account with this email, password and profile (name, time zone, creation time), then
    /// these goals with their records and notes, in one transaction. An email that already has an account gives
    /// <see cref="CreateDemoAccountOutcome.EmailTaken"/> and creates nothing. Each goal finds its catalog habit by name
    /// key and is written only when that habit exists in one of the goal's accepted states, with settings version 1 in
    /// the profile's time zone and attempt 1, both from its start date, then its records in list order and its notes;
    /// otherwise it is left out with its records and notes, with no error (FR-030). A goal, record or note that cannot
    /// be written is a fault that rolls everything back; its exception names no goal, habit, note or value (HP-006).
    /// </summary>
    Task<CreateDemoAccountOutcome> CreateDemoHistoryAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoHistoryGoal> goals,
        CancellationToken cancellationToken);
}
