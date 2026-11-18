namespace HT.AcceptanceTests.Support.F004;

/// <summary>
/// ADR 0003: an AC run starts with an empty HT-AC database. Test-run hooks are not allowed per feature,
/// so this feature resets the schema through the public reset entry point before its first scenario.
/// Every scenario of this feature arranges its own accounts, habits and goals.
/// </summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class F004Hooks
{
    [BeforeFeature]
    public static async Task ResetAcDatabaseBeforeFeature()
    {
        await F004Driver.ResetDatabaseAsync();
    }
}
