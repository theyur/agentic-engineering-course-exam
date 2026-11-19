namespace HT.AcceptanceTests.Support.F005;

/// <summary>
/// ADR 0003: an AC run starts with an empty HT-AC database. Test-run hooks are not allowed per feature,
/// so this feature resets the schema through the public reset entry point before its first scenario.
/// Every scenario of this feature arranges its own accounts, habits and goals; the demo scenarios reset
/// the database again themselves ("the AC database is reset").
/// </summary>
[Binding]
[Scope(Tag = "F005")]
public sealed class F005Hooks
{
    [BeforeFeature]
    public static async Task ResetAcDatabaseBeforeFeature()
    {
        await F005Driver.ResetDatabaseAsync();
    }
}
