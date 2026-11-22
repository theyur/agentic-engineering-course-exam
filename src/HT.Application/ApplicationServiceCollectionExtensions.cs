using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
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
        services.AddScoped<DemoHistoryService>();
        services.AddScoped<DemoReportService>();
        services.AddScoped<DemoPauseService>();
        services.AddScoped<DemoGoalChangeService>();
        // Slice 8 (research R-08): the catalog names goals at one time read, so the host builds it with the clock and
        // never falls back to the two-argument constructor. Only the clock is passed; the stores are resolved by the
        // constructor that takes it, so this file names no store port.
        services.AddScoped(provider => ActivatorUtilities.CreateInstance<CatalogService>(provider, provider.GetRequiredService<TimeProvider>()));
        services.AddScoped<CatalogAdministrationService>();
        services.AddScoped<CustomHabitService>();
        services.AddScoped<GoalService>();
        services.AddScoped<GoalChangeService>();
        services.AddScoped<ProfileService>();
        services.AddScoped<RecordingService>();
        services.AddScoped<ReportService>();
        services.AddScoped<AttemptService>();
        services.AddScoped<ExcusalService>();
        services.AddScoped<PauseService>();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(InstallationTimeZone.From(null));
        return services;
    }
}
