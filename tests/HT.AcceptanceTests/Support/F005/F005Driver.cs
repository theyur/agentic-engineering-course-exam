using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.AcceptanceTests.Support.F005;

/// <summary>
/// Builds the application without HT.Ui (ADR 0003: the persistence composition entry point plus the
/// application registration) and runs each use case in its own service scope, as one request would.
/// The current time is supplied by a <see cref="FakeTimeProvider"/> (constitution II). Acceptance tests supply no
/// default time zone, so the installation's default is UTC (acceptance.md, "Demo data").
/// </summary>
public sealed class F005Driver : IDisposable
{
    /// <summary>The time of every step that gives none (acceptance.md, "Time").</summary>
    public static readonly DateTimeOffset DefaultNow = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    /// <summary>No default is configured, so the installation's default time zone is UTC.</summary>
    public static readonly InstallationTimeZone DefaultZone = InstallationTimeZone.From(null);

    private static readonly CancellationToken None = CancellationToken.None;

    private readonly ServiceProvider _services;

    /// <summary>How many use cases of this test were called with each goal identifier.</summary>
    private readonly Dictionary<GoalId, int> _calls = new();

    public F005Driver()
    {
        Time = new FakeTimeProvider(DefaultNow);

        var services = new ServiceCollection();
        services.AddHtPersistence(F005Database.ConnectionString);
        services.AddHtApplication();
        services.AddSingleton<TimeProvider>(new CurrentTimeProvider(this));
        _services = services.BuildServiceProvider();
    }

    /// <summary>The supplied current time of the step that runs now.</summary>
    public FakeTimeProvider Time { get; private set; }

    public DateTimeOffset Now => Time.GetUtcNow();

    /// <summary>
    /// Sets the supplied current time. A fake time provider only moves forward, so a step that runs at an earlier
    /// time than the one before it gets a fresh provider started at its time.
    /// </summary>
    public void SetNow(DateTimeOffset now)
    {
        if (now >= Time.GetUtcNow())
        {
            Time.SetUtcNow(now);
        }
        else
        {
            Time = new FakeTimeProvider(now);
        }
    }

    /// <summary>The number of use-case calls this test made with a goal identifier.</summary>
    public int Calls(GoalId goal) => _calls.TryGetValue(goal, out var count) ? count : 0;

    /// <summary>The reset entry point of ADR 0003: drops and rebuilds the HT-AC schema.</summary>
    public static Task ResetDatabaseAsync() => HtDatabase.ResetAsync(F005Database.ConnectionString, None);

    // ------------------------------------------------------------ accounts and operator capabilities

    public Task<RegistrationResult> RegisterAsync(string? email, string? password, string? timeZone, string? name) =>
        InScopeAsync(services => Accounts(services).RegisterAsync(email, password, timeZone, name, None));

    public Task<SignInResult> SignInAsync(string? email, string? password) =>
        InScopeAsync(services => Accounts(services).SignInAsync(email, password, None));

    public Task<AdministratorCreationResult> CreateAdministratorAsync(string? email, string? password) =>
        InScopeAsync(services => new OperatorService(services.GetRequiredService<IAccountStore>(), Time, DefaultZone)
            .CreateAdministratorAsync(email, password, None));

    public Task<DemoLoadingResult> LoadDemoDataAsync(string? demoPassword) =>
        InScopeAsync(services => new DemoDataService(
                services.GetRequiredService<IAccountStore>(),
                services.GetRequiredService<IDemoDataStore>(),
                Time,
                DefaultZone)
            .LoadAsync(demoPassword, None));

    public Task<DemoHistoryLoadingResult> LoadDemoHistoryAsync(string? demoPassword) =>
        InScopeAsync(services => new DemoHistoryService(
                services.GetRequiredService<IAccountStore>(),
                services.GetRequiredService<IDemoHistoryStore>(),
                Time,
                DefaultZone)
            .LoadAsync(demoPassword, None));

    public Task<TimeZoneChangeResult> ChangeTimeZoneAsync(ActingUser actingUser, string? timeZone) =>
        InScopeAsync(services => Profile(services).ChangeTimeZoneAsync(actingUser, timeZone, None));

    public Task<ProfileReadResult> GetProfileAsync(ActingUser actingUser) =>
        InScopeAsync(services => Profile(services).GetProfileAsync(actingUser, None));

    // ------------------------------------------------------------ habits

    public Task<CustomHabitCreateResult> CreateCustomHabitAsync(ActingUser actingUser, HabitInput input) =>
        InScopeAsync(services => CustomHabits(services).CreateAsync(actingUser, input, None));

    public Task<CustomHabitListResult> ListCustomHabitsAsync(ActingUser actingUser) =>
        InScopeAsync(services => CustomHabits(services).ListAsync(actingUser, None));

    public Task<CatalogHabitCreateResult> CreateCatalogHabitAsync(ActingUser actingUser, CatalogHabitInput input) =>
        InScopeAsync(services => Administration(services).CreateAsync(actingUser, input, None));

