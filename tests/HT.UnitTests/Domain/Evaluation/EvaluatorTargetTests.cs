using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Evaluation.EvaluatorCases;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Evaluation;

/// <summary>
/// SC-002, US2-AS4 to AS10, FR-013, FR-014 and the edge cases: what the target means for every Build or Reduce ×
/// measurement × schedule kind, at, just below and just above it, read after the grace end; and the progress.
/// </summary>
public class EvaluatorTargetTests
{
    private static EvaluationState FinalState(GoalTarget target, bool perWeek, params RecordValue[] records) =>
        Final(target, perWeek, records).State;

    private static EntryEvaluation Final(GoalTarget target, bool perWeek, IReadOnlyList<RecordValue> records)
    {
        var entry = new TargetCase("sample", target, perWeek, new Progress(ProgressKind.RecordedValue, null, 0), []);
        var evaluation = entry.EvaluateAt(entry.AfterGrace, records);
        Assert.True(evaluation.IsFinal);
        return evaluation;
    }

    [Theory]
    [MemberData(nameof(NamesAndSets), MemberType = typeof(EvaluatorCases))]
    public void SC_002_every_combination_below_at_and_above_its_target(string name, string set)
    {
        var targetCase = Named(name);
        var records = targetCase.Set(set);

        var evaluation = targetCase.EvaluateAt(targetCase.AfterGrace, records.Records);

        Assert.Equal(records.Final, evaluation.State);
        Assert.Equal(records.Progress, evaluation.Progress);
        Assert.Equal(records.Progress, Evaluator.ProgressOf(targetCase.Target, records.Records));
        Assert.True(evaluation.IsFinal);
    }

    [Theory]
    [MemberData(nameof(Names), MemberType = typeof(EvaluatorCases))]
    public void FR_013_no_record_is_Missed_for_every_combination_Reduce_included(string name)
    {
        var targetCase = Named(name);

        var evaluation = targetCase.EvaluateAt(targetCase.AfterGrace, []);

        Assert.Equal(EvaluationState.Missed, evaluation.State);
        Assert.Equal(targetCase.NoRecords, evaluation.Progress);
        Assert.Equal(targetCase.NoRecords, Evaluator.ProgressOf(targetCase.Target, []));
    }

    [Fact]
    public void Every_combination_of_Build_or_Reduce_measurement_and_schedule_kind_is_covered()
    {
        var combinations = All
            .Select(c => (c.Target.BuildOrReduce, c.Target.Measurement, c.Target.ScheduleKindOf, c.Target.PeriodLength))
            .Distinct()
            .Count();

        Assert.Equal(2 * 3 * 3, combinations);
    }

    [Fact]
    public void Progress_has_a_null_value_only_for_a_commitment_without_a_record()
    {
        Assert.All(All, c => Assert.Equal(c.Target.ScheduleKindOf == GoalScheduleKind.CommitmentBased, c.NoRecords.Value is null));
        Assert.All(All.Where(c => c.Target.ScheduleKindOf == GoalScheduleKind.PeriodBased), c => Assert.Equal(0, c.NoRecords.Value));
    }

