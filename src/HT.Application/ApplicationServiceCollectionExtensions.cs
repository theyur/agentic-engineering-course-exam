using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HT.Application;

/// <summary>The composition entry point of HT.Application (ADR 0005).</summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the use-case services (scoped, they use the scoped account store), and with TryAdd
    /// <see cref="TimeProvider.System"/> and the UTC <see cref="InstallationTimeZone"/>, so a test can register a fake
    /// clock before or after it, and the host its configured default time zone before it (research R-03).
    /// </summary>
    public static IServiceCollection AddHtApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<AccountService>();
        services.AddScoped<OperatorService>();
        services.AddScoped<DemoDataService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<CatalogAdministrationService>();
        services.AddScoped<CustomHabitService>();
        services.AddScoped<GoalService>();
        services.AddScoped<ProfileService>();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(InstallationTimeZone.From(null));
        return services;
    }
}
