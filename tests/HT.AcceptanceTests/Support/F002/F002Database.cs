namespace HT.AcceptanceTests.Support.F002;

/// <summary>
/// The AC test database of ADR 0003: SQL Server database <c>HT-AC</c>, connection string from the
/// environment variable <c>HT_AC_CONNECTION</c>, with the ADR's default when it is not set.
/// </summary>
public static class F002Database
{
    public const string ConnectionVariable = "HT_AC_CONNECTION";

    public const string DefaultConnectionString =
        @"Server=.\COLL1;Database=HT-AC;Integrated Security=True;TrustServerCertificate=True";

    public static string ConnectionString
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable(ConnectionVariable);
            return string.IsNullOrWhiteSpace(configured) ? DefaultConnectionString : configured;
        }
    }
}
