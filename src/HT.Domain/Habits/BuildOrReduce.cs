namespace HT.Domain.Habits;

/// <summary>
/// The Build or Reduce of a suggested starting goal (SPEC §2 "Build / Reduce"): Build means at least the amount,
/// Reduce at most. A goal-side concept that slice 4's goals reuse; never a habit's <see cref="Intent"/> (research R-05).
/// </summary>
public enum BuildOrReduce
{
    Build,
    Reduce,
}
