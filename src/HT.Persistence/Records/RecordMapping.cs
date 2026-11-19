using System.Globalization;
using HT.Domain.Records;

namespace HT.Persistence.Records;

/// <summary>
/// The only mapping between record and note rows and domain types (data-model.md "Tables", research R-02). A value is
/// rebuilt with the public <see cref="RecordValue.FromAnswer"/> or <see cref="RecordValue.FromAmount"/>, and a note with
/// <see cref="NoteText.TryCreate"/>. A row that breaks a rule (both or neither of <c>Answer</c> and <c>Amount</c>, an
/// <c>Answer</c> other than 0 or 1, an amount out of range, or a note that is empty or too long) is a fault whose
/// message names the row's identifier only, never a value or a note (HP-006).
/// </summary>
internal static class RecordMapping
{
    private const byte No = 0;
    private const byte Yes = 1;

    /// <summary>The record of a row.</summary>
    /// <exception cref="InvalidOperationException">The row breaks a rule; the message names its identifier only.</exception>
    public static GoalRecord ToRecord(GoalRecordRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return new GoalRecord(new RecordId(row.Id), row.Date, ValueOf(row));
    }

    /// <summary>A new record row of goal <paramref name="goalId"/> with a new identifier; the database sets the stored order.</summary>
    public static GoalRecordRow ToRow(Guid goalId, DateOnly date, RecordValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new GoalRecordRow
        {
            Id = Guid.NewGuid(),
            GoalId = goalId,
            Date = date,
            Answer = AnswerOf(value),
            Amount = value.Amount,
        };
    }

    /// <summary>The stored answer of a value: 1 Yes, 0 No, null for an amount.</summary>
    public static byte? AnswerOf(RecordValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Answer switch
        {
            YesNoAnswer.Yes => Yes,
            YesNoAnswer.No => No,
            _ => null,
        };
    }

    /// <summary>The note of a row.</summary>
    /// <exception cref="InvalidOperationException">The text is empty or too long; the message names the row's key only.</exception>
    public static NoteText ToNote(GoalNoteRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (NoteText.TryCreate(row.Text, out var note, out _) && note is not null)
        {
            return note;
        }

        throw new InvalidOperationException(string.Create(
            CultureInfo.InvariantCulture, $"The stored note of goal {row.GoalId} for {row.EntryDate:yyyy-MM-dd} is not a valid note."));
    }

    /// <summary>A note row of goal <paramref name="goalId"/> under the entry key <paramref name="entryDate"/>.</summary>
    public static GoalNoteRow ToRow(Guid goalId, DateOnly entryDate, NoteText note)
    {
        ArgumentNullException.ThrowIfNull(note);
        return new GoalNoteRow { GoalId = goalId, EntryDate = entryDate, Text = note.Value };
    }

    private static RecordValue ValueOf(GoalRecordRow row) => (row.Answer, row.Amount) switch
    {
        (Yes, null) => RecordValue.FromAnswer(YesNoAnswer.Yes),
        (No, null) => RecordValue.FromAnswer(YesNoAnswer.No),
        (null, { } amount) when amount is >= 0 and <= RecordLimits.MaxCount => RecordValue.FromAmount(amount),
        _ => throw new InvalidOperationException($"The stored record {row.Id} has no valid value."),
    };
}
