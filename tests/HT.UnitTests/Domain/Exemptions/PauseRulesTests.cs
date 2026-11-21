using HT.Domain.Exemptions;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Exemptions;

/// <summary>
/// FR-006 steps 4 to 7, FR-010, SC-003, research R-02: each problem at its boundary, the order of two problems at
/// once, and resuming on the first day, a later day and the last day. Today is Monday 16 November 2026.
/// </summary>
public class PauseRulesTests
{
    private static readonly DateOnly Today = Date(2026, 11, 16);

    private static Pause PauseOf(DateOnly first, DateOnly last) => new(new PauseId(Guid.NewGuid()), first, last, null);

    /// <summary>One existing pause, Monday 23 to Sunday 29 November.</summary>
    private static readonly GoalExemptions OnePause = new([], [PauseOf(Date(2026, 11, 23), Date(2026, 11, 29))]);

    [Fact]
    public void A_missing_first_day_a_missing_last_day_or_both_give_DaysMissing()
    {
        Assert.Equal(PauseProblem.DaysMissing, PauseRules.Check(null, Date(2026, 11, 20), Today, GoalExemptions.None));
        Assert.Equal(PauseProblem.DaysMissing, PauseRules.Check(Date(2026, 11, 16), null, Today, GoalExemptions.None));
        Assert.Equal(PauseProblem.DaysMissing, PauseRules.Check(null, null, Today, GoalExemptions.None));
    }

    [Fact]
    public void A_first_day_before_today_gives_StartsBeforeToday()
    {
        Assert.Equal(PauseProblem.StartsBeforeToday, PauseRules.Check(Date(2026, 11, 15), Date(2026, 11, 20), Today, GoalExemptions.None));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    public void A_first_day_today_or_later_passes(int firstDay)
    {
        Assert.Null(PauseRules.Check(Date(2026, 11, firstDay), Date(2026, 11, 20), Today, GoalExemptions.None));
    }

    [Fact]
    public void A_one_day_pause_today_passes()
    {
        Assert.Null(PauseRules.Check(Today, Today, Today, GoalExemptions.None));
    }

    [Fact]
    public void A_last_day_one_day_before_the_first_gives_LastBeforeFirst()
    {
        Assert.Equal(PauseProblem.LastBeforeFirst, PauseRules.Check(Date(2026, 11, 18), Date(2026, 11, 17), Today, GoalExemptions.None));
    }

    [Theory]
    [InlineData(17, 23)]
    [InlineData(29, 30)]
    [InlineData(24, 25)]
    [InlineData(16, 30)]
    public void A_range_sharing_one_day_with_another_pause_gives_Overlaps(int firstDay, int lastDay)
    {
        Assert.Equal(PauseProblem.Overlaps, PauseRules.Check(Date(2026, 11, firstDay), Date(2026, 11, lastDay), Today, OnePause));
    }

    [Theory]
    [InlineData(16, 22)]
    [InlineData(30, 30)]
    public void A_range_touching_another_pause_passes(int firstDay, int lastDay)
    {
        Assert.Null(PauseRules.Check(Date(2026, 11, firstDay), Date(2026, 11, lastDay), Today, OnePause));
    }

    [Fact]
    public void A_first_day_before_today_and_a_last_day_before_it_give_StartsBeforeToday()
    {
        Assert.Equal(PauseProblem.StartsBeforeToday, PauseRules.Check(Date(2026, 11, 15), Date(2026, 11, 14), Today, GoalExemptions.None));
    }

    [Fact]
    public void A_reversed_range_that_also_overlaps_gives_LastBeforeFirst()
    {
        // Friday 27 to Wednesday 25 November: both dates are in the existing pause.
        Assert.Equal(PauseProblem.LastBeforeFirst, PauseRules.Check(Date(2026, 11, 27), Date(2026, 11, 25), Today, OnePause));
    }

    [Fact]
    public void A_first_day_before_today_that_also_overlaps_gives_StartsBeforeToday()
    {
        var current = new GoalExemptions([], [PauseOf(Date(2026, 11, 14), Date(2026, 11, 18))]);

        Assert.Equal(PauseProblem.StartsBeforeToday, PauseRules.Check(Date(2026, 11, 15), Date(2026, 11, 17), Today, current));
    }

    [Fact]
    public void Resuming_on_the_first_day_removes_the_pause()
    {
        var pause = PauseOf(Today, Date(2026, 11, 22));

        var resumption = PauseRules.Resume(pause, Today);

        Assert.True(resumption.Removes);
        Assert.Null(resumption.NewLastDay);
        Assert.Equal(PauseResumption.Remove, resumption);
    }

    [Fact]
    public void Resuming_on_the_second_day_ends_the_pause_on_the_first_day()
    {
        var pause = PauseOf(Date(2026, 11, 15), Date(2026, 11, 22));

        var resumption = PauseRules.Resume(pause, Today);

        Assert.False(resumption.Removes);
        Assert.Equal(Date(2026, 11, 15), resumption.NewLastDay);
    }

    [Fact]
    public void Resuming_on_the_last_day_ends_the_pause_on_the_day_before()
    {
        var pause = PauseOf(Date(2026, 11, 10), Today);

        var resumption = PauseRules.Resume(pause, Today);

        Assert.Equal(PauseResumption.EndOn(Date(2026, 11, 15)), resumption);
    }

    [Theory]
    [InlineData(17, 22)]
    [InlineData(10, 15)]
    public void Resuming_an_upcoming_or_a_past_pause_is_refused(int firstDay, int lastDay)
    {
        var pause = PauseOf(Date(2026, 11, firstDay), Date(2026, 11, lastDay));

        Assert.Throws<ArgumentException>(() => PauseRules.Resume(pause, Today));
    }
}
