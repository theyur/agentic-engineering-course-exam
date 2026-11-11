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
