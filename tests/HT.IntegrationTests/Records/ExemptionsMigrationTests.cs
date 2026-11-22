using System.Globalization;
using HT.Application;
using HT.Application.Accounts;
using HT.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.IntegrationTests.Records;

/// <summary>
/// The migration <c>ExcusalsAndPauses</c> (data-model.md "Tables", research R-04, R-11) on a goal literally created
/// before it, on <c>HT-IT-MIGRATE</c>, on the pattern of <see cref="RecordsMigrationTests"/>: the two tables, their keys,
/// the index on (goal, first day), the nullable reasons, the cascades from <c>Goals</c>, no check constraint, no column
/// for a time or a state (FR-025), the goal unchanged, and <c>Down</c>. The test composes its own provider with its own
/// clock, and the one reflection of the tests resolves the internal context by its type name.
/// </summary>
[Collection(MigrationDatabaseCollection.Name)]
public sealed class ExemptionsMigrationTests(MigrationDatabaseFixture database)
{
    private const string LastMigrationBeforeExemptions = "Records";
    private const string Email = "it-exm-migration@example.com";

    [Fact]
    public async Task ExcusalsAndPauses_adds_the_two_tables_and_leaves_an_existing_goal_unchanged()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(InstallationTimeZone.From("Europe/Kyiv"))
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 11, 21, 10, 0, 0, TimeSpan.Zero)));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Delete the database and migrate to the last migration before ExcusalsAndPauses.
        await MigrateToAsync(provider, LastMigrationBeforeExemptions, deleteFirst: true);
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "GoalExcusals"));
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "GoalPauses"));

        // An account, a custom habit and a goal with its version, attempt and a record, in plain SQL.
        var goal = await InsertGoalAsync();
        var before = await GoalStateAsync(goal);

        // ExcusalsAndPauses itself, not the latest: a later migration (slice 8, GoalChanges) adds columns to the goal's
        // tables, which the before-and-after comparison below would see. The public entry point runs at the end.
        await MigrateToAsync(provider, "ExcusalsAndPauses", deleteFirst: false);

        // Dates and reasons only: no column for a time or a state (FR-025). The reasons are nullable.
        string[] excusalColumns = ["GoalId uniqueidentifier NOT NULL", "Date date NOT NULL", "Reason nvarchar(200) NULL"];
        string[] pauseColumns =
        [
            "Id uniqueidentifier NOT NULL",
            "GoalId uniqueidentifier NOT NULL",
            "FirstDay date NOT NULL",
            "LastDay date NOT NULL",
            "Reason nvarchar(200) NULL",
        ];
        Assert.Equal(excusalColumns, await ColumnsAsync("GoalExcusals"));
        Assert.Equal(pauseColumns, await ColumnsAsync("GoalPauses"));

        // Keys and the index: an excusal's key is (goal, date), so a commitment has at most one; a pause's key is its
        // identifier, with the index (GoalId, FirstDay), which also serves the foreign key.
        string[] excusalIndexes = ["PK_GoalExcusals primary CLUSTERED unique GoalId,Date"];
        string[] pauseIndexes =
        [
            "IX_GoalPauses_GoalId_FirstDay NONCLUSTERED GoalId,FirstDay",
            "PK_GoalPauses primary CLUSTERED unique Id",
        ];
        Assert.Equal(excusalIndexes, await IndexesAsync("GoalExcusals"));
        Assert.Equal(pauseIndexes, await IndexesAsync("GoalPauses"));

        // The foreign keys to Goals cascade; no check constraint and no default.
        Assert.Equal(["FK_GoalExcusals_Goals_GoalId GoalId -> Goals.Id CASCADE"], await ForeignKeysAsync("GoalExcusals"));
        Assert.Equal(["FK_GoalPauses_Goals_GoalId GoalId -> Goals.Id CASCADE"], await ForeignKeysAsync("GoalPauses"));
        foreach (var table in new[] { "GoalExcusals", "GoalPauses" })
        {
            Assert.Equal(0, await CountAsync("sys.check_constraints", table));
            Assert.Equal(0, await CountAsync("sys.default_constraints", table));
        }

        // The goal, its version, attempt and record are unchanged.
        Assert.Equal(before, await GoalStateAsync(goal));

        // A reason may be absent; a second excusal of the same date breaks the key.
        await InsertExcusalAsync(goal, new DateOnly(2026, 11, 18));
        await Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalPauses] ([Id], [GoalId], [FirstDay], [LastDay], [Reason]) VALUES (@id, @goal, '2026-11-23', '2026-11-29', NULL)",
            ("@id", Guid.NewGuid()), ("@goal", goal));
        await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(() => InsertExcusalAsync(goal, new DateOnly(2026, 11, 18)));

        // Down drops both tables and leaves the goal; then the database is migrated to the latest again.
        await MigrateToAsync(provider, LastMigrationBeforeExemptions, deleteFirst: false);
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "GoalExcusals"));
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "GoalPauses"));
        Assert.Equal(before, await GoalStateAsync(goal));
        Assert.True(await HtDatabase.MigrateAsync(provider, CancellationToken.None) > 0);
        Assert.True(await Sql.TableExistsAsync(database.ConnectionString, "GoalExcusals"));
        Assert.True(await Sql.TableExistsAsync(database.ConnectionString, "GoalPauses"));
        Assert.NotNull(await Sql.ReadUserAsync(database.ConnectionString, Email));
    }

    private static async Task MigrateToAsync(IServiceProvider provider, string migration, bool deleteFirst)
    {
        await using var scope = provider.CreateAsyncScope();
        var contextType = typeof(PersistenceAssemblyMarker).Assembly.GetType("HT.Persistence.HtDbContext", throwOnError: true)!;
        var db = Assert.IsType<DbContext>(scope.ServiceProvider.GetRequiredService(contextType), exactMatch: false);
        if (deleteFirst)
        {
            await db.Database.EnsureDeletedAsync();
        }

        await db.GetService<IMigrator>().MigrateAsync(migration);
    }

    private async Task<Guid> InsertGoalAsync()
    {
        var account = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed],
                [SecurityStamp], [ConcurrencyStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled],
                [AccessFailedCount], [Role], [TimeZone], [GoalsRevision])
            VALUES (@id, @email, @email, @email, @email, 0, @stamp, @stamp, 0, 0, 0, 0, N'RegisteredUser', N'UTC', 1)
            """,
            ("@id", account), ("@email", Email), ("@stamp", Guid.NewGuid().ToString("N")));

        var habit = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [CustomHabits] ([Id], [OwnerId], [Name], [NameKey], [Description], [Category], [Intent], [Icon])
            VALUES (@id, @owner, N'It-Exm Migration habit', N'it-exm migration habit', NULL, 'Mind', 'Build', 'book-open')
            """,
            ("@id", habit), ("@owner", account));

        var goal = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [Goals] ([Id], [OwnerId], [CatalogHabitId], [CustomHabitId], [StartDate], [Name])
            VALUES (@id, @owner, NULL, @habit, '2026-11-16', N'It-Exm Migration goal');
            INSERT INTO [GoalSettingsVersions] ([GoalId], [AppliesFrom], [BuildOrReduce], [Measurement], [Amount], [CountUnit],
                [DurationUnit], [ScheduleKind], [Days], [Times], [Period], [TimeZone])
            VALUES (@id, '2026-11-16', 'Build', 'YesNo', NULL, NULL, NULL, 'FixedDays', 42, NULL, NULL, N'UTC');
            INSERT INTO [GoalAttempts] ([GoalId], [Number], [StartDate]) VALUES (@id, 1, '2026-11-16');
            INSERT INTO [GoalRecords] ([Id], [GoalId], [Date], [Answer], [Amount]) VALUES (NEWID(), @id, '2026-11-16', 1, NULL);
            """,
            ("@id", goal), ("@owner", account), ("@habit", habit));
        return goal;
    }

    private Task<int> InsertExcusalAsync(Guid goal, DateOnly date) =>
        Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalExcusals] ([GoalId], [Date], [Reason]) VALUES (@goal, @date, NULL)",
            ("@goal", goal), ("@date", date.ToDateTime(TimeOnly.MinValue)));

    /// <summary>The goal's row, version, attempt and record rows, each column as text, for an exact before-and-after comparison.</summary>
    private async Task<IReadOnlyList<string>> GoalStateAsync(Guid goal)
    {
        var lines = new List<string>();
        foreach (var (table, order) in new[]
                 {
                     ("Goals", "[Id]"), ("GoalSettingsVersions", "[AppliesFrom]"), ("GoalAttempts", "[Number]"), ("GoalRecords", "[Sequence]"),
                 })
        {
            var key = table == "Goals" ? "[Id]" : "[GoalId]";
            var rows = await Sql.QueryAsync(database.ConnectionString,
                $"SELECT * FROM [{table}] WHERE {key} = @goal ORDER BY {order}", ("@goal", goal));
            Assert.NotEmpty(rows);
            lines.AddRange(rows.Select(row => table + ": " + string.Join(", ", row.OrderBy(c => c.Key, StringComparer.Ordinal)
                .Select(c => $"{c.Key}={Convert.ToString(c.Value, CultureInfo.InvariantCulture)}"))));
        }

        return lines;
    }

    /// <summary>"name type NULL|NOT NULL [identity]" per column, in column order; nvarchar with its length.</summary>
    private async Task<IReadOnlyList<string>> ColumnsAsync(string table)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            """
            SELECT c.[name] AS [Name], t.[name] AS [Type], c.[max_length] AS [Length], c.[is_nullable] AS [Nullable],
                c.[is_identity] AS [Identity]
            FROM sys.columns c JOIN sys.types t ON t.[user_type_id] = c.[user_type_id]
            WHERE c.[object_id] = OBJECT_ID(@table)
            ORDER BY c.[column_id]
            """,
            ("@table", table));
        return rows.Select(row =>
        {
            var type = (string)row["Type"]!;
            if (type == "nvarchar")
            {
                type += $"({(short)row["Length"]! / 2})";
            }

            return $"{row["Name"]} {type} {((bool)row["Nullable"]! ? "NULL" : "NOT NULL")}{((bool)row["Identity"]! ? " identity" : string.Empty)}";
        }).ToList();
    }

    /// <summary>"name [primary] CLUSTERED|NONCLUSTERED [unique] columns" per index, columns in key order.</summary>
    private async Task<IReadOnlyList<string>> IndexesAsync(string table)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            """
            SELECT i.[name] AS [Name], i.[type_desc] AS [Kind], i.[is_primary_key] AS [IsPrimary], i.[is_unique] AS [IsUnique],
                STRING_AGG(c.[name], ',') WITHIN GROUP (ORDER BY ic.[key_ordinal]) AS [Columns]
            FROM sys.indexes i
            JOIN sys.index_columns ic ON ic.[object_id] = i.[object_id] AND ic.[index_id] = i.[index_id] AND ic.[key_ordinal] > 0
            JOIN sys.columns c ON c.[object_id] = ic.[object_id] AND c.[column_id] = ic.[column_id]
            WHERE i.[object_id] = OBJECT_ID(@table)
            GROUP BY i.[name], i.[type_desc], i.[is_primary_key], i.[is_unique]
            ORDER BY i.[name]
            """,
            ("@table", table));
        return rows.Select(row => string.Join(" ", new[]
        {
            (string)row["Name"]!,
            (bool)row["IsPrimary"]! ? "primary" : null,
            (string)row["Kind"]!,
            (bool)row["IsUnique"]! ? "unique" : null,
            (string)row["Columns"]!,
        }.Where(part => part is not null))).ToList();
    }

    /// <summary>"name column -> table.column ACTION" per foreign key.</summary>
    private async Task<IReadOnlyList<string>> ForeignKeysAsync(string table)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            """
            SELECT fk.[name] AS [Name], pc.[name] AS [Column], OBJECT_NAME(fk.[referenced_object_id]) AS [Target],
                rc.[name] AS [TargetColumn], fk.[delete_referential_action_desc] AS [OnDelete]
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fkc.[constraint_object_id] = fk.[object_id]
            JOIN sys.columns pc ON pc.[object_id] = fkc.[parent_object_id] AND pc.[column_id] = fkc.[parent_column_id]
            JOIN sys.columns rc ON rc.[object_id] = fkc.[referenced_object_id] AND rc.[column_id] = fkc.[referenced_column_id]
            WHERE fk.[parent_object_id] = OBJECT_ID(@table)
            ORDER BY fk.[name]
            """,
            ("@table", table));
        return rows.Select(row => $"{row["Name"]} {row["Column"]} -> {row["Target"]}.{row["TargetColumn"]} {row["OnDelete"]}").ToList();
    }

    /// <summary>How many rows of the catalog view <paramref name="view"/> (check or default constraints) belong to <paramref name="table"/>.</summary>
    private async Task<int> CountAsync(string view, string table) =>
        (int)(await Sql.ScalarAsync(database.ConnectionString,
            $"SELECT COUNT(*) FROM {view} WHERE [parent_object_id] = OBJECT_ID(@table)", ("@table", table)))!;
}
