using HT.Application;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using GoalOutcome = HT.Domain.Goals.Outcome;

namespace HT.AcceptanceTests.Support.F009;

/// <summary>
/// Builds the application without HT.Ui (ADR 0003: the persistence composition entry point plus the application
/// registration) and runs each use case in its own service scope, as one request would. The current time is supplied
/// by a <see cref="FakeTimeProvider"/> (constitution II). The acceptance tests configure no default time zone, so the
/// installation's default is UTC (acceptance.md, "Demo data").
/// </summary>
public sealed class F009Driver : IDisposable
{
    /// <summary>The default goal-creation time of acceptance.md ("Habits and goals").</summary>
    public static readonly DateTimeOffset DefaultGoalCreation = new(2026, 11, 2, 8, 0, 0, TimeSpan.Zero);

    /// <summary>The default start date of acceptance.md ("Habits and goals").</summary>
    public static readonly DateOnly DefaultStartDate = new(2026, 11, 2);

    /// <summary>No default is configured, so the installation's default time zone is UTC.</summary>
    public static readonly InstallationTimeZone DefaultZone = InstallationTimeZone.From(null);

    private static readonly CancellationToken None = CancellationToken.None;

    private readonly ServiceProvider _services;

    public F009Driver()
    {
        Time = new FakeTimeProvider(DefaultGoalCreation);

        var services = new ServiceCollection();
        services.AddHtPersistence(F009Database.ConnectionString);
        services.AddHtApplication();
        services.AddSingleton<TimeProvider>(new CurrentTimeProvider(this));
        _services = services.BuildServiceProvider();
    }

    /// <summary>The supplied current time of the step that runs now.</summary>
    public FakeTimeProvider Time { get; private set; }

    public DateTimeOffset Now => Time.GetUtcNow();

    /// <summary>
    /// Sets the supplied current time. A fake time provider only moves forward, so a step that runs at an earlier time
    /// than the one before it gets a fresh provider started at its time.
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

    /// <summary>The reset entry point of ADR 0003: drops and rebuilds the HT-AC schema.</summary>
    public static Task ResetDatabaseAsync() => HtDatabase.ResetAsync(F009Database.ConnectionString, None);

    // ------------------------------------------------------------ accounts and operator capabilities

    public Task<RegistrationResult> RegisterAsync(string? email, string? password, string? timeZone, string? name) =>
        InScopeAsync(services => Accounts(services).RegisterAsync(email, password, timeZone, name, None));

    public Task<SignInResult> SignInAsync(string? email, string? password) =>
        InScopeAsync(services => Accounts(services).SignInAsync(email, password, None));

    public Task<AdministratorCreationResult> CreateAdministratorAsync(string? email, string? password) =>
        InScopeAsync(services => new OperatorService(services.GetRequiredService<IAccountStore>(), Time, DefaultZone)
            .CreateAdministratorAsync(email, password, None));

    public Task<TimeZoneChangeResult> ChangeTimeZoneAsync(ActingUser actingUser, string? timeZone) =>
        InScopeAsync(services => Profile(services).ChangeTimeZoneAsync(actingUser, timeZone, None));

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

    public Task<DemoReportLoadingResult> LoadDemoReportAsync(string? demoPassword) =>
        InScopeAsync(services => new DemoReportService(
                services.GetRequiredService<IAccountStore>(),
                services.GetRequiredService<IDemoReportStore>(),
                Time,
                DefaultZone)
            .LoadAsync(demoPassword, None));

    public Task<DemoPauseLoadingResult> LoadDemoPausesAsync(string? demoPassword) =>
        InScopeAsync(services => new DemoPauseService(
                services.GetRequiredService<IDemoPauseStore>(),
                services.GetRequiredService<IAccountStore>(),
                Time,
                DefaultZone)
            .LoadAsync(demoPassword, None));

    /// <summary>FR-031: demo goal-change loading.</summary>
    public Task<DemoGoalChangeLoadingResult> LoadDemoGoalChangesAsync(string? demoPassword) =>
        InScopeAsync(services => new DemoGoalChangeService(
                services.GetRequiredService<IDemoGoalChangeStore>(),
                services.GetRequiredService<IAccountStore>(),
                Time,
                DefaultZone)
            .LoadAsync(demoPassword, None));

    // ------------------------------------------------------------ habits and catalog

    public Task<CustomHabitCreateResult> CreateCustomHabitAsync(ActingUser actingUser, HabitInput input) =>
        InScopeAsync(services => CustomHabits(services).CreateAsync(actingUser, input, None));

