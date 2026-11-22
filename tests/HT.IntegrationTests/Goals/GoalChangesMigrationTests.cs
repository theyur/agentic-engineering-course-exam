using System.Globalization;
using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// The migration <c>GoalChanges</c> (slice 8, data-model.md "Tables" and "The migration's order", research R-05, R-12)
/// on a slice 7 goal literally created before it, on <c>HT-IT-MIGRATE</c>, on the pattern of
/// <c>ExemptionsMigrationTests</c> and <see cref="PreSliceAccountMigrationTests"/>: a goal with two versions (a time zone
/// change), a record and an excusal. After the migration every version belongs to attempt 1, the key is (goal, attempt
/// number, applies-from) with a foreign key to the attempt that takes no action on delete, the three ending columns are
/// null, no default or check constraint remains, and the goal reads back unchanged through the goal store. <c>Down</c>
/// restores the slice 7 rows exactly. The test composes its own provider with its own clock, and the one reflection of
/// the tests resolves the internal context by its type name.
/// </summary>
[Collection(MigrationDatabaseCollection.Name)]
public sealed class GoalChangesMigrationTests(MigrationDatabaseFixture database)
{
    private const string LastMigrationBeforeGoalChanges = "ExcusalsAndPauses";
    private const string GoalChanges = "GoalChanges";
    private const string Email = "it-gcm-migration@example.com";

    private static readonly DateOnly Start = new(2026, 11, 16);
    private static readonly DateOnly ZoneChange = new(2026, 11, 20);

    /// <summary>The slice 7 columns of the two changed tables, compared before and after the migration.</summary>
    private static readonly (string Table, string Columns, string Order)[] Slice7Columns =
    [
        ("GoalSettingsVersions",
            "[GoalId], [AppliesFrom], [BuildOrReduce], [Measurement], [Amount], [CountUnit], [DurationUnit], [ScheduleKind], [Days], [Times], [Period], [TimeZone]",
            "[AppliesFrom]"),
        ("GoalAttempts", "[GoalId], [Number], [StartDate]", "[Number]"),
    ];

    /// <summary>The tables the migration does not change, compared column for column.</summary>
    private static readonly (string Table, string Columns, string Order)[] UnchangedTables =
    [
        ("Goals", "*", "[Id]"),
        ("GoalRecords", "*", "[Sequence]"),
        ("GoalExcusals", "*", "[Date]"),
    ];

