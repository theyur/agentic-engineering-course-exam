using HT.Application.Operator;
using HT.Persistence;

namespace HT.Ui.Operator;

/// <summary>
/// The operator steps the host runs after <c>Build()</c> and before <c>Run()</c> (ADR 0007, research R-11).
/// Each step runs in its own scope and its own <c>try/catch</c>; a fault is logged without secrets and the start
/// goes on (HP-018, HP-021, HP-022). Log lines: contracts/host.md, written by source-generated log methods.
/// </summary>
internal static partial class StartupTasks
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
                LogMigrationsApplied(logger, count);
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
            LogDemoLoading(logger, result.Message);
            return;
        }

        foreach (var account in result.Accounts)
        {
            LogDemoAccount(logger, account.Email, account.Message);
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
            LogAdministratorCreation(logger, result.Message);
        }
        else
        {
            var configuredEmail = email.Trim();
            LogAdministratorCreationFor(logger, result.Message, configuredEmail);
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
            LogStepFailed(logger, exception, step);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Schema migrations applied: {Count}")]
    private static partial void LogMigrationsApplied(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Administrator creation: {Result}")]
    private static partial void LogAdministratorCreation(ILogger logger, string result);

    [LoggerMessage(Level = LogLevel.Information, Message = "Administrator creation: {Result} ({Email})")]
    private static partial void LogAdministratorCreationFor(ILogger logger, string result, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo loading: {Result}")]
    private static partial void LogDemoLoading(ILogger logger, string? result);

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo account {Email}: {Result}")]
    private static partial void LogDemoAccount(ILogger logger, string email, string result);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Step} failed; the app starts without it")]
    private static partial void LogStepFailed(ILogger logger, Exception exception, string step);
}
