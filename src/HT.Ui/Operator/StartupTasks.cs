using HT.Application.Operator;
using HT.Persistence;

namespace HT.Ui.Operator;

/// <summary>
/// The operator steps the host runs after <c>Build()</c> and before <c>Run()</c> (ADR 0007, research R-11).
/// Each step runs in its own scope and its own <c>try/catch</c>; a fault is logged without secrets and the start
/// goes on (HP-018, HP-021, HP-022). Log lines: contracts/host.md.
/// </summary>
internal static class StartupTasks
{
    /// <summary>The logger category of the operator log.</summary>
    public const string LoggerCategory = "HT.Ui.Operator.StartupTasks";

    /// <summary>The configuration keys of the first administrator (research R-11), kept out of source control.</summary>
    public const string AdministratorEmailKey = "Operator:AdministratorEmail";

    public const string AdministratorPasswordKey = "Operator:AdministratorPassword";

    /// <summary>The configuration key of the demo password (research R-11), kept out of source control.</summary>
    public const string DemoPasswordKey = "Operator:DemoPassword";

    public static async Task RunAsync(IServiceProvider services, IHostEnvironment environment, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        if (environment.IsDevelopment())
        {
            await RunStepAsync("Schema migrations", logger, async () =>
            {
                var count = await HtDatabase.MigrateAsync(services, CancellationToken.None);
                logger.LogInformation("Schema migrations applied: {Count}", count);
            });
        }

        await RunStepAsync("Administrator creation", logger, () => CreateAdministratorAsync(services, logger));
        await RunStepAsync("Demo loading", logger, () => LoadDemoDataAsync(services, logger));
    }

    // HP-021: at every start, right after administrator creation. The demo password is passed on and never logged.
    private static async Task LoadDemoDataAsync(IServiceProvider services, ILogger logger)
    {
        var configuration = services.GetRequiredService<IConfiguration>();

        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<DemoDataService>()
            .LoadAsync(configuration[DemoPasswordKey], CancellationToken.None);

        if (result.Outcome != DemoLoadingOutcome.Loaded)
        {
            logger.LogInformation("Demo loading: {Result}", result.Message);
            return;
        }

        foreach (var account in result.Accounts)
        {
            logger.LogInformation("Demo account {Email}: {Result}", account.Email, account.Message);
        }
    }

    // HP-018: at every start. The password is passed on and never logged.
    private static async Task CreateAdministratorAsync(IServiceProvider services, ILogger logger)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var email = configuration[AdministratorEmailKey];

        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<OperatorService>()
            .CreateAdministratorAsync(email, configuration[AdministratorPasswordKey], CancellationToken.None);

        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogInformation("Administrator creation: {Result}", result.Message);
        }
        else
        {
            logger.LogInformation("Administrator creation: {Result} ({Email})", result.Message, email.Trim());
        }
    }

    private static async Task RunStepAsync(string step, ILogger logger, Func<Task> run)
    {
        try
        {
            await run();
        }
        catch (Exception exception) // A fault in one step must not stop the start (HP-018, HP-021).
        {
            logger.LogError(exception, "{Step} failed; the app starts without it", step);
        }
    }
}
