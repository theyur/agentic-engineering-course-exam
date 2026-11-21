using HT.Domain.Evaluation;

namespace HT.Domain.Reports;

/// <summary>
/// A goal's strict streaks over its final results in date order (FR-003, D-1; research R-01): the
/// <paramref name="Current"/> run of Met results after the latest Failed or Missed one, and the
/// <paramref name="Longest"/> run of Met results in a row. A pure value: no clock, no time zone and no culture.
/// </summary>
/// <exception cref="ArgumentOutOfRangeException">
/// <paramref name="Current"/> is negative or greater than <paramref name="Longest"/>.
/// </exception>
public sealed record Streaks(int Current, int Longest)
{
    /// <summary>The Met results after the latest Failed or Missed one; never more than <see cref="Longest"/>.</summary>
    public int Current { get; } = Current >= 0 && Current <= Longest
        ? Current
        : throw new ArgumentOutOfRangeException(nameof(Current), "The current streak must be from 0 to the longest one.");

    /// <summary>The longest run of Met results in a row.</summary>
    public int Longest { get; } = Longest;

    /// <summary>
    /// The streaks of <paramref name="statesInDateOrder"/> (a per-week period by its first date): Met adds 1 to the
    /// run, Failed or Missed ends it, and Upcoming, Due and Pending are skipped, so they neither extend nor reset a
    /// run, also when their records already reach the target (D-1). Excused and Paused are skipped the same way
    /// (slice 7 FR-014).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A state is not defined.</exception>
    public static Streaks Of(IEnumerable<EvaluationState> statesInDateOrder)
    {
        ArgumentNullException.ThrowIfNull(statesInDateOrder);
        var current = 0;
        var longest = 0;
        foreach (var state in statesInDateOrder)
        {
            switch (state)
            {
                case EvaluationState.Met:
                    current++;
                    longest = Math.Max(longest, current);
                    break;
                case EvaluationState.Failed or EvaluationState.Missed:
                    current = 0;
                    break;
                case EvaluationState.Upcoming or EvaluationState.Due or EvaluationState.Pending
                    or EvaluationState.Excused or EvaluationState.Paused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statesInDateOrder), "A state is not defined.");
            }
        }

        return new Streaks(current, longest);
    }
}
