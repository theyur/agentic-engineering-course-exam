namespace HT.AcceptanceTests.Support.F009;

/// <summary>
/// ADR 0003: an AC run starts with an empty HT-AC database. Test-run hooks are not allowed per feature, so this
/// feature resets the schema through the public reset entry point before its first scenario. Every scenario of this
/// feature arranges its own accounts, habits and goals; the demo and catalog scenarios reset the database again
/// themselves (acceptance.md, "Demo data").
/// </summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class F009Hooks
{
    [BeforeFeature]
    public static async Task ResetAcDatabaseBeforeFeature()
    {
        await F009Driver.ResetDatabaseAsync();
    }
}
