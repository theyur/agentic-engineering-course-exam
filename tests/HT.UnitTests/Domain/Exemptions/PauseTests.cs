using HT.Domain.Exemptions;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Exemptions;

/// <summary>
/// FR-012, Edge Cases, research R-02: a pause's timing around its first and last day, what it covers, which ranges it
/// overlaps (touching ranges do not) and that its last day is never before its first. Monday 16 November 2026 is a
/// Monday.
/// </summary>
public class PauseTests
{
    private static readonly PauseId Id = new(Guid.Parse("6f9a3c1e-0000-4000-8000-0000000000a1"));

    /// <summary>Monday 16 to Sunday 22 November 2026.</summary>
    private static readonly Pause Week = new(Id, Date(2026, 11, 16), Date(2026, 11, 22), null);

    [Theory]
    [InlineData(15, PauseTiming.Upcoming)]
    [InlineData(16, PauseTiming.Current)]
    [InlineData(19, PauseTiming.Current)]
    [InlineData(22, PauseTiming.Current)]
    [InlineData(23, PauseTiming.Past)]
    public void Its_timing_around_the_first_and_the_last_day(int day, PauseTiming timing)
    {
        Assert.Equal(timing, Week.TimingAt(Date(2026, 11, day)));
    }

    [Theory]
    [InlineData(17, PauseTiming.Upcoming)]
    [InlineData(18, PauseTiming.Current)]
    [InlineData(19, PauseTiming.Past)]
    public void A_one_day_pause_is_current_only_on_its_day(int day, PauseTiming timing)
    {
        var oneDay = new Pause(Id, Date(2026, 11, 18), Date(2026, 11, 18), null);

        Assert.Equal(timing, oneDay.TimingAt(Date(2026, 11, day)));
        Assert.Equal(day == 18, oneDay.Covers(Date(2026, 11, day)));
    }

    [Theory]
    [InlineData(15, false)]
    [InlineData(16, true)]
    [InlineData(20, true)]
    [InlineData(22, true)]
    [InlineData(23, false)]
    public void It_covers_its_first_and_last_day_and_the_days_between(int day, bool covers)
    {
        Assert.Equal(covers, Week.Covers(Date(2026, 11, day)));
    }

    [Fact]
    public void A_range_sharing_one_day_at_either_end_overlaps()
    {
        Assert.True(Week.Overlaps(Date(2026, 11, 10), Date(2026, 11, 16)));
        Assert.True(Week.Overlaps(Date(2026, 11, 22), Date(2026, 11, 28)));
        Assert.True(Week.Overlaps(Date(2026, 11, 18), Date(2026, 11, 19)));
        Assert.True(Week.Overlaps(Date(2026, 11, 1), Date(2026, 11, 30)));
    }

    [Fact]
    public void Touching_ranges_do_not_overlap()
    {
        // Ends on Sunday 15 November, or starts on Monday 23 November.
        Assert.False(Week.Overlaps(Date(2026, 11, 9), Date(2026, 11, 15)));
        Assert.False(Week.Overlaps(Date(2026, 11, 23), Date(2026, 11, 29)));
    }

    [Fact]
    public void A_last_day_before_the_first_day_is_refused()
    {
        Assert.Throws<ArgumentException>(() => new Pause(Id, Date(2026, 11, 22), Date(2026, 11, 21), null));
    }

    [Fact]
    public void It_keeps_its_identifier_and_reason()
    {
        Assert.True(ExemptionReason.TryCreate("Travelling", out var reason, out _));

        var pause = new Pause(Id, Date(2026, 11, 16), Date(2026, 11, 22), reason);

        Assert.Equal(Id, pause.Id);
        Assert.Equal(reason, pause.Reason);
        Assert.Equal(Date(2026, 11, 16), pause.FirstDay);
        Assert.Equal(Date(2026, 11, 22), pause.LastDay);
    }
}
