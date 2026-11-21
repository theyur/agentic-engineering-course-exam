using HT.Domain.Evaluation;
using static HT.UnitTests.Domain.Evaluation.EvaluatorCases;

namespace HT.UnitTests.Domain.Evaluation;

/// <summary>
/// Slice 7 FR-003, FR-007, FR-009, FR-015, research R-02: with Excused or Paused, a commitment, a day and a week read
/// that state at every instant, keep the progress of their records and the grace end, and are never final; any
/// other exemption is refused; with none, the result is exactly the overload without an exemption, for every case of
/// <see cref="EvaluatorCases"/>.
/// </summary>
public class EvaluatorExemptionTests
{
    private const string BeforeStart = "starts - 1 tick";
    private const string AtStart = "starts";
    private const string BeforeEnd = "ends - 1 tick";
    private const string AtEnd = "ends";
    private const string BeforeGraceEnd = "grace end - 1 tick";
    private const string AtGraceEnd = "grace end";
    private const string AfterGraceEnd = "grace end + 1 tick";
    private const string LongAfter = "a year after the grace end";

    private static readonly string[] Moments =
        [BeforeStart, AtStart, BeforeEnd, AtEnd, BeforeGraceEnd, AtGraceEnd, AfterGraceEnd, LongAfter];

    private static DateTimeOffset InstantOf(TargetCase targetCase, string instant) => instant switch
    {
        BeforeStart => targetCase.Starts.AddTicks(-1),
        AtStart => targetCase.Starts,
        BeforeEnd => targetCase.Ends.AddTicks(-1),
        AtEnd => targetCase.Ends,
        BeforeGraceEnd => targetCase.GraceEnds.AddTicks(-1),
        AtGraceEnd => targetCase.GraceEnds,
        AfterGraceEnd => targetCase.GraceEnds.AddTicks(1),
        LongAfter => targetCase.GraceEnds.AddYears(1),
        _ => throw new ArgumentOutOfRangeException(nameof(instant)),
    };

    public static TheoryData<string, string, EvaluationState> CasesInstantsAndExemptions()
    {
        var data = new TheoryData<string, string, EvaluationState>();
        foreach (var targetCase in All)
        {
            foreach (var instant in Moments)
            {
                data.Add(targetCase.Name, instant, EvaluationState.Excused);
                data.Add(targetCase.Name, instant, EvaluationState.Paused);
            }
        }

        return data;
    }

    public static TheoryData<string, string> CasesAndInstants()
    {
        var data = new TheoryData<string, string>();
        foreach (var targetCase in All)
        {
            foreach (var instant in Moments)
            {
                data.Add(targetCase.Name, instant);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(CasesInstantsAndExemptions))]
    public void An_exempt_entry_without_records_reads_its_exemption_at_every_instant_and_is_never_final(
        string name, string instant, EvaluationState exemption)
    {
        var targetCase = Named(name);

        var evaluation = Evaluator.Evaluate(targetCase.Target, targetCase.Starts, targetCase.Ends, [], InstantOf(targetCase, instant), exemption);

        Assert.Equal(exemption, evaluation.State);
        Assert.False(evaluation.IsFinal);
        Assert.Equal(targetCase.GraceEnds, evaluation.GraceEnds);
        Assert.Equal(targetCase.NoRecords, evaluation.Progress);
    }

    [Theory]
    [MemberData(nameof(CasesInstantsAndExemptions))]
    public void An_exempt_entry_keeps_the_progress_of_its_records(string name, string instant, EvaluationState exemption)
    {
        var targetCase = Named(name);
        var now = InstantOf(targetCase, instant);

        foreach (var set in targetCase.Sets)
        {
            var exempt = Evaluator.Evaluate(targetCase.Target, targetCase.Starts, targetCase.Ends, set.Records, now, exemption);
            var plain = targetCase.EvaluateAt(now, set.Records);

            Assert.Equal(exemption, exempt.State);
            Assert.Equal(set.Progress, exempt.Progress);
            Assert.Equal(plain.Progress, exempt.Progress);
            Assert.Equal(plain.GraceEnds, exempt.GraceEnds);
            Assert.False(exempt.IsFinal);
        }
    }

    [Theory]
    [MemberData(nameof(CasesAndInstants))]
    public void No_exemption_gives_exactly_the_result_of_the_overload_without_one(string name, string instant)
    {
        var targetCase = Named(name);
        var now = InstantOf(targetCase, instant);

        Assert.Equal(targetCase.EvaluateAt(now, []), Evaluator.Evaluate(targetCase.Target, targetCase.Starts, targetCase.Ends, [], now, null));
        foreach (var set in targetCase.Sets)
        {
            Assert.Equal(
                targetCase.EvaluateAt(now, set.Records),
                Evaluator.Evaluate(targetCase.Target, targetCase.Starts, targetCase.Ends, set.Records, now, null));
        }
    }

    [Theory]
    [InlineData(EvaluationState.Upcoming)]
    [InlineData(EvaluationState.Due)]
    [InlineData(EvaluationState.Pending)]
    [InlineData(EvaluationState.Met)]
    [InlineData(EvaluationState.Failed)]
    [InlineData(EvaluationState.Missed)]
    [InlineData((EvaluationState)99)]
    public void Only_Excused_or_Paused_can_be_an_exemption(EvaluationState exemption)
    {
        var targetCase = All[0];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Evaluator.Evaluate(targetCase.Target, targetCase.Starts, targetCase.Ends, [], targetCase.Ends, exemption));
    }

    [Fact]
    public void An_exempt_entry_still_refuses_records_that_do_not_match_its_target()
    {
        var yesNo = Named("Build yes/no commitment");

        Assert.Throws<ArgumentException>(() =>
            Evaluator.Evaluate(yesNo.Target, yesNo.Starts, yesNo.Ends, Amounts(3), yesNo.Ends, EvaluationState.Paused));
    }
}
