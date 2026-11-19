using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// The live progress of the RecordDialog (HP-002): what an entry's progress would be if the value being entered were
/// saved. It counts nothing itself, but passes the values to <see cref="Evaluator.ProgressOf"/> with the goal's target,
/// so no component evaluates FR-014. Reads no clock and no culture.
/// </summary>
public static class RecordPreview
{
    /// <summary>
    /// The progress of <paramref name="entry"/> with the candidate value. The entry's other records are kept, except
    /// <paramref name="replacing"/> (the record being edited) and, for a commitment, its existing record in every case,
    /// because saving replaces it (FR-003): so a commitment never gets more than one value. The candidate is added when
    /// <see cref="RecordValue.TryCreate"/> accepts it for the goal's measurement (FR-004, FR-005); a missing or
    /// invalid candidate gives the progress without it.
    /// </summary>
    public static Progress Progress(GoalHistory history, HistoryEntry entry, RecordId? replacing, YesNoAnswer? answer, int? amount)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(entry);

        var values = new List<RecordValue>();
        if (history.GoalScheduleKind != GoalScheduleKind.CommitmentBased)
        {
            values.AddRange(entry.Records.Where(record => record.Id != replacing).Select(ValueOf));
        }

        if (RecordValue.TryCreate(history.Measurement, answer, amount, out var candidate, out _))
        {
            values.Add(candidate);
        }

        return Evaluator.ProgressOf(history.Target, values);
    }

    /// <exception cref="ArgumentException">The record has neither an answer nor an amount; the message names no value.</exception>
    private static RecordValue ValueOf(RecordView record) =>
        record.Answer is { } answer ? RecordValue.FromAnswer(answer)
        : record.Amount is { } amount ? RecordValue.FromAmount(amount)
        : throw new ArgumentException("A record of the entry has no value.", nameof(record));
}
