using HT.Application;
using HT.Application.Accounts;
using HT.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Records;

/// <summary>
/// The migration <c>Records</c> (data-model.md "Tables", research R-06) on a goal literally created before it, on its
/// own database <c>HT-IT-MIGRATE</c>, on the pattern of <c>PreSliceAccountMigrationTests</c>: the two tables, their
/// keys, the clustered index, the identity and the cascades, no check constraint, the goal unchanged, and <c>Down</c>.
/// The one reflection of the tests: the internal context resolved by its type name.
/// </summary>
[Collection(MigrationDatabaseCollection.Name)]
public sealed class RecordsMigrationTests(MigrationDatabaseFixture database)
{
    private const string LastMigrationBeforeRecords = "Goals";
    private const string Email = "it-rec-migration@example.com";

    [Fact]
    public async Task Records_adds_the_two_tables_and_leaves_an_existing_goal_unchanged()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(InstallationTimeZone.From("Europe/Kyiv"))
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(database.Clock);
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Delete the database and migrate to the last migration before Records.
        await MigrateToAsync(provider, LastMigrationBeforeRecords, deleteFirst: true);
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "GoalRecords"));
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "GoalNotes"));

        // An account, a custom habit and a goal with its version and attempt, in plain SQL.
        var goal = await InsertGoalAsync();
        var before = await GoalStateAsync(goal);

        // Records itself, not the latest: a later migration (slice 8, GoalChanges) adds columns to the goal's tables,
        // which the before-and-after comparison below would see. The public entry point runs at the end.
        await MigrateToAsync(provider, "Records", deleteFirst: false);

        Assert.True(await Sql.TableExistsAsync(database.ConnectionString, "GoalRecords"));
        Assert.True(await Sql.TableExistsAsync(database.ConnectionString, "GoalNotes"));
        string[] recordColumns =
        [
            "Id uniqueidentifier NOT NULL",
            "GoalId uniqueidentifier NOT NULL",
            "Date date NOT NULL",
            "Answer tinyint NULL",
            "Amount int NULL",
            "Sequence bigint NOT NULL identity",
        ];
        string[] noteColumns = ["GoalId uniqueidentifier NOT NULL", "EntryDate date NOT NULL", "Text nvarchar(500) NOT NULL"];
        Assert.Equal(recordColumns, await ColumnsAsync("GoalRecords"));
        Assert.Equal(noteColumns, await ColumnsAsync("GoalNotes"));

        // Keys and the clustered index: the records' key is nonclustered, the index (GoalId, Date, Sequence) clustered,
        // and no other index (the foreign key's index is the clustered one).
        string[] recordIndexes =
        [
            "IX_GoalRecords_GoalId_Date_Sequence CLUSTERED GoalId,Date,Sequence",
            "PK_GoalRecords primary NONCLUSTERED unique Id",
        ];
        string[] noteIndexes = ["PK_GoalNotes primary CLUSTERED unique GoalId,EntryDate"];
        Assert.Equal(recordIndexes, await IndexesAsync("GoalRecords"));
        Assert.Equal(noteIndexes, await IndexesAsync("GoalNotes"));

        // The foreign keys to Goals cascade; no check constraint.
        string[] recordKeys = ["FK_GoalRecords_Goals_GoalId GoalId -> Goals.Id CASCADE"];
        string[] noteKeys = ["FK_GoalNotes_Goals_GoalId GoalId -> Goals.Id CASCADE"];
        Assert.Equal(recordKeys, await ForeignKeysAsync("GoalRecords"));
        Assert.Equal(noteKeys, await ForeignKeysAsync("GoalNotes"));
        Assert.Equal(0, await CheckConstraintCountAsync("GoalRecords"));
        Assert.Equal(0, await CheckConstraintCountAsync("GoalNotes"));

        // The goal, its version and its attempt are unchanged.
        Assert.Equal(before, await GoalStateAsync(goal));

        // The identity numbers the records in insertion order.
        await InsertRecordAsync(goal, new DateOnly(2026, 11, 18), answer: null, amount: 12);
        await InsertRecordAsync(goal, new DateOnly(2026, 11, 18), answer: 1, amount: null);
        var sequences = (await Sql.QueryAsync(database.ConnectionString,
                "SELECT [Sequence] FROM [GoalRecords] WHERE [GoalId] = @goal ORDER BY [Sequence]", ("@goal", goal)))
            .Select(row => (long)row["Sequence"]!)
            .ToList();
        Assert.Equal(2, sequences.Count);
        Assert.True(sequences[0] < sequences[1]);

        // Down drops both tables and leaves the goal; then the database is migrated to the latest again.
        await MigrateToAsync(provider, LastMigrationBeforeRecords, deleteFirst: false);
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "GoalRecords"));
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "GoalNotes"));
        Assert.Equal(before, await GoalStateAsync(goal));
        Assert.True(await HtDatabase.MigrateAsync(provider, CancellationToken.None) > 0);
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
            VALUES (@id, @email, @email, @email, @email, 0, @stamp, @stamp, 0, 0, 0, 0, N'RegisteredUser', N'Europe/Kyiv', 1)
            """,
            ("@id", account), ("@email", Email), ("@stamp", Guid.NewGuid().ToString("N")));

        var habit = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [CustomHabits] ([Id], [OwnerId], [Name], [NameKey], [Description], [Category], [Intent], [Icon])
            VALUES (@id, @owner, N'It-Rec Migration habit', N'it-rec migration habit', NULL, 'Mind', 'Build', 'book-open')
            """,
            ("@id", habit), ("@owner", account));

        var goal = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [Goals] ([Id], [OwnerId], [CatalogHabitId], [CustomHabitId], [StartDate], [Name])
            VALUES (@id, @owner, NULL, @habit, '2026-11-18', N'It-Rec Migration goal');
            INSERT INTO [GoalSettingsVersions] ([GoalId], [AppliesFrom], [BuildOrReduce], [Measurement], [Amount], [CountUnit],
                [DurationUnit], [ScheduleKind], [Days], [Times], [Period], [TimeZone])
            VALUES (@id, '2026-11-18', 'Build', 'Duration', 20, NULL, 'Minutes', 'FixedDays', 127, NULL, NULL, N'Europe/Kyiv');
            INSERT INTO [GoalAttempts] ([GoalId], [Number], [StartDate]) VALUES (@id, 1, '2026-11-18');
            """,
            ("@id", goal), ("@owner", account), ("@habit", habit));
        return goal;
    }

    /// <summary>The goal's row, version rows and attempt rows, each column as text, for an exact before-and-after comparison.</summary>
    private async Task<IReadOnlyList<string>> GoalStateAsync(Guid goal)
    {
        var lines = new List<string>();
        foreach (var (table, order) in new[] { ("Goals", "[Id]"), ("GoalSettingsVersions", "[AppliesFrom]"), ("GoalAttempts", "[Number]") })
        {
            var key = table == "Goals" ? "[Id]" : "[GoalId]";
            var rows = await Sql.QueryAsync(database.ConnectionString,
                $"SELECT * FROM [{table}] WHERE {key} = @goal ORDER BY {order}", ("@goal", goal));
            Assert.NotEmpty(rows);
            lines.AddRange(rows.Select(row => table + ": " + string.Join(", ", row.OrderBy(c => c.Key, StringComparer.Ordinal)
                .Select(c => $"{c.Key}={Convert.ToString(c.Value, System.Globalization.CultureInfo.InvariantCulture)}"))));
        }

        return lines;
    }

    private Task<int> InsertRecordAsync(Guid goal, DateOnly date, byte? answer, int? amount) =>
        Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalRecords] ([Id], [GoalId], [Date], [Answer], [Amount]) VALUES (@id, @goal, @date, @answer, @amount)",
            ("@id", Guid.NewGuid()), ("@goal", goal), ("@date", date.ToDateTime(TimeOnly.MinValue)),
            ("@answer", (object?)answer ?? DBNull.Value), ("@amount", (object?)amount ?? DBNull.Value));

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

    private async Task<int> CheckConstraintCountAsync(string table) =>
        (int)(await Sql.ScalarAsync(database.ConnectionString,
            "SELECT COUNT(*) FROM sys.check_constraints WHERE [parent_object_id] = OBJECT_ID(@table)", ("@table", table)))!;
}
