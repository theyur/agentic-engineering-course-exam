namespace HT.Domain.Evaluation;

/// <summary>
/// The system-wide grace period after a commitment or period ends (FR-016): 48 elapsed hours, computed on UTC
/// instants, so a clock change in between changes only how its end reads on the local clock (research R-01).
/// </summary>
public static class GracePeriod
{
    public static readonly TimeSpan Length = TimeSpan.FromHours(48);

    /// <summary>The instant the grace period of an entry that ends at <paramref name="ends"/> ends: <paramref name="ends"/> + 48 h.</summary>
    /// <exception cref="ArgumentException"><paramref name="ends"/> does not have a UTC offset.</exception>
    public static DateTimeOffset EndOf(DateTimeOffset ends) =>
        ends.Offset == TimeSpan.Zero
            ? ends + Length
            : throw new ArgumentException("The end instant must have a UTC offset.", nameof(ends));
}
