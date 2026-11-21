namespace HT.AcceptanceTests.Support.F007;

/// <summary>
/// ADR 0003: an AC run starts with an empty HT-AC database. Test-run hooks are not allowed per feature, so this
/// feature resets the schema through the public reset entry point before its first scenario. Every scenario of this
/// feature arranges its own accounts, habits and goals; the demo scenarios reset the database again themselves.
/// </summary>
[Binding]
[Scope(Tag = "F007")]
public sealed class F007Hooks
{
    [BeforeFeature]
    public static async Task ResetAcDatabaseBeforeFeature()
    {
        await F007Driver.ResetDatabaseAsync();
    }
}
