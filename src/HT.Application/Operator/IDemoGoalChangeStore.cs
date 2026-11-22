using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// The demo goal-change port (slice 8 FR-031, research R-09, ADR 0013, ADR 0026): one member, one database transaction,
/// so the demo account with goal changes is created with its profile, goals, settings versions, attempts, endings,
/// records and notes as a whole or not at all. It has the contract of <see cref="IDemoPauseStore"/>, which stays
/// unchanged, with each goal's attempts and their versions and endings in place of version 1 and attempt 1, and is a
/// separate port so that each is fenced to its own service. Its only read is the state of the named catalog habits,
/// which is public catalog data; it reads no private data. Public for the AC reference export; architecture rules keep
/// every HT.Ui type, and every HT.Application type other than <see cref="DemoGoalChangeService"/>, away from it.
/// </summary>
public interface IDemoGoalChangeStore
{
    /// <summary>
    /// Creates a registered-user account with this email, password and profile (name, time zone, creation time), then
    /// these goals with their attempts, settings versions, endings, records and notes, in one transaction. An email
    /// that already has an account gives <see cref="CreateDemoAccountOutcome.EmailTaken"/> and creates nothing. Each
    /// goal finds its catalog habit by name key and is written only when that habit exists in one of the goal's
    /// accepted states: the goal from its attempt 1's start date, each attempt with its ending and each of its versions
    /// in the profile's time zone, then its records in list order and its notes, stored directly as facts (FR-026);
    /// otherwise it is left out with all of them, with no error (FR-031). A goal, attempt, version, record or note that
    /// cannot be written is a fault that rolls everything back; its exception names no goal, habit, note, outcome or
    /// value (HP-010).
    /// </summary>
    Task<CreateDemoAccountOutcome> CreateDemoGoalChangeAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoGoalChangeGoal> goals,
        CancellationToken cancellationToken);
}
