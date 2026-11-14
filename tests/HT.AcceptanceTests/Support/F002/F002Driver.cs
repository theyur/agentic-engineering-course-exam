using HT.Application;
using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.AcceptanceTests.Support.F002;

/// <summary>
/// Builds the application without HT.Ui (ADR 0003: the persistence composition entry point plus the
/// application registration) and runs each use case in its own service scope, as one request would.
/// Sign-in reads the current time from a <see cref="FakeTimeProvider"/>; nothing in this feature depends on it.
/// </summary>
public sealed class F002Driver : IDisposable
{
    public static readonly DateTimeOffset ClockStart = new(2026, 11, 14, 0, 0, 0, TimeSpan.Zero);

    private static readonly CancellationToken None = CancellationToken.None;

    private readonly ServiceProvider _services;

    public F002Driver()
    {
        Time = new FakeTimeProvider(ClockStart);

        var services = new ServiceCollection();
        services.AddHtPersistence(F002Database.ConnectionString);
        services.AddHtApplication();
        services.AddSingleton<TimeProvider>(Time);
        _services = services.BuildServiceProvider();
    }

    public FakeTimeProvider Time { get; }

    /// <summary>The reset entry point of ADR 0003: drops and rebuilds the HT-AC schema.</summary>
    public static Task ResetDatabaseAsync() => HtDatabase.ResetAsync(F002Database.ConnectionString, None);

    // ------------------------------------------------------------ accounts and operator capabilities

    public Task<RegistrationResult> RegisterAsync(string? email, string? password) =>
        InScopeAsync(services => Accounts(services).RegisterAsync(email, password, None));

    public Task<SignInResult> SignInAsync(string? email, string? password) =>
        InScopeAsync(services => Accounts(services).SignInAsync(email, password, None));

    public Task<AdministratorCreationResult> CreateAdministratorAsync(string? email, string? password) =>
        InScopeAsync(services => new OperatorService(services.GetRequiredService<IAccountStore>())
            .CreateAdministratorAsync(email, password, None));

    public Task<DemoLoadingResult> LoadDemoDataAsync(string? demoPassword) =>
        InScopeAsync(services => new DemoDataService(
                services.GetRequiredService<IAccountStore>(),
                services.GetRequiredService<IDemoDataStore>())
            .LoadAsync(demoPassword, None));

    // ------------------------------------------------------------ public catalog (any audience)

    public Task<CatalogBrowseResult> BrowseAsync(CatalogFilter filter) =>
        InScopeAsync(services => Catalog(services).BrowseAsync(filter, None));

    public Task<HomePreviewResult> GetHomePreviewAsync() =>
        InScopeAsync(services => Catalog(services).GetHomePreviewAsync(None));

    public Task<CatalogHabitDetailsResult> GetDetailsAsync(HabitId id) =>
        InScopeAsync(services => Catalog(services).GetDetailsAsync(id, None));

    public Task<CategoryFilterResult> GetCategoryFilterAsync() =>
        InScopeAsync(services => Catalog(services).GetCategoryFilterAsync(None));

    // ------------------------------------------------------------ catalog administration

    public Task<CatalogHabitListResult> ListCatalogAsync(ActingUser actingUser, string? search) =>
        InScopeAsync(services => Administration(services).ListAsync(actingUser, search, None));

    public Task<CatalogHabitAdminResult> GetCatalogHabitAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Administration(services).GetAsync(actingUser, id, None));

    public Task<CatalogHabitCreateResult> CreateCatalogHabitAsync(ActingUser actingUser, CatalogHabitInput input) =>
        InScopeAsync(services => Administration(services).CreateAsync(actingUser, input, None));

    public Task<CatalogHabitEditResult> EditCatalogHabitAsync(ActingUser actingUser, HabitId id, CatalogHabitInput input) =>
        InScopeAsync(services => Administration(services).EditAsync(actingUser, id, input, None));

    public Task<CatalogHabitPublishResult> PublishAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Administration(services).PublishAsync(actingUser, id, None));

    public Task<CatalogHabitUnpublishResult> UnpublishAsync(ActingUser actingUser, HabitId id) =>
        InScopeAsync(services => Administration(services).UnpublishAsync(actingUser, id, None));

    // ------------------------------------------------------------ custom habits

    public Task<CustomHabitListResult> ListCustomHabitsAsync(ActingUser actingUser) =>
        InScopeAsync(services => CustomHabits(services).ListAsync(actingUser, None));

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
        new(services.GetRequiredService<IAccountStore>(), Time);

    private static CatalogService Catalog(IServiceProvider services) =>
        new(services.GetRequiredService<ICatalogHabitStore>());

    private static CatalogAdministrationService Administration(IServiceProvider services) =>
        new(services.GetRequiredService<ICatalogHabitStore>());

    private static CustomHabitService CustomHabits(IServiceProvider services) =>
        new(services.GetRequiredService<ICustomHabitStore>());

    private async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> useCase)
    {
        await using var scope = _services.CreateAsyncScope();
        return await useCase(scope.ServiceProvider);
    }
}
