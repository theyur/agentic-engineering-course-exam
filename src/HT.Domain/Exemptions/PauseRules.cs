namespace HT.Domain.Exemptions;

/// <summary>
/// The rules of setting and resuming a pause (FR-006, FR-010; research R-02). Pure functions of dates; the caller
/// supplies the pause date (research R-03). The order of a use case's other checks stays in Application.
/// </summary>
public static class PauseRules
{
    /// <summary>
    /// The first problem of a pause from <paramref name="first"/> to <paramref name="last"/> on <paramref name="today"/>,
    /// in FR-006's order: <see cref="PauseProblem.DaysMissing"/>; <see cref="PauseProblem.StartsBeforeToday"/>
    /// (<c>first &lt; today</c>); <see cref="PauseProblem.LastBeforeFirst"/>; <see cref="PauseProblem.Overlaps"/> (a
    /// shared date with a pause of <paramref name="exemptions"/>; touching ranges pass); or null.
    /// </summary>
    public static PauseProblem? Check(DateOnly? first, DateOnly? last, DateOnly today, GoalExemptions exemptions)
    {
        ArgumentNullException.ThrowIfNull(exemptions);
        if (first is not { } firstDay || last is not { } lastDay)
        {
            return PauseProblem.DaysMissing;
        }

        if (firstDay < today)
        {
            return PauseProblem.StartsBeforeToday;
        }

        if (lastDay < firstDay)
        {
            return PauseProblem.LastBeforeFirst;
        }

        return exemptions.Overlaps(firstDay, lastDay) ? PauseProblem.Overlaps : null;
    }

    /// <summary>
    /// Resuming <paramref name="current"/> on <paramref name="today"/> (FR-010): removes it when it began today, else
    /// ends it on the day before today.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="current"/> is not current on <paramref name="today"/>.</exception>
    public static PauseResumption Resume(Pause current, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.TimingAt(today) != PauseTiming.Current)
        {
            throw new ArgumentException("Only a current pause can be resumed.", nameof(current));
        }

        return current.FirstDay == today ? PauseResumption.Remove : PauseResumption.EndOn(today.AddDays(-1));
    }
}
