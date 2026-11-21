using HT.Application;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.IntegrationTests.Accounts;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// Privacy of excusals and pauses on SQL Server (US5-AS1 to AS4, FR-021, FR-022, SC-005, constitution III; T081): over the
/// real stores, user B, a stored administrator and an anonymous visitor call the six use cases of
/// <see cref="ExcusalService"/> and <see cref="PauseService"/> on user A's goal, with arguments that succeed for A, and B
/// also uses A's <see cref="PauseId"/>s through B's own goal; and the six exemption members of <see cref="IRecordStore"/>
/// are called with B's account and the administrator's as the owner. None of them changes a row of A's or B's (read with
/// plain SQL before and after) or returns anything of A's. The test builds its own provider with its own
/// <see cref="FakeTimeProvider"/> over the fixture's connection string and moves only that clock. The goals "Reading,
/// 20 min every day" are in UTC and start on Monday 9 November 2026, created at 08:00Z that day; A records 25 min with a
/// note on Friday 13 at 12:00Z; at 10:00Z on Wednesday 18 (the pause date) A excuses Tuesday 17 and pauses Wednesday 18
/// to Thursday 19 and Monday 23 to Friday 27, each with a reason, and B excuses B's Tuesday 17 and pauses B's goal from
/// Monday 23 to Friday 27. At that time Monday 16 is Pending with no record (it can be excused), Tuesday 17's excusal is
/// inside its grace period (it can be removed), Saturday 28 to Sunday 29 is free (it can be paused), A's goal is paused
/// today (it can be resumed) and its second pause has not begun (it can be cancelled). Shared database: own prefixes
/// ("it-exp-", "It-Exp"); rows are read per owner only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ExemptionPrivacyTests(DatabaseFixture database)
{
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string NotSignedIn = "You are not signed in.";
    private const string PauseNotFound = "This pause does not exist.";
    private const string NotPaused = "This goal is not paused.";

    private static readonly DateOnly Mon9 = new(2026, 11, 9);
    private static readonly DateOnly Fri13 = new(2026, 11, 13);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Tue17 = new(2026, 11, 17);
    private static readonly DateOnly Wed18 = new(2026, 11, 18);
    private static readonly DateOnly Thu19 = new(2026, 11, 19);
    private static readonly DateOnly Sun22 = new(2026, 11, 22);
    private static readonly DateOnly Mon23 = new(2026, 11, 23);
    private static readonly DateOnly Fri27 = new(2026, 11, 27);
    private static readonly DateOnly Sat28 = new(2026, 11, 28);
    private static readonly DateOnly Sun29 = new(2026, 11, 29);

    /// <summary>08:00Z on Monday 9 November 2026, when the test's own clock starts and the goals are created.</summary>
    private static readonly DateTimeOffset ClockStart = new(2026, 11, 9, 8, 0, 0, TimeSpan.Zero);

    /// <summary>12:00Z on Friday 13 November 2026, when A records Friday 13.</summary>
    private static readonly DateTimeOffset Fri13Noon = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);

    /// <summary>10:00Z on Wednesday 18 November 2026, the time of every other call.</summary>
    private static readonly DateTimeOffset Wed18TenOClock = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static CancellationToken None => CancellationToken.None;

    /// <summary>Build, duration, at least 20 min, every day: commitment-based.</summary>
    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    [Fact]
    public async Task US5_AS1_FR_021_B_calling_every_use_case_on_As_goal_or_with_As_pauses_changes_no_row_and_gets_nothing_of_As()
    {
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var world = await ArrangeAsync(services, clock, "b");
        var before = await SnapshotAsync(world);

        // On A's goal: each use case gives "This goal does not exist." with no view, as for a goal that does not exist.
        foreach (var target in new[] { world.AGoal, new GoalId(Guid.NewGuid()) })
        {
            await AssertEveryUseCaseRefusedAsync(services, world.B, target, world.AUpcoming, "NotFound", GoalNotFound);
        }

        // A's pauses through B's own goal: not among that goal's pauses.
        foreach (var pause in new[] { world.ACurrent, world.AUpcoming })
        {
            var cancelled = await PausesAsync(services, s => s.CancelPauseAsync(world.B, world.BGoal, pause, None));
            Assert.Equal((CancelPauseOutcome.PauseNotFound, PauseNotFound), (cancelled.Outcome, cancelled.Message));
        }

        // B's own goal shows B's exemptions only: A's current pause does not make it paused, and no reason of A's appears.
        var resumed = await PausesAsync(services, s => s.ResumeAsync(world.B, world.BGoal, None));
        Assert.Equal((ResumeOutcome.NotPaused, NotPaused), (resumed.Outcome, resumed.Message));
        var bsPauses = await PausesAsync(services, s => s.GetPausesAsync(world.B, world.BGoal, None));
        Assert.Equal(PausesOutcome.Found, bsPauses.Outcome);
        var bsPause = Assert.Single(bsPauses.Pauses);
        Assert.Equal((world.BUpcoming, Mon23, Fri27, world.BReason), (bsPause.Id, bsPause.FirstDay, bsPause.LastDay, bsPause.Reason));
        var bsHistory = await RecordingAsync(services, s => s.GetHistoryAsync(world.B, world.BGoal, Mon9, Sun22, None));
        Assert.Equal(HistoryOutcome.Found, bsHistory.Outcome);
        Assert.All(bsHistory.History!.Entries, entry =>
        {
            Assert.False(entry.ExcusalReason is { } reason && world.AReasons.Contains(reason));
            Assert.NotEqual(world.ANote, entry.Note);
        });

        Assert.Equal(before, await SnapshotAsync(world));
        await AssertTheOwnerSeesWhatTheOthersWereRefusedAsync(services, world);
    }

    [Fact]
    public async Task US5_AS2_AS3_FR_022_an_administrator_and_an_anonymous_visitor_change_no_row_and_get_nothing()
    {
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var world = await ArrangeAsync(services, clock, "adm");
        var before = await SnapshotAsync(world);

        await AssertEveryUseCaseRefusedAsync(services, world.Administrator, world.AGoal, world.AUpcoming, "NotForAdministrators", NotForAdministrators);
        await AssertEveryUseCaseRefusedAsync(services, world.Administrator, world.BGoal, world.BUpcoming, "NotForAdministrators", NotForAdministrators);
        await AssertEveryUseCaseRefusedAsync(services, ActingUser.Anonymous, world.AGoal, world.AUpcoming, "NotSignedIn", NotSignedIn);

        Assert.Equal(before, await SnapshotAsync(world));
        await AssertTheOwnerSeesWhatTheOthersWereRefusedAsync(services, world);
    }

    [Fact]
    public async Task FR_021_every_exemption_store_member_called_with_B_or_the_administrator_as_owner_gives_nothing_and_writes_nothing()
    {
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var world = await ArrangeAsync(services, clock, "store");
        var before = await SnapshotAsync(world);
        var reason = Reason("Someone else's reason.");

        foreach (var owner in new[] { world.B.AccountId!.Value, world.Administrator.AccountId!.Value })
        {
            Assert.Empty((await StoreAsync(services, s => s.ReadExemptionsAsync(owner, world.AGoal, None))).Excusals);
            Assert.Empty((await StoreAsync(services, s => s.ReadExemptionsAsync(owner, world.AGoal, None))).Pauses);
            var writes = new Func<IRecordStore, Task<ExemptionStoreWrite>>[]
            {
                s => s.ExcuseAsync(owner, world.AGoal, Mon16, reason, None),
                s => s.RemoveExcusalAsync(owner, world.AGoal, Tue17, None),
                s => s.AddPauseAsync(owner, world.AGoal, Sat28, Sun29, reason, None),
                s => s.SetPauseLastDayAsync(owner, world.AGoal, world.ACurrent, Wed18, None),
                s => s.DeletePauseAsync(owner, world.AGoal, world.AUpcoming, None),
            };
            foreach (var write in writes)
            {
                var result = await StoreAsync(services, write);
                Assert.Equal(ExemptionStoreWriteOutcome.GoalNotFound, result.Outcome);
                Assert.Null(result.PauseId);
            }
        }

        // A's pauses through B's own goal: found only among that goal's pauses.
        var b = world.B.AccountId!.Value;
        foreach (var pause in new[] { world.ACurrent, world.AUpcoming })
        {
            Assert.Equal(
                ExemptionStoreWriteOutcome.PauseNotFound,
                (await StoreAsync(services, s => s.SetPauseLastDayAsync(b, world.BGoal, pause, Fri27, None))).Outcome);
            Assert.Equal(
                ExemptionStoreWriteOutcome.PauseNotFound,
                (await StoreAsync(services, s => s.DeletePauseAsync(b, world.BGoal, pause, None))).Outcome);
        }

        Assert.Equal(before, await SnapshotAsync(world));
    }

    /// <summary>
    /// Every use case by <paramref name="user"/> on <paramref name="goal"/>, with the arguments that succeed for its owner
    /// (<paramref name="upcoming"/> is the goal's upcoming pause): each gives <paramref name="outcome"/> and
    /// <paramref name="message"/>, and no view.
    /// </summary>
    private static async Task AssertEveryUseCaseRefusedAsync(
        IServiceProvider services, ActingUser user, GoalId goal, PauseId upcoming, string outcome, string message)
    {
        var excused = await ExcusalsAsync(services, s => s.ExcuseAsync(user, goal, Mon16, "Someone else's reason.", None));
        var removed = await ExcusalsAsync(services, s => s.RemoveExcusalAsync(user, goal, Tue17, None));
        var paused = await PausesAsync(services, s => s.PauseAsync(user, goal, Sat28, Sun29, "Someone else's reason.", None));
        var resumed = await PausesAsync(services, s => s.ResumeAsync(user, goal, None));
        var cancelled = await PausesAsync(services, s => s.CancelPauseAsync(user, goal, upcoming, None));
        var read = await PausesAsync(services, s => s.GetPausesAsync(user, goal, None));

        Assert.All(
            new[]
            {
                (excused.Outcome.ToString(), excused.Message),
                (removed.Outcome.ToString(), removed.Message),
                (paused.Outcome.ToString(), paused.Message),
                (resumed.Outcome.ToString(), resumed.Message),
                (cancelled.Outcome.ToString(), cancelled.Message),
                (read.Outcome.ToString(), read.Message!),
            },
            result => Assert.Equal((outcome, message), result));
        Assert.Null(excused.Date);
        Assert.Null(removed.State);
        Assert.Null(paused.Pause);
        Assert.Empty(read.Pauses);
        Assert.Null(read.PauseDate);
    }

    /// <summary>
    /// A reads A's own pauses and history: the arrangement is what the others were refused, so their refusals are not for
    /// want of data.
    /// </summary>
    private static async Task AssertTheOwnerSeesWhatTheOthersWereRefusedAsync(IServiceProvider services, World world)
    {
        var pauses = await PausesAsync(services, s => s.GetPausesAsync(world.A, world.AGoal, None));
        Assert.Equal(PausesOutcome.Found, pauses.Outcome);
        Assert.Equal(
            [(world.AUpcoming, world.AUpcomingReason), (world.ACurrent, world.ACurrentReason)],
            pauses.Pauses.Select(p => (p.Id, p.Reason)));
        var history = await RecordingAsync(services, s => s.GetHistoryAsync(world.A, world.AGoal, Fri13, Tue17, None));
        Assert.Equal(HistoryOutcome.Found, history.Outcome);
        var entries = history.History!.Entries;
        Assert.Equal(world.AExcusalReason, Assert.Single(entries, e => e.FirstDate == Tue17).ExcusalReason);
        Assert.Equal(world.ANote, Assert.Single(entries, e => e.FirstDate == Fri13).Note);
    }

    /// <summary>
    /// User A with the goal of the class summary, its record, note, excusal and two pauses; user B with a goal of B's own
    /// on the same habit, an excusal and a pause; and a stored administrator. The clock is left at Wednesday 18, 10:00Z.
    /// </summary>
    private async Task<World> ArrangeAsync(IServiceProvider services, FakeTimeProvider clock, string who)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var a = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-exp-{who}-a-{suffix}@example.com");
        var b = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-exp-{who}-b-{suffix}@example.com");
        var administrator = await AdministratorAsync(services, $"it-exp-{who}-admin-{suffix}@example.com");
        var habit = new GoalHabit(await CatalogHabitAsync(services, $"It-Exp Reading {who} {suffix}"), HabitKind.Catalog);
        var aGoal = await CreateGoalAsync(services, a, habit);
        var bGoal = await CreateGoalAsync(services, b, habit);

        clock.SetUtcNow(Fri13Noon);
        var aNote = $"It-Exp A's note {suffix}";
        var recorded = await RecordingAsync(services, s => s.RecordAsync(a, aGoal, Fri13, new RecordInput { Amount = 25, Note = aNote }, None));
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);

        clock.SetUtcNow(Wed18TenOClock);
        var aExcusalReason = $"It-Exp A's excusal reason {suffix}";
        var aCurrentReason = $"It-Exp A's current pause reason {suffix}";
        var aUpcomingReason = $"It-Exp A's upcoming pause reason {suffix}";
        var bReason = $"It-Exp B's reason {suffix}";
        Assert.Equal(ExcuseOutcome.Excused, (await ExcusalsAsync(services, s => s.ExcuseAsync(a, aGoal, Tue17, aExcusalReason, None))).Outcome);
        var aCurrent = await PausedAsync(services, a, aGoal, Wed18, Thu19, aCurrentReason);
        var aUpcoming = await PausedAsync(services, a, aGoal, Mon23, Fri27, aUpcomingReason);
        Assert.Equal(ExcuseOutcome.Excused, (await ExcusalsAsync(services, s => s.ExcuseAsync(b, bGoal, Tue17, bReason, None))).Outcome);
        var bUpcoming = await PausedAsync(services, b, bGoal, Mon23, Fri27, bReason);
        return new World(
            a, b, administrator, aGoal, bGoal, aCurrent, aUpcoming, bUpcoming, aNote, aExcusalReason, aCurrentReason, aUpcomingReason, bReason);
    }

    private static async Task<PauseId> PausedAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly first, DateOnly last, string reason)
    {
        var paused = await PausesAsync(services, s => s.PauseAsync(user, goal, first, last, reason, None));
        Assert.Equal(PauseOutcome.Paused, paused.Outcome);
        return paused.Pause!.Id;
    }

    /// <summary>Creates a stored administrator with this email (deleting an earlier one) and returns it as an acting user.</summary>
    private async Task<ActingUser> AdministratorAsync(IServiceProvider services, string email)
    {
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<IAccountStore>()
            .CreateAsync(parsed, Role.Administrator, HabitAccounts.Password, TestProfiles.Utc, None);
        var account = created.Account ?? throw new InvalidOperationException("The test email is taken.");
        return ActingUser.ForAccount(account.Id, account.Role);
    }

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, GoalHabit habit)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = habit, Target = Reading20MinDaily, StartDate = Mon9 }, None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private static ExemptionReason? Reason(string text)
    {
        Assert.True(ExemptionReason.TryCreate(text, out var reason, out _));
        return reason;
    }

    private static async Task<T> ExcusalsAsync<T>(IServiceProvider services, Func<ExcusalService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<ExcusalService>());
    }

    private static async Task<T> PausesAsync<T>(IServiceProvider services, Func<PauseService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<PauseService>());
    }

    private static async Task<T> RecordingAsync<T>(IServiceProvider services, Func<RecordingService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<RecordingService>());
    }

    private static async Task<T> StoreAsync<T>(IServiceProvider services, Func<IRecordStore, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<IRecordStore>());
    }

    /// <summary>Every row of <paramref name="table"/> belonging to the owner's goals, as one text per row, in a stable order.</summary>
    private async Task<IReadOnlyList<string>> RowsOfOwnerAsync(string table, string columns, string order, AccountId owner)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            $"SELECT {columns} FROM [{table}] t JOIN [Goals] g ON g.[Id] = t.[GoalId] WHERE g.[OwnerId] = @owner ORDER BY {order}",
            ("@owner", owner.Value));
        return rows.Select(r => string.Join("|", r.Values)).ToList();
    }

    private async Task<IReadOnlyList<string>> RowsOfOwnerAsync(AccountId owner) =>
    [
        .. await RowsOfOwnerAsync("GoalExcusals", "t.[GoalId], t.[Date], t.[Reason]", "t.[GoalId], t.[Date]", owner),
        .. await RowsOfOwnerAsync("GoalPauses", "t.[Id], t.[GoalId], t.[FirstDay], t.[LastDay], t.[Reason]", "t.[GoalId], t.[FirstDay]", owner),
        .. await RowsOfOwnerAsync("GoalRecords", "t.[Id], t.[GoalId], t.[Date], t.[Answer], t.[Amount], t.[Sequence]", "t.[GoalId], t.[Date], t.[Sequence]", owner),
        .. await RowsOfOwnerAsync("GoalNotes", "t.[GoalId], t.[EntryDate], t.[Text]", "t.[GoalId], t.[EntryDate]", owner),
    ];

    /// <summary>A's and B's excusal, pause, record and note rows, with their counts checked against the arrangement.</summary>
    private async Task<Snapshot> SnapshotAsync(World world)
    {
        var snapshot = new Snapshot(await RowsOfOwnerAsync(world.A.AccountId!.Value), await RowsOfOwnerAsync(world.B.AccountId!.Value));

        // A: one excusal, two pauses, one record and one note; B: one excusal and one pause.
        Assert.Equal(5, snapshot.A.Count);
        Assert.Equal(2, snapshot.B.Count);
        return snapshot;
    }

    /// <summary>The application as the integration tests build it, with the test's own clock.</summary>
    private ServiceProvider BuildServices(FakeTimeProvider clock)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private sealed record World(
        ActingUser A,
        ActingUser B,
        ActingUser Administrator,
        GoalId AGoal,
        GoalId BGoal,
        PauseId ACurrent,
        PauseId AUpcoming,
        PauseId BUpcoming,
        string ANote,
        string AExcusalReason,
        string ACurrentReason,
        string AUpcomingReason,
        string BReason)
    {
        public IReadOnlyList<string> AReasons => [AExcusalReason, ACurrentReason, AUpcomingReason];
    }

    /// <summary>A's and B's rows; equal when every column of every row is equal.</summary>
    private sealed record Snapshot(IReadOnlyList<string> A, IReadOnlyList<string> B)
    {
        public bool Equals(Snapshot? other) => other is not null && A.SequenceEqual(other.A) && B.SequenceEqual(other.B);

        public override int GetHashCode() => HashCode.Combine(A.Count, B.Count);
    }
}
