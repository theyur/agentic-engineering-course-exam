using System.Globalization;
using HT.Application;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// The goal-change members of the goal store and the record store on SQL Server (slice 8, research R-05, R-12):
/// <see cref="IGoalStore.ReadOwnerGoalAsync"/>, the three conditional writes with their re-checks and their outcomes
/// that write nothing, the ending's cuts in one transaction, versions of two attempts from the same date, deleting a
/// goal with two attempts, the bounded <see cref="IRecordStore.ReadRecentNoteKeysAsync(AccountId, GoalId, DateOnly, DateOnly?, int, CancellationToken)"/>,
/// the pause refusal after an ending, the catalog counts and goal list, and a time zone change on the latest attempt.
/// The stores read no time; the test still composes its own provider with its own clock and never touches the
/// fixture's. Shared database: own prefixes ("it-gc-", "It-Gc"), checks by inclusion, exclusion and relative order.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalChangeStoreTests(DatabaseFixture database)
{
    private static readonly CancellationToken None = CancellationToken.None;

    /// <summary>Monday 16 November 2026.</summary>
    private static readonly DateOnly Start = new(2026, 11, 16);

    private static GoalTarget YesNoDaily() => YesNoOn(Enum.GetValues<DayOfWeek>());

    private static GoalTarget Minutes(int amount) => Target(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = amount, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    });

    private static GoalSettingsVersion Version(GoalTarget target, DateOnly from, string zone = "UTC") => new(new GoalSettings(target, Zone(zone)), from);

    [Fact]
    public async Task ReadOwnerGoalAsync_reads_the_zone_and_revision_then_the_goal_and_nothing_of_another_owner()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"read-{suffix}");
        var other = await OwnerAsync($"read-other-{suffix}");
        var goal = await GoalAsync(services, owner, $"Read {suffix}", YesNoDaily());

        var read = await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None));
        Assert.NotNull(read);
        Assert.Equal(("UTC", (await AccountStateAsync(database.ConnectionString, owner)).Revision), (read.StoredTimeZone, read.GoalsRevision));
        Assert.Equal(goal, read.Goal?.Goal.Id);

        var foreign = await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(other, goal, None));
        Assert.NotNull(foreign);
        Assert.Null(foreign.Goal);
        Assert.Null(await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(new AccountId(Guid.NewGuid()), goal, None)));
    }

    [Fact]
    public async Task TryChangeSettingsAsync_writes_the_name_and_the_versions_and_increments_the_revision()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"settings-{suffix}");
        var goal = await GoalAsync(services, owner, $"Settings {suffix}", Minutes(15));
        var read = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;

        var versions = new[] { Version(Minutes(15), Start), Version(Minutes(20), new DateOnly(2026, 11, 19)) };
        var write = new GoalSettingsWrite(goal, GoalAttempt.First, Name($"It-Gc Own {suffix}"), versions);
        Assert.Equal(GoalChangeWriteOutcome.Written,
            await WithGoalsAsync(services, s => s.TryChangeSettingsAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, None)));

        var after = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;
        Assert.Equal(read.GoalsRevision + 1, after.GoalsRevision);
        Assert.Equal($"It-Gc Own {suffix}", after.Goal!.Goal.OwnName?.Value);
        Assert.Equal(versions, after.Goal.Goal.Versions);

        // A name-only change keeps the versions; null removes the own name.
        var nameOnly = new GoalSettingsWrite(goal, GoalAttempt.First, null, null);
        Assert.Equal(GoalChangeWriteOutcome.Written,
            await WithGoalsAsync(services, s => s.TryChangeSettingsAsync(owner, after.StoredTimeZone, after.GoalsRevision, nameOnly, None)));
        var renamed = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;
        Assert.Null(renamed.Goal!.Goal.OwnName);
        Assert.Equal(versions, renamed.Goal.Goal.Versions);
        Assert.Equal(after.GoalsRevision + 1, renamed.GoalsRevision);
    }

    [Fact]
    public async Task TryEndAsync_writes_the_ending_and_cuts_pauses_and_excusals_after_the_last_date_in_one_write()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"end-{suffix}");
        var goal = await GoalAsync(services, owner, $"End {suffix}", YesNoDaily());

        // Ending on Friday 20 November gives the last date Thursday 19 November.
        await WithRecordsAsync(services, async records =>
        {
            Assert.Equal(ExemptionStoreWriteOutcome.Written, (await records.ExcuseAsync(owner, goal, D(17), null, None)).Outcome);
            Assert.Equal(ExemptionStoreWriteOutcome.Written, (await records.ExcuseAsync(owner, goal, D(23), null, None)).Outcome);
            Assert.Equal(ExemptionStoreWriteOutcome.Written, (await records.AddPauseAsync(owner, goal, D(19), D(21), null, None)).Outcome);
            Assert.Equal(ExemptionStoreWriteOutcome.Written, (await records.AddPauseAsync(owner, goal, D(25), D(27), null, None)).Outcome);
            await AddRecordAsync(records, owner, goal, D(18));
            await AddRecordAsync(records, owner, goal, D(22));
            await SaveNoteAsync(records, owner, goal, D(22), "Kept after the last date.");
        });

        var read = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;
        var effect = GoalTimeline.EndingOf(read.Goal!.Goal, D(20));
        Assert.Equal(D(19), effect.LastDate);
        var ending = new GoalEnding(D(20), Outcome.Abandoned, Note("Too busy."));
        var write = new GoalEndingWrite(goal, GoalAttempt.First, ending, effect.LastDate, effect.KeptVersions);
        Assert.Equal(GoalChangeWriteOutcome.Written,
            await WithGoalsAsync(services, s => s.TryEndAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, None)));

        var after = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;
        Assert.Equal(read.GoalsRevision + 1, after.GoalsRevision);
        Assert.True(after.Goal!.Goal.IsEnded);
        Assert.Equal(ending, after.Goal.Goal.Attempt.Ending);
        Assert.Equal("Too busy.", after.Goal.Goal.Attempt.Ending!.Note!.Value.Value);
        Assert.Equal(effect.KeptVersions, after.Goal.Goal.Versions);

        await WithRecordsAsync(services, async records =>
        {
            var exemptions = await records.ReadExemptionsAsync(owner, goal, None);
            Assert.Equal([D(17)], exemptions.Excusals.Select(e => e.Date));
            Assert.Equal([(D(19), D(19))], exemptions.Pauses.Select(p => (p.FirstDay, p.LastDay)));
            var stored = await records.ReadAsync(owner, goal, D(16), D(30), None);
            Assert.Equal([D(18), D(22)], stored.Records.Select(r => r.Date));
            Assert.Equal([D(22)], stored.Notes.Keys);
        });
    }

    [Fact]
    public async Task TryStartAttemptAsync_adds_the_attempt_with_its_first_version_and_two_attempts_can_have_versions_from_the_same_date()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"start-{suffix}");

        // Attempt 1 from Monday 30 November, ended on Wednesday 25 November before it began: it keeps its first version.
        var goal = await GoalAsync(services, owner, $"Start {suffix}", Minutes(5), start: D(30));
        await EndAsync(services, owner, goal, D(25));

        var read = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;
        var first = Version(Minutes(10), D(30), "Europe/Kyiv");
        var write = new GoalAttemptWrite(goal, 2, first);
        Assert.Equal(GoalChangeWriteOutcome.Written,
            await WithGoalsAsync(services, s => s.TryStartAttemptAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, None)));

        var after = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;
        Assert.Equal(read.GoalsRevision + 1, after.GoalsRevision);
        var stored = after.Goal!.Goal;
        Assert.Equal([1, 2], stored.Attempts.Select(a => a.Attempt.Number));
        Assert.Equal(new GoalAttempt(2, D(30)), stored.Attempt);
        Assert.False(stored.IsEnded);
        Assert.Equal([first], stored.Versions);
        Assert.Equal([Version(Minutes(5), D(30))], stored.VersionsOf(1));
        Assert.Equal(D(30), stored.StartDate);
    }

    [Fact]
    public async Task A_failed_re_check_gives_Stale_and_writes_nothing()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"stale-{suffix}");
        var running = await GoalAsync(services, owner, $"Stale running {suffix}", YesNoDaily());
        var ended = await GoalAsync(services, owner, $"Stale ended {suffix}", YesNoDaily());
        await EndAsync(services, owner, ended, D(20));

        var read = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, running, None)))!;
        var (zone, revision) = (read.StoredTimeZone, read.GoalsRevision);
        var settings = new GoalSettingsWrite(running, GoalAttempt.First, Name($"It-Gc Stale {suffix}"), [Version(YesNoDaily(), Start)]);
        var ending = new GoalEndingWrite(running, GoalAttempt.First, new GoalEnding(D(20), Outcome.Achieved, null), D(19), [Version(YesNoDaily(), Start)]);
        var attempt = new GoalAttemptWrite(ended, 2, Version(YesNoDaily(), D(23)));
        var before = await DescribeAsync(services, owner, running, ended);

        Func<IGoalStore, Task<GoalChangeWriteOutcome>>[] writes =
        [
            // A stale zone or revision.
            s => s.TryChangeSettingsAsync(owner, "Europe/Kyiv", revision, settings, None),
            s => s.TryChangeSettingsAsync(owner, zone, revision - 1, settings, None),
            s => s.TryEndAsync(owner, null, revision, ending, None),
            s => s.TryEndAsync(owner, zone, revision + 1, ending, None),
            s => s.TryStartAttemptAsync(owner, "Europe/Kyiv", revision, attempt, None),
            s => s.TryStartAttemptAsync(owner, zone, revision - 1, attempt, None),

            // The latest attempt is not the one written.
            s => s.TryChangeSettingsAsync(owner, zone, revision, settings with { Attempt = 2 }, None),
            s => s.TryEndAsync(owner, zone, revision, ending with { Attempt = 2 }, None),

            // Already ended.
            s => s.TryChangeSettingsAsync(owner, zone, revision, settings with { Goal = ended }, None),
            s => s.TryEndAsync(owner, zone, revision, ending with { Goal = ended }, None),

            // Not ended, or the wrong number, for a start.
            s => s.TryStartAttemptAsync(owner, zone, revision, attempt with { Goal = running }, None),
            s => s.TryStartAttemptAsync(owner, zone, revision, attempt with { Number = 3 }, None),
            s => s.TryStartAttemptAsync(owner, zone, revision, attempt with { Number = 1 }, None),
        ];
        foreach (var write in writes)
        {
            Assert.Equal(GoalChangeWriteOutcome.Stale, await WithGoalsAsync(services, write));
        }

        Assert.Equal(before, await DescribeAsync(services, owner, running, ended));
        Assert.Equal(revision, (await AccountStateAsync(database.ConnectionString, owner)).Revision);
    }

    [Fact]
    public async Task Another_owners_goal_gives_GoalNotFound_and_writes_nothing()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"foreign-{suffix}");
        var other = await OwnerAsync($"foreign-other-{suffix}");
        var running = await GoalAsync(services, owner, $"Foreign running {suffix}", YesNoDaily());
        var ended = await GoalAsync(services, owner, $"Foreign ended {suffix}", YesNoDaily());
        await EndAsync(services, owner, ended, D(20));
        var before = await DescribeAsync(services, owner, running, ended);

        // The other owner's own zone and revision pass, so only the goal decides.
        var read = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(other, running, None)))!;
        Assert.Null(read.Goal);
        var (zone, revision) = (read.StoredTimeZone, read.GoalsRevision);
        Func<IGoalStore, Task<GoalChangeWriteOutcome>>[] writes =
        [
            s => s.TryChangeSettingsAsync(other, zone, revision, new GoalSettingsWrite(running, GoalAttempt.First, null, null), None),
            s => s.TryEndAsync(other, zone, revision,
                new GoalEndingWrite(running, GoalAttempt.First, new GoalEnding(D(20), Outcome.Achieved, null), D(19), [Version(YesNoDaily(), Start)]), None),
            s => s.TryStartAttemptAsync(other, zone, revision, new GoalAttemptWrite(ended, 2, Version(YesNoDaily(), D(23))), None),
            s => s.TryChangeSettingsAsync(other, zone, revision, new GoalSettingsWrite(new GoalId(Guid.NewGuid()), GoalAttempt.First, null, null), None),
        ];
        foreach (var write in writes)
        {
            Assert.Equal(GoalChangeWriteOutcome.GoalNotFound, await WithGoalsAsync(services, write));
        }

        Assert.Equal(before, await DescribeAsync(services, owner, running, ended));
        Assert.Equal(revision, (await AccountStateAsync(database.ConnectionString, other)).Revision);
    }

    [Fact]
    public async Task A_goal_with_two_attempts_is_deleted_with_its_goal_and_with_its_account()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var email = $"it-gc-delete-account-{suffix}@example.com";
        var owner = await OwnerAsync($"delete-{suffix}");
        var accountOwner = (await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, email)).AccountId!.Value;
        var goal = await TwoAttemptGoalAsync(services, owner, $"Delete goal {suffix}");
        var onAccount = await TwoAttemptGoalAsync(services, accountOwner, $"Delete account {suffix}");
        Assert.Equal((2, 2), await AttemptAndVersionCountsAsync(goal));

        await Sql.ExecuteAsync(database.ConnectionString, "DELETE FROM [Goals] WHERE [Id] = @id", ("@id", goal.Value));
        Assert.Equal((0, 0), await AttemptAndVersionCountsAsync(goal));

        await Sql.DeleteUserAsync(database.ConnectionString, email);
        Assert.Equal(0, await GoalCountAsync(database.ConnectionString, accountOwner));
        Assert.Equal((0, 0), await AttemptAndVersionCountsAsync(onAccount));
    }

    [Fact]
    public async Task The_bounded_recent_note_keys_leave_out_notes_outside_the_bounds()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"notes-{suffix}");
        var other = await OwnerAsync($"notes-other-{suffix}");

        // Attempt 1 from 16 November ended on 20 November (last date 19 November); attempt 2 from 23 November.
        var goal = await TwoAttemptGoalAsync(services, owner, $"Notes {suffix}");
        await WithRecordsAsync(services, async records =>
        {
            int[] noteDays = [17, 18, 20, 21, 24];
            foreach (var day in noteDays)
            {
                await SaveNoteAsync(records, owner, goal, D(day), $"Note {day}.");
            }
        });

        await WithRecordsAsync(services, async records =>
        {
            Assert.Equal([D(24)], await records.ReadRecentNoteKeysAsync(owner, goal, D(23), null, 10, None));
            Assert.Equal([D(18), D(17)], await records.ReadRecentNoteKeysAsync(owner, goal, D(16), D(19), 10, None));
            Assert.Equal([D(18)], await records.ReadRecentNoteKeysAsync(owner, goal, D(16), D(19), 1, None));
            Assert.Equal([D(24), D(21)], await records.ReadRecentNoteKeysAsync(owner, goal, D(16), null, 2, None));
            Assert.Empty(await records.ReadRecentNoteKeysAsync(owner, goal, D(16), null, 0, None));
            Assert.Empty(await records.ReadRecentNoteKeysAsync(other, goal, D(16), null, 10, None));

            // The old overload is unchanged.
            Assert.Equal([D(24), D(21), D(20)], await records.ReadRecentNoteKeysAsync(owner, goal, 3, None));
        });
    }

    [Fact]
    public async Task AddPauseAsync_after_an_ending_gives_GoalEnded_and_writes_nothing()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"pause-{suffix}");
        var goal = await GoalAsync(services, owner, $"Pause {suffix}", YesNoDaily());
        await EndAsync(services, owner, goal, D(20));

        await WithRecordsAsync(services, async records =>
        {
            Assert.Equal(ExemptionStoreWriteOutcome.GoalEnded, (await records.AddPauseAsync(owner, goal, D(23), D(25), null, None)).Outcome);
            Assert.Empty((await records.ReadExemptionsAsync(owner, goal, None)).Pauses);
        });

        // A new attempt can be paused again.
        await StartAttemptAsync(services, owner, goal, Version(YesNoDaily(), D(23)));
        await WithRecordsAsync(services, async records =>
        {
            Assert.Equal(ExemptionStoreWriteOutcome.Written, (await records.AddPauseAsync(owner, goal, D(23), D(25), null, None)).Outcome);
        });
    }

    [Fact]
    public async Task ExcuseAsync_on_or_after_the_ending_date_gives_GoalEnded_and_writes_nothing_and_the_last_date_still_works()
    {
        // From 16 November, ended on 20 November (last date 19): an excusal of the 20th or later would be one that the
        // ending removes (FR-009), so the store refuses it under the lock; the ended attempt's 19th can still be excused.
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"excuse-{suffix}");
        var goal = await GoalAsync(services, owner, $"Excuse {suffix}", YesNoDaily());
        await EndAsync(services, owner, goal, D(20));

        await WithRecordsAsync(services, async records =>
        {
            Assert.Equal(ExemptionStoreWriteOutcome.GoalEnded, (await records.ExcuseAsync(owner, goal, D(20), null, None)).Outcome);
            Assert.Equal(ExemptionStoreWriteOutcome.GoalEnded, (await records.ExcuseAsync(owner, goal, D(21), null, None)).Outcome);
            Assert.Equal(ExemptionStoreWriteOutcome.Written, (await records.ExcuseAsync(owner, goal, D(19), null, None)).Outcome);
            Assert.Equal([D(19)], (await records.ReadExemptionsAsync(owner, goal, None)).Excusals.Select(e => e.Date));
        });
    }

    [Fact]
    public async Task The_catalog_counts_and_goal_list_leave_out_an_ended_goal_and_the_used_by_goals_counts_keep_it()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"catalog-{suffix}");
        var catalog = await CatalogHabitAsync(database.Services, $"It-Gc Catalog {suffix}");
        var custom = await CustomHabitAsync(database.Services, owner, $"It-Gc Custom {suffix}");
        var running = await GoalOnAsync(services, owner, new GoalHabit(catalog, HabitKind.Catalog), YesNoDaily());
        var ended = await GoalOnAsync(services, owner, new GoalHabit(catalog, HabitKind.Catalog), YesNoDaily());
        var restarted = await GoalOnAsync(services, owner, new GoalHabit(catalog, HabitKind.Catalog), YesNoDaily());
        var onCustom = await GoalOnAsync(services, owner, new GoalHabit(custom, HabitKind.Custom), YesNoDaily());
        await EndAsync(services, owner, ended, D(20));
        await EndAsync(services, owner, restarted, D(20));
        await StartAttemptAsync(services, owner, restarted, Version(YesNoDaily(), D(23)));
        await EndAsync(services, owner, onCustom, D(20));

        var counts = await WithGoalsAsync(services, s => s.CountByCatalogHabitAsync(owner, None));
        Assert.Equal(2, counts[catalog]);
        var listed = await WithGoalsAsync(services, s => s.ListOnCatalogHabitAsync(owner, catalog, None));
        Assert.Equal([running, restarted], listed.Select(g => g.Goal.Id));

        Assert.Equal(3, await WithGoalsAsync(services, s => s.CountUsingCatalogHabitAsync(catalog, None)));
        Assert.Equal(1, await WithGoalsAsync(services, s => s.CountUsingCustomHabitAsync(owner, custom, None)));
        Assert.Equal([running, ended, restarted, onCustom], (await WithGoalsAsync(services, s => s.ListAsync(owner, None))).Select(g => g.Goal.Id));
    }

    [Fact]
    public async Task TryChangeTimeZoneAsync_writes_only_the_latest_attempts_versions()
    {
        await using var services = BuildServices();
        var suffix = Suffix();
        var owner = await OwnerAsync($"zone-{suffix}");
        var goal = await TwoAttemptGoalAsync(services, owner, $"Zone {suffix}");

        var read = (await WithGoalsAsync(services, s => s.ReadOwnerGoalsAsync(owner, None)))!;
        var kyiv = Zone("Europe/Kyiv");
        GoalVersionWrite[] writes = [new(goal, GoalVersionChange.Add(D(30), kyiv))];
        Assert.True(await WithGoalsAsync(services, s => s.TryChangeTimeZoneAsync(owner, read.StoredTimeZone, read.GoalsRevision, kyiv, writes, None)));

        var stored = (await WithGoalsAsync(services, s => s.FindAsync(owner, goal, None)))!.Goal;
        Assert.Equal([Version(YesNoDaily(), Start)], stored.VersionsOf(1));
        Assert.Equal([Version(YesNoDaily(), D(23)), Version(YesNoDaily(), D(30), "Europe/Kyiv")], stored.VersionsOf(2));
    }

    private static DateOnly D(int novemberDay) => new(2026, 11, novemberDay);

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    private static EndingNote Note(string text)
    {
        Assert.True(EndingNote.TryCreate(text, out var note, out _));
        return note!.Value;
    }

    private ServiceProvider BuildServices()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 11, 22, 10, 0, 0, TimeSpan.Zero)));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private static async Task<T> WithGoalsAsync<T>(ServiceProvider services, Func<IGoalStore, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<IGoalStore>());
    }

    private static async Task WithRecordsAsync(ServiceProvider services, Func<IRecordStore, Task> call)
    {
        await using var scope = services.CreateAsyncScope();
        await call(scope.ServiceProvider.GetRequiredService<IRecordStore>());
    }

    /// <summary>A registered user <c>it-gc-{name}@example.com</c> whose stored zone is UTC.</summary>
    private async Task<AccountId> OwnerAsync(string name)
    {
        var user = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-gc-{name}@example.com");
        var owner = user.AccountId!.Value;
        await SetZoneAsync(database.ConnectionString, owner, "UTC");
        return owner;
    }

    /// <summary>A goal on a new Published catalog habit <c>It-Gc {name}</c>, from <paramref name="start"/> (default Monday 16 November) in UTC.</summary>
    private async Task<GoalId> GoalAsync(ServiceProvider services, AccountId owner, string name, GoalTarget target, DateOnly? start = null)
    {
        var habit = await CatalogHabitAsync(database.Services, $"It-Gc {name}");
        return await GoalOnAsync(services, owner, new GoalHabit(habit, HabitKind.Catalog), target, start);
    }

    private static async Task<GoalId> GoalOnAsync(ServiceProvider services, AccountId owner, GoalHabit habit, GoalTarget target, DateOnly? start = null)
    {
        var outcome = await WithGoalsAsync(services, s => s.CreateAsync(
            owner, new NewGoal(habit, start ?? Start, null, target), NamedTimeZone.Utc, NamedTimeZone.Utc, None));
        Assert.Equal(GoalWriteOutcomeKind.Created, outcome.Kind);
        return outcome.Id!.Value;
    }

    /// <summary>Ends the goal's latest attempt on <paramref name="endingDate"/> as the Domain decides it, with the outcome Abandoned.</summary>
    private static async Task EndAsync(ServiceProvider services, AccountId owner, GoalId goal, DateOnly endingDate)
    {
        var read = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;
        var stored = read.Goal!.Goal;
        var effect = GoalTimeline.EndingOf(stored, endingDate);
        var write = new GoalEndingWrite(goal, stored.Attempt.Number, new GoalEnding(endingDate, Outcome.Abandoned, null), effect.LastDate, effect.KeptVersions);
        Assert.Equal(GoalChangeWriteOutcome.Written,
            await WithGoalsAsync(services, s => s.TryEndAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, None)));
    }

    private static async Task StartAttemptAsync(ServiceProvider services, AccountId owner, GoalId goal, GoalSettingsVersion first)
    {
        var read = (await WithGoalsAsync(services, s => s.ReadOwnerGoalAsync(owner, goal, None)))!;
        var write = new GoalAttemptWrite(goal, read.Goal!.Goal.Attempt.Number + 1, first);
        Assert.Equal(GoalChangeWriteOutcome.Written,
            await WithGoalsAsync(services, s => s.TryStartAttemptAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, None)));
    }

    /// <summary>Daily yes/no from Monday 16 November, ended on Friday 20 November, and attempt 2 from Monday 23 November.</summary>
    private async Task<GoalId> TwoAttemptGoalAsync(ServiceProvider services, AccountId owner, string name)
    {
        var goal = await GoalAsync(services, owner, name, YesNoDaily());
        await EndAsync(services, owner, goal, D(20));
        await StartAttemptAsync(services, owner, goal, Version(YesNoDaily(), D(23)));
        return goal;
    }

    private static async Task AddRecordAsync(IRecordStore records, AccountId owner, GoalId goal, DateOnly date)
    {
        var written = await records.AddAsync(
            owner, goal, new EntrySpan(date, date, date), true, date, RecordValue.FromAnswer(YesNoAnswer.Yes), NoteWrite.Keep, 1, None);
        Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
    }

    private static async Task SaveNoteAsync(IRecordStore records, AccountId owner, GoalId goal, DateOnly key, string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        var written = await records.SaveNoteAsync(owner, goal, new EntrySpan(key, key, key), note, None);
        Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
    }

    /// <summary>The goals as read, each attempt with its ending and versions, as text for an exact comparison.</summary>
    private static async Task<IReadOnlyList<string>> DescribeAsync(ServiceProvider services, AccountId owner, params GoalId[] goals)
    {
        var lines = new List<string>();
        foreach (var id in goals)
        {
            var goal = (await WithGoalsAsync(services, s => s.FindAsync(owner, id, None)))!.Goal;
            lines.Add($"{goal.Id} {goal.OwnName?.Value}");
            foreach (var attempt in goal.Attempts)
            {
                var ending = attempt.Attempt.Ending;
                lines.Add(string.Create(CultureInfo.InvariantCulture,
                    $"  {attempt.Attempt.Number} {attempt.Attempt.StartDate:O} {ending?.Date:O} {ending?.Outcome} {ending?.Note?.Value}"));
                lines.AddRange(attempt.Versions.Select(v => string.Create(CultureInfo.InvariantCulture,
                    $"    {v.AppliesFrom:O} {v.Settings.TimeZone.Id} {GoalText.TargetText(v.Settings.Target)}")));
            }
        }

        return lines;
    }

    private async Task<(int Attempts, int Versions)> AttemptAndVersionCountsAsync(GoalId goal) =>
    (
        (int)(await Sql.ScalarAsync(database.ConnectionString, "SELECT COUNT(*) FROM [GoalAttempts] WHERE [GoalId] = @id", ("@id", goal.Value)))!,
        (int)(await Sql.ScalarAsync(database.ConnectionString, "SELECT COUNT(*) FROM [GoalSettingsVersions] WHERE [GoalId] = @id", ("@id", goal.Value)))!);
}
