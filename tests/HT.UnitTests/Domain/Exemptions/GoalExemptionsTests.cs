using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Exemptions;

/// <summary>
/// FR-003, FR-007, D-1, research R-02: a commitment or a day is Paused exactly on its paused date; a week is Paused
/// when any of its seven dates is paused; an excusal exempts only a commitment; a pause wins over an excusal of the
/// same date and keeps it. Monday 9, 16 and 23 November 2026 are Mondays. The instants of the entries do not matter
/// to the rule; they are each date's midnight in UTC.
/// </summary>
public class GoalExemptionsTests
{
    private const GoalScheduleKind Commitments = GoalScheduleKind.CommitmentBased;
    private const GoalScheduleKind Periods = GoalScheduleKind.PeriodBased;

    private static DateTimeOffset Midnight(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private static CalendarEntry Day(DateOnly date) => new(date, date, Midnight(date), Midnight(date.AddDays(1)));

    private static CalendarEntry Week(DateOnly monday) => new(monday, monday.AddDays(6), Midnight(monday), Midnight(monday.AddDays(7)));

    private static Pause PauseOf(DateOnly first, DateOnly last) => new(new PauseId(Guid.NewGuid()), first, last, null);

    private static Excusal ExcusalOf(DateOnly date, string? reason = null)
    {
        Assert.True(ExemptionReason.TryCreate(reason, out var text, out _));
        return new Excusal(date, text);
    }

    private static GoalExemptions With(Excusal[]? excusals = null, Pause[]? pauses = null) => new(excusals ?? [], pauses ?? []);

    [Theory]
    [InlineData(GoalScheduleKind.CommitmentBased)]
    [InlineData(GoalScheduleKind.PeriodBased)]
    public void A_commitment_or_a_day_in_a_pause_is_Paused_and_the_dates_around_it_are_not(GoalScheduleKind kind)
    {
        // Paused Wednesday 18 to Friday 20 November.
        var exemptions = With(pauses: [PauseOf(Date(2026, 11, 18), Date(2026, 11, 20))]);

        Assert.Null(exemptions.ExemptionOf(Day(Date(2026, 11, 17)), kind));
        Assert.Equal(EvaluationState.Paused, exemptions.ExemptionOf(Day(Date(2026, 11, 18)), kind));
        Assert.Equal(EvaluationState.Paused, exemptions.ExemptionOf(Day(Date(2026, 11, 19)), kind));
        Assert.Equal(EvaluationState.Paused, exemptions.ExemptionOf(Day(Date(2026, 11, 20)), kind));
        Assert.Null(exemptions.ExemptionOf(Day(Date(2026, 11, 21)), kind));
        Assert.True(exemptions.IsPaused(Date(2026, 11, 18)));
        Assert.False(exemptions.IsPaused(Date(2026, 11, 21)));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(19)]
    [InlineData(22)]
    public void D_1_a_week_with_one_paused_date_is_Paused_and_the_weeks_around_it_are_not(int pausedDay)
    {
        // One paused date in the week of Monday 16 November: its Monday, a middle day or its Sunday.
        var exemptions = With(pauses: [PauseOf(Date(2026, 11, pausedDay), Date(2026, 11, pausedDay))]);

        Assert.Null(exemptions.ExemptionOf(Week(Date(2026, 11, 9)), Periods));
        Assert.Equal(EvaluationState.Paused, exemptions.ExemptionOf(Week(Date(2026, 11, 16)), Periods));
        Assert.Null(exemptions.ExemptionOf(Week(Date(2026, 11, 23)), Periods));
    }

    [Fact]
    public void D_1_a_pause_across_a_weekend_pauses_both_weeks()
    {
        // Sunday 15 to Monday 16 November.
        var exemptions = With(pauses: [PauseOf(Date(2026, 11, 15), Date(2026, 11, 16))]);

        Assert.Equal(EvaluationState.Paused, exemptions.ExemptionOf(Week(Date(2026, 11, 9)), Periods));
        Assert.Equal(EvaluationState.Paused, exemptions.ExemptionOf(Week(Date(2026, 11, 16)), Periods));
        Assert.Null(exemptions.ExemptionOf(Week(Date(2026, 11, 23)), Periods));
    }

    [Fact]
    public void An_excused_commitment_is_Excused_and_the_others_are_not()
    {
        var exemptions = With(excusals: [ExcusalOf(Date(2026, 11, 11), "Ill")]);

        Assert.Equal(EvaluationState.Excused, exemptions.ExemptionOf(Day(Date(2026, 11, 11)), Commitments));
        Assert.Null(exemptions.ExemptionOf(Day(Date(2026, 11, 9)), Commitments));
        Assert.True(exemptions.IsExcused(Date(2026, 11, 11)));
        Assert.False(exemptions.IsExcused(Date(2026, 11, 9)));
        Assert.Equal("Ill", exemptions.ExcusalOf(Date(2026, 11, 11))?.Reason?.Value);
        Assert.Null(exemptions.ExcusalOf(Date(2026, 11, 9)));
    }

    [Fact]
    public void FR_007_an_excused_commitment_inside_a_pause_is_Paused_and_keeps_its_excusal()
    {
        var excusal = ExcusalOf(Date(2026, 11, 18), "Ill");
        var exemptions = With([excusal], [PauseOf(Date(2026, 11, 16), Date(2026, 11, 22))]);

        Assert.Equal(EvaluationState.Paused, exemptions.ExemptionOf(Day(Date(2026, 11, 18)), Commitments));
        Assert.Equal(excusal, exemptions.ExcusalOf(Date(2026, 11, 18)));
        Assert.True(exemptions.IsExcused(Date(2026, 11, 18)));
    }

    [Fact]
    public void An_excusal_never_exempts_a_day_or_a_week()
    {
        var exemptions = With(excusals: [ExcusalOf(Date(2026, 11, 16)), ExcusalOf(Date(2026, 11, 18))]);

        Assert.Null(exemptions.ExemptionOf(Day(Date(2026, 11, 16)), Periods));
        Assert.Null(exemptions.ExemptionOf(Day(Date(2026, 11, 18)), Periods));
        Assert.Null(exemptions.ExemptionOf(Week(Date(2026, 11, 16)), Periods));
    }

    [Fact]
    public void The_current_pause_is_the_one_covering_today()
    {
        var past = PauseOf(Date(2026, 11, 2), Date(2026, 11, 4));
        var current = PauseOf(Date(2026, 11, 16), Date(2026, 11, 22));
        var upcoming = PauseOf(Date(2026, 11, 30), Date(2026, 12, 2));
        var exemptions = With(pauses: [past, upcoming, current]);

        Assert.Equal(current, exemptions.CurrentPause(Date(2026, 11, 16)));
        Assert.Equal(current, exemptions.CurrentPause(Date(2026, 11, 22)));
        Assert.Equal(past, exemptions.CurrentPause(Date(2026, 11, 3)));
        Assert.Null(exemptions.CurrentPause(Date(2026, 11, 23)));
        Assert.Null(exemptions.CurrentPause(Date(2026, 11, 15)));
        Assert.Null(exemptions.CurrentPause(Date(2026, 11, 29)));
    }

    [Fact]
    public void FR_012_the_pauses_are_the_latest_first_day_first_and_the_excusals_by_date()
    {
        var first = PauseOf(Date(2026, 11, 2), Date(2026, 11, 4));
        var second = PauseOf(Date(2026, 11, 16), Date(2026, 11, 22));
        var third = PauseOf(Date(2026, 11, 30), Date(2026, 12, 2));
        var exemptions = With([ExcusalOf(Date(2026, 11, 13)), ExcusalOf(Date(2026, 11, 9))], [second, first, third]);

        Assert.Equal([third, second, first], exemptions.Pauses);
        Assert.Equal([Date(2026, 11, 9), Date(2026, 11, 13)], exemptions.Excusals.Select(excusal => excusal.Date));
    }

    [Fact]
    public void Overlaps_finds_a_shared_date_and_lets_touching_ranges_pass()
    {
        var exemptions = With(pauses: [PauseOf(Date(2026, 11, 16), Date(2026, 11, 22))]);

        Assert.True(exemptions.Overlaps(Date(2026, 11, 22), Date(2026, 11, 25)));
        Assert.True(exemptions.Overlaps(Date(2026, 11, 10), Date(2026, 11, 16)));
        Assert.False(exemptions.Overlaps(Date(2026, 11, 23), Date(2026, 11, 25)));
        Assert.False(exemptions.Overlaps(Date(2026, 11, 10), Date(2026, 11, 15)));
    }

    [Fact]
    public void None_exempts_nothing()
    {
        var none = GoalExemptions.None;

        Assert.Empty(none.Excusals);
        Assert.Empty(none.Pauses);
        Assert.Null(none.ExemptionOf(Day(Date(2026, 11, 16)), Commitments));
        Assert.Null(none.ExemptionOf(Week(Date(2026, 11, 16)), Periods));
        Assert.False(none.IsPaused(Date(2026, 11, 16)));
        Assert.False(none.IsExcused(Date(2026, 11, 16)));
        Assert.Null(none.CurrentPause(Date(2026, 11, 16)));
        Assert.False(none.Overlaps(DateOnly.MinValue, DateOnly.MaxValue));
    }

    [Fact]
    public void Two_excusals_of_one_date_or_two_overlapping_pauses_cannot_exist()
    {
        Assert.Throws<ArgumentException>(() => With(excusals: [ExcusalOf(Date(2026, 11, 11)), ExcusalOf(Date(2026, 11, 11))]));
        Assert.Throws<ArgumentException>(() =>
            With(pauses: [PauseOf(Date(2026, 11, 16), Date(2026, 11, 22)), PauseOf(Date(2026, 11, 22), Date(2026, 11, 25))]));

        // Touching pauses can.
        var touching = With(pauses: [PauseOf(Date(2026, 11, 16), Date(2026, 11, 22)), PauseOf(Date(2026, 11, 23), Date(2026, 11, 25))]);
        Assert.Equal(2, touching.Pauses.Count);
    }
}
