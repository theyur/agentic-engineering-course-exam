using HT.Persistence;

namespace HT.AcceptanceTests.Support.F001;

/// <summary>
/// ADR 0003: an AC run starts with an empty HT-AC database. Test-run hooks are not allowed per feature,
/// so this feature resets the schema through the public reset entry point before its first scenario.
/// Every scenario of this feature arranges its own accounts.
/// </summary>
[Binding]
[Scope(Tag = "F001")]
public sealed class F001Hooks
{
    [BeforeFeature]
    public static async Task ResetAcDatabaseBeforeFeature()
    {
        await HtDatabase.ResetAsync(F001Database.ConnectionString, CancellationToken.None);
    }
}
