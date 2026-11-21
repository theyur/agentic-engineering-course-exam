namespace HT.AcceptanceTests.Support.F008;

/// <summary>
/// ADR 0003: an AC run starts with an empty HT-AC database. Test-run hooks are not allowed per feature, so this
/// feature resets the schema through the public reset entry point before its first scenario. Every scenario of this
/// feature arranges its own accounts, habits and goals; the demo scenarios reset the database again themselves.
/// </summary>
[Binding]
[Scope(Tag = "F008")]
public sealed class F008Hooks
{
    [BeforeFeature]
    public static async Task ResetAcDatabaseBeforeFeature()
    {
        await F008Driver.ResetDatabaseAsync();
    }
}
