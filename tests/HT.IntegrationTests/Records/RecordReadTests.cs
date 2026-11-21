using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Records;
using HT.Persistence;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Records.RecordSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// Reading records on SQL Server (US5, FR-009; research R-05, R-07): <see cref="IRecordStore.ReadAsync"/> gives the
/// records dated in the range and the notes keyed in it, in date and then stored order; a week read from its Monday to
/// its Sunday gets its note and all its records; and <see cref="RecordingService.GetHistoryAsync"/> over the real store
/// gives the same entries as over <see cref="FakeRecordStore"/> with the same records. The goals are in UTC and start on
/// Monday 16 November 2026. Shared database: own prefixes (<see cref="RecordSamples.EmailPrefix"/>,
/// <see cref="RecordSamples.HabitPrefix"/>); rows are read per goal only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class RecordReadTests(DatabaseFixture database)
{
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Tue17 = new(2026, 11, 17);
    private static readonly DateOnly Wed18 = new(2026, 11, 18);
    private static readonly DateOnly Thu19 = new(2026, 11, 19);
    private static readonly DateOnly Sun22 = new(2026, 11, 22);
    private static readonly DateOnly Mon23 = new(2026, 11, 23);
    private static readonly DateOnly Tue24 = new(2026, 11, 24);

    private static CancellationToken None => CancellationToken.None;

    private async Task<T> WithStoreAsync<T>(Func<IRecordStore, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<IRecordStore>());
    }

    private Task<RecordStoreWrite> AddAsync(AccountId owner, GoalId goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value, NoteText? note = null) =>
        WithStoreAsync(s => s.AddAsync(
            owner, goal, entry, isCommitment, date, value, note is null ? NoteWrite.Keep : NoteWrite.Replace(note), RecordLimits.MaxRecordsPerPeriod, None));

    private static NoteText Note(string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        return note!;
    }

    private static RecordValue Amount(int amount) => RecordValue.FromAmount(amount);

    [Fact]
    public async Task ReadAsync_gives_the_records_dated_in_the_range_and_the_notes_keyed_in_it_in_date_and_stored_order()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "read-range", suffix);
        var goal = await GoalAsync(database, owner, "Read range", suffix, CoffeeAtMost2Cups(), Mon16);
        await AddAsync(owner, goal, Day(Mon16), false, Mon16, Amount(1), Note("Before the range."));
        await AddAsync(owner, goal, Day(Wed18), false, Wed18, Amount(1));
        var tuesdayFirst = await AddAsync(owner, goal, Day(Tue17), false, Tue17, Amount(2), Note("Long meeting."));
        var tuesdaySecond = await AddAsync(owner, goal, Day(Tue17), false, Tue17, Amount(0));
        await AddAsync(owner, goal, Day(Thu19), false, Thu19, Amount(3), Note("After the range."));

        var read = await WithStoreAsync(s => s.ReadAsync(owner, goal, Tue17, Wed18, None));

        Assert.Equal([(Tue17, 2), (Tue17, 0), (Wed18, 1)], read.Records.Select(r => (r.Date, r.Value.Amount!.Value)));
        Assert.Equal([tuesdayFirst.RecordId!.Value, tuesdaySecond.RecordId!.Value], read.Records.Take(2).Select(r => r.Id));
        var note = Assert.Single(read.Notes);
        Assert.Equal((Tue17, Note("Long meeting.")), (note.Key, note.Value));
    }

    [Fact]
    public async Task ReadAsync_of_a_week_from_its_Monday_to_its_Sunday_gets_its_note_and_all_its_records()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "read-week", suffix);
        var goal = await GoalAsync(database, owner, "Read week", suffix, Pages20ThreeTimesAWeek(), Mon16);
        await AddAsync(owner, goal, Week(Mon16), false, Sun22, Amount(5));
        await AddAsync(owner, goal, Week(Mon16), false, Mon16, Amount(20));
        await AddAsync(owner, goal, Week(Mon16), false, Thu19, Amount(25), Note("Knee was sore."));
        await AddAsync(owner, goal, Week(Mon23), false, Mon23, Amount(30), Note("Next week."));

        var read = await WithStoreAsync(s => s.ReadAsync(owner, goal, Mon16, Sun22, None));

        Assert.Equal([(Mon16, 20), (Thu19, 25), (Sun22, 5)], read.Records.Select(r => (r.Date, r.Value.Amount!.Value)));
        var note = Assert.Single(read.Notes);
        Assert.Equal((Mon16, Note("Knee was sore.")), (note.Key, note.Value));
    }

    [Fact]
    public async Task GetHistoryAsync_over_the_real_store_gives_the_same_entries_as_over_the_fake_for_weeks_the_range_covers_in_part()
    {
        // The range Thursday 19 to Tuesday 24 November covers the weeks of 16 and 23 November only in part. At 10:00Z on
        // Wednesday 25 the first week is final (its grace period ended at 00:00Z that day): 2 of 3 records reach 20 pages,
        // so it is Failed. The second is Due.
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "history-week", suffix);
        var goal = await GoalAsync(database, owner, "History week", suffix, Pages20ThreeTimesAWeek(), Mon16);
        (EntrySpan Entry, DateOnly Date, RecordValue Value)[] records =
        [
            (Week(Mon16), Mon16, Amount(20)),
            (Week(Mon16), Wed18, Amount(10)),
            (Week(Mon16), Wed18, Amount(25)),
            (Week(Mon23), Tue24, Amount(30)),
        ];
        (DateOnly Key, NoteText Note)[] notes = [(Mon16, Note("Knee was sore."))];

        var (real, fake) = await HistoriesAsync(owner, goal, false, records, notes, Thu19, Tue24, new DateTimeOffset(2026, 11, 25, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal([(Mon16, Sun22), (Mon23, new DateOnly(2026, 11, 29))], real.Entries.Select(e => (e.FirstDate, e.LastDate)));
        Assert.Equal([EvaluationState.Failed, EvaluationState.Due], real.Entries.Select(e => e.State));
        Assert.Equal([Mon16, Wed18, Wed18], real.Entries[0].Records.Select(r => r.Date));
        Assert.Equal("Knee was sore.", real.Entries[0].Note);
        Assert.Equal(Comparable(fake), Comparable(real));
    }

    [Fact]
    public async Task GetHistoryAsync_over_the_real_store_gives_the_same_entries_as_over_the_fake_for_commitments()
    {
        // At 10:00Z on Thursday 19 November: Monday 16 is final (Met at 25 of 20), Tuesday 17 is Pending at 12 of 20,
        // Wednesday 18 Pending without a record, Thursday 19 Due.
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "history-day", suffix);
        var goal = await GoalAsync(database, owner, "History day", suffix, Reading20MinDaily(), Mon16);
        (EntrySpan Entry, DateOnly Date, RecordValue Value)[] records =
        [
            (Day(Mon16), Mon16, Minutes(25)),
            (Day(Tue17), Tue17, Minutes(12)),
        ];
        (DateOnly Key, NoteText Note)[] notes = [(Tue17, Note("Fell asleep early.")), (Wed18, Note("Away for the weekend."))];

        var (real, fake) = await HistoriesAsync(owner, goal, true, records, notes, Mon16, Thu19, new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal(
            [EvaluationState.Met, EvaluationState.Pending, EvaluationState.Pending, EvaluationState.Due],
            real.Entries.Select(e => e.State));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), real.Entries[1].Progress);
        Assert.Equal(Comparable(fake), Comparable(real));
    }

    /// <summary>
    /// Writes <paramref name="records"/> and <paramref name="notes"/> to the real store and seeds the same into a
    /// <see cref="FakeRecordStore"/>, then reads the history at <paramref name="now"/> through a
    /// <see cref="RecordingService"/> over each, with the real goal and account stores for both.
    /// </summary>
    private async Task<(GoalHistory Real, GoalHistory Fake)> HistoriesAsync(
        AccountId owner,
        GoalId goal,
        bool isCommitment,
        IReadOnlyList<(EntrySpan Entry, DateOnly Date, RecordValue Value)> records,
        IReadOnlyList<(DateOnly Key, NoteText Note)> notes,
        DateOnly first,
        DateOnly last,
        DateTimeOffset now)
    {
        var fakeGoals = new FakeGoalStore(new FakeAccountStore(), new FakeCatalogHabitStore(), new FakeCustomHabitStore());
        var fakeRecords = new FakeRecordStore(fakeGoals);
        foreach (var (entry, date, value) in records)
        {
            Assert.Equal(RecordStoreWriteOutcome.Written, (await AddAsync(owner, goal, entry, isCommitment, date, value)).Outcome);
            fakeRecords.Seed(goal, date, value);
        }

        foreach (var (key, note) in notes)
        {
            var entry = records.Select(r => r.Entry).FirstOrDefault(e => e.Key == key) ?? new EntrySpan(key, key, key);
            Assert.Equal(RecordStoreWriteOutcome.Written, (await WithStoreAsync(s => s.SaveNoteAsync(owner, goal, entry, note, None))).Outcome);
            fakeRecords.SeedNote(goal, key, note);
        }

        var user = ActingUser.ForAccount(owner, Role.RegisteredUser);
        var clock = new FakeTimeProvider(now);
        await using var services = BuildServices(clock);
        await using var scope = services.CreateAsyncScope();
        var goals = scope.ServiceProvider.GetRequiredService<IGoalStore>();
        var stored = await goals.FindAsync(owner, goal, None);
        fakeGoals.Add(owner, stored!.Goal, stored.Habit);

        var real = await scope.ServiceProvider.GetRequiredService<RecordingService>().GetHistoryAsync(user, goal, first, last, None);
        var fake = await new RecordingService(fakeRecords, goals, scope.ServiceProvider.GetRequiredService<IAccountStore>(), clock)
            .GetHistoryAsync(user, goal, first, last, None);

        Assert.Equal(HistoryOutcome.Found, real.Outcome);
        Assert.Equal(HistoryOutcome.Found, fake.Outcome);
        Assert.Equal([nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.ReadAsync)], fakeRecords.Calls);
        return (real.History!, fake.History!);
    }

    /// <summary>The entries without the record identifiers, which differ between the stores.</summary>
    private static List<string> Comparable(GoalHistory history) =>
        history.Entries
            .Select(e => string.Join(
                " | ",
                e.FirstDate,
                e.LastDate,
                e.Starts.ToString("O"),
                e.Ends.ToString("O"),
                e.State,
                e.GraceEnds?.ToString("O"),
                e.Progress,
                e.Note,
                e.IsCorrection,
                string.Join(", ", e.Records.Select(r => $"{r.Date:O} {r.Answer} {r.Amount}"))))
            .ToList();

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
}
