namespace HT.Domain.Exemptions;

/// <summary>Where a pause lies against the pause date (FR-012): see <see cref="Pause.TimingAt"/>.</summary>
public enum PauseTiming
{
    /// <summary>The first day is after the pause date.</summary>
    Upcoming,

    /// <summary>The pause covers the pause date.</summary>
    Current,

    /// <summary>The last day is before the pause date.</summary>
    Past,
}
