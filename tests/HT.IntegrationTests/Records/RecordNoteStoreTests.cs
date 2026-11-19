using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Records;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Records.RecordSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// The record store's notes on SQL Server (research R-05; US4, FR-007, FR-008): <c>SaveNoteAsync</c> adds, replaces and
/// deletes the one note of an entry key, and <c>ReadRecentNoteKeysAsync</c> gives the latest keys, newest first. The
/// sample goals start on Wednesday 18 November 2026 in UTC, or on Monday 16 November for a per-week goal. Shared
/// database: own prefixes (<see cref="RecordSamples.EmailPrefix"/>, <see cref="RecordSamples.HabitPrefix"/>); rows are
/// read per goal only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class RecordNoteStoreTests(DatabaseFixture database)
{
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Wed18 = Start;
    private static readonly DateOnly Thu19 = Start.AddDays(1);
    private static readonly DateOnly Mon23 = new(2026, 11, 23);

    private static CancellationToken None => CancellationToken.None;

    private async Task<T> WithStoreAsync<T>(Func<IRecordStore, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<IRecordStore>());
    }

    private Task<RecordStoreWrite> SaveNoteAsync(AccountId owner, GoalId goal, EntrySpan entry, NoteText? note) =>
        WithStoreAsync(s => s.SaveNoteAsync(owner, goal, entry, note, None));

    private static NoteText Note(string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        return note!;
    }

    private static DateTime Column(DateOnly date) => date.ToDateTime(TimeOnly.MinValue);

    [Fact]
    public async Task SaveNoteAsync_adds_then_replaces_the_one_note_of_an_entry_and_gives_the_entry_with_its_records()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "note-upsert", suffix);
        var goal = await GoalAsync(database, owner, "Note upsert", suffix, Reading20MinDaily());
        await WithStoreAsync(s => s.AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(12), null, RecordLimits.MaxRecordsPerPeriod, None));

        var added = await SaveNoteAsync(owner, goal, Day(Wed18), Note("Fell asleep early."));
        var addedRows = await NoteRowsAsync(database.ConnectionString, goal);
        var replaced = await SaveNoteAsync(owner, goal, Day(Wed18), Note("Away for the weekend."));

        Assert.Equal(RecordStoreWriteOutcome.Written, added.Outcome);
        Assert.Equal(Note("Fell asleep early."), added.Entry!.Note);
        Assert.Equal(12, Assert.Single(added.Entry.Records).Value.Amount);
        Assert.Equal("Fell asleep early.", Assert.Single(addedRows)["Text"]);
        Assert.Equal(RecordStoreWriteOutcome.Written, replaced.Outcome);
        Assert.Equal(Note("Away for the weekend."), replaced.Entry!.Note);
        var row = Assert.Single(await NoteRowsAsync(database.ConnectionString, goal));
        Assert.Equal("Away for the weekend.", row["Text"]);
        Assert.Equal(Column(Wed18), row["EntryDate"]);
        Assert.Single(await RecordRowsAsync(database.ConnectionString, goal));
    }

    [Fact]
    public async Task SaveNoteAsync_without_a_note_deletes_the_entrys_note_and_leaves_the_others_and_the_records()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "note-delete", suffix);
        var goal = await GoalAsync(database, owner, "Note delete", suffix, Reading20MinDaily());
        await WithStoreAsync(s => s.AddAsync(owner, goal, Day(Wed18), true, Wed18, Minutes(20), Note("Back was tired."), RecordLimits.MaxRecordsPerPeriod, None));
        await SaveNoteAsync(owner, goal, Day(Thu19), Note("Long meeting."));

        var deleted = await SaveNoteAsync(owner, goal, Day(Wed18), null);
        var again = await SaveNoteAsync(owner, goal, Day(Wed18), null);

        Assert.Equal(RecordStoreWriteOutcome.Written, deleted.Outcome);
        Assert.Null(deleted.Entry!.Note);
        Assert.Single(deleted.Entry.Records);
        Assert.Equal(RecordStoreWriteOutcome.Written, again.Outcome);
        Assert.Null(again.Entry!.Note);
        var row = Assert.Single(await NoteRowsAsync(database.ConnectionString, goal));
        Assert.Equal((Column(Thu19), "Long meeting."), ((DateTime)row["EntryDate"]!, (string)row["Text"]!));
        Assert.Single(await RecordRowsAsync(database.ConnectionString, goal));
    }

    [Fact]
    public async Task A_weeks_notes_from_any_of_its_dates_are_one_note_under_its_Monday()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "note-week", suffix);
        var goal = await GoalAsync(database, owner, "Note week", suffix, Pages20ThreeTimesAWeek(), Mon16);

        await SaveNoteAsync(owner, goal, Week(Mon16), Note("Knee was sore."));
        var second = await SaveNoteAsync(owner, goal, Week(Mon16), Note("Better today."));

        Assert.Equal(Note("Better today."), second.Entry!.Note);
        var row = Assert.Single(await NoteRowsAsync(database.ConnectionString, goal));
        Assert.Equal((Column(Mon16), "Better today."), ((DateTime)row["EntryDate"]!, (string)row["Text"]!));
        var read = await WithStoreAsync(s => s.ReadAsync(owner, goal, Mon16, Mon16.AddDays(6), None));
        Assert.Equal(Note("Better today."), Assert.Single(read.Notes, n => n.Key == Mon16).Value);
    }

    [Fact]
    public async Task SaveNoteAsync_on_another_owners_goal_gives_GoalNotFound_and_writes_nothing()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "note-owner", suffix);
        var other = await OwnerAsync(database, "note-other", suffix);
        var goal = await GoalAsync(database, owner, "Note owner", suffix, Reading20MinDaily());
        await SaveNoteAsync(owner, goal, Day(Wed18), Note("Mine."));

        var replaced = await SaveNoteAsync(other, goal, Day(Wed18), Note("Not yours."));
        var deleted = await SaveNoteAsync(other, goal, Day(Wed18), null);

        Assert.Equal(RecordStoreWriteOutcome.GoalNotFound, replaced.Outcome);
        Assert.Null(replaced.Entry);
        Assert.Equal(RecordStoreWriteOutcome.GoalNotFound, deleted.Outcome);
        Assert.Null(deleted.Entry);
        Assert.Equal("Mine.", Assert.Single(await NoteRowsAsync(database.ConnectionString, goal))["Text"]);
    }

    [Fact]
    public async Task ReadRecentNoteKeysAsync_gives_the_latest_keys_newest_first()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "note-recent", suffix);
        var goal = await GoalAsync(database, owner, "Note recent", suffix, Reading20MinDaily());
        var days = Enumerable.Range(0, 7).Select(i => Wed18.AddDays(i)).ToList();
        foreach (var day in new[] { days[3], days[0], days[6], days[1], days[5], days[2], days[4] })
        {
            await SaveNoteAsync(owner, goal, Day(day), Note("A note."));
        }

        var five = await WithStoreAsync(s => s.ReadRecentNoteKeysAsync(owner, goal, 5, None));
        var all = await WithStoreAsync(s => s.ReadRecentNoteKeysAsync(owner, goal, 10, None));

        Assert.Equal([days[6], days[5], days[4], days[3], days[2]], five);
        Assert.Equal(Enumerable.Reverse(days), all);
    }

    [Fact]
    public async Task ReadRecentNoteKeysAsync_gives_a_weeks_Monday_and_nothing_for_another_owner_or_another_goal()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "note-keys", suffix);
        var other = await OwnerAsync(database, "note-keys-other", suffix);
        var goal = await GoalAsync(database, owner, "Note keys", suffix, Pages20ThreeTimesAWeek(), Mon16);
        var empty = await GoalAsync(database, owner, "Note keys empty", suffix, Pages20ThreeTimesAWeek(), Mon16);
        await SaveNoteAsync(owner, goal, Week(Mon16), Note("Knee was sore."));
        await SaveNoteAsync(owner, goal, Week(Mon23), Note("Better today."));

        Assert.Equal([Mon23, Mon16], await WithStoreAsync(s => s.ReadRecentNoteKeysAsync(owner, goal, 5, None)));
        Assert.Empty(await WithStoreAsync(s => s.ReadRecentNoteKeysAsync(other, goal, 5, None)));
        Assert.Empty(await WithStoreAsync(s => s.ReadRecentNoteKeysAsync(owner, empty, 5, None)));
    }
}
