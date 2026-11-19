using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Records;
using HT.Ui.Account;
using HT.Ui.Habits;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// The services the habit and goal pages need in bUnit: the real use cases over the unit tests' fakes (ADR 0006), the
/// per-call scope, and the acting user read from bUnit's authorization. Every page test resolves the same set of goal
/// fakes, registered once by <see cref="AddGoals"/>.
/// </summary>
public static class PageServices
{
    public static FakeCatalogHabitStore AddCatalog(this BunitContext context, FakeCatalogHabitStore? store = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        store ??= new FakeCatalogHabitStore();
        context.Services.AddSingleton<ICatalogHabitStore>(store);
        context.Services.AddScoped<CatalogService>();
        context.Services.AddScoped<CatalogAdministrationService>();
        context.Services.AddSingleton<UseCaseScope>();
        context.Services.AddScoped<ActingUserAccessor>();
        context.AddGoals();
        return store;
    }

    /// <summary>
    /// The custom-habit use cases over the custom-habit fake that <see cref="AddGoals"/> registered (registering the
    /// goal fakes first if needed), next to what <see cref="AddCatalog"/> registers (the custom-habit pages also count
    /// the Published catalog habits).
    /// </summary>
    public static FakeCustomHabitStore AddCustomHabits(this BunitContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.AddGoals();
        context.Services.AddScoped<CustomHabitService>();
        return Registered<ICustomHabitStore, FakeCustomHabitStore>(context.Services);
    }

    /// <summary>
    /// Registers, each with TryAdd, the goal fakes every page test resolves: a <see cref="FakeCustomHabitStore"/>, a
    /// <see cref="FakeAccountStore"/> that knows every signed-in user, a <see cref="FakeGoalStore"/> built from the
    /// registered fakes, a <see cref="FakeRecordStore"/> sharing that goal store's goals, a <see cref="FakeTimeProvider"/>, the UTC <see cref="InstallationTimeZone"/>, and the
    /// <see cref="GoalService"/> and <see cref="ProfileService"/> over them. Returns the
    /// registered goal store. The catalog store is the one <see cref="AddCatalog"/> registered, or a new one.
    /// </summary>
    public static FakeGoalStore AddGoals(this BunitContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var services = context.Services;
        if (services.Any(d => d.ServiceType == typeof(IGoalStore)))
        {
            var goals = Registered<IGoalStore, FakeGoalStore>(services);
            services.TryAddSingleton<IRecordStore>(new FakeRecordStore(goals));
            return goals;
        }

        services.TryAddSingleton<ICatalogHabitStore>(new FakeCatalogHabitStore());
        services.TryAddSingleton<ICustomHabitStore>(new FakeCustomHabitStore());
        services.TryAddSingleton<IAccountStore>(new FakeAccountStore { KnowsEverySignedInUser = true });
        services.TryAddSingleton<IGoalStore>(new FakeGoalStore(
            Registered<IAccountStore, FakeAccountStore>(services),
            Registered<ICatalogHabitStore, FakeCatalogHabitStore>(services),
            Registered<ICustomHabitStore, FakeCustomHabitStore>(services)));
        services.TryAddSingleton<IRecordStore>(new FakeRecordStore(Registered<IGoalStore, FakeGoalStore>(services)));
        services.TryAddSingleton<TimeProvider>(new FakeTimeProvider());
        services.TryAddSingleton(InstallationTimeZone.From(null));
        services.TryAddScoped<GoalService>();
        services.TryAddScoped<ProfileService>();
        return Registered<IGoalStore, FakeGoalStore>(services);
    }

    /// <summary>Signs the context in as an account with this role, with the claims ActingUserAccessor reads.</summary>
    public static Guid SignIn(this BunitAuthorizationContext authorization, string role, string email = "ana@example.com")
    {
        ArgumentNullException.ThrowIfNull(authorization);
        var id = Guid.NewGuid();
        authorization.SetAuthorized(email);
        authorization.SetRoles(role);
        authorization.SetClaims(new Claim(ClaimTypes.NameIdentifier, id.ToString()));
        return id;
    }

    /// <summary>The fake instance registered last for <typeparamref name="TService"/>.</summary>
    private static TFake Registered<TService, TFake>(IServiceCollection services)
        where TFake : class, TService =>
        services.LastOrDefault(d => d.ServiceType == typeof(TService))?.ImplementationInstance as TFake
        ?? throw new InvalidOperationException($"No {typeof(TFake).Name} is registered as {typeof(TService).Name}.");
}
