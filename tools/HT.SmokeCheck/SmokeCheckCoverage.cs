using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.SmokeCheck;

/// <summary>
/// One data parameter of the demo data port <see cref="IDemoDataStore"/> and the smoke check that verifies what it
/// writes (contracts/smoke-check.md "Coverage rule").
/// </summary>
/// <param name="Method">The port method's name.</param>
/// <param name="Parameter">The parameter's name.</param>
/// <param name="Type">The parameter's type.</param>
/// <param name="Check">The check that verifies it.</param>
public sealed record CoverageEntry(string Method, string Parameter, Type Type, string Check);

/// <summary>
/// The coverage map of contracts/smoke-check.md "Coverage rule": every data parameter of every method of
/// <see cref="IDemoDataStore"/>, with the check that verifies it. A test fails when the port gains a method or a
/// parameter, or changes a parameter's type, without an entry here, so a slice that adds demo data extends the checks.
/// </summary>
public static class SmokeCheckCoverage
{
    public static IReadOnlyList<CoverageEntry> Entries { get; } =
    [
        new(nameof(IDemoDataStore.CreateDemoAccountAsync), "email", typeof(Email), "5 demo <email>: the sign-in"),
        new(nameof(IDemoDataStore.CreateDemoAccountAsync), "password", typeof(string), "5 demo <email>: the sign-in"),
        new(nameof(IDemoDataStore.CreateDemoAccountAsync), "customHabits", typeof(IReadOnlyList<HabitDefinition>), "5 demo <email>: the custom habit names"),
        new(nameof(IDemoDataStore.CreateDemoAccountAsync), "profile", typeof(DemoAccountProfile), "5 demo <email>: the account name on Profile"),
        new(nameof(IDemoDataStore.CreateDemoAccountAsync), "goals", typeof(IReadOnlyList<DemoGoal>), "5 demo <email>: the goal names on Goals"),
        new(nameof(IDemoDataStore.TryCreateSeedCatalogAsync), "habits", typeof(IReadOnlyList<SeedCatalogHabit>), "3 catalog"),
    ];
}
