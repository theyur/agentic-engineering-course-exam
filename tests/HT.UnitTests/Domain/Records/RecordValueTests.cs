using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.UnitTests.Domain.Records;

/// <summary>FR-004, FR-005 step 5, research R-02: the ranges, the dropped values and the factories of a stored value.</summary>
public class RecordValueTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(9999)]
    public void A_count_from_0_to_9999_is_kept(int amount)
    {
        Assert.True(RecordValue.TryCreate(Measurement.Count, null, amount, out var value, out var problem));

        Assert.Equal(RecordProblem.None, problem);
        Assert.Equal(amount, value.Amount);
        Assert.Null(value.Answer);
    }

    [Theory]
    [InlineData(10000)]
    [InlineData(-1)]
    [InlineData(null)]
    public void A_count_outside_0_to_9999_or_missing_is_refused(int? amount)
    {
        Assert.False(RecordValue.TryCreate(Measurement.Count, null, amount, out var value, out var problem));

        Assert.Equal(RecordProblem.CountOutOfRange, problem);
        Assert.Null(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1440)]
    public void A_duration_from_0_to_1440_minutes_is_kept(int amount)
    {
        Assert.True(RecordValue.TryCreate(Measurement.Duration, null, amount, out var value, out var problem));

        Assert.Equal(RecordProblem.None, problem);
        Assert.Equal(amount, value.Amount);
        Assert.Null(value.Answer);
    }

    [Theory]
    [InlineData(1441)]
    [InlineData(-1)]
    [InlineData(null)]
    public void A_duration_outside_0_to_1440_minutes_or_missing_is_refused(int? amount)
    {
        Assert.False(RecordValue.TryCreate(Measurement.Duration, null, amount, out var value, out var problem));

        Assert.Equal(RecordProblem.DurationOutOfRange, problem);
        Assert.Null(value);
    }

    [Theory]
    [InlineData(YesNoAnswer.Yes)]
    [InlineData(YesNoAnswer.No)]
    public void A_yes_no_answer_is_kept(YesNoAnswer answer)
    {
        Assert.True(RecordValue.TryCreate(Measurement.YesNo, answer, null, out var value, out var problem));

        Assert.Equal(RecordProblem.None, problem);
        Assert.Equal(answer, value.Answer);
        Assert.Null(value.Amount);
    }

    [Fact]
    public void Yes_with_an_amount_on_yes_no_keeps_Yes_and_drops_the_amount()
    {
        Assert.True(RecordValue.TryCreate(Measurement.YesNo, YesNoAnswer.Yes, 5, out var value, out _));

        Assert.Equal(YesNoAnswer.Yes, value.Answer);
        Assert.Null(value.Amount);
    }

    [Fact]
    public void An_amount_with_Yes_on_count_keeps_the_amount_and_drops_Yes()
    {
        Assert.True(RecordValue.TryCreate(Measurement.Count, YesNoAnswer.Yes, 5, out var value, out _));

        Assert.Equal(5, value.Amount);
        Assert.Null(value.Answer);
    }

    [Fact]
    public void An_amount_with_No_on_duration_keeps_the_amount_and_drops_No()
    {
        Assert.True(RecordValue.TryCreate(Measurement.Duration, YesNoAnswer.No, 25, out var value, out _));

        Assert.Equal(25, value.Amount);
        Assert.Null(value.Answer);
    }

    [Fact]
    public void Only_Yes_on_count_is_no_value()
    {
        Assert.False(RecordValue.TryCreate(Measurement.Count, YesNoAnswer.Yes, null, out var value, out var problem));

        Assert.Equal(RecordProblem.CountOutOfRange, problem);
        Assert.Null(value);
    }

    [Fact]
    public void Only_an_amount_on_yes_no_is_no_value()
    {
        Assert.False(RecordValue.TryCreate(Measurement.YesNo, null, 1, out var value, out var problem));

        Assert.Equal(RecordProblem.AnswerMissing, problem);
        Assert.Null(value);
    }

    [Fact]
    public void Only_Yes_on_duration_is_no_value()
    {
        Assert.False(RecordValue.TryCreate(Measurement.Duration, YesNoAnswer.Yes, null, out var value, out var problem));

        Assert.Equal(RecordProblem.DurationOutOfRange, problem);
        Assert.Null(value);
    }

    [Fact]
    public void Nothing_on_yes_no_is_AnswerMissing()
    {
        Assert.False(RecordValue.TryCreate(Measurement.YesNo, null, null, out _, out var problem));

        Assert.Equal(RecordProblem.AnswerMissing, problem);
    }

    [Fact]
    public void An_undefined_answer_on_yes_no_is_AnswerMissing()
    {
        Assert.False(RecordValue.TryCreate(Measurement.YesNo, (YesNoAnswer)7, null, out _, out var problem));

        Assert.Equal(RecordProblem.AnswerMissing, problem);
    }

    [Fact]
    public void An_undefined_measurement_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RecordValue.TryCreate((Measurement)7, YesNoAnswer.Yes, 1, out _, out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9999)]
    public void FromAmount_rebuilds_an_amount_from_0_to_9999(int amount)
    {
        var value = RecordValue.FromAmount(amount);

        Assert.Equal(amount, value.Amount);
        Assert.Null(value.Answer);
    }

    [Theory]
    [InlineData(10000)]
    [InlineData(-1)]
    public void FromAmount_outside_0_to_9999_throws_with_a_message_that_names_no_value(int amount)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecordValue.FromAmount(amount));

        Assert.DoesNotContain(exception.Message, char.IsAsciiDigit);
        Assert.Null(exception.ActualValue);
    }

    [Theory]
    [InlineData(YesNoAnswer.Yes)]
    [InlineData(YesNoAnswer.No)]
    public void FromAnswer_rebuilds_an_answer(YesNoAnswer answer)
    {
        var value = RecordValue.FromAnswer(answer);

        Assert.Equal(answer, value.Answer);
        Assert.Null(value.Amount);
    }

    [Fact]
    public void FromAnswer_with_an_undefined_answer_throws_with_a_message_that_names_no_value()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecordValue.FromAnswer((YesNoAnswer)7));

        Assert.DoesNotContain(exception.Message, char.IsAsciiDigit);
    }

    [Fact]
    public void Values_with_the_same_answer_or_amount_are_equal()
    {
        Assert.True(RecordValue.TryCreate(Measurement.Count, null, 12, out var entered, out _));

        Assert.Equal(RecordValue.FromAmount(12), entered);
        Assert.Equal(RecordValue.FromAmount(12).GetHashCode(), entered.GetHashCode());
        Assert.Equal(RecordValue.FromAnswer(YesNoAnswer.Yes), RecordValue.FromAnswer(YesNoAnswer.Yes));
        Assert.NotEqual(RecordValue.FromAmount(12), RecordValue.FromAmount(13));
        Assert.NotEqual(RecordValue.FromAnswer(YesNoAnswer.Yes), RecordValue.FromAnswer(YesNoAnswer.No));
        Assert.NotEqual(RecordValue.FromAmount(1), RecordValue.FromAnswer(YesNoAnswer.Yes));
        Assert.False(RecordValue.FromAmount(1).Equals(null));
    }

    [Fact]
    public void A_value_never_prints_its_amount()
    {
        Assert.DoesNotContain("4321", RecordValue.FromAmount(4321).ToString(), StringComparison.Ordinal);
    }
}