    [Theory]
    [InlineData(19, EvaluationState.Failed)]
    [InlineData(20, EvaluationState.Met)]
    [InlineData(21, EvaluationState.Met)]
    [InlineData(45, EvaluationState.Met)]
    public void Build_duration_20_min_keeps_the_amount_entered(int minutes, EvaluationState state)
    {
        var target = Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));

        var evaluation = Final(target, false, Amounts(minutes));

        Assert.Equal(state, evaluation.State);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, minutes, 20), evaluation.Progress);
    }

    [Theory]
    [InlineData(1, EvaluationState.Met)]
    [InlineData(2, EvaluationState.Met)]
    [InlineData(3, EvaluationState.Failed)]
    public void Reduce_count_at_most_2(int cups, EvaluationState state)
    {
        Assert.Equal(state, FinalState(Target(CountDays(2, "cups", BuildOrReduce.Reduce, AllDays)), false, Amounts(cups)));
    }

    [Theory]
    [InlineData(0, EvaluationState.Met)]
    [InlineData(1, EvaluationState.Failed)]
    public void Reduce_amount_0(int cigarettes, EvaluationState state)
    {
        Assert.Equal(state, FinalState(Target(CountDays(0, "cigarettes", BuildOrReduce.Reduce, AllDays)), false, Amounts(cigarettes)));
    }

    [Fact]
    public void Build_yes_no_Yes_is_Met_and_No_Failed()
    {
        var target = Target(YesNoDays(BuildOrReduce.Build, MonWedFri));

        Assert.Equal(EvaluationState.Met, FinalState(target, false, Yes));
        Assert.Equal(EvaluationState.Failed, FinalState(target, false, No));
    }

    [Fact]
    public void Reduce_yes_no_No_is_Met_and_Yes_Failed()
    {
        var target = Target(YesNoDays(BuildOrReduce.Reduce, MonWedFri));

        Assert.Equal(EvaluationState.Met, FinalState(target, false, No));
        Assert.Equal(EvaluationState.Failed, FinalState(target, false, Yes));
    }

    [Fact]
    public void Per_week_Build_yes_no_3_times()
    {
        var target = Target(YesNoTimes(3, SchedulePeriod.Week));

        Assert.Equal(EvaluationState.Met, FinalState(target, true, Yes, Yes, Yes));
        Assert.Equal(EvaluationState.Failed, FinalState(target, true, Yes, Yes, No));
        Assert.Equal(EvaluationState.Met, FinalState(target, true, Yes, Yes, Yes, Yes));
    }

    [Fact]
    public void Per_week_Build_count_20_pages_3_times()
    {
        var target = Target(CountTimes(20, "pages", 3, SchedulePeriod.Week));

        Assert.Equal(EvaluationState.Failed, FinalState(target, true, Amounts(20, 25, 12)));
        Assert.Equal(EvaluationState.Met, FinalState(target, true, Amounts(20, 25, 12, 30)));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), Evaluator.ProgressOf(target, Amounts(20, 25, 12)));
    }

    [Fact]
    public void Per_day_Reduce_count_at_most_1()
    {
        var target = Target(CountTimes(1, "cigarette", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));

        Assert.Equal(EvaluationState.Met, FinalState(target, false, Amounts(0, 1)));
        Assert.Equal(EvaluationState.Failed, FinalState(target, false, Amounts(1, 1)));
    }

    [Fact]
    public void Per_day_Reduce_yes_no_at_most_once()
    {
        var target = Target(YesNoTimes(1, SchedulePeriod.Day, BuildOrReduce.Reduce));

        Assert.Equal(EvaluationState.Met, FinalState(target, false, Yes));
        Assert.Equal(EvaluationState.Failed, FinalState(target, false, Yes, Yes));
        Assert.Equal(EvaluationState.Met, FinalState(target, false, No));
    }

    [Fact]
    public void Per_day_Build_yes_no_2_times()
    {
        var target = Target(YesNoTimes(2, SchedulePeriod.Day));

        Assert.Equal(EvaluationState.Met, FinalState(target, false, Yes, Yes));
        Assert.Equal(EvaluationState.Failed, FinalState(target, false, Yes));
    }

    [Theory]
    [InlineData(59, EvaluationState.Failed)]
    [InlineData(60, EvaluationState.Met)]
    [InlineData(61, EvaluationState.Met)]
    public void A_duration_in_hours_is_compared_in_minutes(int minutes, EvaluationState state)
    {
        var target = Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, AllDays));

        var evaluation = Final(target, false, Amounts(minutes));

        Assert.Equal(state, evaluation.State);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, minutes, 60), evaluation.Progress);
    }

    [Fact]
    public void A_commitment_with_more_than_one_record_is_a_programming_error_whose_message_names_no_value()
    {
        var target = Target(CountDays(20, "pages", BuildOrReduce.Build, AllDays));

        var exception = Assert.Throws<ArgumentException>(() => Evaluator.ProgressOf(target, Amounts(1234, 5678)));
        Assert.Throws<ArgumentException>(() => Final(target, false, Amounts(1234, 5678)));

        Assert.DoesNotContain(exception.Message, char.IsAsciiDigit);
    }

    [Fact]
    public void A_record_of_another_measurement_is_a_programming_error_whose_message_names_no_value()
    {
        var count = Target(CountDays(20, "pages", BuildOrReduce.Build, AllDays));
        var yesNo = Target(YesNoTimes(3, SchedulePeriod.Week));

        var exception = Assert.Throws<ArgumentException>(() => Evaluator.ProgressOf(count, [Yes]));
        Assert.Throws<ArgumentException>(() => Evaluator.ProgressOf(yesNo, Amounts(1234)));

        Assert.DoesNotContain(exception.Message, char.IsAsciiDigit);
    }
}
