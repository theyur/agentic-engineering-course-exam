using HT.Domain.Evaluation;

namespace HT.Domain.Reports;

/// <summary>
/// The consistency of a goal over its final results (FR-001, FR-002; research R-01): the Met results
/// (<paramref name="Successful"/>) of the Met, Failed and Missed ones (<paramref name="Counted"/>). A pure value: no
/// clock, no time zone and no culture.
/// </summary>
/// <exception cref="ArgumentOutOfRangeException">
/// <paramref name="Successful"/> is negative or greater than <paramref name="Counted"/>.
/// </exception>
public sealed record Consistency(int Successful, int Counted)
{
    /// <summary>The Met results; never negative and never more than <see cref="Counted"/>.</summary>
    public int Successful { get; } = Successful >= 0 && Successful <= Counted
        ? Successful
        : throw new ArgumentOutOfRangeException(nameof(Successful), "The successful results must be from 0 to the counted ones.");

    /// <summary>The Met, Failed and Missed results.</summary>
    public int Counted { get; } = Counted;

    /// <summary>
    /// The whole percentage <c>Successful × 100 / Counted</c>, rounded down (integer division, FR-002), so 100 only
    /// when every counted result is Met; null when nothing is counted.
    /// </summary>
    public int? Percentage => Counted == 0 ? null : Successful * 100 / Counted;

    /// <summary>
    /// The consistency of <paramref name="states"/> (FR-001): Met is successful; Met, Failed and Missed are counted;
    /// Upcoming, Due and Pending are skipped. The order does not matter.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A state is not defined.</exception>
    public static Consistency Of(IEnumerable<EvaluationState> states)
    {
        ArgumentNullException.ThrowIfNull(states);
        var successful = 0;
        var counted = 0;
        foreach (var state in states)
        {
            switch (state)
            {
                case EvaluationState.Met:
                    successful++;
                    counted++;
                    break;
                case EvaluationState.Failed or EvaluationState.Missed:
                    counted++;
                    break;
                case EvaluationState.Upcoming or EvaluationState.Due or EvaluationState.Pending:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(states), "A state is not defined.");
            }
        }

        return new Consistency(successful, counted);
    }
}
