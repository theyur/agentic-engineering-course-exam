namespace HT.AcceptanceTests.Support.F002;

/// <summary>
/// ADR 0003: an AC run starts with an empty HT-AC database. Test-run hooks are not allowed per feature,
/// so this feature resets the schema through the public reset entry point before its first scenario.
/// Every scenario of this feature arranges its own accounts and habits.
/// </summary>
[Binding]
[Scope(Tag = "F002")]
public sealed class F002Hooks
{
    [BeforeFeature]
    public static async Task ResetAcDatabaseBeforeFeature()
    {
        await F002Driver.ResetDatabaseAsync();
    }
}
