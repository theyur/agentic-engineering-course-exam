// Fake git for the guard oracle: records exactly what the shell delivered, then succeeds.
using System.Text.Json;

var log = Environment.GetEnvironmentVariable("ORACLE_LOG");
if (!string.IsNullOrEmpty(log))
{
    var env = Environment.GetEnvironmentVariables().Keys.Cast<string>()
        .Where(k => k.StartsWith("GIT_", StringComparison.OrdinalIgnoreCase))
        .ToDictionary(k => k, Environment.GetEnvironmentVariable);
    var record = new { cwd = Environment.CurrentDirectory, args, env };
    await File.AppendAllTextAsync(log, JsonSerializer.Serialize(record) + "\n");
}
return 0;
