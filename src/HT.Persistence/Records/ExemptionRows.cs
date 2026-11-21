namespace HT.Persistence.Records;

/// <summary>
/// A row of <c>GoalExcusals</c> (data-model.md "Tables", research R-04): the excusal of one commitment of a goal; key
/// (goal, date), so a commitment has at most one excusal. No column for a time or a state (FR-025). Every statement on
/// it joins <c>Goals</c> on the owner or runs under the owner's goal lock (constitution III). Only
/// <see cref="ExemptionMapping"/> maps it to and from domain types.
/// </summary>
internal sealed class GoalExcusalRow
{
    public Guid GoalId { get; set; }

    /// <summary>The commitment's date.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Trimmed, 1 to 200 characters; null means none.</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// A row of <c>GoalPauses</c> (data-model.md "Tables", research R-04): one pause of a goal, from its first to its last
/// day, both included. The pauses of one goal never share a date; the store re-checks that under the goal lock, not a
/// constraint. No column for a time or a state (FR-025). Only <see cref="ExemptionMapping"/> maps it to and from domain
/// types.
/// </summary>
internal sealed class GoalPauseRow
{
    /// <summary>Made by the store.</summary>
    public Guid Id { get; set; }

    public Guid GoalId { get; set; }

    /// <summary>The first paused day; never after <see cref="LastDay"/>.</summary>
    public DateOnly FirstDay { get; set; }

    /// <summary>The last paused day.</summary>
    public DateOnly LastDay { get; set; }

    /// <summary>Trimmed, 1 to 200 characters; null means none.</summary>
    public string? Reason { get; set; }
}
