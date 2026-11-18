using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.AcceptanceTests.Support.F004;

/// <summary>
/// Builds the application without HT.Ui (ADR 0003: the persistence composition entry point plus the
/// application registration) and runs each use case in its own service scope, as one request would.
/// The current time is supplied by a <see cref="FakeTimeProvider"/> (constitution II). Acceptance tests supply no
/// default time zone, so the installation's default is UTC (spec FR-025).
/// </summary>
public sealed class F004Driver : IDisposable
{
    /// <summary>The time of every step that gives none (acceptance.md, "Time").</summary>
    public static readonly DateTimeOffset DefaultNow = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    /// <summary>No default is configured, so the installation's default time zone is UTC (FR-025).</summary>
    public static readonly InstallationTimeZone DefaultZone = InstallationTimeZone.From(null);

    private static readonly CancellationToken None = CancellationToken.None;

    private readonly ServiceProvider _services;

    public F004Driver()
    {
        Time = new FakeTimeProvider(DefaultNow);

        var services = new ServiceCollection();
        services.AddHtPersistence(F004Database.ConnectionString);
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

    /// <summary>The reset entry point of ADR 0003: drops and rebuilds the HT-AC schema.</summary>
    public static Task ResetDatabaseAsync() => HtDatabase.ResetAsync(F004Database.ConnectionString, None);

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

    // ------------------------------------------------------------ profile: time zone, name, Home heading

    public Task<TimeZoneReadResult> GetTimeZoneAsync(ActingUser actingUser) =>
        InScopeAsync(services => Profile(services).GetTimeZoneAsync(actingUser, None));

    public Task<TimeZoneChangeResult> ChangeTimeZoneAsync(ActingUser actingUser, string? timeZone) =>
        InScopeAsync(services => Profile(services).ChangeTimeZoneAsync(actingUser, timeZone, None));

    public Task<NameChangeResult> ChangeNameAsync(ActingUser actingUser, string? name) =>
        InScopeAsync(services => Profile(services).ChangeNameAsync(actingUser, name, None));

    public Task<ProfileReadResult> GetProfileAsync(ActingUser actingUser) =>
        InScopeAsync(services => Profile(services).GetProfileAsync(actingUser, None));

    public Task<HomeHeadingResult> GetHomeHeadingAsync(ActingUser actingUser) =>
        InScopeAsync(services => Profile(services).GetHomeHeadingAsync(actingUser, None));

    // ------------------------------------------------------------ goals

    public Task<GoalStartResult> StartNewGoalAsync(ActingUser actingUser, GoalHabit? habit, int? position) =>
        InScopeAsync(services => Goals(services).StartNewGoalAsync(actingUser, habit, position, None));

    public Task<GoalCreateResult> CreateGoalAsync(ActingUser actingUser, GoalInput input) =>
        InScopeAsync(services => Goals(services).CreateAsync(actingUser, input, None));

    public Task<GoalListResult> ListGoalsAsync(ActingUser actingUser) =>
        InScopeAsync(services => Goals(services).ListAsync(actingUser, None));

    public Task<GoalReadResult> GetGoalAsync(ActingUser actingUser, GoalId id) =>
        InScopeAsync(services => Goals(services).GetAsync(actingUser, id, None));

    public Task<GoalCalendarResult> GetCalendarAsync(ActingUser actingUser, GoalId id, DateOnly first, DateOnly last) =>
        InScopeAsync(services => Goals(services).GetCalendarAsync(actingUser, id, first, last, None));

    // ------------------------------------------------------------ public catalog

    public Task<CatalogBrowseResult> BrowseAsync(CatalogFilter filter) =>
        InScopeAsync(services => Catalog(services).BrowseAsync(filter, None));

    public Task<CatalogBrowseResult> BrowseAsync(ActingUser actingUser, CatalogFilter filter, bool onlyTracked) =>
        InScopeAsync(services => Catalog(services).BrowseAsync(actingUser, filter, onlyTracked, None));

    public Task<CatalogHabitDetailsResult> GetDetailsAsync(HabitId id) =>
        InScopeAsync(services => Catalog(services).GetDetailsAsync(id, None));

    public Task<CatalogHabitDetailsResult> GetDetailsAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Catalog(services).GetDetailsAsync(actingUser, id, None));

    // ------------------------------------------------------------ catalog administration

    public Task<CatalogHabitAdminResult> GetCatalogHabitAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Administration(services).GetAsync(actingUser, id, None));

    public Task<CatalogHabitCreateResult> CreateCatalogHabitAsync(ActingUser actingUser, CatalogHabitInput input) =>
        InScopeAsync(services => Administration(services).CreateAsync(actingUser, input, None));

    public Task<CatalogHabitEditResult> EditCatalogHabitAsync(ActingUser actingUser, HabitId id, CatalogHabitInput input) =>
        InScopeAsync(services => Administration(services).EditAsync(actingUser, id, input, None));

    public Task<CatalogHabitPublishResult> PublishAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Administration(services).PublishAsync(actingUser, id, None));

    public Task<CatalogHabitUnpublishResult> UnpublishAsync(ActingUser actingUser, HabitId id, bool confirmed) =>
        InScopeAsync(services => Administration(services).UnpublishAsync(actingUser, id, confirmed, None));

    // ------------------------------------------------------------ custom habits

    public Task<CustomHabitResult> GetCustomHabitAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => CustomHabits(services).GetAsync(actingUser, id, None));

    public Task<CustomHabitCreateResult> CreateCustomHabitAsync(ActingUser actingUser, HabitInput input) =>
        InScopeAsync(services => CustomHabits(services).CreateAsync(actingUser, input, None));

    public Task<CustomHabitEditResult> EditCustomHabitAsync(ActingUser actingUser, HabitId id, HabitInput input) =>
        InScopeAsync(services => CustomHabits(services).EditAsync(actingUser, id, input, None));

    public Task<CustomHabitDeleteResult> DeleteCustomHabitAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => CustomHabits(services).DeleteAsync(actingUser, id, None));

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

    private static CatalogService Catalog(IServiceProvider services) =>
        new(services.GetRequiredService<ICatalogHabitStore>(), services.GetRequiredService<IGoalStore>());

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
    private sealed class CurrentTimeProvider(F004Driver driver) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => driver.Now;
    }
}