    [Fact]
    public async Task GoalChanges_puts_every_version_in_attempt_1_and_leaves_an_existing_goal_unchanged()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(InstallationTimeZone.From("Europe/Kyiv"))
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 11, 22, 10, 0, 0, TimeSpan.Zero)));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Delete the database and migrate to the last migration before GoalChanges.
        await MigrateToAsync(provider, LastMigrationBeforeGoalChanges, deleteFirst: true);
        Assert.DoesNotContain("AttemptNumber int NOT NULL", await ColumnsAsync("GoalSettingsVersions"));

        // A slice 7 goal in plain SQL: two versions (a time zone change), its attempt, a record and an excusal.
        var (owner, habit, goal) = await InsertGoalAsync();
        var before = await StateAsync(goal, [.. Slice7Columns, .. UnchangedTables]);
        var slice7Rows = await StateAsync(goal, [.. Slice7Columns.Select(t => (t.Table, "*", t.Order)), .. UnchangedTables]);

        await MigrateToAsync(provider, GoalChanges, deleteFirst: false);

        // (1) The attempt number, not null, 1 for every existing version, and no default left on it (the model has none).
        Assert.Contains("AttemptNumber int NOT NULL", await ColumnsAsync("GoalSettingsVersions"));
        var numbers = await Sql.QueryAsync(database.ConnectionString,
            "SELECT [AttemptNumber] FROM [GoalSettingsVersions] WHERE [GoalId] = @goal ORDER BY [AppliesFrom]", ("@goal", goal));
        int[] firstAttemptOnly = [1, 1];
        Assert.Equal(firstAttemptOnly, numbers.Select(row => (int)row["AttemptNumber"]!));

        // (2) The key (goal, attempt number, applies-from), which also serves both foreign keys; the cascade from Goals
        // stays and the foreign key to the attempt takes no action on delete.
        Assert.Equal(["PK_GoalSettingsVersions primary CLUSTERED unique GoalId,AttemptNumber,AppliesFrom"], await IndexesAsync("GoalSettingsVersions"));
        string[] versionKeys =
        [
            "FK_GoalSettingsVersions_GoalAttempts_GoalId_AttemptNumber GoalId,AttemptNumber -> GoalAttempts.GoalId,Number NO_ACTION",
            "FK_GoalSettingsVersions_Goals_GoalId GoalId -> Goals.Id CASCADE",
        ];
        Assert.Equal(versionKeys, await ForeignKeysAsync("GoalSettingsVersions"));

        // (3) The three ending columns, nullable and null on the existing attempt.
        string[] attemptColumns =
        [
            "GoalId uniqueidentifier NOT NULL",
            "Number int NOT NULL",
            "StartDate date NOT NULL",
            "EndingDate date NULL",
            "EndingNote nvarchar(500) NULL",
            "Outcome varchar(16) NULL",
        ];
        Assert.Equal(attemptColumns, await ColumnsAsync("GoalAttempts"));
        var ending = Assert.Single(await Sql.QueryAsync(database.ConnectionString,
            "SELECT [EndingDate], [Outcome], [EndingNote] FROM [GoalAttempts] WHERE [GoalId] = @goal", ("@goal", goal)));
        Assert.All(ending.Values, Assert.Null);

        // No check or default constraint on either table.
        foreach (var table in new[] { "GoalSettingsVersions", "GoalAttempts" })
        {
            Assert.Equal(0, await CountAsync("sys.check_constraints", table));
            Assert.Equal(0, await CountAsync("sys.default_constraints", table));
        }

        // The slice 7 columns and the other rows are unchanged, and the goal reads back as it was written.
        Assert.Equal(before, await StateAsync(goal, [.. Slice7Columns, .. UnchangedTables]));
        await using (var scope = provider.CreateAsyncScope())
        {
            var stored = await scope.ServiceProvider.GetRequiredService<IGoalStore>().FindAsync(owner, new GoalId(goal), CancellationToken.None);
            Assert.NotNull(stored);
            var read = stored.Goal;
            Assert.Equal((new GoalHabit(habit, HabitKind.Custom), Start, "It-Gcm Migration goal"), (read.Habit, read.StartDate, read.OwnName?.Value));
            var attempt = Assert.Single(read.Attempts);
            Assert.Equal(new GoalAttempt(GoalAttempt.First, Start), attempt.Attempt);
            Assert.False(read.IsEnded);
            Assert.Equal(ExpectedVersions(), attempt.Versions);
        }

        // Down restores the slice 7 rows exactly; then the database is migrated to the latest again.
        await MigrateToAsync(provider, LastMigrationBeforeGoalChanges, deleteFirst: false);
        Assert.DoesNotContain("AttemptNumber int NOT NULL", await ColumnsAsync("GoalSettingsVersions"));
        Assert.Equal(["PK_GoalSettingsVersions primary CLUSTERED unique GoalId,AppliesFrom"], await IndexesAsync("GoalSettingsVersions"));
        Assert.Equal(slice7Rows, await StateAsync(goal, [.. Slice7Columns.Select(t => (t.Table, "*", t.Order)), .. UnchangedTables]));
        Assert.True(await HtDatabase.MigrateAsync(provider, CancellationToken.None) > 0);
        Assert.NotNull(await Sql.ReadUserAsync(database.ConnectionString, Email));
    }

    /// <summary>Daily yes/no from Monday 16 November in UTC, then the same target in Kyiv from Friday 20 November.</summary>
    private static List<GoalSettingsVersion> ExpectedVersions()
    {
        var target = YesNoOn(Enum.GetValues<DayOfWeek>());
        return
        [
            new GoalSettingsVersion(new GoalSettings(target, Zone("UTC")), Start),
            new GoalSettingsVersion(new GoalSettings(target, Zone("Europe/Kyiv")), ZoneChange),
        ];
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

    private async Task<(AccountId Owner, HabitId Habit, Guid Goal)> InsertGoalAsync()
    {
        var account = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed],
                [SecurityStamp], [ConcurrencyStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled],
                [AccessFailedCount], [Role], [TimeZone], [GoalsRevision])
            VALUES (@id, @email, @email, @email, @email, 0, @stamp, @stamp, 0, 0, 0, 0, N'RegisteredUser', N'Europe/Kyiv', 2)
            """,
            ("@id", account), ("@email", Email), ("@stamp", Guid.NewGuid().ToString("N")));

        var habit = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [CustomHabits] ([Id], [OwnerId], [Name], [NameKey], [Description], [Category], [Intent], [Icon])
            VALUES (@id, @owner, N'It-Gcm Migration habit', N'it-gcm migration habit', NULL, 'Mind', 'Build', 'book-open')
            """,
            ("@id", habit), ("@owner", account));

        // Every day is the mask 127 (Monday 1 … Sunday 64).
        var goal = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [Goals] ([Id], [OwnerId], [CatalogHabitId], [CustomHabitId], [StartDate], [Name])
            VALUES (@id, @owner, NULL, @habit, '2026-11-16', N'It-Gcm Migration goal');
            INSERT INTO [GoalSettingsVersions] ([GoalId], [AppliesFrom], [BuildOrReduce], [Measurement], [Amount], [CountUnit],
                [DurationUnit], [ScheduleKind], [Days], [Times], [Period], [TimeZone])
            VALUES (@id, '2026-11-16', 'Build', 'YesNo', NULL, NULL, NULL, 'FixedDays', 127, NULL, NULL, N'UTC'),
                   (@id, '2026-11-20', 'Build', 'YesNo', NULL, NULL, NULL, 'FixedDays', 127, NULL, NULL, N'Europe/Kyiv');
            INSERT INTO [GoalAttempts] ([GoalId], [Number], [StartDate]) VALUES (@id, 1, '2026-11-16');
            INSERT INTO [GoalRecords] ([Id], [GoalId], [Date], [Answer], [Amount]) VALUES (NEWID(), @id, '2026-11-16', 1, NULL);
            INSERT INTO [GoalExcusals] ([GoalId], [Date], [Reason]) VALUES (@id, '2026-11-18', NULL);
            """,
            ("@id", goal), ("@owner", account), ("@habit", habit));
        return (new AccountId(account), new HabitId(habit), goal);
    }

    /// <summary>The goal's rows of each table, with the columns named (or all), each column as text, for an exact comparison.</summary>
    private async Task<IReadOnlyList<string>> StateAsync(Guid goal, IEnumerable<(string Table, string Columns, string Order)> tables)
    {
        var lines = new List<string>();
        foreach (var (table, columns, order) in tables)
        {
            var key = table == "Goals" ? "[Id]" : "[GoalId]";
            var rows = await Sql.QueryAsync(database.ConnectionString,
                $"SELECT {columns} FROM [{table}] WHERE {key} = @goal ORDER BY {order}", ("@goal", goal));
            Assert.NotEmpty(rows);
            lines.AddRange(rows.Select(row => table + ": " + string.Join(", ", row.OrderBy(c => c.Key, StringComparer.Ordinal)
                .Select(c => $"{c.Key}={Convert.ToString(c.Value, CultureInfo.InvariantCulture)}"))));
        }

        return lines;
    }

    /// <summary>"name type NULL|NOT NULL" per column, in column order; nvarchar and varchar with their length.</summary>
    private async Task<IReadOnlyList<string>> ColumnsAsync(string table)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            """
            SELECT c.[name] AS [Name], t.[name] AS [Type], c.[max_length] AS [Length], c.[is_nullable] AS [Nullable]
            FROM sys.columns c JOIN sys.types t ON t.[user_type_id] = c.[user_type_id]
            WHERE c.[object_id] = OBJECT_ID(@table)
            ORDER BY c.[column_id]
            """,
            ("@table", table));
        return rows.Select(row =>
        {
            var type = (string)row["Type"]!;
            var length = (short)row["Length"]!;
            type = type switch
            {
                "nvarchar" => $"{type}({length / 2})",
                "varchar" => $"{type}({length})",
                _ => type,
            };
            return $"{row["Name"]} {type} {((bool)row["Nullable"]! ? "NULL" : "NOT NULL")}";
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

    /// <summary>"name columns -> table.columns ACTION" per foreign key, columns in key order.</summary>
    private async Task<IReadOnlyList<string>> ForeignKeysAsync(string table)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            """
            SELECT fk.[name] AS [Name],
                STRING_AGG(pc.[name], ',') WITHIN GROUP (ORDER BY fkc.[constraint_column_id]) AS [Columns],
                OBJECT_NAME(fk.[referenced_object_id]) AS [Target],
                STRING_AGG(rc.[name], ',') WITHIN GROUP (ORDER BY fkc.[constraint_column_id]) AS [TargetColumns],
                fk.[delete_referential_action_desc] AS [OnDelete]
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fkc.[constraint_object_id] = fk.[object_id]
            JOIN sys.columns pc ON pc.[object_id] = fkc.[parent_object_id] AND pc.[column_id] = fkc.[parent_column_id]
            JOIN sys.columns rc ON rc.[object_id] = fkc.[referenced_object_id] AND rc.[column_id] = fkc.[referenced_column_id]
            WHERE fk.[parent_object_id] = OBJECT_ID(@table)
            GROUP BY fk.[name], fk.[referenced_object_id], fk.[delete_referential_action_desc]
            ORDER BY fk.[name]
            """,
            ("@table", table));
        return rows.Select(row => $"{row["Name"]} {row["Columns"]} -> {row["Target"]}.{row["TargetColumns"]} {row["OnDelete"]}").ToList();
    }

    /// <summary>How many rows of the catalog view <paramref name="view"/> (check or default constraints) belong to <paramref name="table"/>.</summary>
    private async Task<int> CountAsync(string view, string table) =>
        (int)(await Sql.ScalarAsync(database.ConnectionString,
            $"SELECT COUNT(*) FROM {view} WHERE [parent_object_id] = OBJECT_ID(@table)", ("@table", table)))!;
}
