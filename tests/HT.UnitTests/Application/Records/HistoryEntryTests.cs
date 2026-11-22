using HT.Application.Records;
using HT.Domain.Evaluation;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// Slice 8 (T116; FR-016, FR-019): an entry built without the attempt fields, as every entry of slices 5 to 7 is, belongs
/// to attempt 1, which is the latest, with no attempt name and no settings of its own.
/// </summary>
public sealed class HistoryEntryTests
{
    [Fact]
    public void An_entry_without_attempt_fields_is_of_the_latest_attempt_1()
    {
        var starts = new DateTimeOffset(2026, 11, 16, 0, 0, 0, TimeSpan.Zero);
        var entry = new HistoryEntry
        {
            FirstDate = new DateOnly(2026, 11, 16), LastDate = new DateOnly(2026, 11, 16), Starts = starts, Ends = starts.AddDays(1),
            State = EvaluationState.Due, Records = [], Progress = new Progress(ProgressKind.RecordedValue, null, 1), IsCorrection = false,
        };

        Assert.True(entry.IsLatestAttempt);
        Assert.Equal(1, entry.AttemptNumber);
        Assert.Null(entry.AttemptName);
    }
}
