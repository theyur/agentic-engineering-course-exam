using System.Diagnostics.CodeAnalysis;
using HT.Domain.Habits;

namespace HT.Domain.Records;

/// <summary>
/// What a record says (FR-001, FR-004): an answer for a yes/no goal, or an amount (a count, or whole minutes) for a
/// count or duration goal; exactly one of them is set (research R-02). A class with a private constructor, so no
/// value with neither or both can exist. Two values with the same answer or amount are equal. <see cref="object.ToString"/>
/// is not overridden, so a value never reaches a log or a message by accident (HP-006).
/// </summary>
public sealed class RecordValue : IEquatable<RecordValue>
{
    private RecordValue(YesNoAnswer? answer, int? amount)
    {
        Answer = answer;
        Amount = amount;
    }

    /// <summary>The answer of a yes/no record; null for an amount.</summary>
    public YesNoAnswer? Answer { get; }

    /// <summary>The amount of a count or duration record (minutes for a duration); null for an answer.</summary>
    public int? Amount { get; }

    /// <summary>
    /// The value a user enters for a goal of <paramref name="measurement"/> (FR-004, FR-005 step 5). Yes/no keeps the
    /// answer and drops an amount; with no answer it gives <see cref="RecordProblem.AnswerMissing"/>. Count keeps an
    /// amount from 0 to <see cref="RecordLimits.MaxCount"/> and drops an answer, otherwise
    /// <see cref="RecordProblem.CountOutOfRange"/>. Duration keeps an amount from 0 to <see cref="RecordLimits.MaxMinutes"/>
    /// and drops an answer, otherwise <see cref="RecordProblem.DurationOutOfRange"/>. So a value of only the kind that
    /// does not belong is no value, and gets that measurement's problem.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="measurement"/> is not defined.</exception>
    public static bool TryCreate(
        Measurement measurement, YesNoAnswer? answer, int? amount, [NotNullWhen(true)] out RecordValue? value, out RecordProblem problem)
    {
        switch (measurement)
        {
            case Measurement.YesNo when answer is { } given && Enum.IsDefined(given):
                value = new RecordValue(given, null);
                problem = RecordProblem.None;
                return true;
            case Measurement.YesNo:
                value = null;
                problem = RecordProblem.AnswerMissing;
                return false;
            case Measurement.Count:
                return TryAmount(amount, RecordLimits.MaxCount, RecordProblem.CountOutOfRange, out value, out problem);
            case Measurement.Duration:
                return TryAmount(amount, RecordLimits.MaxMinutes, RecordProblem.DurationOutOfRange, out value, out problem);
            default:
                throw new ArgumentOutOfRangeException(nameof(measurement), "The measurement is not defined.");
        }
    }

    /// <summary>Rebuilds a stored yes/no value (research R-02).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="answer"/> is not defined.</exception>
    public static RecordValue FromAnswer(YesNoAnswer answer) =>
        Enum.IsDefined(answer)
            ? new RecordValue(answer, null)
            : throw new ArgumentOutOfRangeException(nameof(answer), "The answer is not defined.");

    /// <summary>
    /// Rebuilds a stored amount, from 0 to <see cref="RecordLimits.MaxCount"/>, the widest range of any measurement
    /// (research R-02). The range of the goal's own measurement is checked by <see cref="TryCreate"/> when the value
    /// enters.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is outside the range; the message names no value.</exception>
    public static RecordValue FromAmount(int amount) =>
        amount is >= 0 and <= RecordLimits.MaxCount
            ? new RecordValue(null, amount)
            : throw new ArgumentOutOfRangeException(nameof(amount), "The amount is outside the range of a record.");

    public bool Equals(RecordValue? other) => other is not null && Answer == other.Answer && Amount == other.Amount;

    public override bool Equals(object? obj) => Equals(obj as RecordValue);

    public override int GetHashCode() => HashCode.Combine(Answer, Amount);

    private static bool TryAmount(
        int? amount, int max, RecordProblem outOfRange, [NotNullWhen(true)] out RecordValue? value, out RecordProblem problem)
    {
        if (amount is { } given && given >= 0 && given <= max)
        {
            value = new RecordValue(null, given);
            problem = RecordProblem.None;
            return true;
        }

        value = null;
        problem = outOfRange;
        return false;
    }
}
