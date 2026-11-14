using HT.Application;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HT.IntegrationTests;

/// <summary>
/// Captures every log entry at <see cref="LogLevel.Trace"/>, for every category, with the full text of its
/// exception, so a test can prove that no custom-habit value reaches a log (HP-015, research R-13, R-14).
/// Thread-safe.
/// </summary>
public sealed class LogCapture : ILoggerProvider
{
    private readonly object _gate = new();
    private readonly List<Entry> _entries = [];

    public IReadOnlyList<Entry> Entries
    {
        get
        {
            lock (_gate)
            {
                return [.. _entries];
            }
        }
    }

    /// <summary>
    /// A provider over <paramref name="connectionString"/> with only this capture as its logger, at
    /// <see cref="LogLevel.Trace"/>, and the application built as the other integration tests build it.
    /// </summary>
    public ServiceProvider BuildServices(string connectionString)
    {
        var services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddProvider(this);
            })
            .AddHtApplication()
            .AddHtPersistence(connectionString);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    /// <summary>
    /// Fails when any entry's message or exception text contains one of <paramref name="values"/>, ordinally
    /// ignoring case. The failure names only the entry's category and level, never the value.
    /// </summary>
    public void AssertContainsNone(params string[] values)
    {
        foreach (var entry in Entries)
        {
            foreach (var value in values)
            {
                if (entry.Message.Contains(value, StringComparison.OrdinalIgnoreCase)
                    || (entry.Exception?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    Assert.Fail($"A log entry of category {entry.Category} at level {entry.Level} contains a protected value.");
                }
            }
        }
    }

    public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

    public void Dispose()
    {
    }

    private void Add(Entry entry)
    {
        lock (_gate)
        {
            _entries.Add(entry);
        }
    }

    /// <summary>One captured entry; <see cref="Exception"/> holds every message, inner exception and <c>ToString()</c>.</summary>
    public sealed record Entry(string Category, LogLevel Level, string Message, string? Exception);

    private sealed class Logger(LogCapture capture, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            capture.Add(new Entry(category, logLevel, formatter(state, exception), exception is null ? null : FullText(exception)));
        }

        private static string FullText(Exception exception)
        {
            var parts = new List<string>();
            for (Exception? current = exception; current is not null; current = current.InnerException)
            {
                parts.Add(current.Message);
            }

            if (exception is AggregateException aggregate)
            {
                parts.AddRange(aggregate.Flatten().InnerExceptions.Select(e => e.ToString()));
            }

            parts.Add(exception.ToString());
            return string.Join(Environment.NewLine, parts);
        }
    }
}
