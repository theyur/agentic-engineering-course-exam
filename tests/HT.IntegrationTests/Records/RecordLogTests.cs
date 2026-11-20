using System.Globalization;
using System.Text.RegularExpressions;
using HT.Application;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// HP-006, SC-006 (constitution III, research R-14): every <see cref="RecordingService"/> use case and every
/// <see cref="IRecordStore"/> member runs on SQL Server with every log entry captured at <see cref="LogLevel.Trace"/>,
/// successes, refusals and faults included, and no captured message or exception text, no thrown exception and no
/// result text contains a note, a recorded value, a unit, a goal name or a habit name. Each of those carries a unique
/// marker. The faults are forced without test hooks in production code, as slice 4's log and fault tests force them:
/// a store decorator that gives an outcome a use case does not have; rows arranged with plain SQL that the code must
/// refuse to read (a record or a note outside every entry, an amount out of range); a pre-cancelled token; and SQL
/// errors raised by test triggers on this test's goal only, whose error text carries the value written, so that a
/// database error that names a value is shown not to reach a log or an exception (the triggers are dropped afterwards).
/// The goals are in UTC and start on Monday 16 November 2026, created at 08:00Z that day on a clock of this class; the
/// calls are made on Wednesday 18 November at 10:00Z. Shared database: own prefixes ("it-rl-", "It-Rl").
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class RecordLogTests(DatabaseFixture database)
{
    private const string GoalMarker = "Zqgoalmark";
    private const string CustomHabitMarker = "Zqcustommark";
    private const string CatalogHabitMarker = "Zqcatalogmark";
    private const string UnitMarker = "zqunitmark";
    private const string NoteMarker = "Zqnotemark";

    /// <summary>The recorded values: durations, counts, out-of-range inputs and an out-of-range stored amount.</summary>
    private const int Duration = 1373;
    private const int ChangedDuration = 1291;
    private const int Count = 7311;
    private const int CountTooHigh = 98765;
    private const int DurationTooHigh = 4321;
    private const int StoredOutOfRange = 86420;

    private static readonly int[] Values = [Duration, ChangedDuration, Count, CountTooHigh, DurationTooHigh, StoredOutOfRange];
    private static readonly string[] TextMarkers = [GoalMarker, CustomHabitMarker, CatalogHabitMarker, UnitMarker, NoteMarker];

    private static readonly DateOnly Sun15 = new(2026, 11, 15);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Tue17 = new(2026, 11, 17);
    private static readonly DateOnly Wed18 = new(2026, 11, 18);
    private static readonly DateOnly Thu19 = new(2026, 11, 19);
    private static readonly DateOnly Sun22 = new(2026, 11, 22);

    /// <summary>Monday 16 November 2026, 08:00Z: the goals are created, starting that day.</summary>
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 16, 8, 0, 0, TimeSpan.Zero);

    /// <summary>Wednesday 18 November 2026, 10:00Z: Monday 16 to Wednesday 18 have begun, Thursday 19 has not.</summary>
    private static readonly DateTimeOffset CalledAt = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static CancellationToken None => CancellationToken.None;

    private static string Note(string what) => $"{NoteMarker} {what}";

    private static RecordInput Minutes(int minutes, string? note = null) => new() { Amount = minutes, Note = note };

    private static NoteText NoteOf(string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        return note!;
    }

    /// <summary>A record write's <see cref="NoteWrite.Replace"/> with the marked note <paramref name="what"/>.</summary>
    private static NoteWrite ReplaceWith(string what) => NoteWrite.Replace(NoteOf(Note(what)));

    /// <summary>
    /// The application as the integration tests build it, with <paramref name="clock"/>, only <paramref name="capture"/>
    /// as its logger, and the real record store behind a decorator that gives a wrong outcome for the member
    /// <paramref name="faults"/> names.
    /// </summary>
    private ServiceProvider BuildServices(LogCapture capture, FakeTimeProvider clock, FaultSwitch faults)
    {
        var services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddProvider(capture);
            })
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        var real = services.Last(d => d.ServiceType == typeof(IRecordStore));
        services.Remove(real);
        services.AddSingleton(faults);
        services.AddScoped<IRecordStore>(provider => new FaultingRecordStore(
            (IRecordStore)ActivatorUtilities.CreateInstance(provider, real.ImplementationType!),
            provider.GetRequiredService<FaultSwitch>()));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
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

    /// <summary>
    /// The owner, another registered user, and three goals of the owner, created at <see cref="CreatedAt"/> through
    /// <see cref="GoalService"/>, after which the clock is moved to <see cref="CalledAt"/>: a daily goal of at least 20 min
    /// with its own name on a custom habit, a 3-times-a-week goal of at least 20 units with its own name on a catalog
    /// habit, and a yes/no goal on Monday, Wednesday and Friday with the derived name on the same catalog habit. Every
    /// name and the unit carry a marker.
    /// </summary>
    private async Task<World> ArrangeAsync(IServiceProvider services, FakeTimeProvider clock, string who)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rl-{who}-{suffix}@example.com");
        var other = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rl-{who}-other-{suffix}@example.com");
        var custom = await CustomHabitAsync(services, user.AccountId!.Value, $"It-Rl {CustomHabitMarker} {suffix}");
        var catalog = await CatalogHabitAsync(services, $"It-Rl {CatalogHabitMarker} {suffix}");
        var daily = await CreateGoalAsync(services, user, new GoalHabit(custom, HabitKind.Custom), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, $"It-Rl {GoalMarker} daily {suffix}");
        var weekly = await CreateGoalAsync(services, user, new GoalHabit(catalog, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 20, CountUnit = UnitMarker,
            ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, $"It-Rl {GoalMarker} weekly {suffix}");
        var yesNo = await CreateGoalAsync(services, user, new GoalHabit(catalog, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
        }, null);
        clock.SetUtcNow(CalledAt);
        return new World(user, other, daily, weekly, yesNo);
    }

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, GoalHabit habit, GoalTargetFields target, string? name)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = habit, Target = target, StartDate = Mon16, Name = name }, None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    /// <summary>
    /// Fails when a captured entry, a thrown exception or a result text contains a marker or a value. A value counts as a
    /// whole number that is not part of a GUID, a stack trace's line number or a duration in milliseconds, so that a
    /// GUID or a timing that happens to hold its digits is not taken for it. The failure names no value.
    /// </summary>
    private static void AssertNoProtectedValue(LogCapture capture, IEnumerable<Exception> exceptions, IEnumerable<string?> results)
    {
        Assert.NotEmpty(capture.Entries);
        capture.AssertContainsNone(TextMarkers);
        foreach (var entry in capture.Entries)
        {
            Assert.False(HasValue(entry.Message) || HasValue(entry.Exception), $"A log entry of category {entry.Category} at level {entry.Level} contains a recorded value.");
        }

        foreach (var exception in exceptions)
        {
            Assert.False(HasProtected(exception.ToString()), $"A thrown {exception.GetType().Name} contains a protected value.");
        }

        foreach (var text in results)
        {
            Assert.False(HasProtected(text), "A result text contains a protected value.");
        }
    }

    private static bool HasProtected(string? text) =>
        text is not null && (TextMarkers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase)) || HasValue(text));

    private static bool HasValue(string? text) =>
        text is not null && Values.Any(v => ValuePattern(v).IsMatch(text));

    private static Regex ValuePattern(int value) =>
        new($"(?<!:line )(?<![0-9A-Fa-f-]){value.ToString(CultureInfo.InvariantCulture)}(?![0-9A-Fa-f-])(?!ms)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    /// <summary>Stores a record row by plain SQL, with no check, and gives its identifier.</summary>
    private async Task<RecordId> InsertRecordAsync(GoalId goal, DateOnly date, int amount)
    {
        var id = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalRecords] ([Id], [GoalId], [Date], [Answer], [Amount]) VALUES (@id, @goal, @date, NULL, @amount)",
            ("@id", id), ("@goal", goal.Value), ("@date", date.ToDateTime(TimeOnly.MinValue)), ("@amount", amount));
        return new RecordId(id);
    }

    /// <summary>Stores a note row by plain SQL, with no check; gives the number of rows written.</summary>
    private Task<int> InsertNoteAsync(GoalId goal, DateOnly key, string text) =>
        Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalNotes] ([GoalId], [EntryDate], [Text]) VALUES (@goal, @date, @text)",
            ("@goal", goal.Value), ("@date", key.ToDateTime(TimeOnly.MinValue)), ("@text", text));

    [Fact]
    public async Task HP_006_no_use_case_or_refusal_logs_or_returns_a_note_value_unit_goal_name_or_habit_name()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(CreatedAt);
        await using var services = BuildServices(capture, clock, new FaultSwitch());
        var world = await ArrangeAsync(services, clock, "use-cases");
        var owner = world.User;
        var results = new List<string?>();

        // Successes: records with notes, a change, notes saved, replaced and removed, the readings, and a delete.
        var daily = await RecordingAsync(services, s => s.RecordAsync(owner, world.Daily, Mon16, Minutes(Duration, Note("daily")), None));
        var weekly = await RecordingAsync(services, s => s.RecordAsync(owner, world.Weekly, Mon16, Minutes(Count, Note("weekly")), None));
        var yes = await RecordingAsync(services, s => s.RecordAsync(owner, world.YesNo, Mon16, new RecordInput { Answer = YesNoAnswer.Yes, Note = Note("yes") }, None));
        Assert.Equal([RecordWriteOutcome.Recorded, RecordWriteOutcome.Recorded, RecordWriteOutcome.Recorded], new[] { daily.Outcome, weekly.Outcome, yes.Outcome });
        var dailyRecord = daily.RecordId!.Value;
        var weeklyRecord = weekly.RecordId!.Value;
        var changed = await RecordingAsync(services, s => s.ChangeRecordAsync(owner, world.Daily, dailyRecord, Minutes(ChangedDuration, Note("changed")), None));
        Assert.Equal(RecordWriteOutcome.Changed, changed.Outcome);
        foreach (var text in new[] { Note("saved"), Note("replaced"), "   " })
        {
            var saved = await RecordingAsync(services, s => s.SaveNoteAsync(owner, world.Daily, Tue17, text, None));
            Assert.Equal(NoteSaveOutcome.Saved, saved.Outcome);
        }

        foreach (var goal in new[] { world.Daily, world.Weekly, world.YesNo })
        {
            Assert.Equal(HistoryOutcome.Found, (await RecordingAsync(services, s => s.GetHistoryAsync(owner, goal, Mon16, Sun22, None))).Outcome);
            Assert.Equal(RecordableOutcome.Found, (await RecordingAsync(services, s => s.GetRecordableAsync(owner, goal, None))).Outcome);
            var recent = await RecordingAsync(services, s => s.GetRecentNotesAsync(owner, goal, None));
            Assert.Equal(RecentNotesOutcome.Found, recent.Outcome);
            Assert.NotEmpty(recent.Notes);
        }

        // The refusals of the acting user (FR-005 steps 1 to 3, FR-017), each on every use case with values that carry markers.
        var actingUsers = new[]
        {
            ActingUser.Anonymous,
            ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator),
            ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser),
            world.Other,
        };
        foreach (var actingUser in actingUsers)
        {
            results.Add((await RecordingAsync(services, s => s.RecordAsync(actingUser, world.Daily, Tue17, Minutes(Duration, Note("refused")), None))).Message);
            results.Add((await RecordingAsync(services, s => s.ChangeRecordAsync(actingUser, world.Daily, dailyRecord, Minutes(Duration, Note("refused")), None))).Message);
            results.Add((await RecordingAsync(services, s => s.DeleteRecordAsync(actingUser, world.Daily, dailyRecord, None))).Message);
            results.Add((await RecordingAsync(services, s => s.SaveNoteAsync(actingUser, world.Daily, Tue17, Note("refused"), None))).Message);
            results.Add((await RecordingAsync(services, s => s.GetHistoryAsync(actingUser, world.Weekly, Mon16, Sun22, None))).Message);
            results.Add((await RecordingAsync(services, s => s.GetRecordableAsync(actingUser, world.Weekly, None))).Message);
            results.Add((await RecordingAsync(services, s => s.GetRecentNotesAsync(actingUser, world.Weekly, None))).Message);
        }

        // Every later refusal of the slice, each with values that carry markers.
        var unknownGoal = new GoalId(Guid.NewGuid());
        var refusals = new List<(Enum Expected, Enum Outcome, string? Message)>();
        async Task WriteAsync(RecordWriteOutcome expected, Func<RecordingService, Task<RecordWriteResult>> call)
        {
            var result = await RecordingAsync(services, call);
            refusals.Add((expected, result.Outcome, result.Message));
        }

        await WriteAsync(RecordWriteOutcome.NotFound, s => s.RecordAsync(owner, unknownGoal, Tue17, Minutes(Duration, Note("unknown goal")), None));
        await WriteAsync(RecordWriteOutcome.NothingScheduled, s => s.RecordAsync(owner, world.Daily, Sun15, Minutes(Duration, Note("before the start")), None));
        await WriteAsync(RecordWriteOutcome.NothingScheduled, s => s.RecordAsync(owner, world.YesNo, Tue17, new RecordInput { Answer = YesNoAnswer.No, Note = Note("unscheduled") }, None));
        await WriteAsync(RecordWriteOutcome.DayNotBegun, s => s.RecordAsync(owner, world.Daily, Thu19, Minutes(Duration, Note("not begun")), None));
        await WriteAsync(RecordWriteOutcome.AnswerMissing, s => s.RecordAsync(owner, world.YesNo, Wed18, Minutes(Count, Note("no answer")), None));
        await WriteAsync(RecordWriteOutcome.CountOutOfRange, s => s.RecordAsync(owner, world.Weekly, Tue17, Minutes(CountTooHigh, Note("too many")), None));
        await WriteAsync(RecordWriteOutcome.DurationOutOfRange, s => s.RecordAsync(owner, world.Daily, Tue17, Minutes(DurationTooHigh, Note("too long")), None));
        await WriteAsync(RecordWriteOutcome.NoteTooLong, s => s.RecordAsync(owner, world.Daily, Tue17, Minutes(Duration, Note(new string('x', 500))), None));
        await WriteAsync(RecordWriteOutcome.RecordNotFound, s => s.ChangeRecordAsync(owner, world.Weekly, dailyRecord, Minutes(Count, Note("other goal")), None));
        await WriteAsync(RecordWriteOutcome.RecordNotFound, s => s.DeleteRecordAsync(owner, world.Daily, new RecordId(Guid.NewGuid()), None));
        await WriteAsync(RecordWriteOutcome.CountOutOfRange, s => s.ChangeRecordAsync(owner, world.Weekly, weeklyRecord, Minutes(CountTooHigh, Note("too many")), None));
        await WriteAsync(RecordWriteOutcome.NoteTooLong, s => s.ChangeRecordAsync(owner, world.Weekly, weeklyRecord, Minutes(Count, Note(new string('x', 500))), None));

        // A week that already has 50 records.
        await Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalRecords] ([Id], [GoalId], [Date], [Answer], [Amount]) SELECT TOP (49) NEWID(), @goal, @date, NULL, 1 FROM sys.all_objects",
            ("@goal", world.Weekly.Value), ("@date", Tue17.ToDateTime(TimeOnly.MinValue)));
        await WriteAsync(RecordWriteOutcome.RecordLimitReached, s => s.RecordAsync(owner, world.Weekly, Wed18, Minutes(Count, Note("one too many")), None));

        var notes = new[]
        {
            await RecordingAsync(services, s => s.SaveNoteAsync(owner, unknownGoal, Tue17, Note("unknown goal"), None)),
            await RecordingAsync(services, s => s.SaveNoteAsync(owner, world.Daily, Sun15, Note("before the start"), None)),
            await RecordingAsync(services, s => s.SaveNoteAsync(owner, world.Daily, Thu19, Note("not begun"), None)),
            await RecordingAsync(services, s => s.SaveNoteAsync(owner, world.Daily, Tue17, Note(new string('x', 500)), None)),
        };
        Assert.Equal(
            [NoteSaveOutcome.NotFound, NoteSaveOutcome.NothingScheduled, NoteSaveOutcome.DayNotBegun, NoteSaveOutcome.NoteTooLong],
            notes.Select(n => n.Outcome));
        var outOfRange = await RecordingAsync(services, s => s.GetHistoryAsync(owner, world.Daily, Mon16, Mon16.AddDays(366), None));
        Assert.Equal(HistoryOutcome.RangeOutOfBounds, outOfRange.Outcome);

        var deleted = await RecordingAsync(services, s => s.DeleteRecordAsync(owner, world.Weekly, weeklyRecord, None));
        Assert.Equal(RecordWriteOutcome.Deleted, deleted.Outcome);

        Assert.All(refusals, r => Assert.Equal(r.Expected, r.Outcome));
        results.AddRange(refusals.Select(r => r.Message));
        results.AddRange(notes.Select(n => n.Message));
        results.Add(outOfRange.Message);
        Assert.All(results, message => Assert.NotNull(message));
        AssertNoProtectedValue(capture, [], results);
    }

    [Fact]
    public async Task HP_006_no_store_member_logs_a_note_or_value_when_it_writes_reads_refuses_or_is_cancelled()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(CreatedAt);
        await using var services = BuildServices(capture, clock, new FaultSwitch());
        var world = await ArrangeAsync(services, clock, "store");
        var owner = world.User.AccountId!.Value;
        var other = world.Other.AccountId!.Value;
        var day = new EntrySpan(Mon16, Mon16, Mon16);
        var week = new EntrySpan(Mon16, Mon16, Sun22);
        var exceptions = new List<Exception>();

        // The owner: every member writes or reads.
        var added = await StoreAsync(services, s => s.AddAsync(owner, world.Daily, day, true, Mon16, RecordValue.FromAmount(Duration), ReplaceWith("added"), 50, None));
        var addedToWeek = await StoreAsync(services, s => s.AddAsync(owner, world.Weekly, week, false, Tue17, RecordValue.FromAmount(Count), ReplaceWith("week"), 50, None));
        Assert.Equal(RecordStoreWriteOutcome.Written, added.Outcome);
        Assert.Equal(RecordStoreWriteOutcome.Written, addedToWeek.Outcome);
        var record = added.RecordId!.Value;
        Assert.Equal(Mon16, await StoreAsync(services, s => s.FindRecordDateAsync(owner, world.Daily, record, None)));
        Assert.Equal(RecordStoreWriteOutcome.Written, (await StoreAsync(services, s => s.ChangeAsync(owner, world.Daily, record, day, RecordValue.FromAmount(ChangedDuration), ReplaceWith("changed"), None))).Outcome);
        Assert.Equal(RecordStoreWriteOutcome.Written, (await StoreAsync(services, s => s.SaveNoteAsync(owner, world.Daily, day, NoteOf(Note("saved")), None))).Outcome);
        var removed = await StoreAsync(services, s => s.ChangeAsync(owner, world.Daily, record, day, RecordValue.FromAmount(Duration), NoteWrite.Remove, None));
        Assert.Equal(RecordStoreWriteOutcome.Written, removed.Outcome);
        Assert.Null(removed.Entry!.Note);
        Assert.NotEmpty((await StoreAsync(services, s => s.ReadAsync(owner, world.Daily, Mon16, Sun22, None))).Records);
        Assert.NotEmpty(await StoreAsync(services, s => s.ReadRecentNoteKeysAsync(owner, world.Weekly, 5, None)));

        // Another owner: every member gives nothing.
        Assert.Equal(RecordStoreWriteOutcome.GoalNotFound, (await StoreAsync(services, s => s.AddAsync(other, world.Daily, day, true, Mon16, RecordValue.FromAmount(Duration), ReplaceWith("other"), 50, None))).Outcome);
        Assert.Null(await StoreAsync(services, s => s.FindRecordDateAsync(other, world.Daily, record, None)));
        Assert.Equal(RecordStoreWriteOutcome.GoalNotFound, (await StoreAsync(services, s => s.ChangeAsync(other, world.Daily, record, day, RecordValue.FromAmount(Duration), ReplaceWith("other"), None))).Outcome);
        Assert.Equal(RecordStoreWriteOutcome.GoalNotFound, (await StoreAsync(services, s => s.DeleteAsync(other, world.Daily, record, day, None))).Outcome);
        Assert.Equal(RecordStoreWriteOutcome.GoalNotFound, (await StoreAsync(services, s => s.SaveNoteAsync(other, world.Daily, day, NoteOf(Note("other")), None))).Outcome);
        Assert.Empty((await StoreAsync(services, s => s.ReadAsync(other, world.Daily, Mon16, Sun22, None))).Records);
        Assert.Empty(await StoreAsync(services, s => s.ReadRecentNoteKeysAsync(other, world.Daily, 5, None)));

        // The owner's own refusals: a record of another goal, and a period at its limit.
        Assert.Equal(RecordStoreWriteOutcome.RecordNotFound, (await StoreAsync(services, s => s.ChangeAsync(owner, world.Weekly, record, week, RecordValue.FromAmount(Count), ReplaceWith("wrong goal"), None))).Outcome);
        Assert.Equal(RecordStoreWriteOutcome.RecordNotFound, (await StoreAsync(services, s => s.DeleteAsync(owner, world.Weekly, record, week, None))).Outcome);
        Assert.Equal(RecordStoreWriteOutcome.LimitReached, (await StoreAsync(services, s => s.AddAsync(owner, world.Weekly, week, false, Wed18, RecordValue.FromAmount(Count), ReplaceWith("limit"), 1, None))).Outcome);

        // Every member with a cancelled token.
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        var token = cancelled.Token;
        var calls = new Func<IRecordStore, Task>[]
        {
            s => s.AddAsync(owner, world.Daily, day, true, Mon16, RecordValue.FromAmount(Duration), ReplaceWith("cancelled"), 50, token),
            s => s.FindRecordDateAsync(owner, world.Daily, record, token),
            s => s.ChangeAsync(owner, world.Daily, record, day, RecordValue.FromAmount(Duration), ReplaceWith("cancelled"), token),
            s => s.DeleteAsync(owner, world.Daily, record, day, token),
            s => s.SaveNoteAsync(owner, world.Daily, day, NoteOf(Note("cancelled")), token),
            s => s.ReadAsync(owner, world.Daily, Mon16, Sun22, token),
            s => s.ReadRecentNoteKeysAsync(owner, world.Daily, 5, token),
        };
        foreach (var call in calls)
        {
            exceptions.Add(await Assert.ThrowsAnyAsync<OperationCanceledException>(() => StoreAsync(services, async s =>
            {
                await call(s);
                return 0;
            })));
        }

        AssertNoProtectedValue(capture, exceptions, []);
    }

    [Fact]
    public async Task HP_006_no_fault_of_a_use_case_or_of_the_store_puts_a_note_or_value_in_a_log_or_an_exception()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(CreatedAt);
        var faults = new FaultSwitch();
        await using var services = BuildServices(capture, clock, faults);
        var world = await ArrangeAsync(services, clock, "faults");
        var owner = world.User;
        var exceptions = new List<Exception>();

        async Task FailsAsync<T>(Func<RecordingService, Task<T>> call, string message)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => RecordingAsync(services, call));
            Assert.Equal(message, exception.Message);
            exceptions.Add(exception);
        }

        var recorded = await RecordingAsync(services, s => s.RecordAsync(owner, world.Daily, Mon16, Minutes(Duration, Note("kept")), None));
        var record = recorded.RecordId!.Value;

        // A store outcome that the use case does not have.
        faults.Member = nameof(IRecordStore.AddAsync);
        await FailsAsync(s => s.RecordAsync(owner, world.Daily, Tue17, Minutes(Duration, Note("odd outcome")), None),
            "The record store gave an outcome that adding a record does not have.");
        faults.Member = nameof(IRecordStore.ChangeAsync);
        await FailsAsync(s => s.ChangeRecordAsync(owner, world.Daily, record, Minutes(ChangedDuration, Note("odd outcome")), None),
            "The record store gave an outcome that changing a record does not have.");
        faults.Member = nameof(IRecordStore.DeleteAsync);
        await FailsAsync(s => s.DeleteRecordAsync(owner, world.Daily, record, None),
            "The record store gave an outcome that deleting a record does not have.");
        faults.Member = nameof(IRecordStore.SaveNoteAsync);
        await FailsAsync(s => s.SaveNoteAsync(owner, world.Daily, Tue17, Note("odd outcome"), None),
            "The record store gave an outcome that saving a note does not have.");
        faults.Member = null;

        // A record and a note stored outside every entry of the goal (before its start date).
        var stray = await InsertRecordAsync(world.Daily, Sun15, Duration);
        await InsertNoteAsync(world.Daily, Sun15, Note("stray"));
        await FailsAsync(s => s.ChangeRecordAsync(owner, world.Daily, stray, Minutes(ChangedDuration, Note("stray change")), None),
            "A stored record's date belongs to no commitment or period of its goal.");
        await FailsAsync(s => s.DeleteRecordAsync(owner, world.Daily, stray, None),
            "A stored record's date belongs to no commitment or period of its goal.");
        await FailsAsync(s => s.GetRecentNotesAsync(owner, world.Daily, None),
            "A stored note's key belongs to no commitment or period of its goal.");

        // A stored amount out of range: every read of its week fails, and so does a write in it, which is rolled back.
        var invalid = await InsertRecordAsync(world.Weekly, Mon16, StoredOutOfRange);
        var invalidMessage = $"The stored record {invalid.Value} has no valid value.";
        await FailsAsync(s => s.GetHistoryAsync(owner, world.Weekly, Mon16, Sun22, None), invalidMessage);
        await FailsAsync(s => s.GetRecordableAsync(owner, world.Weekly, None), invalidMessage);
        await FailsAsync(s => s.RecordAsync(owner, world.Weekly, Tue17, Minutes(Count, Note("in a broken week")), None), invalidMessage);
        var direct = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            StoreAsync(services, s => s.ReadAsync(owner.AccountId!.Value, world.Weekly, Mon16, Sun22, None)));
        exceptions.Add(direct);
        Assert.Single(await RecordSamples.RecordRowsAsync(database.ConnectionString, world.Weekly));

        exceptions.AddRange(await DatabaseFaultsAsync(services, world, record));
        exceptions.AddRange(await NoteDeleteFaultsAsync(services, world, record));
        AssertNoProtectedValue(capture, exceptions, []);
    }

    /// <summary>
    /// The rollback of a record's write whose note delete fails (specs/006-note-editing FR-003, ADR 0019 "a refused or
    /// failed save"): only the notes trigger of <see cref="DatabaseFaultsAsync"/> is created, so a change of the daily
    /// goal's record, which has a note, writes its record row and then fails on the note's delete, whose error text
    /// carries the note. A store change with <see cref="NoteWrite.Remove"/> and a use-case change with an empty note
    /// each fail with the store's own exception, and the record rows and the note rows stay as they were read before.
    /// The trigger is dropped afterwards.
    /// </summary>
    private async Task<IReadOnlyList<Exception>> NoteDeleteFaultsAsync(IServiceProvider services, World world, RecordId record)
    {
        const string failed = "Saving a record failed (SQL error 50000).";
        var owner = world.User;
        var account = owner.AccountId!.Value;
        var day = new EntrySpan(Mon16, Mon16, Mon16);
        var exceptions = new List<Exception>();
        var recordsBefore = await RecordSamples.RecordRowsAsync(database.ConnectionString, world.Daily);
        var notesBefore = await RecordSamples.NoteRowsAsync(database.ConnectionString, world.Daily);
        Assert.Contains(notesBefore, n => (DateTime)n["EntryDate"]! == Mon16.ToDateTime(TimeOnly.MinValue));
        await Sql.ExecuteAsync(database.ConnectionString, NotesTrigger(world.Daily));
        try
        {
            var writes = new Func<IServiceProvider, Task>[]
            {
                p => StoreAsync(p, s => s.ChangeAsync(account, world.Daily, record, day, RecordValue.FromAmount(ChangedDuration), NoteWrite.Remove, None)),
                p => RecordingAsync(p, s => s.ChangeRecordAsync(owner, world.Daily, record, Minutes(ChangedDuration, string.Empty), None)),
            };
            foreach (var write in writes)
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => write(services));
                Assert.Equal(failed, exception.Message);
                Assert.Null(exception.InnerException);
                exceptions.Add(exception);
            }
        }
        finally
        {
            await Sql.ExecuteAsync(database.ConnectionString, "DROP TRIGGER IF EXISTS [dbo].[TR_It_Rl_GoalNotes_Fault]");
        }

        Assert.Equal(recordsBefore.Select(r => string.Join("|", r.Values)), (await RecordSamples.RecordRowsAsync(database.ConnectionString, world.Daily)).Select(r => string.Join("|", r.Values)));
        Assert.Equal(notesBefore.Select(r => string.Join("|", r.Values)), (await RecordSamples.NoteRowsAsync(database.ConnectionString, world.Daily)).Select(r => string.Join("|", r.Values)));
        return exceptions;
    }

    /// <summary>
    /// SQL errors whose text carries the value written: test triggers on the daily goal's records (update and delete)
    /// and notes (insert, update and delete) raise error 50000 with the amount or the note text. Each write of a use
    /// case and of the store then fails with the store's own exception, naming the error number only, and rolls back.
    /// A plain SQL update first shows that the forced error does carry the value. The triggers are dropped afterwards.
    /// </summary>
    private async Task<IReadOnlyList<Exception>> DatabaseFaultsAsync(IServiceProvider services, World world, RecordId record)
    {
        const string failed = "Saving a record failed (SQL error 50000).";
        var owner = world.User;
        var account = owner.AccountId!.Value;
        var day = new EntrySpan(Mon16, Mon16, Mon16);
        var exceptions = new List<Exception>();
        var recordsBefore = await RecordSamples.RecordRowsAsync(database.ConnectionString, world.Daily);
        var notesBefore = await RecordSamples.NoteRowsAsync(database.ConnectionString, world.Daily);
        await Sql.ExecuteAsync(database.ConnectionString, RecordsTrigger(world.Daily));
        await Sql.ExecuteAsync(database.ConnectionString, NotesTrigger(world.Daily));
        try
        {
            var forced = await Assert.ThrowsAsync<SqlException>(() => Sql.ExecuteAsync(database.ConnectionString,
                "UPDATE [GoalRecords] SET [Amount] = @amount WHERE [Id] = @id", ("@amount", ChangedDuration), ("@id", record.Value)));
            Assert.True(HasValue(forced.Message));

            var writes = new Func<IServiceProvider, Task>[]
            {
                // An existing commitment's record is replaced in place; a new record's note is inserted.
                p => RecordingAsync(p, s => s.RecordAsync(owner, world.Daily, Mon16, Minutes(ChangedDuration), None)),
                p => RecordingAsync(p, s => s.RecordAsync(owner, world.Daily, Tue17, Minutes(Duration, Note("new note")), None)),
                p => RecordingAsync(p, s => s.ChangeRecordAsync(owner, world.Daily, record, Minutes(ChangedDuration, Note("changed")), None)),
                p => RecordingAsync(p, s => s.DeleteRecordAsync(owner, world.Daily, record, None)),
                p => RecordingAsync(p, s => s.SaveNoteAsync(owner, world.Daily, Mon16, Note("replaced"), None)),
                p => RecordingAsync(p, s => s.SaveNoteAsync(owner, world.Daily, Mon16, null, None)),
                p => StoreAsync(p, s => s.AddAsync(account, world.Daily, day, true, Mon16, RecordValue.FromAmount(ChangedDuration), NoteWrite.Keep, 50, None)),
                p => StoreAsync(p, s => s.ChangeAsync(account, world.Daily, record, day, RecordValue.FromAmount(ChangedDuration), ReplaceWith("changed"), None)),

                // The record's own note delete: the record's UPDATE fires its trigger first, so the delete is not reached.
                p => StoreAsync(p, s => s.ChangeAsync(account, world.Daily, record, day, RecordValue.FromAmount(ChangedDuration), NoteWrite.Remove, None)),
                p => StoreAsync(p, s => s.DeleteAsync(account, world.Daily, record, day, None)),
                p => StoreAsync(p, s => s.SaveNoteAsync(account, world.Daily, day, NoteOf(Note("replaced")), None)),
                p => StoreAsync(p, s => s.SaveNoteAsync(account, world.Daily, day, null, None)),
            };
            foreach (var write in writes)
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => write(services));
                Assert.Equal(failed, exception.Message);
                Assert.Null(exception.InnerException);
                exceptions.Add(exception);
            }
        }
        finally
        {
            await Sql.ExecuteAsync(database.ConnectionString, "DROP TRIGGER IF EXISTS [dbo].[TR_It_Rl_GoalRecords_Fault]");
            await Sql.ExecuteAsync(database.ConnectionString, "DROP TRIGGER IF EXISTS [dbo].[TR_It_Rl_GoalNotes_Fault]");
        }

        Assert.Equal(recordsBefore.Select(r => string.Join("|", r.Values)), (await RecordSamples.RecordRowsAsync(database.ConnectionString, world.Daily)).Select(r => string.Join("|", r.Values)));
        Assert.Equal(notesBefore.Select(r => string.Join("|", r.Values)), (await RecordSamples.NoteRowsAsync(database.ConnectionString, world.Daily)).Select(r => string.Join("|", r.Values)));
        return exceptions;
    }

    private static string RecordsTrigger(GoalId goal) => $"""
        CREATE TRIGGER [dbo].[TR_It_Rl_GoalRecords_Fault] ON [dbo].[GoalRecords] AFTER UPDATE, DELETE AS
        BEGIN
            SET NOCOUNT ON;
            DECLARE @message nvarchar(2048) = (
                SELECT TOP (1) CONCAT(N'Forced fault: ', CAST(COALESCE(i.[Amount], d.[Amount]) AS nvarchar(12)), N'.')
                FROM deleted d LEFT JOIN inserted i ON i.[Id] = d.[Id]
                WHERE d.[GoalId] = '{goal.Value}');
            IF @message IS NOT NULL THROW 50000, @message, 1;
        END
        """;

    private static string NotesTrigger(GoalId goal) => $"""
        CREATE TRIGGER [dbo].[TR_It_Rl_GoalNotes_Fault] ON [dbo].[GoalNotes] AFTER INSERT, UPDATE, DELETE AS
        BEGIN
            SET NOCOUNT ON;
            DECLARE @message nvarchar(2048) = (
                SELECT TOP (1) CONCAT(N'Forced fault: ', x.[Text])
                FROM (SELECT [GoalId], [Text] FROM inserted UNION ALL SELECT [GoalId], [Text] FROM deleted) x
                WHERE x.[GoalId] = '{goal.Value}');
            IF @message IS NOT NULL THROW 50000, @message, 1;
        END
        """;

    private sealed record World(ActingUser User, ActingUser Other, GoalId Daily, GoalId Weekly, GoalId YesNo);

    /// <summary>Which store member gives a wrong outcome; null for none. One per test.</summary>
    private sealed class FaultSwitch
    {
        public string? Member { get; set; }
    }

    /// <summary>
    /// The real record store, except that the member <see cref="FaultSwitch.Member"/> names gives, without writing, an
    /// outcome that its use case does not have.
    /// </summary>
    private sealed class FaultingRecordStore(IRecordStore inner, FaultSwitch faults) : IRecordStore
    {
        public Task<RecordStoreWrite> AddAsync(
            AccountId owner, GoalId goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value, NoteWrite note, int limit,
            CancellationToken cancellationToken) =>
            faults.Member == nameof(AddAsync)
                ? Task.FromResult(RecordStoreWrite.RecordNotFound)
                : inner.AddAsync(owner, goal, entry, isCommitment, date, value, note, limit, cancellationToken);

        public Task<DateOnly?> FindRecordDateAsync(AccountId owner, GoalId goal, RecordId record, CancellationToken cancellationToken) =>
            inner.FindRecordDateAsync(owner, goal, record, cancellationToken);

        public Task<RecordStoreWrite> ChangeAsync(
            AccountId owner, GoalId goal, RecordId record, EntrySpan entry, RecordValue value, NoteWrite note, CancellationToken cancellationToken) =>
            faults.Member == nameof(ChangeAsync)
                ? Task.FromResult(RecordStoreWrite.LimitReached)
                : inner.ChangeAsync(owner, goal, record, entry, value, note, cancellationToken);

        public Task<RecordStoreWrite> DeleteAsync(AccountId owner, GoalId goal, RecordId record, EntrySpan entry, CancellationToken cancellationToken) =>
            faults.Member == nameof(DeleteAsync)
                ? Task.FromResult(RecordStoreWrite.LimitReached)
                : inner.DeleteAsync(owner, goal, record, entry, cancellationToken);

        public Task<RecordStoreWrite> SaveNoteAsync(AccountId owner, GoalId goal, EntrySpan entry, NoteText? note, CancellationToken cancellationToken) =>
            faults.Member == nameof(SaveNoteAsync)
                ? Task.FromResult(RecordStoreWrite.RecordNotFound)
                : inner.SaveNoteAsync(owner, goal, entry, note, cancellationToken);

        public Task<StoredEntries> ReadAsync(AccountId owner, GoalId goal, DateOnly from, DateOnly to, CancellationToken cancellationToken) =>
            inner.ReadAsync(owner, goal, from, to, cancellationToken);

        public Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(AccountId owner, GoalId goal, int count, CancellationToken cancellationToken) =>
            inner.ReadRecentNoteKeysAsync(owner, goal, count, cancellationToken);
    }
}
