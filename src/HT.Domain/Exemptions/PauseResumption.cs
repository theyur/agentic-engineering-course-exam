namespace HT.Domain.Exemptions;

/// <summary>
/// What resuming a goal does to its current pause (FR-010): <paramref name="Removes"/> it when it began on the pause
/// date, else ends it on <paramref name="NewLastDay"/>, the day before. Made by <see cref="PauseRules.Resume"/>.
/// </summary>
public sealed record PauseResumption(bool Removes, DateOnly? NewLastDay)
{
    /// <summary>The pause is removed (it began on the pause date).</summary>
    public static PauseResumption Remove { get; } = new(true, null);

    /// <summary>The pause ends on <paramref name="lastDay"/>.</summary>
    public static PauseResumption EndOn(DateOnly lastDay) => new(false, lastDay);
}
