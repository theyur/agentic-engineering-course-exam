using HT.Domain.Evaluation;
using HT.Domain.Reports;

namespace HT.UnitTests.Domain.Reports;

/// <summary>
/// FR-001, FR-002, US1-AS2 to AS4: Met is successful; Met, Failed and Missed are counted; Upcoming, Due and Pending
/// are skipped; the percentage is rounded down and absent when nothing is counted.
/// </summary>
public class ConsistencyTests
{
    private static IEnumerable<EvaluationState> Many(EvaluationState state, int count) => Enumerable.Repeat(state, count);

    [Fact]
    public void US1_AS2_eighteen_Met_one_Failed_and_one_Missed_give_90_of_18_and_20()
    {
        var consistency = Consistency.Of([.. Many(EvaluationState.Met, 18), EvaluationState.Failed, EvaluationState.Missed]);

        Assert.Equal(18, consistency.Successful);
        Assert.Equal(20, consistency.Counted);
        Assert.Equal(90, consistency.Percentage);
    }

    [Fact]
    public void US1_AS2_Pending_Due_and_Upcoming_change_nothing()
    {
        EvaluationState[] skipped = [EvaluationState.Pending, EvaluationState.Due, EvaluationState.Upcoming, EvaluationState.Pending];

        var consistency = Consistency.Of(
            [EvaluationState.Upcoming, .. Many(EvaluationState.Met, 9), .. skipped, .. Many(EvaluationState.Met, 9),
                EvaluationState.Missed, EvaluationState.Due, EvaluationState.Failed]);

        Assert.Equal(new Consistency(18, 20), consistency);
        Assert.Equal(90, consistency.Percentage);
    }

    [Fact]
    public void US1_AS3_no_counted_state_gives_no_percentage()
    {
        Assert.Equal(new Consistency(0, 0), Consistency.Of([]));
        Assert.Null(Consistency.Of([]).Percentage);

        var onlySkipped = Consistency.Of([EvaluationState.Upcoming, EvaluationState.Due, EvaluationState.Pending]);
        Assert.Equal(0, onlySkipped.Counted);
        Assert.Null(onlySkipped.Percentage);
    }

    [Theory]
    [InlineData(2, 3, 66)]
    [InlineData(1, 3, 33)]
    [InlineData(199, 200, 99)]
    [InlineData(1, 1, 100)]
    [InlineData(7, 7, 100)]
    [InlineData(200, 200, 100)]
    [InlineData(0, 2, 0)]
    [InlineData(0, 1, 0)]
    public void US1_AS4_the_percentage_is_rounded_down(int met, int counted, int percentage)
    {
        var consistency = Consistency.Of([.. Many(EvaluationState.Met, met), .. Many(EvaluationState.Failed, counted - met)]);

        Assert.Equal(met, consistency.Successful);
        Assert.Equal(counted, consistency.Counted);
        Assert.Equal(percentage, consistency.Percentage);
    }

    [Fact]
    public void Failed_and_Missed_count_alike()
    {
        Assert.Equal(new Consistency(2, 3), Consistency.Of([EvaluationState.Met, EvaluationState.Missed, EvaluationState.Met]));
        Assert.Equal(new Consistency(2, 3), Consistency.Of([EvaluationState.Met, EvaluationState.Failed, EvaluationState.Met]));
    }

    [Fact]
    public void The_order_of_the_states_does_not_matter()
    {
        Assert.Equal(
            Consistency.Of([EvaluationState.Failed, EvaluationState.Met, EvaluationState.Pending, EvaluationState.Met]),
            Consistency.Of([EvaluationState.Met, EvaluationState.Pending, EvaluationState.Met, EvaluationState.Failed]));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 2)]
    public void A_consistency_with_more_successful_than_counted_results_cannot_exist(int successful, int counted)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Consistency(successful, counted));
    }

    [Fact]
    public void An_undefined_state_is_refused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Consistency.Of([(EvaluationState)99]));
    }
}
