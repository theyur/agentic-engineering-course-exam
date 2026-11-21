using System.Globalization;
using HT.Application;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Records;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Records.RecordSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// The excusal and pause members of the record store on SQL Server (research R-04, R-11; FR-021, FR-025): each member's
/// happy path, each re-check of contracts/application-api.md "Re-checked under the lock", the record add refused on an
/// excused or paused entry, owner scoping, and the cascade from an account through its goals. The store is used through
/// the test's own provider with its own clock, never the fixture's shared one; the sample goals start on Wednesday 18
/// November 2026 in UTC. Shared database: own prefixes (<see cref="RecordSamples.EmailPrefix"/>,
/// <see cref="RecordSamples.HabitPrefix"/>); rows are read per goal only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ExemptionStoreTests(DatabaseFixture database) : IAsyncLifetime
{
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Wed18 = Start;
    private static readonly DateOnly Fri20 = Start.AddDays(2);
    private static readonly DateOnly Mon23 = Start.AddDays(5);
    private static readonly DateOnly Wed25 = Start.AddDays(7);
    private static readonly DateOnly Sun29 = Start.AddDays(11);

    private ServiceProvider _services = null!;

    private static CancellationToken None => CancellationToken.None;

    public Task InitializeAsync()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero)));
        _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _services.DisposeAsync();

    [Fact]
    public async Task Excusals_and_pauses_are_written_read_changed_and_removed()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-happy", suffix);
        var goal = await GoalAsync(database, owner, "Exm happy", suffix, YogaMonWedFri());

        var excused = await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Wed18, Reason("Ill"), None));
        var excusedWithoutReason = await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Fri20, null, None));
        var paused = await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Mon23, Sun29, Reason("Family visit"), None));

        Assert.Equal(ExemptionStoreWriteOutcome.Written, excused.Outcome);
        Assert.Null(excused.PauseId);
        Assert.Equal(ExemptionStoreWriteOutcome.Written, excusedWithoutReason.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Written, paused.Outcome);
        var pauseId = Assert.IsType<PauseId>(paused.PauseId);
        var read = await WithStoreAsync(s => s.ReadExemptionsAsync(owner, goal, None));
        Assert.Equal([new Excusal(Wed18, Reason("Ill")), new Excusal(Fri20, null)], read.Excusals);
        Assert.Equal([new Pause(pauseId, Mon23, Sun29, Reason("Family visit"))], read.Pauses);
        Assert.Equal(
            ["2026-11-18 Ill", "2026-11-20 "],
            (await ExcusalRowsAsync(goal)).Select(r => ((DateTime)r["Date"]!).ToString("yyyy-MM-dd ", CultureInfo.InvariantCulture) + r["Reason"]));
        var row = Assert.Single(await PauseRowsAsync(goal));
        Assert.Equal(pauseId.Value, row["Id"]);
        Assert.Equal("Family visit", row["Reason"]);

        var removed = await WithStoreAsync(s => s.RemoveExcusalAsync(owner, goal, Wed18, None));
        var shortened = await WithStoreAsync(s => s.SetPauseLastDayAsync(owner, goal, pauseId, Wed25, None));

        Assert.Equal(ExemptionStoreWriteOutcome.Written, removed.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Written, shortened.Outcome);
        read = await WithStoreAsync(s => s.ReadExemptionsAsync(owner, goal, None));
        Assert.Equal([new Excusal(Fri20, null)], read.Excusals);
        Assert.Equal([new Pause(pauseId, Mon23, Wed25, Reason("Family visit"))], read.Pauses);

        var deleted = await WithStoreAsync(s => s.DeletePauseAsync(owner, goal, pauseId, None));

        Assert.Equal(ExemptionStoreWriteOutcome.Written, deleted.Outcome);
        Assert.Empty(await PauseRowsAsync(goal));
        Assert.Empty((await WithStoreAsync(s => s.ReadExemptionsAsync(owner, goal, None))).Pauses);
    }

    [Fact]
    public async Task A_goal_without_excusals_and_pauses_reads_none()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-none", suffix);
        var goal = await GoalAsync(database, owner, "Exm none", suffix, YogaMonWedFri());

        var read = await WithStoreAsync(s => s.ReadExemptionsAsync(owner, goal, None));

        Assert.Empty(read.Excusals);
        Assert.Empty(read.Pauses);
    }

    [Fact]
    public async Task Excusing_an_excused_a_recorded_or_a_paused_date_is_refused_and_writes_nothing()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-excuse", suffix);
        var goal = await GoalAsync(database, owner, "Exm excuse", suffix, YogaMonWedFri());
        await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Wed18, Reason("Ill"), None));
        await WithStoreAsync(s => s.AddAsync(
            owner, goal, Day(Fri20), true, Fri20, No(), NoteWrite.Keep, RecordLimits.MaxRecordsPerPeriod, None));
        await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Mon23, Sun29, null, None));
        var excusalsBefore = await ExcusalRowsAsync(goal);

        var again = await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Wed18, Reason("Other"), None));
        var recorded = await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Fri20, null, None));
        var paused = await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Wed25, null, None));
        var pausedFirstDay = await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Mon23, null, None));

        Assert.Equal(ExemptionStoreWriteOutcome.AlreadyExcused, again.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.HasRecord, recorded.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Paused, paused.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Paused, pausedFirstDay.Outcome);
        Assert.Equal(excusalsBefore, await ExcusalRowsAsync(goal));
        Assert.Single(await RecordRowsAsync(database.ConnectionString, goal));
    }

    [Fact]
    public async Task Removing_an_excusal_that_does_not_exist_gives_NotExcused()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-remove", suffix);
        var goal = await GoalAsync(database, owner, "Exm remove", suffix, YogaMonWedFri());
        await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Wed18, null, None));

        var removed = await WithStoreAsync(s => s.RemoveExcusalAsync(owner, goal, Fri20, None));

        Assert.Equal(ExemptionStoreWriteOutcome.NotExcused, removed.Outcome);
        Assert.Single(await ExcusalRowsAsync(goal));
    }

    [Fact]
    public async Task A_pause_that_shares_one_date_at_either_end_is_refused_and_a_touching_one_is_added()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-overlap", suffix);
        var goal = await GoalAsync(database, owner, "Exm overlap", suffix, YogaMonWedFri());
        await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Mon23, Sun29, null, None));

        var overlapsFirstDay = await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Wed18, Mon23, null, None));
        var overlapsLastDay = await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Sun29, Sun29.AddDays(3), null, None));
        var inside = await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Wed25, Wed25, null, None));
        var before = await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Wed18, Mon23.AddDays(-1), null, None));
        var after = await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Sun29.AddDays(1), Sun29.AddDays(1), null, None));

        Assert.Equal(ExemptionStoreWriteOutcome.Overlaps, overlapsFirstDay.Outcome);
        Assert.Null(overlapsFirstDay.PauseId);
        Assert.Equal(ExemptionStoreWriteOutcome.Overlaps, overlapsLastDay.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Overlaps, inside.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Written, before.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Written, after.Outcome);
        Assert.Equal(
            [(Sun29.AddDays(1), Sun29.AddDays(1)), (Mon23, Sun29), (Wed18, Mon23.AddDays(-1))],
            (await WithStoreAsync(s => s.ReadExemptionsAsync(owner, goal, None))).Pauses.Select(p => (p.FirstDay, p.LastDay)));
    }

    [Fact]
    public async Task Changing_or_deleting_another_goals_pause_gives_PauseNotFound_and_leaves_it()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-other-pause", suffix);
        var goal = await GoalAsync(database, owner, "Exm other pause A", suffix, YogaMonWedFri());
        var other = await GoalAsync(database, owner, "Exm other pause B", suffix, YogaMonWedFri());
        var othersPause = (await WithStoreAsync(s => s.AddPauseAsync(owner, other, Mon23, Sun29, null, None))).PauseId!.Value;
        var unknown = new PauseId(Guid.NewGuid());

        var setOthers = await WithStoreAsync(s => s.SetPauseLastDayAsync(owner, goal, othersPause, Wed25, None));
        var deleteOthers = await WithStoreAsync(s => s.DeletePauseAsync(owner, goal, othersPause, None));
        var setUnknown = await WithStoreAsync(s => s.SetPauseLastDayAsync(owner, other, unknown, Wed25, None));
        var deleteUnknown = await WithStoreAsync(s => s.DeletePauseAsync(owner, other, unknown, None));

        Assert.Equal(ExemptionStoreWriteOutcome.PauseNotFound, setOthers.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.PauseNotFound, deleteOthers.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.PauseNotFound, setUnknown.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.PauseNotFound, deleteUnknown.Outcome);
        Assert.Equal(
            [new Pause(othersPause, Mon23, Sun29, null)],
            (await WithStoreAsync(s => s.ReadExemptionsAsync(owner, other, None))).Pauses);
    }

    [Fact]
    public async Task A_later_last_day_that_reaches_another_pause_gives_Overlaps()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-extend", suffix);
        var goal = await GoalAsync(database, owner, "Exm extend", suffix, YogaMonWedFri());
        var first = (await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Wed18, Fri20, null, None))).PauseId!.Value;
        await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Mon23, Sun29, null, None));

        var reaches = await WithStoreAsync(s => s.SetPauseLastDayAsync(owner, goal, first, Mon23, None));
        var touches = await WithStoreAsync(s => s.SetPauseLastDayAsync(owner, goal, first, Mon23.AddDays(-1), None));

        Assert.Equal(ExemptionStoreWriteOutcome.Overlaps, reaches.Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Written, touches.Outcome);
        Assert.Contains(new Pause(first, Wed18, Mon23.AddDays(-1), null),
            (await WithStoreAsync(s => s.ReadExemptionsAsync(owner, goal, None))).Pauses);
    }

    [Fact]
    public async Task Adding_a_record_on_an_excused_date_a_paused_date_or_a_week_with_one_paused_date_gives_Exempt()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-add", suffix);
        var commitments = await GoalAsync(database, owner, "Exm add yoga", suffix, YogaMonWedFri());
        var days = await GoalAsync(database, owner, "Exm add coffee", suffix, CoffeeAtMost2Cups());
        var weeks = await GoalAsync(database, owner, "Exm add pages", suffix, Pages20ThreeTimesAWeek(), Mon16);
        await WithStoreAsync(s => s.ExcuseAsync(owner, commitments, Wed18, null, None));
        await WithStoreAsync(s => s.AddPauseAsync(owner, commitments, Mon23, Mon23, null, None));
        await WithStoreAsync(s => s.AddPauseAsync(owner, days, Fri20, Fri20, null, None));
        await WithStoreAsync(s => s.AddPauseAsync(owner, weeks, Sun29, Sun29, null, None));

        var excused = await AddAsync(owner, commitments, Day(Wed18), true, Wed18, Yes());
        var pausedCommitment = await AddAsync(owner, commitments, Day(Mon23), true, Mon23, Yes());
        var pausedDay = await AddAsync(owner, days, Day(Fri20), false, Fri20, RecordValue.FromAmount(1));
        var pausedWeek = await AddAsync(owner, weeks, Week(Mon23), false, Wed25, RecordValue.FromAmount(20));
        var counted = await AddAsync(owner, weeks, Week(Mon16), false, Wed18, RecordValue.FromAmount(20));

        Assert.All(new[] { excused, pausedCommitment, pausedDay, pausedWeek }, write =>
        {
            Assert.Equal(RecordStoreWriteOutcome.Exempt, write.Outcome);
            Assert.Null(write.RecordId);
            Assert.Null(write.Entry);
        });
        Assert.Equal(RecordStoreWriteOutcome.Written, counted.Outcome);
        Assert.Empty(await RecordRowsAsync(database.ConnectionString, commitments));
        Assert.Empty(await RecordRowsAsync(database.ConnectionString, days));
        Assert.Single(await RecordRowsAsync(database.ConnectionString, weeks));
        Assert.Empty(await NoteRowsAsync(database.ConnectionString, commitments));
        Assert.Empty(await NoteRowsAsync(database.ConnectionString, days));
        Assert.DoesNotContain(await NoteRowsAsync(database.ConnectionString, weeks), r => (DateTime)r["EntryDate"]! == Mon23.ToDateTime(TimeOnly.MinValue));
    }

    [Fact]
    public async Task Another_owners_goal_gives_GoalNotFound_on_every_write_writes_nothing_and_reads_none()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-owner", suffix);
        var intruder = await OwnerAsync(database, "exm-intruder", suffix);
        var goal = await GoalAsync(database, owner, "Exm owner", suffix, YogaMonWedFri());
        await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Wed18, Reason("Ill"), None));
        var pause = (await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Mon23, Sun29, null, None))).PauseId!.Value;
        var excusalsBefore = await ExcusalRowsAsync(goal);
        var pausesBefore = await PauseRowsAsync(goal);

        ExemptionStoreWrite[] writes =
        [
            await WithStoreAsync(s => s.ExcuseAsync(intruder, goal, Fri20, null, None)),
            await WithStoreAsync(s => s.RemoveExcusalAsync(intruder, goal, Wed18, None)),
            await WithStoreAsync(s => s.AddPauseAsync(intruder, goal, Sun29.AddDays(1), Sun29.AddDays(2), null, None)),
            await WithStoreAsync(s => s.SetPauseLastDayAsync(intruder, goal, pause, Wed25, None)),
            await WithStoreAsync(s => s.DeletePauseAsync(intruder, goal, pause, None)),
        ];
        var read = await WithStoreAsync(s => s.ReadExemptionsAsync(intruder, goal, None));
        var add = await AddAsync(intruder, goal, Day(Fri20), true, Fri20, Yes());

        Assert.All(writes, w => Assert.Equal(ExemptionStoreWriteOutcome.GoalNotFound, w.Outcome));
        Assert.Same(GoalExemptions.None, read);
        Assert.Equal(RecordStoreWriteOutcome.GoalNotFound, add.Outcome);
        Assert.Equal(excusalsBefore, await ExcusalRowsAsync(goal));
        Assert.Equal(pausesBefore, await PauseRowsAsync(goal));
    }

    [Fact]
    public async Task Deleting_an_account_removes_its_goals_excusals_and_pauses()
    {
        var suffix = Suffix();
        var email = $"{EmailPrefix}exm-cascade-{suffix}@example.com";
        var owner = await OwnerAsync(database, "exm-cascade", suffix);
        var goal = await GoalAsync(database, owner, "Exm cascade", suffix, YogaMonWedFri());
        await WithStoreAsync(s => s.ExcuseAsync(owner, goal, Wed18, Reason("Ill"), None));
        await WithStoreAsync(s => s.AddPauseAsync(owner, goal, Mon23, Sun29, Reason("Family visit"), None));
        Assert.Single(await ExcusalRowsAsync(goal));
        Assert.Single(await PauseRowsAsync(goal));

        await Sql.DeleteUserAsync(database.ConnectionString, email);

        Assert.Empty(await ExcusalRowsAsync(goal));
        Assert.Empty(await PauseRowsAsync(goal));
    }

    private static ExemptionReason Reason(string text)
    {
        Assert.True(ExemptionReason.TryCreate(text, out var reason, out _));
        return reason!;
    }

    /// <summary>One store call in a scope (and so a database context) of its own, from the test's own provider.</summary>
    private async Task<T> WithStoreAsync<T>(Func<IRecordStore, Task<T>> call)
    {
        await using var scope = _services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<IRecordStore>());
    }

    private Task<RecordStoreWrite> AddAsync(AccountId owner, GoalId goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value) =>
        WithStoreAsync(s => s.AddAsync(
            owner, goal, entry, isCommitment, date, value, NoteWrite.Replace(Note()), RecordLimits.MaxRecordsPerPeriod, None));

    private static NoteText Note()
    {
        Assert.True(NoteText.TryCreate("Not written.", out var note, out _));
        return note!;
    }

    /// <summary>The goal's <c>GoalExcusals</c> rows by date, each as column name to value.</summary>
    private Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ExcusalRowsAsync(GoalId goal) =>
        Sql.QueryAsync(database.ConnectionString,
            "SELECT [GoalId], [Date], [Reason] FROM [GoalExcusals] WHERE [GoalId] = @goal ORDER BY [Date]",
            ("@goal", goal.Value));

    /// <summary>The goal's <c>GoalPauses</c> rows by first day, each as column name to value.</summary>
    private Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> PauseRowsAsync(GoalId goal) =>
        Sql.QueryAsync(database.ConnectionString,
            "SELECT [Id], [GoalId], [FirstDay], [LastDay], [Reason] FROM [GoalPauses] WHERE [GoalId] = @goal ORDER BY [FirstDay]",
            ("@goal", goal.Value));
}
