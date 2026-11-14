using Microsoft.Data.SqlClient;

namespace HT.IntegrationTests;

/// <summary>Plain-SQL helpers over a connection string. Tests never use the app's database context (ADR 0010).</summary>
public static class Sql
{
    /// <summary>The columns of the <c>AspNetUsers</c> row with this user name, or null when there is none.</summary>
    public static async Task<IReadOnlyDictionary<string, object?>?> ReadUserAsync(string connectionString, string userName)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT * FROM [AspNetUsers] WHERE [UserName] = @userName", connection);
        command.Parameters.AddWithValue("@userName", userName);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var row = new Dictionary<string, object?>(StringComparer.Ordinal);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }

        return row;
    }

    /// <summary>How many <c>AspNetUsers</c> rows have this user name.</summary>
    public static async Task<int> CountUsersAsync(string connectionString, string userName)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT COUNT(*) FROM [AspNetUsers] WHERE [UserName] = @userName", connection);
        command.Parameters.AddWithValue("@userName", userName);
        return (int)(await command.ExecuteScalarAsync())!;
    }

    /// <summary>Deletes the <c>AspNetUsers</c> rows with this user name.</summary>
    public static async Task DeleteUserAsync(string connectionString, string userName)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("DELETE FROM [AspNetUsers] WHERE [UserName] = @userName", connection);
        command.Parameters.AddWithValue("@userName", userName);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>The rows of a parameterized query, each as column name to value (null for DBNull).</summary>
    public static async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(
        string connectionString, string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<IReadOnlyDictionary<string, object?>>();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.Ordinal);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>The single value of a parameterized query.</summary>
    public static async Task<object?> ScalarAsync(string connectionString, string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        var result = await command.ExecuteScalarAsync();
        return result is DBNull ? null : result;
    }

    /// <summary>True when a table with this name exists in the database.</summary>
    public static async Task<bool> TableExistsAsync(string connectionString, string table)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT OBJECT_ID(@table, N'U')", connection);
        command.Parameters.AddWithValue("@table", table);
        return await command.ExecuteScalarAsync() is not (null or DBNull);
    }
}
