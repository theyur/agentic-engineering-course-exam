using System.Globalization;
using System.Text.RegularExpressions;
using HT.Application;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// Slice 8 (T102; US5-AS5, HP-010): the seven goal-change use cases (<see cref="GoalChangeService"/>'s
/// <c>ChangeSettingsAsync</c>, <c>EndAsync</c>, <c>GetNewAttemptValuesAsync</c>, <c>StartAttemptAsync</c> and
/// <see cref="AttemptService"/>'s <c>GetAttemptsAsync</c>, <c>GetAttemptAsync</c>, <c>GetGoalHistoryAsync</c>) run on SQL
/// Server with every log entry captured at <see cref="LogLevel.Trace"/>, successes, refusals and faults included, and no
/// captured message or exception text, no thrown exception and no result text contains an ending note, a note, a recorded
/// value, a target amount, a unit, a goal name, a habit name or an outcome word. Each text carries a unique marker. The
/// faults are forced without test hooks in production code:
/// <list type="bullet">
///   <item>a goal store decorator whose three conditional writes always give <see cref="GoalChangeWriteOutcome.Stale"/>,
///   so each write use case exhausts its retries and throws;</item>
///   <item>SQL errors raised by test triggers on this test's goals only, whose error text carries the goal name or the
///   ending note written (the triggers are dropped afterwards);</item>
///   <item>a corrupted ending row arranged with plain SQL (an ending with its outcome and note but no ending date), which
///   every use case must refuse to read (the row is restored afterwards).</item>
/// </list>
/// The goals are in UTC and start on Monday 9 November 2026, created at 08:00Z that day on a clock of this test; the calls
/// are made on Wednesday 18 November at 10:00Z, when Monday 16 is still Pending. Shared database: own prefixes
/// ("it-gcl-", "It-Gcl").
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalChangeLogTests(DatabaseFixture database)
{
    private const string GoalMarker = "Zqgcgoalmark";
    private const string CustomHabitMarker = "Zqgccustommark";
    private const string CatalogHabitMarker = "Zqgccatalogmark";
    private const string UnitMarker = "zqgcunitmark";
    private const string NoteMarker = "Zqgcnotemark";
    private const string EndingNoteMarker = "Zqgcendingmark";

    /// <summary>The recorded count, and the target amounts before and after the settings change.</summary>
    private const int Recorded = 6173;
    private const int TargetAmount = 947;
    private const int ChangedAmount = 863;

    private const string KeptChanging = "The goal kept changing while it was changed.";
    private const string SaveFailed = "Saving a goal failed (SQL error 50000).";
    private const string IncompleteEnding = "A stored attempt has an incomplete ending.";

    private static readonly int[] Values = [Recorded, TargetAmount, ChangedAmount];

    private static readonly string[] TextMarkers = [GoalMarker, CustomHabitMarker, CatalogHabitMarker, UnitMarker, NoteMarker, EndingNoteMarker];

    /// <summary>Every outcome as a word or a constant name; none may reach a log, an exception or a result text.</summary>
    private static readonly string[] OutcomeWords = ["Achieved", "Abandoned", "No longer relevant", nameof(Outcome.NoLongerRelevant)];

    private static readonly DateOnly Mon9 = new(2026, 11, 9);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Tue17 = new(2026, 11, 17);
    private static readonly DateOnly Wed18 = new(2026, 11, 18);
    private static readonly DateOnly Thu19 = new(2026, 11, 19);

    /// <summary>Monday 9 November 2026, 08:00Z: the goals are created, starting that day.</summary>
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 9, 8, 0, 0, TimeSpan.Zero);

    /// <summary>Wednesday 18 November 2026, 10:00Z: Monday 16 and Tuesday 17 are Pending, Wednesday 18 is Due.</summary>
    private static readonly DateTimeOffset CalledAt = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static CancellationToken None => CancellationToken.None;

    private static string Ending(string what) => $"{EndingNoteMarker} {what}";

    private static GoalTargetFields Count(int amount, string unit = UnitMarker) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = amount, CountUnit = unit,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields MinutesDaily(int minutes) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields ThreeTimesAWeek => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    };

    /// <summary>
    /// The application as the integration tests build it, with <paramref name="clock"/>, only <paramref name="capture"/>
    /// as its logger, and the real goal store behind a decorator whose conditional writes give
    /// <see cref="GoalChangeWriteOutcome.Stale"/> while <paramref name="stale"/> is on.
    /// </summary>
    private ServiceProvider BuildServices(LogCapture capture, FakeTimeProvider clock, StaleSwitch stale)
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
        var real = services.Last(d => d.ServiceType == typeof(IGoalStore));
        services.Remove(real);
        services.AddSingleton(stale);
        services.AddScoped<IGoalStore>(provider => new StaleGoalStore(
            (IGoalStore)ActivatorUtilities.CreateInstance(provider, real.ImplementationType!),
            provider.GetRequiredService<StaleSwitch>()));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private static async Task<T> InScopeAsync<TService, T>(IServiceProvider services, Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private static Task<T> ChangesAsync<T>(IServiceProvider services, Func<GoalChangeService, Task<T>> call) => InScopeAsync(services, call);

    private static Task<T> AttemptsAsync<T>(IServiceProvider services, Func<AttemptService, Task<T>> call) => InScopeAsync(services, call);

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, GoalHabit habit, GoalTargetFields target, string? name)
    {
        var created = await InScopeAsync<GoalService, GoalCreateResult>(services, s => s.CreateAsync(
            user, new GoalInput { Habit = habit, Target = target, StartDate = Mon9, Name = name }, None));
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    /// <summary>
    /// The owner, another registered user and four goals of the owner, created at <see cref="CreatedAt"/>:
    /// <list type="bullet">
    ///   <item>"Counted": at least <see cref="TargetAmount"/> of the marked unit every day, with the derived name on a
    ///   custom habit whose name carries a marker; <see cref="Recorded"/> with a marked note is recorded for Monday 16 at
    ///   <see cref="CalledAt"/>;</item>
    ///   <item>"Timed": at least 20 min every day with its own marked name on a catalog habit whose name carries a marker;</item>
    ///   <item>"Weekly": 3 times a week, yes/no, with its own marked name on the same catalog habit;</item>
    ///   <item>"Faulted": like "Timed", for the forced faults.</item>
    /// </list>
    /// The clock is left at <see cref="CalledAt"/>.
    /// </summary>
    private async Task<World> ArrangeAsync(IServiceProvider services, FakeTimeProvider clock)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gcl-owner-{suffix}@example.com");
        var other = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gcl-other-{suffix}@example.com");
        var custom = new GoalHabit(await CustomHabitAsync(services, user.AccountId!.Value, $"It-Gcl {CustomHabitMarker} {suffix}"), HabitKind.Custom);
        var catalog = new GoalHabit(await CatalogHabitAsync(services, $"It-Gcl {CatalogHabitMarker} {suffix}"), HabitKind.Catalog);
        var counted = await CreateGoalAsync(services, user, custom, Count(TargetAmount), null);
        var timed = await CreateGoalAsync(services, user, catalog, MinutesDaily(20), $"It-Gcl {GoalMarker} timed {suffix}");
        var weekly = await CreateGoalAsync(services, user, catalog, ThreeTimesAWeek, $"It-Gcl {GoalMarker} weekly {suffix}");
        var faulted = await CreateGoalAsync(services, user, catalog, MinutesDaily(20), $"It-Gcl {GoalMarker} faulted {suffix}");
        clock.SetUtcNow(CalledAt);
        var recorded = await InScopeAsync<RecordingService, RecordWriteResult>(services, s => s.RecordAsync(
            user, counted, Mon16, new RecordInput { Amount = Recorded, Note = $"{NoteMarker} counted" }, None));
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
        return new World(user, other, counted, timed, weekly, faulted, suffix);
    }

    /// <summary>
    /// Fails when a captured entry, a thrown exception or a result text contains a marker, a value or an outcome word. A
    /// value counts as a whole number that is not part of a GUID, a stack trace's line number or a duration in
    /// milliseconds. The failure names no value.
    /// </summary>
    private static void AssertNoProtectedValue(LogCapture capture, IEnumerable<Exception> exceptions, IEnumerable<string?> results)
    {
        Assert.NotEmpty(capture.Entries);
        capture.AssertContainsNone([.. TextMarkers, .. OutcomeWords]);
        foreach (var entry in capture.Entries)
        {
            Assert.False(HasValue(entry.Message) || HasValue(entry.Exception), $"A log entry of category {entry.Category} at level {entry.Level} contains a value.");
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
        text is not null
        && (TextMarkers.Concat(OutcomeWords).Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase)) || HasValue(text));

    private static bool HasValue(string? text) =>
        text is not null && Values.Any(v => ValuePattern(v).IsMatch(text));

    private static Regex ValuePattern(int value) =>
        new($"(?<!:line )(?<![0-9A-Fa-f-]){value.ToString(CultureInfo.InvariantCulture)}(?![0-9A-Fa-f-])(?!ms)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    [Fact]
    public async Task HP_010_no_goal_change_use_case_success_or_refusal_logs_or_returns_a_note_value_unit_name_or_outcome()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(CreatedAt);
        await using var services = BuildServices(capture, clock, new StaleSwitch());
        var world = await ArrangeAsync(services, clock);
        var owner = world.User;
        var administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);
        var results = new List<string?>();
        var outcomes = new List<(Enum Expected, Enum Outcome)>();

        void Add(Enum expected, Enum outcome, string? message)
        {
            outcomes.Add((expected, outcome));
            results.Add(message);
        }

        async Task ChangeAsync(SettingsChangeOutcome expected, ActingUser user, GoalId goal, GoalTargetFields target, string? name)
        {
            var result = await ChangesAsync(services, s => s.ChangeSettingsAsync(user, goal, new GoalSettingsInput { Target = target, Name = name }, None));
            Add(expected, result.Outcome, result.Message);
        }

        async Task EndAsync(GoalEndOutcome expected, ActingUser user, GoalId goal, Outcome? outcome, string? note)
        {
            var result = await ChangesAsync(services, s => s.EndAsync(user, goal, outcome, note, None));
            Add(expected, result.Outcome, result.Message);
        }

        async Task ValuesAsync(NewAttemptValuesOutcome expected, ActingUser user, GoalId goal)
        {
            var result = await ChangesAsync(services, s => s.GetNewAttemptValuesAsync(user, goal, None));
            Add(expected, result.Outcome, result.Message);
        }

        async Task StartAsync(AttemptStartOutcome expected, ActingUser user, GoalId goal, GoalTargetFields target, DateOnly? start)
        {
            var result = await ChangesAsync(services, s => s.StartAttemptAsync(user, goal, new NewAttemptInput { Target = target, StartDate = start }, None));
            Add(expected, result.Outcome, result.Message);
        }

        async Task AttemptsListAsync(AttemptListOutcome expected, ActingUser user, GoalId goal)
        {
            var result = await AttemptsAsync(services, s => s.GetAttemptsAsync(user, goal, None));
            Add(expected, result.Outcome, result.Message);
        }

        async Task AttemptAsync(AttemptReadOutcome expected, ActingUser user, GoalId goal, int number)
        {
            var result = await AttemptsAsync(services, s => s.GetAttemptAsync(user, goal, number, None));
            Add(expected, result.Outcome, result.Message);
        }

        async Task HistoryAsync(GoalHistoryListOutcome expected, ActingUser user)
        {
            var result = await AttemptsAsync(services, s => s.GetGoalHistoryAsync(user, None));
            Add(expected, result.Outcome, result.Message);
        }

        var ownName = $"It-Gcl {GoalMarker} counted {world.Suffix}";

        // Settings changes: a target and an own name, a name only, unchanged, and each kind of refusal.
        await ChangeAsync(SettingsChangeOutcome.Changed, owner, world.Counted, Count(ChangedAmount), ownName);
        await ChangeAsync(SettingsChangeOutcome.Changed, owner, world.Weekly, ThreeTimesAWeek, $"It-Gcl {GoalMarker} weekly renamed {world.Suffix}");
        await ChangeAsync(SettingsChangeOutcome.Unchanged, owner, world.Counted, Count(ChangedAmount), ownName);
        await ChangeAsync(SettingsChangeOutcome.CountUnitTooLong, owner, world.Counted, Count(ChangedAmount, UnitMarker + new string('x', 40)), ownName);
        await ChangeAsync(SettingsChangeOutcome.ScheduleKindChanged, owner, world.Counted, Count(ChangedAmount) with { ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week }, ownName);
        await ChangeAsync(SettingsChangeOutcome.NameTooLong, owner, world.Counted, Count(ChangedAmount), ownName + new string('x', 60));
        await ChangeAsync(SettingsChangeOutcome.NotSignedIn, ActingUser.Anonymous, world.Counted, Count(ChangedAmount), ownName);
        await ChangeAsync(SettingsChangeOutcome.NotForAdministrators, administrator, world.Counted, Count(ChangedAmount), ownName);
        await ChangeAsync(SettingsChangeOutcome.NotFound, world.Other, world.Counted, Count(ChangedAmount), ownName);

        // Endings: each outcome with a marked note, and each kind of refusal.
        await EndAsync(GoalEndOutcome.OutcomeMissing, owner, world.Timed, null, Ending("missing outcome"));
        await EndAsync(GoalEndOutcome.NoteTooLong, owner, world.Timed, Outcome.Abandoned, Ending(new string('x', 500)));
        await EndAsync(GoalEndOutcome.NotSignedIn, ActingUser.Anonymous, world.Timed, Outcome.Abandoned, Ending("anonymous"));
        await EndAsync(GoalEndOutcome.NotForAdministrators, administrator, world.Timed, Outcome.Abandoned, Ending("administrator"));
        await EndAsync(GoalEndOutcome.NotFound, world.Other, world.Timed, Outcome.Abandoned, Ending("other"));
        await EndAsync(GoalEndOutcome.Ended, owner, world.Timed, Outcome.Abandoned, Ending("timed"));
        await EndAsync(GoalEndOutcome.Ended, owner, world.Counted, Outcome.NoLongerRelevant, Ending("counted"));
        await EndAsync(GoalEndOutcome.AlreadyEnded, owner, world.Timed, Outcome.Achieved, Ending("again"));

        // Starting values and new attempts, with each kind of refusal; the new attempt of "Timed" counts the marked unit.
        await ValuesAsync(NewAttemptValuesOutcome.Found, owner, world.Timed);
        await ValuesAsync(NewAttemptValuesOutcome.NotEnded, owner, world.Weekly);
        await ValuesAsync(NewAttemptValuesOutcome.NotSignedIn, ActingUser.Anonymous, world.Timed);
        await ValuesAsync(NewAttemptValuesOutcome.NotForAdministrators, administrator, world.Timed);
        await ValuesAsync(NewAttemptValuesOutcome.NotFound, world.Other, world.Timed);
        await StartAsync(AttemptStartOutcome.NotEnded, owner, world.Weekly, Count(TargetAmount), Thu19);
        await StartAsync(AttemptStartOutcome.CountUnitTooLong, owner, world.Timed, Count(TargetAmount, UnitMarker + new string('x', 40)), Thu19);
        await StartAsync(AttemptStartOutcome.StartDateMissing, owner, world.Timed, Count(TargetAmount), null);
        await StartAsync(AttemptStartOutcome.StartDateInPast, owner, world.Timed, Count(TargetAmount), Tue17);
        await StartAsync(AttemptStartOutcome.StartDateNotAfterEnding, owner, world.Timed, Count(TargetAmount), Wed18);
        await StartAsync(AttemptStartOutcome.StartDateTooFar, owner, world.Timed, Count(TargetAmount), Wed18.AddDays(366));
        await StartAsync(AttemptStartOutcome.NotSignedIn, ActingUser.Anonymous, world.Timed, Count(TargetAmount), Thu19);
        await StartAsync(AttemptStartOutcome.NotForAdministrators, administrator, world.Timed, Count(TargetAmount), Thu19);
        await StartAsync(AttemptStartOutcome.NotFound, world.Other, world.Timed, Count(TargetAmount), Thu19);
        await StartAsync(AttemptStartOutcome.Started, owner, world.Timed, Count(TargetAmount), Thu19);

        // The attempts, each attempt, and the goal history, for the owner and each refusal.
        foreach (var goal in new[] { world.Counted, world.Timed })
        {
            await AttemptsListAsync(AttemptListOutcome.Found, owner, goal);
            await AttemptAsync(AttemptReadOutcome.Found, owner, goal, 1);
            await AttemptAsync(AttemptReadOutcome.AttemptNotFound, owner, goal, 9);
            await AttemptsListAsync(AttemptListOutcome.NotSignedIn, ActingUser.Anonymous, goal);
            await AttemptAsync(AttemptReadOutcome.NotForAdministrators, administrator, goal, 1);
            await AttemptsListAsync(AttemptListOutcome.NotFound, world.Other, goal);
            await AttemptAsync(AttemptReadOutcome.NotFound, world.Other, goal, 1);
        }

        await AttemptAsync(AttemptReadOutcome.Found, owner, world.Timed, 2);
        await HistoryAsync(GoalHistoryListOutcome.Found, owner);
        await HistoryAsync(GoalHistoryListOutcome.Found, world.Other);
        await HistoryAsync(GoalHistoryListOutcome.NotSignedIn, ActingUser.Anonymous);
        await HistoryAsync(GoalHistoryListOutcome.NotForAdministrators, administrator);

        Assert.All(outcomes, o => Assert.Equal(o.Expected, o.Outcome));
        var history = await AttemptsAsync(services, s => s.GetGoalHistoryAsync(owner, None));
        Assert.Contains(history.Entries, e => e.EndingNote == Ending("timed"));
        AssertNoProtectedValue(capture, [], results);
    }

    [Fact]
    public async Task HP_010_no_fault_of_a_goal_change_use_case_or_of_the_store_puts_a_note_name_or_outcome_in_a_log_or_an_exception()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(CreatedAt);
        var stale = new StaleSwitch();
        await using var services = BuildServices(capture, clock, stale);
        var world = await ArrangeAsync(services, clock);
        var owner = world.User;
        var exceptions = new List<Exception>();

        async Task FailsAsync<TService, T>(Func<TService, Task<T>> call, string message)
            where TService : notnull
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => InScopeAsync(services, call));
            Assert.Equal(message, exception.Message);
            exceptions.Add(exception);
        }

        // Arranged: "Timed" ended with a marked note and restarted from Thursday 19; "Counted" ended with a marked note.
        Assert.Equal(GoalEndOutcome.Ended, (await ChangesAsync(services, s => s.EndAsync(owner, world.Timed, Outcome.Abandoned, Ending("timed"), None))).Outcome);
        Assert.Equal(AttemptStartOutcome.Started, (await ChangesAsync(services, s => s.StartAttemptAsync(
            owner, world.Timed, new NewAttemptInput { Target = Count(TargetAmount), StartDate = Thu19 }, None))).Outcome);
        Assert.Equal(GoalEndOutcome.Ended, (await ChangesAsync(services, s => s.EndAsync(owner, world.Counted, Outcome.Achieved, Ending("counted"), None))).Outcome);

        // Every write stays stale: each write use case retries and throws, naming no value.
        stale.On = true;
        try
        {
            await FailsAsync<GoalChangeService, SettingsChangeResult>(s => s.ChangeSettingsAsync(
                owner, world.Faulted, new GoalSettingsInput { Target = MinutesDaily(25), Name = $"It-Gcl {GoalMarker} stale {world.Suffix}" }, None), KeptChanging);
            await FailsAsync<GoalChangeService, GoalEndResult>(s => s.EndAsync(owner, world.Faulted, Outcome.NoLongerRelevant, Ending("stale"), None), KeptChanging);
            await FailsAsync<GoalChangeService, AttemptStartResult>(s => s.StartAttemptAsync(
                owner, world.Counted, new NewAttemptInput { Target = Count(ChangedAmount), StartDate = Thu19 }, None), KeptChanging);
        }
        finally
        {
            stale.On = false;
        }

        exceptions.AddRange(await DatabaseFaultsAsync(services, world));
        exceptions.AddRange(await CorruptedEndingAsync(services, world));
        AssertNoProtectedValue(capture, exceptions, []);
    }

    /// <summary>
    /// SQL errors whose text carries the goal name or the ending note written: test triggers on this test's goals raise
    /// error 50000 with the new name of a goal row, and with the ending note and the goal's name of an attempt row
    /// inserted or updated. A settings change, an ending and a new attempt then fail with the store's own exception,
    /// naming the error number only, and roll back. A plain SQL update first shows that the forced error does carry the
    /// marker. The triggers are dropped afterwards.
    /// </summary>
    private async Task<IReadOnlyList<Exception>> DatabaseFaultsAsync(IServiceProvider services, World world)
    {
        var owner = world.User;
        var goals = new[] { world.Counted, world.Timed, world.Weekly, world.Faulted };
        var exceptions = new List<Exception>();
        var before = await RowsAsync(goals);
        await Sql.ExecuteAsync(database.ConnectionString, GoalsTrigger(goals));
        await Sql.ExecuteAsync(database.ConnectionString, AttemptsTrigger(goals));
        try
        {
            var forced = await Assert.ThrowsAsync<SqlException>(() => Sql.ExecuteAsync(database.ConnectionString,
                "UPDATE [Goals] SET [Name] = [Name] WHERE [Id] = @goal", ("@goal", world.Faulted.Value)));
            Assert.Contains(GoalMarker, forced.Message, StringComparison.Ordinal);

            var writes = new Func<IServiceProvider, Task>[]
            {
                p => ChangesAsync(p, s => s.ChangeSettingsAsync(
                    owner, world.Faulted, new GoalSettingsInput { Target = MinutesDaily(25), Name = $"It-Gcl {GoalMarker} faulted rename {world.Suffix}" }, None)),
                p => ChangesAsync(p, s => s.EndAsync(owner, world.Faulted, Outcome.Achieved, Ending("faulted"), None)),
                p => ChangesAsync(p, s => s.StartAttemptAsync(owner, world.Counted, new NewAttemptInput { Target = Count(ChangedAmount), StartDate = Thu19 }, None)),
            };
            foreach (var write in writes)
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => write(services));
                Assert.Equal(SaveFailed, exception.Message);
                Assert.Null(exception.InnerException);
                exceptions.Add(exception);
            }
        }
        finally
        {
            await Sql.ExecuteAsync(database.ConnectionString, "DROP TRIGGER IF EXISTS [dbo].[TR_It_Gcl_Goals_Fault]");
            await Sql.ExecuteAsync(database.ConnectionString, "DROP TRIGGER IF EXISTS [dbo].[TR_It_Gcl_GoalAttempts_Fault]");
        }

        Assert.Equal(before, await RowsAsync(goals));
        return exceptions;
    }

    /// <summary>
    /// An ending row with its outcome and marked note but no ending date: every one of the seven use cases that reads
    /// "Timed" fails with the mapping's exception, which names no value. The ending date is restored afterwards.
    /// </summary>
    private async Task<IReadOnlyList<Exception>> CorruptedEndingAsync(IServiceProvider services, World world)
    {
        var owner = world.User;
        var goal = world.Timed;
        var exceptions = new List<Exception>();
        var corrupted = await Sql.ExecuteAsync(database.ConnectionString,
            "UPDATE [GoalAttempts] SET [EndingDate] = NULL WHERE [GoalId] = @goal AND [Number] = 1 AND [EndingNote] IS NOT NULL",
            ("@goal", goal.Value));
        Assert.Equal(1, corrupted);
        try
        {
            var reads = new Func<IServiceProvider, Task>[]
            {
                p => ChangesAsync(p, s => s.ChangeSettingsAsync(owner, goal, new GoalSettingsInput { Target = Count(ChangedAmount) }, None)),
                p => ChangesAsync(p, s => s.EndAsync(owner, goal, Outcome.Achieved, Ending("corrupted"), None)),
                p => ChangesAsync(p, s => s.GetNewAttemptValuesAsync(owner, goal, None)),
                p => ChangesAsync(p, s => s.StartAttemptAsync(owner, goal, new NewAttemptInput { Target = Count(TargetAmount), StartDate = Thu19 }, None)),
                p => AttemptsAsync(p, s => s.GetAttemptsAsync(owner, goal, None)),
                p => AttemptsAsync(p, s => s.GetAttemptAsync(owner, goal, 1, None)),
                p => AttemptsAsync(p, s => s.GetGoalHistoryAsync(owner, None)),
            };
            foreach (var read in reads)
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => read(services));
                Assert.Equal(IncompleteEnding, exception.Message);
                exceptions.Add(exception);
            }
        }
        finally
        {
            await Sql.ExecuteAsync(database.ConnectionString,
                "UPDATE [GoalAttempts] SET [EndingDate] = @date WHERE [GoalId] = @goal AND [Number] = 1",
                ("@date", Wed18.ToDateTime(TimeOnly.MinValue)), ("@goal", goal.Value));
        }

        var restored = await AttemptsAsync(services, s => s.GetAttemptAsync(owner, goal, 1, None));
        Assert.Equal(AttemptReadOutcome.Found, restored.Outcome);
        return exceptions;
    }

    /// <summary>Every goal, version and attempt row of <paramref name="goals"/>, as one text per row, in a stable order.</summary>
    private async Task<IReadOnlyList<string>> RowsAsync(IEnumerable<GoalId> goals)
    {
        var rows = new List<string>();
        foreach (var goal in goals)
        {
            foreach (var (table, column) in new[] { ("Goals", "Id"), ("GoalSettingsVersions", "GoalId"), ("GoalAttempts", "GoalId") })
            {
                var found = await Sql.QueryAsync(database.ConnectionString, $"SELECT * FROM [{table}] WHERE [{column}] = @goal", ("@goal", goal.Value));
                rows.AddRange(found.Select(r => table + "|" + string.Join("|", r.Values)).Order(StringComparer.Ordinal));
            }
        }

        return rows;
    }

    private static string GoalList(IEnumerable<GoalId> goals) => string.Join(", ", goals.Select(g => $"'{g.Value}'"));

    private static string GoalsTrigger(IEnumerable<GoalId> goals) => $"""
        CREATE TRIGGER [dbo].[TR_It_Gcl_Goals_Fault] ON [dbo].[Goals] AFTER UPDATE AS
        BEGIN
            SET NOCOUNT ON;
            DECLARE @message nvarchar(2048) = (
                SELECT TOP (1) CONCAT(N'Forced fault: ', i.[Name])
                FROM inserted i
                WHERE i.[Id] IN ({GoalList(goals)}));
            IF @message IS NOT NULL THROW 50000, @message, 1;
        END
        """;

    private static string AttemptsTrigger(IEnumerable<GoalId> goals) => $"""
        CREATE TRIGGER [dbo].[TR_It_Gcl_GoalAttempts_Fault] ON [dbo].[GoalAttempts] AFTER INSERT, UPDATE AS
        BEGIN
            SET NOCOUNT ON;
            DECLARE @message nvarchar(2048) = (
                SELECT TOP (1) CONCAT(N'Forced fault: ', i.[EndingNote], N' ', i.[Outcome], N' ', g.[Name])
                FROM inserted i JOIN [dbo].[Goals] g ON g.[Id] = i.[GoalId]
                WHERE i.[GoalId] IN ({GoalList(goals)}));
            IF @message IS NOT NULL THROW 50000, @message, 1;
        END
        """;

    private sealed record World(ActingUser User, ActingUser Other, GoalId Counted, GoalId Timed, GoalId Weekly, GoalId Faulted, string Suffix);

    /// <summary>Whether the decorated goal store's conditional writes give <see cref="GoalChangeWriteOutcome.Stale"/>.</summary>
    private sealed class StaleSwitch
    {
        public bool On { get; set; }
    }

    /// <summary>
    /// The real goal store, except that while <see cref="StaleSwitch.On"/> its three conditional goal-change writes give
    /// <see cref="GoalChangeWriteOutcome.Stale"/> without writing.
    /// </summary>
    private sealed class StaleGoalStore(IGoalStore inner, StaleSwitch stale) : IGoalStore
    {
        public Task<GoalWriteOutcome> CreateAsync(
            AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken cancellationToken) =>
            inner.CreateAsync(owner, goal, expectedZone, defaultZone, cancellationToken);

        public Task<OwnerGoals?> ReadOwnerGoalsAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalsAsync(owner, cancellationToken);

        public Task<bool> TryChangeTimeZoneAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, NamedTimeZone next, IReadOnlyList<GoalVersionWrite> writes,
            CancellationToken cancellationToken) =>
            inner.TryChangeTimeZoneAsync(owner, expectedStoredZone, expectedRevision, next, writes, cancellationToken);

        public Task<OwnerGoal?> ReadOwnerGoalAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalAsync(owner, goal, cancellationToken);

        public Task<GoalChangeWriteOutcome> TryChangeSettingsAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalSettingsWrite write, CancellationToken cancellationToken) =>
            stale.On
                ? Task.FromResult(GoalChangeWriteOutcome.Stale)
                : inner.TryChangeSettingsAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public Task<GoalChangeWriteOutcome> TryEndAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalEndingWrite write, CancellationToken cancellationToken) =>
            stale.On
                ? Task.FromResult(GoalChangeWriteOutcome.Stale)
                : inner.TryEndAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public Task<GoalChangeWriteOutcome> TryStartAttemptAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalAttemptWrite write, CancellationToken cancellationToken) =>
            stale.On
                ? Task.FromResult(GoalChangeWriteOutcome.Stale)
                : inner.TryStartAttemptAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public Task<IReadOnlyList<StoredGoal>> ListAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ListAsync(owner, cancellationToken);

        public Task<StoredGoal?> FindAsync(AccountId owner, GoalId id, CancellationToken cancellationToken) =>
            inner.FindAsync(owner, id, cancellationToken);

        public Task<int> CountUsingCustomHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken) =>
            inner.CountUsingCustomHabitAsync(owner, habit, cancellationToken);

        public Task<IReadOnlyDictionary<HabitId, int>> CountByCatalogHabitAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.CountByCatalogHabitAsync(owner, cancellationToken);

        public Task<IReadOnlyList<StoredGoal>> ListOnCatalogHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken) =>
            inner.ListOnCatalogHabitAsync(owner, habit, cancellationToken);

        public Task<int> CountUsingCatalogHabitAsync(HabitId habit, CancellationToken cancellationToken) =>
            inner.CountUsingCatalogHabitAsync(habit, cancellationToken);
    }
}
