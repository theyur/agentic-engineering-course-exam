namespace HT.Persistence.Records;

/// <summary>
/// A row of <c>GoalRecords</c> (data-model.md "Tables"): one record of a goal, dated, with exactly one of
/// <see cref="Answer"/> and <see cref="Amount"/> set. The mapping checks that rule, not a check constraint. Every
/// statement on it joins <c>Goals</c> on the owner or runs under the owner's goal lock (constitution III). Only
/// <see cref="RecordMapping"/> maps it to and from domain types.
/// </summary>
internal sealed class GoalRecordRow
{
    public Guid Id { get; set; }

    public Guid GoalId { get; set; }

    /// <summary>The record's date (FR-001).</summary>
    public DateOnly Date { get; set; }

    /// <summary>1 Yes, 0 No; only for a yes/no goal.</summary>
    public byte? Answer { get; set; }

    /// <summary>The amount, in minutes for a duration; only for a count or duration goal.</summary>
    public int? Amount { get; set; }

    /// <summary>An identity column: the stored order (FR-009), which a change of value keeps.</summary>
    public long Sequence { get; set; }
}

/// <summary>
/// A row of <c>GoalNotes</c> (data-model.md "Tables"); key (goal, entry date), so each commitment or period has at most
/// one note (FR-007). Only <see cref="RecordMapping"/> maps it to and from domain types.
/// </summary>
internal sealed class GoalNoteRow
{
    public Guid GoalId { get; set; }

    /// <summary>The entry key: the commitment's date, the day, or the week's Monday.</summary>
    public DateOnly EntryDate { get; set; }

    /// <summary>Trimmed, 1 to 500 characters.</summary>
    public string Text { get; set; } = string.Empty;
}
