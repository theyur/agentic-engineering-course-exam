namespace HT.Domain.Accounts;

/// <summary>
/// The sign-in block state of an account (FR-008): the count of failed sign-ins and the instant the current
/// block began. The rule reads no clock; every method takes the time of the attempt.
/// </summary>
public readonly record struct SignInBlock(int FailedCount, DateTimeOffset? BlockedSince)
{
    public const int FailuresToBlock = 5;

    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(5);

    public static SignInBlock None => new(0, null);

    /// <summary>True when a block began and has not ended at <paramref name="now"/>.</summary>
    public bool IsBlockedAt(DateTimeOffset now) => BlockedSince is { } since && now < since + Duration;

    /// <summary>Returns <see cref="None"/> when a block has ended at <paramref name="now"/>, otherwise this state.</summary>
    public SignInBlock SettledAt(DateTimeOffset now) =>
        BlockedSince is { } since && now >= since + Duration ? None : this;

    /// <summary>
    /// The state after a wrong password at <paramref name="now"/>. A blocked state is left unchanged; otherwise
    /// the count goes up by one, and the fifth failure starts the block at <paramref name="now"/>.
    /// </summary>
    public SignInBlock AfterFailureAt(DateTimeOffset now)
    {
        var settled = SettledAt(now);
        if (settled.IsBlockedAt(now))
        {
            return settled;
        }

        var count = settled.FailedCount + 1;
        return count >= FailuresToBlock ? new SignInBlock(FailuresToBlock, now) : new SignInBlock(count, null);
    }

    /// <summary>The state after a correct password.</summary>
    public static SignInBlock AfterSuccess() => None;
}
