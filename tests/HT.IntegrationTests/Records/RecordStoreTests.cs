using HT.Application;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Records;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Records.RecordSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// The record store on SQL Server (research R-05, R-14; US1, FR-003, FR-005, FR-007): the stored order of a period's
/// records, a commitment's record replaced in place, the 50-record limit, a note given with a record, and recording
/// through <see cref="RecordingService"/> over the real stores on a goal whose catalog habit is then Unpublished (slice 4
/// FR-034). The sample goals start on Wednesday 18 November 2026 in UTC. Shared database: own prefixes
/// (<see cref="RecordSamples.EmailPrefix"/>, <see cref="RecordSamples.HabitPrefix"/>); rows are read per goal only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class RecordStoreTests(DatabaseFixture database)
{
    private static readonly DateOnly Wed18 = Start;
    private static readonly DateOnly Thu19 = Start.AddDays(1);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);

    private static CancellationToken None => CancellationToken.None;

    private async Task<T> WithStoreAsync<T>(Func<IRecordStore, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<IRecordStore>());
    }

    private Task<RecordStoreWrite> AddAsync(
        AccountId owner, GoalId goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value, NoteText? note = null) =>
        WithStoreAsync(s => s.AddAsync(owner, goal, entry, isCommitment, date, value, note, RecordLimits.MaxRecordsPerPeriod, None));

    private static NoteText Note(string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        return note!;
    }

    [Fact]
    public async Task A_periods_records_on_one_date_keep_the_order_they_were_stored_in_after_the_date_order()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "order", suffix);
        var goal = await GoalAsync(database, owner, "Order", suffix, Pages20ThreeTimesAWeek(), Mon16);
        var week = Week(Mon16);

        var first = await AddAsync(owner, goal, week, false, Wed18, RecordValue.FromAmount(5));
        await AddAsync(owner, goal, week, false, Thu19, RecordValue.FromAmount(30));
        var third = await AddAsync(owner, goal, week, false, Wed18, RecordValue.FromAmount(25));
        var fourth = await AddAsync(owner, goal, week, false, Wed18, RecordValue.FromAmount(15));

        Assert.Equal(RecordStoreWriteOutcome.Written, fourth.Outcome);
        Assert.Equal([(Wed18, 5), (Wed18, 25), (Wed18, 15), (Thu19, 30)], fourth.Entry!.Records.Select(r => (r.Date, r.Value.Amount!.Value)));
        Assert.Equal(first.RecordId, fourth.Entry.Records[0].Id);
        Assert.Equal(third.RecordId, fourth.Entry.Records[1].Id);
        var read = await WithStoreAsync(s => s.ReadAsync(owner, goal, week.FirstDate, week.LastDate, None));
        Assert.Equal(fourth.Entry.Records, read.Records);
        var rows = await RecordRowsAsync(database.ConnectionString, goal);
        var wednesday = rows.Where(r => (DateTime)r["Date"]! == Wed18.ToDateTime(TimeOnly.MinValue)).Select(r => (long)r["Sequence"]!).ToList();
        Assert.Equal(wednesday.Order(), wednesday);
    }

    [Fact]
    public async Task A_commitments_record_is_replaced_in_place_and_keeps_its_identifier_and_sequence()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "replace", suffix);
        var goal = await GoalAsync(database, owner, "Replace", suffix, Reading20MinDaily());

        var first = await AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(12));
        var before = Assert.Single(await RecordRowsAsync(database.ConnectionString, goal));
        var second = await AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(25));

        Assert.Equal(RecordStoreWriteOutcome.Written, second.Outcome);
        Assert.Equal(first.RecordId, second.RecordId);
        var record = Assert.Single(second.Entry!.Records);
        Assert.Equal(new GoalRecord(first.RecordId!.Value, Wed18, Minutes(25)), record);
        var after = Assert.Single(await RecordRowsAsync(database.ConnectionString, goal));
        Assert.Equal(before["Id"], after["Id"]);
        Assert.Equal(before["Sequence"], after["Sequence"]);
        Assert.Equal(25, after["Amount"]);
    }

    [Fact]
    public async Task The_51st_record_of_a_period_gives_LimitReached_and_writes_nothing()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "limit", suffix);
        var goal = await GoalAsync(database, owner, "Limit", suffix, CoffeeAtMost2Cups());
        await Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalRecords] ([Id], [GoalId], [Date], [Answer], [Amount]) "
            + "SELECT NEWID(), @goal, @date, NULL, 1 FROM (SELECT TOP (49) 1 AS [X] FROM sys.all_objects) AS [N]",
            ("@goal", goal.Value), ("@date", Wed18.ToDateTime(TimeOnly.MinValue)));

        var fiftieth = await AddAsync(owner, goal, Day(Wed18), false, Wed18, RecordValue.FromAmount(2));
        var fiftyFirst = await AddAsync(owner, goal, Day(Wed18), false, Wed18, RecordValue.FromAmount(3), Note("Too many."));

        Assert.Equal(RecordStoreWriteOutcome.Written, fiftieth.Outcome);
        Assert.Equal(50, fiftieth.Entry!.Records.Count);
        Assert.Equal(RecordStoreWriteOutcome.LimitReached, fiftyFirst.Outcome);
        Assert.Null(fiftyFirst.Entry);
        var rows = await RecordRowsAsync(database.ConnectionString, goal);
        Assert.Equal(50, rows.Count);
        Assert.DoesNotContain(rows, r => (int?)r["Amount"] == 3);
        Assert.Empty(await NoteRowsAsync(database.ConnectionString, goal));
    }

    [Fact]
    public async Task A_note_with_a_record_is_upserted_with_text_and_left_unchanged_without()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "note", suffix);
        var goal = await GoalAsync(database, owner, "Note", suffix, Reading20MinDaily());

        var added = await AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(12), Note("Back was tired."));
        var kept = await AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(15));
        var keptRows = await NoteRowsAsync(database.ConnectionString, goal);
        var replaced = await AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(20), Note("Better today."));

        Assert.Equal(Note("Back was tired."), added.Entry!.Note);
        Assert.Equal(Note("Back was tired."), kept.Entry!.Note);
        Assert.Equal("Back was tired.", Assert.Single(keptRows)["Text"]);
        Assert.Equal(Note("Better today."), replaced.Entry!.Note);
        var row = Assert.Single(await NoteRowsAsync(database.ConnectionString, goal));
        Assert.Equal("Better today.", row["Text"]);
        Assert.Equal(Wed18.ToDateTime(TimeOnly.MinValue), row["EntryDate"]);
    }

    [Fact]
    public async Task A_note_of_a_week_is_filed_under_its_Monday()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "week-note", suffix);
        var goal = await GoalAsync(database, owner, "Week note", suffix, Pages20ThreeTimesAWeek(), Mon16);

        var written = await AddAsync(owner, goal, Week(Mon16), false, Thu19, RecordValue.FromAmount(20), Note("Knee was sore."));

        Assert.Equal(Note("Knee was sore."), written.Entry!.Note);
        Assert.Equal(Mon16.ToDateTime(TimeOnly.MinValue), Assert.Single(await NoteRowsAsync(database.ConnectionString, goal))["EntryDate"]);
    }

    [Fact]
    public async Task A_change_gives_the_record_its_new_value_and_keeps_its_identifier_date_and_sequence()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "change", suffix);
        var goal = await GoalAsync(database, owner, "Change", suffix, Pages20ThreeTimesAWeek(), Mon16);
        var week = Week(Mon16);
        var first = await AddAsync(owner, goal, week, false, Wed18, RecordValue.FromAmount(10));
        var second = await AddAsync(owner, goal, week, false, Wed18, RecordValue.FromAmount(20));
        var third = await AddAsync(owner, goal, week, false, Wed18, RecordValue.FromAmount(30));
        var before = await RecordRowsAsync(database.ConnectionString, goal);

        var changed = await WithStoreAsync(s => s.ChangeAsync(owner, goal, second.RecordId!.Value, week, RecordValue.FromAmount(5), null, None));

        Assert.Equal(RecordStoreWriteOutcome.Written, changed.Outcome);
        Assert.Equal(second.RecordId, changed.RecordId);
        Assert.Equal(
            [
                new GoalRecord(first.RecordId!.Value, Wed18, RecordValue.FromAmount(10)),
                new GoalRecord(second.RecordId!.Value, Wed18, RecordValue.FromAmount(5)),
                new GoalRecord(third.RecordId!.Value, Wed18, RecordValue.FromAmount(30)),
            ],
            changed.Entry!.Records);
        var after = await RecordRowsAsync(database.ConnectionString, goal);
        Assert.Equal(before.Select(r => (r["Id"], r["Date"], r["Sequence"])), after.Select(r => (r["Id"], r["Date"], r["Sequence"])));
        Assert.Equal([10, 5, 30], after.Select(r => (int)r["Amount"]!));
    }

    [Fact]
    public async Task A_change_with_a_note_with_text_replaces_the_entrys_note_and_one_without_or_with_only_spaces_leaves_it()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "change-note", suffix);
        var goal = await GoalAsync(database, owner, "Change note", suffix, Reading20MinDaily());
        var record = (await AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(12), Note("Back was tired."))).RecordId!.Value;
        Assert.True(NoteText.TryCreate("   ", out var spaces, out _));

        var withoutNote = await WithStoreAsync(s => s.ChangeAsync(owner, goal, record, Day(Wed18), Minutes(15), null, None));
        var withSpaces = await WithStoreAsync(s => s.ChangeAsync(owner, goal, record, Day(Wed18), Minutes(18), spaces, None));
        var keptRows = await NoteRowsAsync(database.ConnectionString, goal);
        var withNote = await WithStoreAsync(s => s.ChangeAsync(owner, goal, record, Day(Wed18), Minutes(20), Note("Better today."), None));

        Assert.Null(spaces);
        Assert.Equal(Note("Back was tired."), withoutNote.Entry!.Note);
        Assert.Equal(Note("Back was tired."), withSpaces.Entry!.Note);
        Assert.Equal("Back was tired.", Assert.Single(keptRows)["Text"]);
        Assert.Equal(Note("Better today."), withNote.Entry!.Note);
        var row = Assert.Single(await NoteRowsAsync(database.ConnectionString, goal));
        Assert.Equal("Better today.", row["Text"]);
        Assert.Equal(Wed18.ToDateTime(TimeOnly.MinValue), row["EntryDate"]);
        Assert.Equal(20, Assert.Single(await RecordRowsAsync(database.ConnectionString, goal))["Amount"]);
    }

    [Fact]
    public async Task A_delete_removes_only_that_record_and_leaves_the_note()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "delete", suffix);
        var goal = await GoalAsync(database, owner, "Delete", suffix, Pages20ThreeTimesAWeek(), Mon16);
        var week = Week(Mon16);
        var first = await AddAsync(owner, goal, week, false, Wed18, RecordValue.FromAmount(10), Note("Knee was sore."));
        var second = await AddAsync(owner, goal, week, false, Thu19, RecordValue.FromAmount(20));
        var third = await AddAsync(owner, goal, week, false, Wed18, RecordValue.FromAmount(30));

        var deleted = await WithStoreAsync(s => s.DeleteAsync(owner, goal, first.RecordId!.Value, week, None));

        Assert.Equal(RecordStoreWriteOutcome.Written, deleted.Outcome);
        Assert.Null(deleted.RecordId);
        Assert.Equal([third.RecordId!.Value, second.RecordId!.Value], deleted.Entry!.Records.Select(r => r.Id));
        Assert.Equal(Note("Knee was sore."), deleted.Entry.Note);
        var rows = await RecordRowsAsync(database.ConnectionString, goal);
        Assert.Equal([third.RecordId!.Value.Value, second.RecordId!.Value.Value], rows.Select(r => (Guid)r["Id"]!));
        Assert.Single(await NoteRowsAsync(database.ConnectionString, goal));
    }

    [Fact]
    public async Task A_change_and_a_delete_of_a_record_of_another_goal_of_the_same_owner_give_RecordNotFound_and_write_nothing()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "other-goal", suffix);
        var goal = await GoalAsync(database, owner, "Other goal A", suffix, Reading20MinDaily());
        var otherGoal = await GoalAsync(database, owner, "Other goal B", suffix, Reading20MinDaily());
        await AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(12));
        var ofOtherGoal = (await AddAsync(owner, otherGoal, Day(Wed18), true, Wed18, Minutes(25))).RecordId!.Value;
        var before = await RecordRowsAsync(database.ConnectionString, otherGoal);

        var changed = await WithStoreAsync(s => s.ChangeAsync(owner, goal, ofOtherGoal, Day(Wed18), Minutes(5), Note("Wrong goal."), None));
        var deleted = await WithStoreAsync(s => s.DeleteAsync(owner, goal, ofOtherGoal, Day(Wed18), None));

        Assert.Equal(RecordStoreWriteOutcome.RecordNotFound, changed.Outcome);
        Assert.Null(changed.Entry);
        Assert.Equal(RecordStoreWriteOutcome.RecordNotFound, deleted.Outcome);
        Assert.Null(deleted.Entry);
        var after = await RecordRowsAsync(database.ConnectionString, otherGoal);
        Assert.Equal(before.Select(r => (r["Id"], r["Amount"], r["Sequence"])), after.Select(r => (r["Id"], r["Amount"], r["Sequence"])));
        Assert.Equal(12, Assert.Single(await RecordRowsAsync(database.ConnectionString, goal))["Amount"]);
        Assert.Empty(await NoteRowsAsync(database.ConnectionString, goal));
        Assert.Empty(await NoteRowsAsync(database.ConnectionString, otherGoal));
    }

    [Fact]
    public async Task Edge_case_a_goal_whose_catalog_habit_is_then_Unpublished_still_records_and_reads_its_states_as_before()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "unpublished", suffix);
        var goal = await GoalAsync(database, owner, "Unpublished", suffix, Reading20MinDaily());
        var user = ActingUser.ForAccount(owner, Role.RegisteredUser);
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero));
        await using var services = BuildServices(clock);

        var before = await InScopeAsync(services, s => s.RecordAsync(user, goal, Wed18, new RecordInput { Amount = 12 }, None));
        await Sql.ExecuteAsync(database.ConnectionString,
            "UPDATE [CatalogHabits] SET [State] = 'Unpublished' WHERE [Id] = (SELECT [CatalogHabitId] FROM [Goals] WHERE [Id] = @goal)",
            ("@goal", goal.Value));
        clock.SetUtcNow(new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero));
        var after = await InScopeAsync(services, s => s.RecordAsync(user, goal, Thu19, new RecordInput { Amount = 25 }, None));
        var recordable = await InScopeAsync(services, s => s.GetRecordableAsync(user, goal, None));

        Assert.Equal(RecordWriteOutcome.Recorded, before.Outcome);
        Assert.Equal(EvaluationState.Due, before.Entry!.State);
        Assert.Equal(RecordWriteOutcome.Recorded, after.Outcome);
        Assert.Equal(EvaluationState.Due, after.Entry!.State);
        Assert.Equal(RecordableOutcome.Found, recordable.Outcome);
        var history = recordable.History!;
        Assert.Equal(Thu19, history.CurrentDate);
        Assert.Equal([Thu19, Wed18], history.Entries.Select(e => e.FirstDate));
        Assert.Equal([EvaluationState.Due, EvaluationState.Pending], history.Entries.Select(e => e.State));
        Assert.Equal([25, 12], history.Entries.Select(e => e.Records.Single().Amount!.Value));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), history.Entries[1].Progress);
    }

    /// <summary>The application as the integration tests build it, with its own clock.</summary>
    private ServiceProvider BuildServices(FakeTimeProvider clock)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private static async Task<T> InScopeAsync<T>(IServiceProvider services, Func<RecordingService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<RecordingService>());
    }
}
