using HT.Domain.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Goals;

/// <summary>Slice 8 FR-007, FR-026: an ending holds its date, a defined outcome and an optional note; an attempt's equality includes it.</summary>
public class GoalEndingTests
{
    private static readonly DateOnly Ended = Date(2026, 11, 16);

    [Fact]
    public void An_ending_keeps_what_it_is_given()
    {
        Assert.True(EndingNote.TryCreate("Too busy.", out var note, out _));

        var ending = new GoalEnding(Ended, Outcome.Abandoned, note);

        Assert.Equal(Ended, ending.Date);
        Assert.Equal(Outcome.Abandoned, ending.Outcome);
        Assert.Equal("Too busy.", ending.Note!.Value.Value);
        Assert.Null(new GoalEnding(Ended, Outcome.Achieved, null).Note);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void An_undefined_outcome_throws(int outcome)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoalEnding(Ended, (Outcome)outcome, null));
    }

    [Fact]
    public void The_three_outcomes_in_their_order()
    {
        Assert.Equal([Outcome.Achieved, Outcome.Abandoned, Outcome.NoLongerRelevant], Enum.GetValues<Outcome>());
    }

    [Fact]
    public void Endings_with_the_same_values_are_equal()
    {
        Assert.Equal(new GoalEnding(Ended, Outcome.NoLongerRelevant, null), new GoalEnding(Ended, Outcome.NoLongerRelevant, null));
        Assert.NotEqual(new GoalEnding(Ended, Outcome.NoLongerRelevant, null), new GoalEnding(Ended, Outcome.Achieved, null));
        Assert.NotEqual(new GoalEnding(Ended, Outcome.Achieved, null), new GoalEnding(Ended.AddDays(1), Outcome.Achieved, null));
    }

    [Fact]
    public void An_attempt_is_not_ended_until_it_has_an_ending()
    {
        Assert.Null(new GoalAttempt(1, Ended).Ending);
    }

    [Fact]
    public void Attempt_equality_includes_the_ending()
    {
        var running = new GoalAttempt(1, Date(2026, 11, 2));
        var ended = running with { Ending = new GoalEnding(Ended, Outcome.Abandoned, null) };

        Assert.NotEqual(running, ended);
        Assert.Equal(ended, new GoalAttempt(1, Date(2026, 11, 2)) { Ending = new GoalEnding(Ended, Outcome.Abandoned, null) });
        Assert.NotEqual(ended, running with { Ending = new GoalEnding(Ended, Outcome.Achieved, null) });
        Assert.Equal(running, new GoalAttempt(1, Date(2026, 11, 2)));
        Assert.Equal(ended.GetHashCode(), (running with { Ending = new GoalEnding(Ended, Outcome.Abandoned, null) }).GetHashCode());
    }
}
