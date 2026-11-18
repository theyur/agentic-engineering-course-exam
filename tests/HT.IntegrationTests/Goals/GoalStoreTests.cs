using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.IntegrationTests.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// The goal store on SQL Server (research R-08, R-14): round trips, owner scoping, creation order, version 1 and
/// attempt 1, and the outcomes that write nothing. Shared database: own prefixes ("it-go-", "It-Go"), checks by
/// inclusion, exclusion and relative order.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalStoreTests(DatabaseFixture database)
{
    private static readonly DateOnly Start = new(2026, 11, 18);

    private async Task<T> WithStoreAsync<T>(Func<IGoalStore, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<IGoalStore>());
    }

    private async Task<AccountId> OwnerAsync(string email) =>
        (await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, email)).AccountId!.Value;

    private Task<GoalWriteOutcome> CreateAsync(AccountId owner, NewGoal goal, NamedTimeZone expected, NamedTimeZone? defaultZone = null) =>
        WithStoreAsync(s => s.CreateAsync(owner, goal, expected, defaultZone ?? NamedTimeZone.Utc, CancellationToken.None));

    private async Task<GoalId> CreatedAsync(AccountId owner, NewGoal goal, NamedTimeZone expected, NamedTimeZone? defaultZone = null)
    {
        var outcome = await CreateAsync(owner, goal, expected, defaultZone);
        Assert.Equal(GoalWriteOutcomeKind.Created, outcome.Kind);
        return outcome.Id!.Value;
    }

    private static NewGoal OnCatalog(HabitId habit, GoalTarget? target = null, DateOnly? start = null, GoalName? name = null) =>
        new(new GoalHabit(habit, HabitKind.Catalog), start ?? Start, name, target ?? YesNoOn(DayOfWeek.Monday));

    private static NewGoal OnCustom(HabitId habit, GoalTarget? target = null, DateOnly? start = null, GoalName? name = null) =>
        new(new GoalHabit(habit, HabitKind.Custom), start ?? Start, name, target ?? YesNoOn(DayOfWeek.Monday));

    private static readonly (string Zone, string Kind, GoalTargetFields Fields)[] RoundTripCases =
    [
        ("UTC", "catalog", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday } }),
        ("Europe/Kyiv", "custom", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 20, CountUnit = "pages", ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week }),
        ("Europe/Kiev", "catalog", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes, ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday } }),
        ("UTC", "custom", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 2, DurationUnit = DurationUnit.Hours, ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day }),
        ("Europe/Kyiv", "catalog", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 3, CountUnit = "Cups of Coffee", ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day }),
        ("Europe/Kiev", "custom", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet() }),
        ("UTC", "catalog", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Duration, Amount = 90, DurationUnit = DurationUnit.Minutes, ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Week }),
        ("Europe/Kyiv", "custom", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Day }),
        ("UTC", "catalog", new GoalTargetFields { BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 999, CountUnit = "push-ups", ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday } }),
    ];

    public static TheoryData<int> RoundTrips => new(Enumerable.Range(0, RoundTripCases.Length));

    [Theory]
    [MemberData(nameof(RoundTrips))]
    public async Task Every_field_round_trips_through_create_find_and_list(int index)
    {
        var (zoneId, kind, fields) = RoundTripCases[index];
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var owner = await OwnerAsync($"it-go-round-{suffix}@example.com");
        var habit = kind == "catalog"
            ? await CatalogHabitAsync(database.Services, $"It-Go Round {suffix}", category: Category.Health)
            : await CustomHabitAsync(database.Services, owner, $"It-Go Round {suffix}", Category.Money);
        var habitKind = kind == "catalog" ? HabitKind.Catalog : HabitKind.Custom;
        var zone = Zone(zoneId);
        var target = Target(fields);
        var start = new DateOnly(2026, 11, 23);
        var name = Name($"  It-Go Own name {suffix}  ");
        await SetZoneAsync(database.ConnectionString, owner, zoneId);

        var unnamed = await CreatedAsync(owner, new NewGoal(new GoalHabit(habit, habitKind), start, null, target), zone);
        var named = await CreatedAsync(owner, new NewGoal(new GoalHabit(habit, habitKind), start, name, target), zone);

        var found = await WithStoreAsync(s => s.FindAsync(owner, unnamed, CancellationToken.None));
        Assert.NotNull(found);
        AssertStored(found, unnamed, null);
        var list = await WithStoreAsync(s => s.ListAsync(owner, CancellationToken.None));
        Assert.Equal(new[] { unnamed, named }, list.Select(g => g.Goal.Id));
        AssertStored(list[0], unnamed, null);
        AssertStored(list[1], named, $"It-Go Own name {suffix}");

        void AssertStored(StoredGoal stored, GoalId id, string? ownName)
        {
            var goal = stored.Goal;
            Assert.Equal((id, owner, new GoalHabit(habit, habitKind), start), (goal.Id, goal.Owner, goal.Habit, goal.StartDate));
            Assert.Equal(ownName, goal.OwnName?.Value);
            Assert.Equal((GoalAttempt.First, start), (goal.Attempt.Number, goal.Attempt.StartDate));
            var version = Assert.Single(goal.Versions);
            Assert.Equal(start, version.AppliesFrom);
            Assert.Equal(zoneId, version.Settings.TimeZone.Id);
            Assert.Equal(target, version.Settings.Target);
            Assert.Equal(fields.Days ?? new HashSet<DayOfWeek>(), version.Settings.Target.Schedule.Days.ToHashSet());
            Assert.Equal(target.CountUnit, version.Settings.Target.CountUnit);
            Assert.Equal(
                new GoalHabitView(habit, habitKind, $"It-Go Round {suffix}", kind == "catalog" ? Category.Health : Category.Money, kind == "catalog" ? (bool?)true : null),
                stored.Habit);
        }
    }

    [Fact]
    public async Task Reads_give_each_habits_current_name_category_and_state()
    {
        var owner = await OwnerAsync("it-go-current@example.com");
        var catalog = await CatalogHabitAsync(database.Services, "It-Go Current catalog");
        var goal = await CreatedAsync(owner, OnCatalog(catalog), NamedTimeZone.Utc);
        await Sql.ExecuteAsync(database.ConnectionString,
            "UPDATE [CatalogHabits] SET [Name] = N'It-Go Current renamed', [Category] = 'Home', [State] = 'Unpublished' WHERE [Id] = @id",
            ("@id", catalog.Value));

        var found = (await WithStoreAsync(s => s.FindAsync(owner, goal, CancellationToken.None)))!;
        Assert.Equal(new GoalHabitView(catalog, HabitKind.Catalog, "It-Go Current renamed", Category.Home, false), found.Habit);
        Assert.Equal(found.Habit, Assert.Single(await WithStoreAsync(s => s.ListAsync(owner, CancellationToken.None))).Habit);
    }

    [Fact]
    public async Task Another_owner_finds_no_goal_and_lists_without_it()
    {
        var ownerA = await OwnerAsync("it-go-scope-a@example.com");
        var ownerB = await OwnerAsync("it-go-scope-b@example.com");
        var catalog = await CatalogHabitAsync(database.Services, "It-Go Scope catalog");
        var custom = await CustomHabitAsync(database.Services, ownerA, "It-Go Scope custom");
        var onCatalog = await CreatedAsync(ownerA, OnCatalog(catalog), NamedTimeZone.Utc);
        var onCustom = await CreatedAsync(ownerA, OnCustom(custom), NamedTimeZone.Utc);
        var ownOfB = await CreatedAsync(ownerB, OnCatalog(catalog), NamedTimeZone.Utc);

        Assert.Null(await WithStoreAsync(s => s.FindAsync(ownerB, onCatalog, CancellationToken.None)));
        Assert.Null(await WithStoreAsync(s => s.FindAsync(ownerB, onCustom, CancellationToken.None)));
        Assert.Null(await WithStoreAsync(s => s.FindAsync(ownerA, new GoalId(Guid.NewGuid()), CancellationToken.None)));
        var listOfB = await WithStoreAsync(s => s.ListAsync(ownerB, CancellationToken.None));
        Assert.Equal(new[] { ownOfB }, listOfB.Select(g => g.Goal.Id));
        var listOfA = (await WithStoreAsync(s => s.ListAsync(ownerA, CancellationToken.None))).Select(g => g.Goal.Id).ToList();
        Assert.Equal(new[] { onCatalog, onCustom }, listOfA);
        Assert.NotNull(await WithStoreAsync(s => s.FindAsync(ownerA, onCustom, CancellationToken.None)));
    }

    [Fact]
    public async Task Creation_order_follows_insertion_for_equal_names_and_equal_start_dates()
    {
        var owner = await OwnerAsync("it-go-order@example.com");
        var catalog = await CatalogHabitAsync(database.Services, "It-Go Order catalog");
        var custom = await CustomHabitAsync(database.Services, owner, "It-Go Order custom");
        var name = Name("It-Go Same name");

        var created = new List<GoalId>();
        for (var i = 0; i < 6; i++)
        {
            var goal = i % 2 == 0 ? OnCustom(custom, name: name) : OnCatalog(catalog, name: name);
            created.Add(await CreatedAsync(owner, goal, NamedTimeZone.Utc));
        }

        var listed = (await WithStoreAsync(s => s.ListAsync(owner, CancellationToken.None))).Select(g => g.Goal.Id).ToList();
        Assert.Equal(created, listed);
    }

    [Fact]
    public async Task Version_1_keeps_the_expected_zone_from_the_start_date_and_attempt_1_starts_on_it()
    {
        var owner = await OwnerAsync("it-go-version@example.com");
        await SetZoneAsync(database.ConnectionString, owner, null);
        var catalog = await CatalogHabitAsync(database.Services, "It-Go Version catalog");
        var start = new DateOnly(2027, 3, 1);

        var id = await CreatedAsync(owner, OnCatalog(catalog, start: start), Zone("Europe/Kyiv"), Zone("Europe/Kyiv"));

        var version = Assert.Single(await Sql.QueryAsync(database.ConnectionString,
            "SELECT [AppliesFrom], [TimeZone] FROM [GoalSettingsVersions] WHERE [GoalId] = @id", ("@id", id.Value)));
        Assert.Equal((new DateTime(2027, 3, 1), (object?)"Europe/Kyiv"), ((DateTime)version["AppliesFrom"]!, version["TimeZone"]));
        var attempt = Assert.Single(await Sql.QueryAsync(database.ConnectionString,
            "SELECT [Number], [StartDate] FROM [GoalAttempts] WHERE [GoalId] = @id", ("@id", id.Value)));
        Assert.Equal((1, new DateTime(2027, 3, 1)), ((int)attempt["Number"]!, (DateTime)attempt["StartDate"]!));
        var goal = Assert.Single(await Sql.QueryAsync(database.ConnectionString,
            "SELECT [StartDate], [CatalogHabitId], [CustomHabitId], [Name] FROM [Goals] WHERE [Id] = @id", ("@id", id.Value)));
        Assert.Equal((new DateTime(2027, 3, 1), (object?)catalog.Value, (object?)null, (object?)null),
            ((DateTime)goal["StartDate"]!, goal["CatalogHabitId"], goal["CustomHabitId"], goal["Name"]));
    }

    [Fact]
    public async Task A_changed_zone_writes_nothing()
    {
        var owner = await OwnerAsync("it-go-zone-changed@example.com");
        var catalog = await CatalogHabitAsync(database.Services, "It-Go Zone changed catalog");

        // The stored zone UTC is not the expected Kyiv.
        Assert.Same(GoalWriteOutcome.TimeZoneChanged, await CreateAsync(owner, OnCatalog(catalog), Zone("Europe/Kyiv")));
        Assert.Equal(((string?)"UTC", 0), await AccountStateAsync(database.ConnectionString, owner));

        // A link is not its zone: the comparison is ordinal on the identifier.
        await SetZoneAsync(database.ConnectionString, owner, "Europe/Kiev");
        Assert.Same(GoalWriteOutcome.TimeZoneChanged, await CreateAsync(owner, OnCatalog(catalog), Zone("Europe/Kyiv")));
        Assert.Equal(((string?)"Europe/Kiev", 0), await AccountStateAsync(database.ConnectionString, owner));

        // No stored zone reads as the default, which is not the expected zone.
        await SetZoneAsync(database.ConnectionString, owner, null);
        Assert.Same(GoalWriteOutcome.TimeZoneChanged, await CreateAsync(owner, OnCatalog(catalog), Zone("Europe/Kyiv"), NamedTimeZone.Utc));
        Assert.Equal(((string?)null, 0), await AccountStateAsync(database.ConnectionString, owner));

        Assert.Equal(0, await GoalCountAsync(database.ConnectionString, owner));
        Assert.Empty(await WithStoreAsync(s => s.ListAsync(owner, CancellationToken.None)));
    }

    [Fact]
    public async Task A_missing_account_gives_a_changed_zone_and_writes_nothing()
    {
        var catalog = await CatalogHabitAsync(database.Services, "It-Go No account catalog");
        var nobody = new AccountId(Guid.NewGuid());

        Assert.Same(GoalWriteOutcome.TimeZoneChanged, await CreateAsync(nobody, OnCatalog(catalog), NamedTimeZone.Utc));
        Assert.Equal(0, await GoalCountAsync(database.ConnectionString, nobody));
    }

    [Fact]
    public async Task A_missing_zone_becomes_the_expected_zone_with_the_first_goal_and_every_goal_increments_the_revision()
    {
        var owner = await OwnerAsync("it-go-revision@example.com");
        await SetZoneAsync(database.ConnectionString, owner, null);
        var catalog = await CatalogHabitAsync(database.Services, "It-Go Revision catalog");
        var custom = await CustomHabitAsync(database.Services, owner, "It-Go Revision custom");
        Assert.Equal(((string?)null, 0), await AccountStateAsync(database.ConnectionString, owner));

        await CreatedAsync(owner, OnCatalog(catalog), Zone("Europe/Kyiv"), Zone("Europe/Kyiv"));
        Assert.Equal(((string?)"Europe/Kyiv", 1), await AccountStateAsync(database.ConnectionString, owner));

        // From now on the stored zone counts, not the default.
        Assert.Same(GoalWriteOutcome.TimeZoneChanged, await CreateAsync(owner, OnCustom(custom), NamedTimeZone.Utc, NamedTimeZone.Utc));
        await CreatedAsync(owner, OnCustom(custom), Zone("Europe/Kyiv"), NamedTimeZone.Utc);
        Assert.Equal(((string?)"Europe/Kyiv", 2), await AccountStateAsync(database.ConnectionString, owner));
        await CreatedAsync(owner, OnCatalog(catalog), Zone("Europe/Kyiv"), NamedTimeZone.Utc);
        Assert.Equal(((string?)"Europe/Kyiv", 3), await AccountStateAsync(database.ConnectionString, owner));
        Assert.Equal(3, await GoalCountAsync(database.ConnectionString, owner));
    }

    [Fact]
    public async Task A_habit_that_cannot_take_a_goal_gives_habit_gone_and_writes_nothing()
    {
        var owner = await OwnerAsync("it-go-gone@example.com");
        var other = await OwnerAsync("it-go-gone-other@example.com");
        await SetZoneAsync(database.ConnectionString, owner, null);
        var draft = await CatalogHabitAsync(database.Services, "It-Go Gone draft", CatalogHabitState.Draft);
        var unpublished = await CatalogHabitAsync(database.Services, "It-Go Gone unpublished", CatalogHabitState.Unpublished);
        var published = await CatalogHabitAsync(database.Services, "It-Go Gone published");
        var othersCustom = await CustomHabitAsync(database.Services, other, "It-Go Gone others");
        var ownCustom = await CustomHabitAsync(database.Services, owner, "It-Go Gone own");

        NewGoal[] refused =
        [
            OnCatalog(draft),
            OnCatalog(unpublished),
            OnCatalog(new HabitId(Guid.NewGuid())),
            OnCustom(new HabitId(Guid.NewGuid())),
            OnCustom(othersCustom),
            OnCustom(published),
            OnCatalog(ownCustom),
        ];
        foreach (var goal in refused)
        {
            Assert.Same(GoalWriteOutcome.HabitGone, await CreateAsync(owner, goal, Zone("Europe/Kyiv"), Zone("Europe/Kyiv")));
        }

        Assert.Equal(((string?)null, 0), await AccountStateAsync(database.ConnectionString, owner));
        Assert.Equal(0, await GoalCountAsync(database.ConnectionString, owner));
        Assert.Equal(0, await GoalCountAsync(database.ConnectionString, other));

        await CreatedAsync(owner, OnCatalog(published), Zone("Europe/Kyiv"), Zone("Europe/Kyiv"));
        await CreatedAsync(owner, OnCustom(ownCustom), Zone("Europe/Kyiv"), Zone("Europe/Kyiv"));
        Assert.Equal(((string?)"Europe/Kyiv", 2), await AccountStateAsync(database.ConnectionString, owner));
    }

    [Fact]
    public async Task Deleting_an_account_removes_its_goals()
    {
        var owner = await OwnerAsync("it-go-cascade@example.com");
        var custom = await CustomHabitAsync(database.Services, owner, "It-Go Cascade custom");
        var catalog = await CatalogHabitAsync(database.Services, "It-Go Cascade catalog");
        var onCustom = await CreatedAsync(owner, OnCustom(custom), NamedTimeZone.Utc);
        await CreatedAsync(owner, OnCatalog(catalog), NamedTimeZone.Utc);

        await Sql.DeleteUserAsync(database.ConnectionString, "it-go-cascade@example.com");

        Assert.Equal(0, await GoalCountAsync(database.ConnectionString, owner));
        Assert.Equal(0, await Sql.ScalarAsync(database.ConnectionString,
            "SELECT COUNT(*) FROM [GoalSettingsVersions] WHERE [GoalId] = @id", ("@id", onCustom.Value)));
    }
}