    public Task<CustomHabitResult> GetCustomHabitAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => CustomHabits(services).GetAsync(actingUser, id, None));

    public Task<CustomHabitEditResult> EditCustomHabitAsync(ActingUser actingUser, HabitId id, HabitInput input) =>
        InScopeAsync(services => CustomHabits(services).EditAsync(actingUser, id, input, None));

    public Task<CustomHabitDeleteResult> DeleteCustomHabitAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => CustomHabits(services).DeleteAsync(actingUser, id, None));

    public Task<CatalogHabitCreateResult> CreateCatalogHabitAsync(ActingUser actingUser, CatalogHabitInput input) =>
        InScopeAsync(services => Administration(services).CreateAsync(actingUser, input, None));

    public Task<CatalogHabitPublishResult> PublishAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Administration(services).PublishAsync(actingUser, id, None));

    public Task<CatalogHabitUnpublishResult> UnpublishAsync(ActingUser actingUser, HabitId id, bool confirmed) =>
        InScopeAsync(services => Administration(services).UnpublishAsync(actingUser, id, confirmed, None));

    public Task<CatalogHabitListResult> ListCatalogHabitsAsync(ActingUser actingUser, string? search) =>
        InScopeAsync(services => Administration(services).ListAsync(actingUser, search, None));

    /// <summary>The public catalog browsing use case (no acting user: the anonymous visitor's view).</summary>
    public Task<CatalogBrowseResult> BrowseAsync(CatalogFilter filter) =>
        InScopeAsync(services => Catalog(services).BrowseAsync(filter, None));

    public Task<CatalogBrowseResult> BrowseAsync(ActingUser actingUser, CatalogFilter filter, bool onlyTracked) =>
        InScopeAsync(services => Catalog(services).BrowseAsync(actingUser, filter, onlyTracked, None));

    public Task<CatalogHabitDetailsResult> GetCatalogDetailsAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Catalog(services).GetDetailsAsync(actingUser, id, None));

    // ------------------------------------------------------------ goals (slice 4)

    public Task<GoalCreateResult> CreateGoalAsync(ActingUser actingUser, GoalInput input) =>
        InScopeAsync(services => Goals(services).CreateAsync(actingUser, input, None));

    public Task<GoalListResult> ListGoalsAsync(ActingUser actingUser) =>
        InScopeAsync(services => Goals(services).ListAsync(actingUser, None));

    public Task<GoalReadResult> GetGoalAsync(ActingUser actingUser, GoalId goal) =>
        InScopeAsync(services => Goals(services).GetAsync(actingUser, goal, None));

    public Task<GoalCalendarResult> GetCalendarAsync(ActingUser actingUser, GoalId goal, DateOnly first, DateOnly last) =>
        InScopeAsync(services => Goals(services).GetCalendarAsync(actingUser, goal, first, last, None));

    // ------------------------------------------------------------ goal changes (this slice)

    public Task<SettingsChangeResult> ChangeSettingsAsync(ActingUser actingUser, GoalId goal, GoalSettingsInput input) =>
        InScopeAsync(services => GoalChanges(services).ChangeSettingsAsync(actingUser, goal, input, None));

    public Task<GoalEndResult> EndGoalAsync(ActingUser actingUser, GoalId goal, GoalOutcome? outcome, string? note) =>
        InScopeAsync(services => GoalChanges(services).EndAsync(actingUser, goal, outcome, note, None));

    public Task<NewAttemptValuesResult> GetNewAttemptValuesAsync(ActingUser actingUser, GoalId goal) =>
        InScopeAsync(services => GoalChanges(services).GetNewAttemptValuesAsync(actingUser, goal, None));

    public Task<AttemptStartResult> StartAttemptAsync(ActingUser actingUser, GoalId goal, NewAttemptInput input) =>
        InScopeAsync(services => GoalChanges(services).StartAttemptAsync(actingUser, goal, input, None));

    public Task<AttemptListResult> GetAttemptsAsync(ActingUser actingUser, GoalId goal) =>
        InScopeAsync(services => Attempts(services).GetAttemptsAsync(actingUser, goal, None));

    public Task<AttemptReadResult> GetAttemptAsync(ActingUser actingUser, GoalId goal, int number) =>
        InScopeAsync(services => Attempts(services).GetAttemptAsync(actingUser, goal, number, None));

    public Task<GoalHistoryListResult> GetGoalHistoryAsync(ActingUser actingUser) =>
        InScopeAsync(services => Attempts(services).GetGoalHistoryAsync(actingUser, None));

    // ------------------------------------------------------------ recording (slice 5)

    public Task<RecordWriteResult> RecordAsync(ActingUser actingUser, GoalId goal, DateOnly date, RecordInput input) =>
        InScopeAsync(services => Recording(services).RecordAsync(actingUser, goal, date, input, None));

    public Task<RecordWriteResult> ChangeRecordAsync(ActingUser actingUser, GoalId goal, RecordId record, RecordInput input) =>
        InScopeAsync(services => Recording(services).ChangeRecordAsync(actingUser, goal, record, input, None));

    public Task<RecordWriteResult> DeleteRecordAsync(ActingUser actingUser, GoalId goal, RecordId record) =>
        InScopeAsync(services => Recording(services).DeleteRecordAsync(actingUser, goal, record, None));

    public Task<NoteSaveResult> SaveNoteAsync(ActingUser actingUser, GoalId goal, DateOnly date, string? note) =>
        InScopeAsync(services => Recording(services).SaveNoteAsync(actingUser, goal, date, note, None));

    public Task<HistoryResult> GetHistoryAsync(ActingUser actingUser, GoalId goal, DateOnly first, DateOnly last) =>
        InScopeAsync(services => Recording(services).GetHistoryAsync(actingUser, goal, first, last, None));

    public Task<RecordableResult> GetRecordableAsync(ActingUser actingUser, GoalId goal) =>
        InScopeAsync(services => Recording(services).GetRecordableAsync(actingUser, goal, None));

    public Task<RecentNotesResult> GetRecentNotesAsync(ActingUser actingUser, GoalId goal) =>
        InScopeAsync(services => Recording(services).GetRecentNotesAsync(actingUser, goal, None));

    // ------------------------------------------------------------ reports (slice 6)

    public Task<GoalReportResult> GetGoalReportAsync(ActingUser actingUser, GoalId goal) =>
        InScopeAsync(services => Reports(services).GetGoalReportAsync(actingUser, goal, None));

    public Task<GoalsOverviewResult> GetGoalsOverviewAsync(ActingUser actingUser) =>
        InScopeAsync(services => Reports(services).GetGoalsOverviewAsync(actingUser, None));

    public Task<DashboardResult> GetDashboardAsync(ActingUser actingUser) =>
        InScopeAsync(services => Reports(services).GetDashboardAsync(actingUser, None));

    // ------------------------------------------------------------ excusals and pauses (slice 7)

    public Task<ExcuseResult> ExcuseAsync(ActingUser actingUser, GoalId goal, DateOnly date, string? reason) =>
        InScopeAsync(services => Excusals(services).ExcuseAsync(actingUser, goal, date, reason, None));

    public Task<RemoveExcusalResult> RemoveExcusalAsync(ActingUser actingUser, GoalId goal, DateOnly date) =>
        InScopeAsync(services => Excusals(services).RemoveExcusalAsync(actingUser, goal, date, None));

    public Task<PauseResult> PauseAsync(ActingUser actingUser, GoalId goal, DateOnly? firstDay, DateOnly? lastDay, string? reason) =>
        InScopeAsync(services => Pauses(services).PauseAsync(actingUser, goal, firstDay, lastDay, reason, None));

    public Task<PausesResult> GetPausesAsync(ActingUser actingUser, GoalId goal) =>
        InScopeAsync(services => Pauses(services).GetPausesAsync(actingUser, goal, None));

    public void Dispose() => _services.Dispose();

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

    private GoalChangeService GoalChanges(IServiceProvider services) =>
        new(
            services.GetRequiredService<IGoalStore>(),
            services.GetRequiredService<IAccountStore>(),
            Time,
            DefaultZone);

    private AttemptService Attempts(IServiceProvider services) =>
        new(
            services.GetRequiredService<IGoalStore>(),
            services.GetRequiredService<IRecordStore>(),
            services.GetRequiredService<IAccountStore>(),
            Time);

    private RecordingService Recording(IServiceProvider services) =>
        new(
            services.GetRequiredService<IRecordStore>(),
            services.GetRequiredService<IGoalStore>(),
            services.GetRequiredService<IAccountStore>(),
            Time);

    private ReportService Reports(IServiceProvider services) =>
        new(
            services.GetRequiredService<IGoalStore>(),
            services.GetRequiredService<IRecordStore>(),
            services.GetRequiredService<IAccountStore>(),
            Time,
            DefaultZone);

    private ExcusalService Excusals(IServiceProvider services) =>
        new(
            services.GetRequiredService<IRecordStore>(),
            services.GetRequiredService<IGoalStore>(),
            services.GetRequiredService<IAccountStore>(),
            Time);

    private PauseService Pauses(IServiceProvider services) =>
        new(
            services.GetRequiredService<IRecordStore>(),
            services.GetRequiredService<IGoalStore>(),
            services.GetRequiredService<IAccountStore>(),
            Time,
            DefaultZone);

    private CatalogService Catalog(IServiceProvider services) =>
        new(services.GetRequiredService<ICatalogHabitStore>(), services.GetRequiredService<IGoalStore>(), Time);

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
    private sealed class CurrentTimeProvider(F009Driver driver) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => driver.Now;
    }
}