    public Task<CatalogHabitPublishResult> PublishAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Administration(services).PublishAsync(actingUser, id, None));

    public Task<CatalogHabitUnpublishResult> UnpublishAsync(ActingUser actingUser, HabitId id, bool confirmed) =>
        InScopeAsync(services => Administration(services).UnpublishAsync(actingUser, id, confirmed, None));

    public Task<CatalogHabitAdminResult> GetCatalogHabitAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Administration(services).GetAsync(actingUser, id, None));

    // ------------------------------------------------------------ goals (slice 4)

    public Task<GoalCreateResult> CreateGoalAsync(ActingUser actingUser, GoalInput input) =>
        InScopeAsync(services => Goals(services).CreateAsync(actingUser, input, None));

    public Task<GoalListResult> ListGoalsAsync(ActingUser actingUser) =>
        InScopeAsync(services => Goals(services).ListAsync(actingUser, None));

    public Task<GoalReadResult> GetGoalAsync(ActingUser actingUser, GoalId id)
    {
        Count(id);
        return InScopeAsync(services => Goals(services).GetAsync(actingUser, id, None));
    }

    public Task<GoalCalendarResult> GetCalendarAsync(ActingUser actingUser, GoalId id, DateOnly first, DateOnly last)
    {
        Count(id);
        return InScopeAsync(services => Goals(services).GetCalendarAsync(actingUser, id, first, last, None));
    }

    // ------------------------------------------------------------ recording (this slice)

    public Task<RecordWriteResult> RecordAsync(ActingUser actingUser, GoalId goal, DateOnly date, RecordInput input)
    {
        Count(goal);
        return InScopeAsync(services => Recording(services).RecordAsync(actingUser, goal, date, input, None));
    }

    public Task<RecordWriteResult> ChangeRecordAsync(ActingUser actingUser, GoalId goal, RecordId record, RecordInput input)
    {
        Count(goal);
        return InScopeAsync(services => Recording(services).ChangeRecordAsync(actingUser, goal, record, input, None));
    }

    public Task<RecordWriteResult> DeleteRecordAsync(ActingUser actingUser, GoalId goal, RecordId record)
    {
        Count(goal);
        return InScopeAsync(services => Recording(services).DeleteRecordAsync(actingUser, goal, record, None));
    }

    public Task<NoteSaveResult> SaveNoteAsync(ActingUser actingUser, GoalId goal, DateOnly date, string? note)
    {
        Count(goal);
        return InScopeAsync(services => Recording(services).SaveNoteAsync(actingUser, goal, date, note, None));
    }

    public Task<HistoryResult> GetHistoryAsync(ActingUser actingUser, GoalId goal, DateOnly first, DateOnly last)
    {
        Count(goal);
        return InScopeAsync(services => Recording(services).GetHistoryAsync(actingUser, goal, first, last, None));
    }

    public Task<RecordableResult> GetRecordableAsync(ActingUser actingUser, GoalId goal)
    {
        Count(goal);
        return InScopeAsync(services => Recording(services).GetRecordableAsync(actingUser, goal, None));
    }

    public Task<RecentNotesResult> GetRecentNotesAsync(ActingUser actingUser, GoalId goal)
    {
        Count(goal);
        return InScopeAsync(services => Recording(services).GetRecentNotesAsync(actingUser, goal, None));
    }

    public void Dispose() => _services.Dispose();

    private void Count(GoalId goal) => _calls[goal] = Calls(goal) + 1;

    private AccountService Accounts(IServiceProvider services) =>
        new(services.GetRequiredService<IAccountStore>(), Time, DefaultZone);

    private ProfileService Profile(IServiceProvider services) =>
        new(services.GetRequiredService<IAccountStore>(), services.GetRequiredService<IGoalStore>(), Time, DefaultZone);

    private GoalService Goals(IServiceProvider services) =>
        new(
            services.GetRequiredService<IGoalStore>(),
            services.GetRequiredService<IAccountStore>(),
            services.GetRequiredService<ICatalogHabitStore>(),
            services.GetRequiredService<ICustomHabitStore>(),
            Time,
            DefaultZone);

    private RecordingService Recording(IServiceProvider services) =>
        new(
            services.GetRequiredService<IRecordStore>(),
            services.GetRequiredService<IGoalStore>(),
            services.GetRequiredService<IAccountStore>(),
            Time);

    private static CatalogAdministrationService Administration(IServiceProvider services) =>
        new(services.GetRequiredService<ICatalogHabitStore>(), services.GetRequiredService<IGoalStore>());

    private static CustomHabitService CustomHabits(IServiceProvider services) =>
        new(services.GetRequiredService<ICustomHabitStore>(), services.GetRequiredService<IGoalStore>());

    private async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> useCase)
    {
        await using var scope = _services.CreateAsyncScope();
        return await useCase(scope.ServiceProvider);
    }

    /// <summary>Gives services resolved from the container the same supplied time as the use cases.</summary>
    private sealed class CurrentTimeProvider(F005Driver driver) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => driver.Now;
    }
}
