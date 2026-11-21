namespace HT.Application.Exemptions;

/// <summary>
/// The texts of excusing and of removing an excusal, worded as in the spec (FR-002, FR-004, FR-005; data-model.md, the
/// texts table; HP-008). Texts shared with earlier slices are not repeated here: "You are not signed in." is
/// <c>AccountTexts.NotSignedIn</c>, "Goals are for registered users." and "This goal does not exist." are
/// <c>GoalTexts</c>', and "Nothing is scheduled on this date." is <c>RecordTexts.NothingScheduled</c>. "This goal is
/// paused on this date." is <see cref="PauseTexts.DatePaused"/>. No text names a reason, a value, a goal or a habit
/// (HP-009).
/// </summary>
public static class ExcusalTexts
{
    /// <summary>FR-002 (4): a period-based goal.</summary>
    public const string NotACommitment = "Only commitments can be excused. This goal is evaluated by period.";

    /// <summary>FR-002 (7): the commitment already has an excusal.</summary>
    public const string AlreadyExcused = "This commitment is already excused.";

    /// <summary>FR-002 (8): the commitment is Upcoming.</summary>
    public const string NotBegun = "You cannot excuse a commitment that has not begun.";

    /// <summary>FR-002 (8): the commitment is final (Met, Failed or Missed).</summary>
    public const string Final = "This commitment is final and can no longer be excused.";

    /// <summary>FR-002 (9): the commitment has a record (D-3).</summary>
    public const string HasRecord = "This commitment has a record. Delete the record to excuse it.";

    /// <summary>FR-002 (10), FR-006 (8): a reason longer than 200 characters after trimming.</summary>
    public const string ReasonTooLong = "Keep the reason to 200 characters or fewer.";

    /// <summary>FR-004 (4): the commitment has no excusal to remove.</summary>
    public const string NotExcused = "This commitment is not excused.";

    /// <summary>FR-004 (5): the commitment's grace period has ended.</summary>
    public const string TooLateToRemove = "This excusal can no longer be removed.";

    /// <summary>FR-005 (4c): recording an excused commitment.</summary>
    public const string CommitmentExcused = "This commitment is excused. Remove the excusal to record it.";

    /// <summary>FR-002: the success word of excusing.</summary>
    public const string Excused = "excused";

    /// <summary>FR-004: the success word of removing an excusal.</summary>
    public const string Removed = "excusal removed";
}
