using System.Globalization;
using System.Text.RegularExpressions;
using HT.Application;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
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
/// HP-009, US5-AS5 (constitution III; T082): excusing, removing an excusal, pausing, resuming, cancelling a pause and
/// reading (the pauses, the history, the reports) run on SQL Server with every log entry captured at
/// <see cref="LogLevel.Trace"/>, successes, refusals and faults included, and no captured message or exception text, no
/// thrown exception and no result text contains a reason, a note, a recorded value, a unit, a goal name or a habit name.
/// Each of those carries a unique marker. The faults are forced as <see cref="RecordLogTests"/> forces them, without
/// test hooks in production code: a store decorator that gives an outcome a use case does not have; rows arranged with
/// plain SQL that the code must refuse to read (a pause that ends before it starts, an excusal with a blank reason); a
/// pre-cancelled token; and SQL errors raised by test triggers on this test's goals only, whose error text carries the
/// reason written, so that a database error that names a reason is shown not to reach a log or an exception (the
/// triggers are dropped afterwards). The goals are in UTC and start on Monday 9 November 2026, created at 08:00Z that
/// day on a clock of this class; the calls are made on Wednesday 18 November at 10:00Z, the pause date. Shared database:
/// own prefixes ("it-exl-", "It-Exl").
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ExemptionLogTests(DatabaseFixture database)
{
    private const string ReasonMarker = "Zqreasonmark";
    private const string NoteMarker = "Zqnotemark";
    private const string GoalMarker = "Zqgoalmark";
    private const string CustomHabitMarker = "Zqcustommark";
    private const string CatalogHabitMarker = "Zqcatalogmark";
    private const string UnitMarker = "zqunitmark";

    /// <summary>The recorded values: a duration and a count.</summary>
    private const int Duration = 1373;
    private const int Count = 7311;

    private const string SaveFailed = "Saving an excusal or a pause failed (SQL error 50000).";

    private static readonly int[] Values = [Duration, Count];
    private static readonly string[] TextMarkers = [ReasonMarker, NoteMarker, GoalMarker, CustomHabitMarker, CatalogHabitMarker, UnitMarker];

    private static readonly DateOnly Mon9 = new(2026, 11, 9);
    private static readonly DateOnly Sun15 = new(2026, 11, 15);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Tue17 = new(2026, 11, 17);
    private static readonly DateOnly Wed18 = new(2026, 11, 18);
    private static readonly DateOnly Thu19 = new(2026, 11, 19);
    private static readonly DateOnly Fri20 = new(2026, 11, 20);
    private static readonly DateOnly Sat21 = new(2026, 11, 21);
    private static readonly DateOnly Sun22 = new(2026, 11, 22);
    private static readonly DateOnly Mon23 = new(2026, 11, 23);
    private static readonly DateOnly Fri27 = new(2026, 11, 27);
    private static readonly DateOnly Sat28 = new(2026, 11, 28);
    private static readonly DateOnly Sun29 = new(2026, 11, 29);

    /// <summary>Monday 9 November 2026, 08:00Z: the goals are created, starting that day.</summary>
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 9, 8, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Wednesday 18 November 2026, 10:00Z: Monday 9 is Missed (its grace period ended on Thursday 12 at 00:00Z), Sunday 15's
    /// grace period has just ended (at 00:00Z), Monday 16 and Tuesday 17 are Pending, Wednesday 18 is Due and Thursday 19
    /// has not begun.
    /// </summary>
    private static readonly DateTimeOffset CalledAt = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static CancellationToken None => CancellationToken.None;

    /// <summary>A reason with the marker; it is 200 characters or fewer.</summary>
    private static string ReasonText(string what) => $"{ReasonMarker} {what}";

    /// <summary>A reason with the marker that is over 200 characters after trimming.</summary>
    private static string ReasonTooLong => $"{ReasonMarker} {new string('x', 200)}";

    private static string Note(string what) => $"{NoteMarker} {what}";

    private static ExemptionReason? Reason(string what)
    {
        Assert.True(ExemptionReason.TryCreate(ReasonText(what), out var reason, out _));
        return reason;
    }

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

    private static async Task<T> InScopeAsync<TService, T>(IServiceProvider services, Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private static Task<T> ExcusalsAsync<T>(IServiceProvider services, Func<ExcusalService, Task<T>> call) => InScopeAsync(services, call);

    private static Task<T> PausesAsync<T>(IServiceProvider services, Func<PauseService, Task<T>> call) => InScopeAsync(services, call);

    private static Task<T> RecordingAsync<T>(IServiceProvider services, Func<RecordingService, Task<T>> call) => InScopeAsync(services, call);

    private static Task<T> ReportsAsync<T>(IServiceProvider services, Func<ReportService, Task<T>> call) => InScopeAsync(services, call);

    private static Task<T> StoreAsync<T>(IServiceProvider services, Func<IRecordStore, Task<T>> call) => InScopeAsync(services, call);

    /// <summary>
    /// The owner, another registered user, and three goals of the owner, created at <see cref="CreatedAt"/> through
    /// <see cref="GoalService"/>: a daily goal of at least 20 min with its own name on a custom habit, a 3-times-a-week
    /// goal of at least 20 units with its own name on a catalog habit (period-based), and a yes/no goal on Monday,
    /// Wednesday and Friday with the derived name on the same catalog habit. Every name and the unit carry a marker. The
    /// owner records <see cref="Duration"/> on the daily goal and <see cref="Count"/> on the weekly goal for Monday 16,
    /// each with a marked note, at <see cref="CalledAt"/>, where the clock is left.
    /// </summary>
    private async Task<World> ArrangeAsync(IServiceProvider services, FakeTimeProvider clock, string who)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-exl-{who}-{suffix}@example.com");
        var other = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-exl-{who}-other-{suffix}@example.com");
        var custom = await CustomHabitAsync(services, user.AccountId!.Value, $"It-Exl {CustomHabitMarker} {suffix}");
        var catalog = await CatalogHabitAsync(services, $"It-Exl {CatalogHabitMarker} {suffix}");
        var daily = await CreateGoalAsync(services, user, new GoalHabit(custom, HabitKind.Custom), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, $"It-Exl {GoalMarker} daily {suffix}");
        var weekly = await CreateGoalAsync(services, user, new GoalHabit(catalog, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 20, CountUnit = UnitMarker,
            ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, $"It-Exl {GoalMarker} weekly {suffix}");
        var yesNo = await CreateGoalAsync(services, user, new GoalHabit(catalog, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
        }, null);
        clock.SetUtcNow(CalledAt);
        var recorded = new[]
        {
            await RecordingAsync(services, s => s.RecordAsync(user, daily, Mon16, new RecordInput { Amount = Duration, Note = Note("daily") }, None)),
            await RecordingAsync(services, s => s.RecordAsync(user, weekly, Mon16, new RecordInput { Amount = Count, Note = Note("weekly") }, None)),
        };
        Assert.All(recorded, r => Assert.Equal(RecordWriteOutcome.Recorded, r.Outcome));
        return new World(user, other, daily, weekly, yesNo);
    }

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, GoalHabit habit, GoalTargetFields target, string? name)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = habit, Target = target, StartDate = Mon9, Name = name }, None);
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

    [Fact]
    public async Task HP_009_no_use_case_read_or_refusal_logs_or_returns_a_reason_note_value_unit_goal_name_or_habit_name()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(CreatedAt);
        await using var services = BuildServices(capture, clock, new FaultSwitch());
        var world = await ArrangeAsync(services, clock, "use-cases");
        var owner = world.User;
        var results = new List<string?>();
        var outcomes = new List<(Enum Expected, Enum Outcome)>();

        void Add(Enum expected, Enum outcome, string? message)
        {
            outcomes.Add((expected, outcome));
            results.Add(message);
        }

        // Successes: excusals, a removal, pauses, a resumption, a pause set again and a cancellation, each with a reason.
        var excused = await ExcusalsAsync(services, s => s.ExcuseAsync(owner, world.Daily, Tue17, ReasonText("excused"), None));
        Add(ExcuseOutcome.Excused, excused.Outcome, excused.Message);
        var excusedYesNo = await ExcusalsAsync(services, s => s.ExcuseAsync(owner, world.YesNo, Mon16, ReasonText("to remove"), None));
        Add(ExcuseOutcome.Excused, excusedYesNo.Outcome, excusedYesNo.Message);
        var removed = await ExcusalsAsync(services, s => s.RemoveExcusalAsync(owner, world.YesNo, Mon16, None));
        Add(RemoveExcusalOutcome.Removed, removed.Outcome, removed.Message);
        var current = await PausesAsync(services, s => s.PauseAsync(owner, world.Daily, Wed18, Thu19, ReasonText("current"), None));
        Add(PauseOutcome.Paused, current.Outcome, current.Message);
        var resumed = await PausesAsync(services, s => s.ResumeAsync(owner, world.Daily, None));
        Add(ResumeOutcome.Resumed, resumed.Outcome, resumed.Message);
        var again = await PausesAsync(services, s => s.PauseAsync(owner, world.Daily, Wed18, Thu19, ReasonText("current again"), None));
        Add(PauseOutcome.Paused, again.Outcome, again.Message);
        var upcoming = await PausesAsync(services, s => s.PauseAsync(owner, world.Daily, Mon23, Fri27, ReasonText("upcoming"), None));
        Add(PauseOutcome.Paused, upcoming.Outcome, upcoming.Message);
        var cancelled = await PausesAsync(services, s => s.CancelPauseAsync(owner, world.Daily, upcoming.Pause!.Id, None));
        Add(CancelPauseOutcome.Cancelled, cancelled.Outcome, cancelled.Message);
        var weeklyPause = await PausesAsync(services, s => s.PauseAsync(owner, world.Weekly, Mon23, Sun29, ReasonText("weekly"), None));
        Add(PauseOutcome.Paused, weeklyPause.Outcome, weeklyPause.Message);

        // The readings: the pauses, the history with its Excused and Paused entries, and the reports.
        foreach (var goal in new[] { world.Daily, world.Weekly, world.YesNo })
        {
            var pauses = await PausesAsync(services, s => s.GetPausesAsync(owner, goal, None));
            Add(PausesOutcome.Found, pauses.Outcome, pauses.Message);
            var history = await RecordingAsync(services, s => s.GetHistoryAsync(owner, goal, Mon9, Sun29, None));
            Add(HistoryOutcome.Found, history.Outcome, history.Message);
            var recordable = await RecordingAsync(services, s => s.GetRecordableAsync(owner, goal, None));
            Add(RecordableOutcome.Found, recordable.Outcome, recordable.Message);
            var report = await ReportsAsync(services, s => s.GetGoalReportAsync(owner, goal, None));
            Add(GoalReportOutcome.Found, report.Outcome, report.Message);
        }

        var excusedEntry = Assert.Single((await RecordingAsync(services, s => s.GetHistoryAsync(owner, world.Daily, Tue17, Tue17, None))).History!.Entries);
        Assert.Equal(ReasonText("excused"), excusedEntry.ExcusalReason);
        var overview = await ReportsAsync(services, s => s.GetGoalsOverviewAsync(owner, None));
        Add(GoalsOverviewOutcome.Found, overview.Outcome, overview.Message);
        var dashboard = await ReportsAsync(services, s => s.GetDashboardAsync(owner, None));
        Add(DashboardOutcome.Found, dashboard.Outcome, dashboard.Message);

        // The refusals of the acting user (FR-022, FR-021), each on every use case with a marked reason.
        var actingUsers = new[]
        {
            ActingUser.Anonymous,
            ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator),
            ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser),
            world.Other,
        };
        var currentPause = again.Pause!.Id;
        foreach (var actingUser in actingUsers)
        {
            results.Add((await ExcusalsAsync(services, s => s.ExcuseAsync(actingUser, world.YesNo, Wed18, ReasonText("refused"), None))).Message);
            results.Add((await ExcusalsAsync(services, s => s.RemoveExcusalAsync(actingUser, world.Daily, Tue17, None))).Message);
            results.Add((await PausesAsync(services, s => s.PauseAsync(actingUser, world.YesNo, Sat28, Sun29, ReasonText("refused"), None))).Message);
            results.Add((await PausesAsync(services, s => s.ResumeAsync(actingUser, world.Daily, None))).Message);
            results.Add((await PausesAsync(services, s => s.CancelPauseAsync(actingUser, world.Weekly, weeklyPause.Pause!.Id, None))).Message);
            results.Add((await PausesAsync(services, s => s.GetPausesAsync(actingUser, world.Daily, None))).Message);
        }

        // Every later refusal of the slice, each with a marked reason where it takes one.
        var unknownGoal = new GoalId(Guid.NewGuid());
        async Task ExcuseAsync(ExcuseOutcome expected, GoalId goal, DateOnly date, string reason)
        {
            var result = await ExcusalsAsync(services, s => s.ExcuseAsync(owner, goal, date, reason, None));
            Add(expected, result.Outcome, result.Message);
        }

        async Task PauseAsync(PauseOutcome expected, GoalId goal, DateOnly? first, DateOnly? last, string reason)
        {
            var result = await PausesAsync(services, s => s.PauseAsync(owner, goal, first, last, reason, None));
            Add(expected, result.Outcome, result.Message);
        }

        await ExcuseAsync(ExcuseOutcome.NotFound, unknownGoal, Tue17, ReasonText("unknown goal"));
        await ExcuseAsync(ExcuseOutcome.NotACommitment, world.Weekly, Tue17, ReasonText("period"));
        await ExcuseAsync(ExcuseOutcome.NothingScheduled, world.YesNo, Tue17, ReasonText("unscheduled"));
        await ExcuseAsync(ExcuseOutcome.DatePaused, world.Daily, Wed18, ReasonText("paused"));
        await ExcuseAsync(ExcuseOutcome.AlreadyExcused, world.Daily, Tue17, ReasonText("twice"));
        await ExcuseAsync(ExcuseOutcome.NotBegun, world.Daily, Fri20, ReasonText("not begun"));
        await ExcuseAsync(ExcuseOutcome.Final, world.Daily, Mon9, ReasonText("final"));
        await ExcuseAsync(ExcuseOutcome.HasRecord, world.Daily, Mon16, ReasonText("recorded"));
        await ExcuseAsync(ExcuseOutcome.ReasonTooLong, world.YesNo, Wed18, ReasonTooLong);

        // Sunday 15's grace period ended at 00:00Z today; its excusal is stored directly, with no clock check.
        Assert.Equal(
            ExemptionStoreWriteOutcome.Written,
            (await StoreAsync(services, s => s.ExcuseAsync(owner.AccountId!.Value, world.Daily, Sun15, Reason("too late"), None))).Outcome);
        var notExcused = await ExcusalsAsync(services, s => s.RemoveExcusalAsync(owner, world.Daily, Mon16, None));
        Add(RemoveExcusalOutcome.NotExcused, notExcused.Outcome, notExcused.Message);
        var tooLate = await ExcusalsAsync(services, s => s.RemoveExcusalAsync(owner, world.Daily, Sun15, None));
        Add(RemoveExcusalOutcome.TooLate, tooLate.Outcome, tooLate.Message);

        await PauseAsync(PauseOutcome.NotFound, unknownGoal, Sat28, Sun29, ReasonText("unknown goal"));
        await PauseAsync(PauseOutcome.DaysMissing, world.YesNo, null, null, ReasonText("no days"));
        await PauseAsync(PauseOutcome.StartsBeforeToday, world.YesNo, Tue17, Thu19, ReasonText("in the past"));
        await PauseAsync(PauseOutcome.LastBeforeFirst, world.YesNo, Fri27, Mon23, ReasonText("backwards"));
        await PauseAsync(PauseOutcome.Overlaps, world.Daily, Thu19, Sat21, ReasonText("overlapping"));
        await PauseAsync(PauseOutcome.ReasonTooLong, world.YesNo, Sat28, Sun29, ReasonTooLong);

        var notPaused = await PausesAsync(services, s => s.ResumeAsync(owner, world.YesNo, None));
        Add(ResumeOutcome.NotPaused, notPaused.Outcome, notPaused.Message);
        var pauseNotFound = await PausesAsync(services, s => s.CancelPauseAsync(owner, world.Daily, weeklyPause.Pause!.Id, None));
        Add(CancelPauseOutcome.PauseNotFound, pauseNotFound.Outcome, pauseNotFound.Message);
        var hasBegun = await PausesAsync(services, s => s.CancelPauseAsync(owner, world.Daily, currentPause, None));
        Add(CancelPauseOutcome.HasBegun, hasBegun.Outcome, hasBegun.Message);

        Assert.All(outcomes, o => Assert.Equal(o.Expected, o.Outcome));
        AssertNoProtectedValue(capture, [], results);
    }

    [Fact]
    public async Task HP_009_no_exemption_store_member_logs_a_reason_when_it_writes_reads_refuses_or_is_cancelled()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(CreatedAt);
        await using var services = BuildServices(capture, clock, new FaultSwitch());
        var world = await ArrangeAsync(services, clock, "store");
        var owner = world.User.AccountId!.Value;
        var other = world.Other.AccountId!.Value;
        var exceptions = new List<Exception>();

        // The owner: every member writes or reads.
        Assert.Equal(ExemptionStoreWriteOutcome.Written, (await StoreAsync(services, s => s.ExcuseAsync(owner, world.Daily, Tue17, Reason("excused"), None))).Outcome);
        var added = await StoreAsync(services, s => s.AddPauseAsync(owner, world.Daily, Wed18, Fri20, Reason("paused"), None));
        Assert.Equal(ExemptionStoreWriteOutcome.Written, added.Outcome);
        var pause = added.PauseId!.Value;
        Assert.Equal(ExemptionStoreWriteOutcome.Written, (await StoreAsync(services, s => s.SetPauseLastDayAsync(owner, world.Daily, pause, Thu19, None))).Outcome);
        var read = await StoreAsync(services, s => s.ReadExemptionsAsync(owner, world.Daily, None));
        Assert.Equal(Tue17, Assert.Single(read.Excusals).Date);
        Assert.Equal(Thu19, Assert.Single(read.Pauses).LastDay);

        // Another owner: every member gives nothing.
        var refusedByOwner = new Func<IRecordStore, Task<ExemptionStoreWrite>>[]
        {
            s => s.ExcuseAsync(other, world.Daily, Mon16, Reason("other"), None),
            s => s.RemoveExcusalAsync(other, world.Daily, Tue17, None),
            s => s.AddPauseAsync(other, world.Daily, Sat28, Sun29, Reason("other"), None),
            s => s.SetPauseLastDayAsync(other, world.Daily, pause, Wed18, None),
            s => s.DeletePauseAsync(other, world.Daily, pause, None),
        };
        foreach (var call in refusedByOwner)
        {
            Assert.Equal(ExemptionStoreWriteOutcome.GoalNotFound, (await StoreAsync(services, call)).Outcome);
        }

        Assert.Empty((await StoreAsync(services, s => s.ReadExemptionsAsync(other, world.Daily, None))).Pauses);

        // The owner's own refusals, each re-checked under the lock.
        var refusals = new (ExemptionStoreWriteOutcome Expected, Func<IRecordStore, Task<ExemptionStoreWrite>> Call)[]
        {
            (ExemptionStoreWriteOutcome.AlreadyExcused, s => s.ExcuseAsync(owner, world.Daily, Tue17, Reason("twice"), None)),
            (ExemptionStoreWriteOutcome.HasRecord, s => s.ExcuseAsync(owner, world.Daily, Mon16, Reason("recorded"), None)),
            (ExemptionStoreWriteOutcome.Paused, s => s.ExcuseAsync(owner, world.Daily, Wed18, Reason("paused"), None)),
            (ExemptionStoreWriteOutcome.NotExcused, s => s.RemoveExcusalAsync(owner, world.Daily, Mon16, None)),
            (ExemptionStoreWriteOutcome.Overlaps, s => s.AddPauseAsync(owner, world.Daily, Thu19, Sat21, Reason("overlapping"), None)),
            (ExemptionStoreWriteOutcome.PauseNotFound, s => s.SetPauseLastDayAsync(owner, world.Daily, new PauseId(Guid.NewGuid()), Thu19, None)),
            (ExemptionStoreWriteOutcome.PauseNotFound, s => s.DeletePauseAsync(owner, world.Weekly, pause, None)),
        };
        foreach (var (expected, call) in refusals)
        {
            Assert.Equal(expected, (await StoreAsync(services, call)).Outcome);
        }

        Assert.Equal(ExemptionStoreWriteOutcome.Written, (await StoreAsync(services, s => s.RemoveExcusalAsync(owner, world.Daily, Tue17, None))).Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Written, (await StoreAsync(services, s => s.DeletePauseAsync(owner, world.Daily, pause, None))).Outcome);

        // Every member with a cancelled token.
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var token = cancellation.Token;
        var calls = new Func<IRecordStore, Task>[]
        {
            s => s.ReadExemptionsAsync(owner, world.Daily, token),
            s => s.ExcuseAsync(owner, world.Daily, Tue17, Reason("cancelled"), token),
            s => s.RemoveExcusalAsync(owner, world.Daily, Tue17, token),
            s => s.AddPauseAsync(owner, world.Daily, Sat28, Sun29, Reason("cancelled"), token),
            s => s.SetPauseLastDayAsync(owner, world.Daily, pause, Wed18, token),
            s => s.DeletePauseAsync(owner, world.Daily, pause, token),
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
    public async Task HP_009_no_fault_of_a_use_case_or_of_the_store_puts_a_reason_in_a_log_or_an_exception()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(CreatedAt);
        var faults = new FaultSwitch();
        await using var services = BuildServices(capture, clock, faults);
        var world = await ArrangeAsync(services, clock, "faults");
        var owner = world.User;
        var account = owner.AccountId!.Value;
        var exceptions = new List<Exception>();

        async Task FailsAsync<TService, T>(Func<TService, Task<T>> call, string message)
            where TService : notnull
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => InScopeAsync(services, call));
            Assert.Equal(message, exception.Message);
            exceptions.Add(exception);
        }

        // Arranged: an excusal of the daily goal's Tuesday 17; a current pause of the weekly goal that began on Monday 16
        // (resuming shortens it); a current pause of the yes/no goal that begins today (resuming removes it) and an
        // upcoming one (it can be cancelled). The pauses that began before today are stored directly.
        Assert.Equal(ExcuseOutcome.Excused, (await ExcusalsAsync(services, s => s.ExcuseAsync(owner, world.Daily, Tue17, ReasonText("kept"), None))).Outcome);
        Assert.Equal(ExemptionStoreWriteOutcome.Written, (await StoreAsync(services, s => s.AddPauseAsync(account, world.Weekly, Mon16, Fri20, Reason("weekly"), None))).Outcome);
        Assert.Equal(PauseOutcome.Paused, (await PausesAsync(services, s => s.PauseAsync(owner, world.YesNo, Wed18, Thu19, ReasonText("today"), None))).Outcome);
        var upcoming = await PausesAsync(services, s => s.PauseAsync(owner, world.YesNo, Mon23, Fri27, ReasonText("upcoming"), None));
        Assert.Equal(PauseOutcome.Paused, upcoming.Outcome);
        var upcomingPause = upcoming.Pause!.Id;

        // A store outcome that the use case does not have.
        faults.Member = nameof(IRecordStore.ExcuseAsync);
        await FailsAsync<ExcusalService, ExcuseResult>(s => s.ExcuseAsync(owner, world.YesNo, Mon16, ReasonText("odd outcome"), None),
            "The record store gave an outcome that excusing does not have.");
        faults.Member = nameof(IRecordStore.RemoveExcusalAsync);
        await FailsAsync<ExcusalService, RemoveExcusalResult>(s => s.RemoveExcusalAsync(owner, world.Daily, Tue17, None),
            "The record store gave an outcome that removing an excusal does not have.");
        faults.Member = nameof(IRecordStore.AddPauseAsync);
        await FailsAsync<PauseService, PauseResult>(s => s.PauseAsync(owner, world.Daily, Sat28, Sun29, ReasonText("odd outcome"), None),
            "The record store gave an outcome that pausing does not have.");
        faults.Member = nameof(IRecordStore.SetPauseLastDayAsync);
        await FailsAsync<PauseService, ResumeResult>(s => s.ResumeAsync(owner, world.Weekly, None),
            "The record store gave an outcome that resuming does not have.");
        faults.Member = nameof(IRecordStore.DeletePauseAsync);
        await FailsAsync<PauseService, ResumeResult>(s => s.ResumeAsync(owner, world.YesNo, None),
            "The record store gave an outcome that resuming does not have.");
        await FailsAsync<PauseService, CancelPauseResult>(s => s.CancelPauseAsync(owner, world.YesNo, upcomingPause, None),
            "The record store gave an outcome that cancelling a pause does not have.");
        faults.Member = null;

        // Rows that the code must refuse to read: a pause that ends before it starts, and an excusal with a blank reason.
        // Each is removed again after its reads fail.
        var badPause = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalPauses] ([Id], [GoalId], [FirstDay], [LastDay], [Reason]) VALUES (@id, @goal, @first, @last, @reason)",
            ("@id", badPause), ("@goal", world.Daily.Value), ("@first", Sat28.ToDateTime(TimeOnly.MinValue)), ("@last", Sat21.ToDateTime(TimeOnly.MinValue)),
            ("@reason", ReasonText("backwards")));
        try
        {
            var message = $"The stored pause {badPause} ends before it starts.";
            await FailsAsync<PauseService, PausesResult>(s => s.GetPausesAsync(owner, world.Daily, None), message);
            await FailsAsync<RecordingService, HistoryResult>(s => s.GetHistoryAsync(owner, world.Daily, Mon16, Sun22, None), message);
            await FailsAsync<ExcusalService, ExcuseResult>(s => s.ExcuseAsync(owner, world.Daily, Wed18, ReasonText("broken"), None), message);
            await FailsAsync<IRecordStore, GoalExemptions>(s => s.ReadExemptionsAsync(account, world.Daily, None), message);
        }
        finally
        {
            await Sql.ExecuteAsync(database.ConnectionString, "DELETE FROM [GoalPauses] WHERE [Id] = @id", ("@id", badPause));
        }

        await Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalExcusals] ([GoalId], [Date], [Reason]) VALUES (@goal, @date, N'   ')",
            ("@goal", world.YesNo.Value), ("@date", Fri20.ToDateTime(TimeOnly.MinValue)));
        try
        {
            var message = $"The stored excusal of goal {world.YesNo.Value} for 2026-11-20 has no valid reason.";
            await FailsAsync<PauseService, PausesResult>(s => s.GetPausesAsync(owner, world.YesNo, None), message);
            await FailsAsync<ReportService, GoalReportResult>(s => s.GetGoalReportAsync(owner, world.YesNo, None), message);
        }
        finally
        {
            await Sql.ExecuteAsync(database.ConnectionString,
                "DELETE FROM [GoalExcusals] WHERE [GoalId] = @goal AND [Date] = @date",
                ("@goal", world.YesNo.Value), ("@date", Fri20.ToDateTime(TimeOnly.MinValue)));
        }

        exceptions.AddRange(await DatabaseFaultsAsync(services, world, upcomingPause));
        AssertNoProtectedValue(capture, exceptions, []);
    }

    /// <summary>
    /// SQL errors whose text carries the reason written: test triggers on this test's goals' excusals and pauses (insert,
    /// update and delete) raise error 50000 with the reason. Each write of a use case and of the store then fails with the
    /// store's own exception, naming the error number only, and rolls back. A plain SQL update first shows that the
    /// forced error does carry the reason. The triggers are dropped afterwards.
    /// </summary>
    private async Task<IReadOnlyList<Exception>> DatabaseFaultsAsync(IServiceProvider services, World world, PauseId upcomingPause)
    {
        var owner = world.User;
        var account = owner.AccountId!.Value;
        var goals = new[] { world.Daily, world.Weekly, world.YesNo };
        var exceptions = new List<Exception>();
        var before = await RowsAsync(goals);
        await Sql.ExecuteAsync(database.ConnectionString, Trigger("GoalExcusals", goals));
        await Sql.ExecuteAsync(database.ConnectionString, Trigger("GoalPauses", goals));
        try
        {
            var forced = await Assert.ThrowsAsync<SqlException>(() => Sql.ExecuteAsync(database.ConnectionString,
                "UPDATE [GoalExcusals] SET [Reason] = [Reason] WHERE [GoalId] = @goal", ("@goal", world.Daily.Value)));
            Assert.Contains(ReasonMarker, forced.Message, StringComparison.Ordinal);

            var writes = new Func<IServiceProvider, Task>[]
            {
                p => ExcusalsAsync(p, s => s.ExcuseAsync(owner, world.YesNo, Mon16, ReasonText("new excusal"), None)),
                p => ExcusalsAsync(p, s => s.RemoveExcusalAsync(owner, world.Daily, Tue17, None)),
                p => PausesAsync(p, s => s.PauseAsync(owner, world.Daily, Sat28, Sun29, ReasonText("new pause"), None)),
                p => PausesAsync(p, s => s.ResumeAsync(owner, world.Weekly, None)),
                p => PausesAsync(p, s => s.ResumeAsync(owner, world.YesNo, None)),
                p => PausesAsync(p, s => s.CancelPauseAsync(owner, world.YesNo, upcomingPause, None)),
                p => StoreAsync(p, s => s.ExcuseAsync(account, world.Daily, Mon9, Reason("store excusal"), None)),
                p => StoreAsync(p, s => s.RemoveExcusalAsync(account, world.Daily, Tue17, None)),
                p => StoreAsync(p, s => s.AddPauseAsync(account, world.Daily, Sat28, Sun29, Reason("store pause"), None)),
                p => StoreAsync(p, s => s.SetPauseLastDayAsync(account, world.YesNo, upcomingPause, Mon23, None)),
                p => StoreAsync(p, s => s.DeletePauseAsync(account, world.YesNo, upcomingPause, None)),
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
            await Sql.ExecuteAsync(database.ConnectionString, "DROP TRIGGER IF EXISTS [dbo].[TR_It_Exl_GoalExcusals_Fault]");
            await Sql.ExecuteAsync(database.ConnectionString, "DROP TRIGGER IF EXISTS [dbo].[TR_It_Exl_GoalPauses_Fault]");
        }

        Assert.Equal(before, await RowsAsync(goals));
        return exceptions;
    }

    /// <summary>Every excusal and pause row of <paramref name="goals"/>, as one text per row, in a stable order.</summary>
    private async Task<IReadOnlyList<string>> RowsAsync(IEnumerable<GoalId> goals)
    {
        var rows = new List<string>();
        foreach (var goal in goals)
        {
            var excusals = await Sql.QueryAsync(database.ConnectionString,
                "SELECT [GoalId], [Date], [Reason] FROM [GoalExcusals] WHERE [GoalId] = @goal ORDER BY [Date]", ("@goal", goal.Value));
            var pauses = await Sql.QueryAsync(database.ConnectionString,
                "SELECT [Id], [GoalId], [FirstDay], [LastDay], [Reason] FROM [GoalPauses] WHERE [GoalId] = @goal ORDER BY [FirstDay]", ("@goal", goal.Value));
            rows.AddRange(excusals.Concat(pauses).Select(r => string.Join("|", r.Values)));
        }

        return rows;
    }

    private static string Trigger(string table, IEnumerable<GoalId> goals) => $"""
        CREATE TRIGGER [dbo].[TR_It_Exl_{table}_Fault] ON [dbo].[{table}] AFTER INSERT, UPDATE, DELETE AS
        BEGIN
            SET NOCOUNT ON;
            DECLARE @message nvarchar(2048) = (
                SELECT TOP (1) CONCAT(N'Forced fault: ', x.[Reason])
                FROM (SELECT [GoalId], [Reason] FROM inserted UNION ALL SELECT [GoalId], [Reason] FROM deleted) x
                WHERE x.[GoalId] IN ({string.Join(", ", goals.Select(g => $"'{g.Value}'"))}));
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
    /// The real record store, except that the exemption member <see cref="FaultSwitch.Member"/> names gives, without
    /// writing, an outcome that its use case does not have.
    /// </summary>
    private sealed class FaultingRecordStore(IRecordStore inner, FaultSwitch faults) : IRecordStore
    {
        public Task<RecordStoreWrite> AddAsync(
            AccountId owner, GoalId goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value, NoteWrite note, int limit,
            CancellationToken cancellationToken) =>
            inner.AddAsync(owner, goal, entry, isCommitment, date, value, note, limit, cancellationToken);

        public Task<DateOnly?> FindRecordDateAsync(AccountId owner, GoalId goal, RecordId record, CancellationToken cancellationToken) =>
            inner.FindRecordDateAsync(owner, goal, record, cancellationToken);

        public Task<RecordStoreWrite> ChangeAsync(
            AccountId owner, GoalId goal, RecordId record, EntrySpan entry, RecordValue value, NoteWrite note, CancellationToken cancellationToken) =>
            inner.ChangeAsync(owner, goal, record, entry, value, note, cancellationToken);

        public Task<RecordStoreWrite> DeleteAsync(AccountId owner, GoalId goal, RecordId record, EntrySpan entry, CancellationToken cancellationToken) =>
            inner.DeleteAsync(owner, goal, record, entry, cancellationToken);

        public Task<RecordStoreWrite> SaveNoteAsync(AccountId owner, GoalId goal, EntrySpan entry, NoteText? note, CancellationToken cancellationToken) =>
            inner.SaveNoteAsync(owner, goal, entry, note, cancellationToken);

        public Task<StoredEntries> ReadAsync(AccountId owner, GoalId goal, DateOnly from, DateOnly to, CancellationToken cancellationToken) =>
            inner.ReadAsync(owner, goal, from, to, cancellationToken);

        public Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(AccountId owner, GoalId goal, int count, CancellationToken cancellationToken) =>
            inner.ReadRecentNoteKeysAsync(owner, goal, count, cancellationToken);

        public Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(
            AccountId owner, GoalId goal, DateOnly from, DateOnly? through, int count, CancellationToken cancellationToken) =>
            inner.ReadRecentNoteKeysAsync(owner, goal, from, through, count, cancellationToken);

        public Task<GoalExemptions> ReadExemptionsAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken) =>
            inner.ReadExemptionsAsync(owner, goal, cancellationToken);

        public Task<ExemptionStoreWrite> ExcuseAsync(
            AccountId owner, GoalId goal, DateOnly date, ExemptionReason? reason, CancellationToken cancellationToken) =>
            faults.Member == nameof(ExcuseAsync)
                ? Task.FromResult(ExemptionStoreWrite.PauseNotFound)
                : inner.ExcuseAsync(owner, goal, date, reason, cancellationToken);

        public Task<ExemptionStoreWrite> RemoveExcusalAsync(AccountId owner, GoalId goal, DateOnly date, CancellationToken cancellationToken) =>
            faults.Member == nameof(RemoveExcusalAsync)
                ? Task.FromResult(ExemptionStoreWrite.PauseNotFound)
                : inner.RemoveExcusalAsync(owner, goal, date, cancellationToken);

        public Task<ExemptionStoreWrite> AddPauseAsync(
            AccountId owner, GoalId goal, DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason, CancellationToken cancellationToken) =>
            faults.Member == nameof(AddPauseAsync)
                ? Task.FromResult(ExemptionStoreWrite.NotExcused)
                : inner.AddPauseAsync(owner, goal, firstDay, lastDay, reason, cancellationToken);

        public Task<ExemptionStoreWrite> SetPauseLastDayAsync(
            AccountId owner, GoalId goal, PauseId pause, DateOnly lastDay, CancellationToken cancellationToken) =>
            faults.Member == nameof(SetPauseLastDayAsync)
                ? Task.FromResult(ExemptionStoreWrite.NotExcused)
                : inner.SetPauseLastDayAsync(owner, goal, pause, lastDay, cancellationToken);

        public Task<ExemptionStoreWrite> DeletePauseAsync(AccountId owner, GoalId goal, PauseId pause, CancellationToken cancellationToken) =>
            faults.Member == nameof(DeletePauseAsync)
                ? Task.FromResult(ExemptionStoreWrite.NotExcused)
                : inner.DeletePauseAsync(owner, goal, pause, cancellationToken);
    }
}
