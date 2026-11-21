using System.Globalization;
using HT.Domain.Exemptions;

namespace HT.Persistence.Records;

/// <summary>
/// The only mapping between excusal and pause rows and domain types (data-model.md "Tables", research R-04). A reason is
/// rebuilt with the public <see cref="ExemptionReason.TryCreate"/>, and a pause with the <see cref="Pause"/>
/// constructor. A row that breaks a rule (a reason that is blank or too long, a last day before the first day) is a
/// fault whose message names the goal and the row's key only, never a reason (HP-009).
/// </summary>
internal static class ExemptionMapping
{
    /// <summary>The excusal of a row.</summary>
    /// <exception cref="InvalidOperationException">The row breaks a rule; the message names its key only.</exception>
    public static Excusal ToExcusal(GoalExcusalRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return new Excusal(row.Date, ReasonOf(row.Reason, string.Create(
            CultureInfo.InvariantCulture, $"The stored excusal of goal {row.GoalId} for {row.Date:yyyy-MM-dd}")));
    }

    /// <summary>The pause of a row.</summary>
    /// <exception cref="InvalidOperationException">The row breaks a rule; the message names its identifier only.</exception>
    public static Pause ToPause(GoalPauseRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        var reason = ReasonOf(row.Reason, $"The stored pause {row.Id}");
        if (row.LastDay < row.FirstDay)
        {
            throw new InvalidOperationException($"The stored pause {row.Id} ends before it starts.");
        }

        return new Pause(new PauseId(row.Id), row.FirstDay, row.LastDay, reason);
    }

    /// <summary>An excusal row of goal <paramref name="goalId"/> for <paramref name="date"/>.</summary>
    public static GoalExcusalRow ToRow(Guid goalId, DateOnly date, ExemptionReason? reason) =>
        new() { GoalId = goalId, Date = date, Reason = reason?.Value };

    /// <summary>A pause row of goal <paramref name="goalId"/> with a new identifier.</summary>
    /// <exception cref="ArgumentException"><paramref name="lastDay"/> is before <paramref name="firstDay"/>.</exception>
    public static GoalPauseRow ToRow(Guid goalId, DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason)
    {
        if (lastDay < firstDay)
        {
            throw new ArgumentException("The last day cannot be before the first day.", nameof(lastDay));
        }

        return new GoalPauseRow { Id = Guid.NewGuid(), GoalId = goalId, FirstDay = firstDay, LastDay = lastDay, Reason = reason?.Value };
    }

    /// <summary>Null for no stored reason; the reason of a valid stored text; a fault naming <paramref name="what"/> otherwise.</summary>
    private static ExemptionReason? ReasonOf(string? stored, string what)
    {
        if (stored is null)
        {
            return null;
        }

        if (ExemptionReason.TryCreate(stored, out var reason, out _) && reason is not null)
        {
            return reason;
        }

        throw new InvalidOperationException($"{what} has no valid reason.");
    }
}
