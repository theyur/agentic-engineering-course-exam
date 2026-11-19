using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.SmokeCheck;

/// <summary>
/// One data parameter of a demo data port, <see cref="IDemoDataStore"/> or <see cref="IDemoHistoryStore"/>, and the
/// smoke check that verifies what it writes (contracts/smoke-check.md "Coverage rule").
/// </summary>
/// <param name="Port">The demo data port.</param>
/// <param name="Method">The port method's name.</param>
/// <param name="Parameter">The parameter's name.</param>
/// <param name="Type">The parameter's type.</param>
/// <param name="Check">The check that verifies it.</param>
public sealed record CoverageEntry(Type Port, string Method, string Parameter, Type Type, string Check);

/// <summary>
/// The coverage map of contracts/smoke-check.md "Coverage rule": every data parameter of every method of the demo data
/// ports <see cref="IDemoDataStore"/> and <see cref="IDemoHistoryStore"/>, with the check that verifies it. A test fails
/// when a port gains a method or a parameter, or changes a parameter's type, without an entry here, so a slice that adds
/// demo data extends the checks.
/// </summary>
public static class SmokeCheckCoverage
{
    public static IReadOnlyList<CoverageEntry> Entries { get; } =
    [
        new(typeof(IDemoDataStore), nameof(IDemoDataStore.CreateDemoAccountAsync), "email", typeof(Email), "5 demo <email>: the sign-in"),
        new(typeof(IDemoDataStore), nameof(IDemoDataStore.CreateDemoAccountAsync), "password", typeof(string), "5 demo <email>: the sign-in"),
        new(typeof(IDemoDataStore), nameof(IDemoDataStore.CreateDemoAccountAsync), "customHabits", typeof(IReadOnlyList<HabitDefinition>), "5 demo <email>: the custom habit names"),
        new(typeof(IDemoDataStore), nameof(IDemoDataStore.CreateDemoAccountAsync), "profile", typeof(DemoAccountProfile), "5 demo <email>: the account name on Profile"),
        new(typeof(IDemoDataStore), nameof(IDemoDataStore.CreateDemoAccountAsync), "goals", typeof(IReadOnlyList<DemoGoal>), "5 demo <email>: the goal names on Goals"),
        new(typeof(IDemoDataStore), nameof(IDemoDataStore.TryCreateSeedCatalogAsync), "habits", typeof(IReadOnlyList<SeedCatalogHabit>), "3 catalog"),
        new(typeof(IDemoHistoryStore), nameof(IDemoHistoryStore.CreateDemoHistoryAccountAsync), "email", typeof(Email), "6 demo history: the sign-in"),
        new(typeof(IDemoHistoryStore), nameof(IDemoHistoryStore.CreateDemoHistoryAccountAsync), "password", typeof(string), "6 demo history: the sign-in"),
        new(typeof(IDemoHistoryStore), nameof(IDemoHistoryStore.CreateDemoHistoryAccountAsync), "profile", typeof(DemoAccountProfile), "6 demo history: the sign-in"),
        new(typeof(IDemoHistoryStore), nameof(IDemoHistoryStore.CreateDemoHistoryAccountAsync), "goals", typeof(IReadOnlyList<DemoHistoryGoal>),
            "6 demo history: the goal names, and the states, partial amount and note on the goal pages"),
    ];
}
