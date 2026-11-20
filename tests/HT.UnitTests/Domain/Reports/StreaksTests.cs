using HT.Domain.Evaluation;
using HT.Domain.Reports;

namespace HT.UnitTests.Domain.Reports;

/// <summary>
/// FR-003, D-1, US2-AS1 to AS5 and AS7, Edge Cases: a Met result extends the run, a Failed or Missed one resets it,
/// and Upcoming, Due and Pending neither extend nor reset it.
/// </summary>
public class StreaksTests
{
    private const EvaluationState Met = EvaluationState.Met;
    private const EvaluationState Failed = EvaluationState.Failed;
    private const EvaluationState Missed = EvaluationState.Missed;
    private const EvaluationState Pending = EvaluationState.Pending;
    private const EvaluationState Due = EvaluationState.Due;
    private const EvaluationState Upcoming = EvaluationState.Upcoming;

    private static IEnumerable<EvaluationState> Many(EvaluationState state, int count) => Enumerable.Repeat(state, count);

    [Fact]
    public void US2_AS1_Met_Met_Failed_Met_Met_Met_gives_3_and_3()
    {
        Assert.Equal(new Streaks(3, 3), Streaks.Of([Met, Met, Failed, Met, Met, Met]));
    }

    [Fact]
    public void US2_AS2_eleven_Met_one_Missed_and_six_Met_give_6_and_11()
    {
        var streaks = Streaks.Of([.. Many(Met, 11), Missed, .. Many(Met, 6)]);

        Assert.Equal(6, streaks.Current);
        Assert.Equal(11, streaks.Longest);
    }

    [Theory]
    [InlineData(EvaluationState.Failed)]
    [InlineData(EvaluationState.Missed)]
    public void US2_AS3_a_latest_Failed_or_Missed_result_gives_0_and_keeps_the_longest(EvaluationState latest)
    {
        Assert.Equal(new Streaks(0, 4), Streaks.Of([Met, Met, Failed, Met, Met, Met, Met, latest]));
    }

    [Fact]
    public void US2_AS4_Pending_and_Due_after_a_run_neither_extend_nor_reset_it()
    {
        Assert.Equal(new Streaks(2, 2), Streaks.Of([Met, Met, Pending, Due]));
    }

    [Fact]
    public void US2_AS4_D_1_the_Pending_result_extends_the_run_only_once_it_is_Met()
    {
        Assert.Equal(new Streaks(2, 2), Streaks.Of([Met, Met, Pending]));
        Assert.Equal(new Streaks(3, 3), Streaks.Of([Met, Met, Met]));
    }

    [Fact]
    public void US2_AS5_Pending_Due_and_Upcoming_inside_a_run_do_not_break_it()
    {
        Assert.Equal(new Streaks(5, 5), Streaks.Of([Met, Pending, Met, Due, Met, Upcoming, Met, Pending, Met]));
    }

    [Fact]
    public void US2_AS7_no_final_result_gives_0_and_0()
    {
        Assert.Equal(new Streaks(0, 0), Streaks.Of([]));
        Assert.Equal(new Streaks(0, 0), Streaks.Of([Upcoming, Due, Pending, Pending]));
    }

    [Fact]
    public void Only_failures_give_0_and_0()
    {
        Assert.Equal(new Streaks(0, 0), Streaks.Of([Failed, Missed, Failed]));
    }

    [Fact]
    public void A_run_with_no_failure_is_both_the_current_and_the_longest()
    {
        Assert.Equal(new Streaks(7, 7), Streaks.Of(Many(Met, 7)));
    }

    [Fact]
    public void Edge_case_a_Missed_result_between_two_runs_corrected_to_Met_joins_them()
    {
        Assert.Equal(new Streaks(4, 5), Streaks.Of([.. Many(Met, 5), Missed, .. Many(Met, 4)]));

        Assert.Equal(new Streaks(10, 10), Streaks.Of([.. Many(Met, 5), Met, .. Many(Met, 4)]));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 2)]
    public void A_current_streak_longer_than_the_longest_cannot_exist(int current, int longest)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Streaks(current, longest));
    }

    [Fact]
    public void An_undefined_state_is_refused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Streaks.Of([Met, (EvaluationState)99]));
    }
}
