using HT.Domain.Accounts;

namespace HT.UnitTests.Domain.Accounts;

public class SignInBlockTests
{
    private static readonly DateTimeOffset T = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void None_has_no_failures_and_no_block()
    {
        Assert.Equal(0, SignInBlock.None.FailedCount);
        Assert.Null(SignInBlock.None.BlockedSince);
        Assert.False(SignInBlock.None.IsBlockedAt(T));
    }

    [Fact]
    public void Rule_constants_are_five_failures_and_five_minutes()
    {
        Assert.Equal(5, SignInBlock.FailuresToBlock);
        Assert.Equal(TimeSpan.FromMinutes(5), SignInBlock.Duration);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    public void A_failure_below_four_adds_one(int before, int after)
    {
        var next = new SignInBlock(before, null).AfterFailureAt(T);

        Assert.Equal(new SignInBlock(after, null), next);
        Assert.False(next.IsBlockedAt(T));
    }

    [Fact]
    public void The_fifth_failure_blocks_at_its_time()
    {
        var next = new SignInBlock(4, null).AfterFailureAt(T);

        Assert.Equal(new SignInBlock(5, T), next);
        Assert.True(next.IsBlockedAt(T));
    }

    [Fact]
    public void Four_failures_are_unblocked_and_the_fifth_blocks()
    {
        var state = SignInBlock.None;
        for (var i = 0; i < 4; i++)
        {
            state = state.AfterFailureAt(T);
        }

        Assert.False(state.IsBlockedAt(T));
        state = state.AfterFailureAt(T);
        Assert.True(state.IsBlockedAt(T));
    }

    [Fact]
    public void A_block_lasts_until_five_minutes_have_passed()
    {
        var blocked = new SignInBlock(5, T);

        Assert.True(blocked.IsBlockedAt(T + TimeSpan.FromSeconds(4 * 60 + 59)));
        Assert.False(blocked.IsBlockedAt(T + TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void A_failure_during_the_block_leaves_the_state_unchanged()
    {
        var blocked = new SignInBlock(5, T);

        Assert.Equal(blocked, blocked.AfterFailureAt(T + TimeSpan.FromMinutes(2)));
    }

    [Fact]
    public void Settling_at_the_end_of_the_block_resets_to_none()
    {
        var blocked = new SignInBlock(5, T);

        Assert.Equal(SignInBlock.None, blocked.SettledAt(T + TimeSpan.FromMinutes(5)));
        Assert.Equal(blocked, blocked.SettledAt(T + TimeSpan.FromSeconds(299)));
    }

    [Fact]
    public void Settling_an_unblocked_state_keeps_it()
    {
        var state = new SignInBlock(3, null);

        Assert.Equal(state, state.SettledAt(T));
    }

    [Fact]
    public void A_failure_after_the_block_ended_counts_from_zero()
    {
        var next = new SignInBlock(5, T).AfterFailureAt(T + TimeSpan.FromMinutes(5));

        Assert.Equal(new SignInBlock(1, null), next);
    }

    [Fact]
    public void A_success_gives_none()
    {
        Assert.Equal(SignInBlock.None, SignInBlock.AfterSuccess());
    }
}
