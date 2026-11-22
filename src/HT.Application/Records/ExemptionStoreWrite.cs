using HT.Domain.Exemptions;

namespace HT.Application.Records;

/// <summary>The kinds of <see cref="ExemptionStoreWrite"/> (research R-04).</summary>
public enum ExemptionStoreWriteOutcome
{
    /// <summary>The goal is not one of the owner's.</summary>
    GoalNotFound,

    /// <summary>The date already has an excusal.</summary>
    AlreadyExcused,

    /// <summary>A record is dated on the date (D-3).</summary>
    HasRecord,

    /// <summary>A pause covers the date (FR-009).</summary>
    Paused,

    /// <summary>The date has no excusal to remove.</summary>
    NotExcused,

    /// <summary>The new pause shares a date with one of the goal's pauses.</summary>
    Overlaps,

    /// <summary>The pause is not among the goal's pauses.</summary>
    PauseNotFound,

    /// <summary>The write was made.</summary>
    Written,

    /// <summary>
    /// Slice 8 (research R-05, FR-009): the goal's latest attempt is ended, so no pause is added and no date on or after
    /// its ending date is excused.
    /// </summary>
    GoalEnded,
}

/// <summary>
/// The outcome of an excusal or pause write of <see cref="IRecordStore"/> (research R-04): the store's outcome, not a
/// use-case outcome. Each refusal is a re-check made under the goal-row lock (contracts/application-api.md, "Re-checked
/// under the lock") and writes nothing. A <see cref="ExemptionStoreWriteOutcome.Written"/> of
/// <see cref="IRecordStore.AddPauseAsync"/> carries the new pause's identifier.
/// </summary>
public sealed class ExemptionStoreWrite
{
    private static readonly ExemptionStoreWrite WrittenWithoutPause = new(ExemptionStoreWriteOutcome.Written, null);

    private ExemptionStoreWrite(ExemptionStoreWriteOutcome outcome, PauseId? pauseId)
    {
        Outcome = outcome;
        PauseId = pauseId;
    }

    /// <summary>The goal is not one of the owner's: nothing was written.</summary>
    public static ExemptionStoreWrite GoalNotFound { get; } = new(ExemptionStoreWriteOutcome.GoalNotFound, null);

    /// <summary>The date already has an excusal: nothing was written.</summary>
    public static ExemptionStoreWrite AlreadyExcused { get; } = new(ExemptionStoreWriteOutcome.AlreadyExcused, null);

    /// <summary>A record is dated on the date: nothing was written.</summary>
    public static ExemptionStoreWrite HasRecord { get; } = new(ExemptionStoreWriteOutcome.HasRecord, null);

    /// <summary>A pause covers the date: nothing was written.</summary>
    public static ExemptionStoreWrite Paused { get; } = new(ExemptionStoreWriteOutcome.Paused, null);

    /// <summary>The date has no excusal: nothing was written.</summary>
    public static ExemptionStoreWrite NotExcused { get; } = new(ExemptionStoreWriteOutcome.NotExcused, null);

    /// <summary>The pause would share a date with another pause of the goal: nothing was written.</summary>
    public static ExemptionStoreWrite Overlaps { get; } = new(ExemptionStoreWriteOutcome.Overlaps, null);

    /// <summary>The pause is not among the goal's pauses: nothing was written.</summary>
    public static ExemptionStoreWrite PauseNotFound { get; } = new(ExemptionStoreWriteOutcome.PauseNotFound, null);

    /// <summary>Slice 8: the goal's latest attempt is ended: nothing was written.</summary>
    public static ExemptionStoreWrite GoalEnded { get; } = new(ExemptionStoreWriteOutcome.GoalEnded, null);

    public ExemptionStoreWriteOutcome Outcome { get; }

    /// <summary>The pause written; set only for a <see cref="ExemptionStoreWriteOutcome.Written"/> of <see cref="IRecordStore.AddPauseAsync"/>.</summary>
    public PauseId? PauseId { get; }

    /// <summary>An excusal was added or removed, or a pause changed or deleted.</summary>
    public static ExemptionStoreWrite Written() => WrittenWithoutPause;

    /// <summary>A pause was added: its new identifier.</summary>
    public static ExemptionStoreWrite Written(PauseId pause) => new(ExemptionStoreWriteOutcome.Written, pause);
}
