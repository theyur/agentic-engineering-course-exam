using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using HT.Application.Habits;
using HT.Ui.Account;
using HT.Ui.Habits;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// The services the habit pages need in bUnit: the real use cases over the unit tests' fakes (ADR 0006), the
/// per-call scope, and the acting user read from bUnit's authorization.
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
        return store;
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
}
