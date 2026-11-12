using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HT.UiTests.Fakes;

/// <summary>One captured log entry.</summary>
public sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);

/// <summary>A hand-written logger provider that keeps every entry in memory (ADR 0006: no mocking library).</summary>
public sealed class ListLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public IReadOnlyList<LogEntry> Entries => [.. _entries];

    public ILogger CreateLogger(string categoryName) => new ListLogger(categoryName, _entries);

    /// <summary>The entries of one category.</summary>
    public IReadOnlyList<LogEntry> EntriesOf(string category) => [.. _entries.Where(e => e.Category == category)];

    public void Dispose()
    {
    }
}

/// <summary>A logger that appends to a shared list.</summary>
public sealed class ListLogger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
{
    public ListLogger(string category)
        : this(category, new ConcurrentQueue<LogEntry>())
    {
    }

    public IReadOnlyList<LogEntry> Entries => [.. entries];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        entries.Enqueue(new LogEntry(category, logLevel, formatter(state, exception), exception));
    }
}
