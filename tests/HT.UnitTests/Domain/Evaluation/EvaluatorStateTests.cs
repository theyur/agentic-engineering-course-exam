using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Evaluation.EvaluatorCases;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Evaluation;

/// <summary>
/// SC-002, SC-003, US2-AS1 to AS3, AS11, AS13, FR-010, FR-012, FR-016: the state of every combination around its
/// start, its end and its grace end; Pending until the grace end; 48 elapsed hours across Kyiv's clock changes; and
/// a result that depends on the records only, never on their order or on when they were saved.
/// </summary>
public class EvaluatorStateTests
{
    private const string BeforeStart = "starts - 1 tick";
    private const string AtStart = "starts";
    private const string BeforeEnd = "ends - 1 tick";
    private const string AtEnd = "ends";
    private const string BeforeGraceEnd = "grace end - 1 tick";
    private const string AtGraceEnd = "grace end";
    private const string AfterGraceEnd = "grace end + 1 tick";

    private static readonly string[] Moments = [BeforeStart, AtStart, BeforeEnd, AtEnd, BeforeGraceEnd, AtGraceEnd, AfterGraceEnd];

    public static TheoryData<string, string> NamesAndInstants()
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

    private static DateTimeOffset InstantOf(TargetCase targetCase, string instant) => instant switch
    {
        BeforeStart => targetCase.Starts.AddTicks(-1),
        AtStart => targetCase.Starts,
        BeforeEnd => targetCase.Ends.AddTicks(-1),
        AtEnd => targetCase.Ends,
        BeforeGraceEnd => targetCase.GraceEnds.AddTicks(-1),
        AtGraceEnd => targetCase.GraceEnds,
        AfterGraceEnd => targetCase.GraceEnds.AddTicks(1),
        _ => throw new ArgumentOutOfRangeException(nameof(instant)),
    };

    /// <summary>The state before the grace end; null when the entry is final there.</summary>
    private static EvaluationState? StateBeforeFinal(string instant) => instant switch
    {
        BeforeStart => EvaluationState.Upcoming,
        AtStart or BeforeEnd => EvaluationState.Due,
        AtEnd or BeforeGraceEnd => EvaluationState.Pending,
        _ => null,
    };

    [Theory]
    [MemberData(nameof(NamesAndInstants))]
    public void SC_002_every_combination_around_its_start_end_and_grace_end(string name, string instant)
    {
        var targetCase = Named(name);
        var now = InstantOf(targetCase, instant);
        var early = StateBeforeFinal(instant);

        var none = targetCase.EvaluateAt(now, []);
        Assert.Equal(early ?? EvaluationState.Missed, none.State);
        Assert.Equal(targetCase.NoRecords, none.Progress);
        Check(targetCase, now, none);

        foreach (var set in targetCase.Sets)
        {
            var evaluation = targetCase.EvaluateAt(now, set.Records);

            Assert.Equal(early ?? set.Final, evaluation.State);
            Assert.Equal(set.Progress, evaluation.Progress);
            Check(targetCase, now, evaluation);
        }
    }

    private static void Check(TargetCase targetCase, DateTimeOffset now, EntryEvaluation evaluation)
    {
        Assert.Equal(targetCase.GraceEnds, evaluation.GraceEnds);
        Assert.Equal(TimeSpan.FromHours(48), evaluation.GraceEnds - targetCase.Ends);
        Assert.Equal(now >= targetCase.GraceEnds, evaluation.IsFinal);
    }

    [Fact]
    public void Every_combination_has_records_below_or_above_and_at_its_target()
    {
        Assert.All(All, c =>
        {
            Assert.Contains(c.Sets, set => set.Name == At && set.Final == EvaluationState.Met);
            Assert.Contains(c.Sets, set => set.Final == EvaluationState.Failed);
        });
    }

    [Fact]
    public void FR_012_a_record_that_reaches_the_target_is_Pending_with_its_progress_until_the_grace_end_then_Met()
    {
        var targetCase = Named("Build duration commitment, at least 20 min");
        var records = Amounts(25);
        var progress = new Progress(ProgressKind.RecordedValue, 25, 20);

        var atEnd = targetCase.EvaluateAt(Utc("2026-11-18T22:00:00Z"), records);
        var lastMoment = targetCase.EvaluateAt(Utc("2026-11-20T21:59:59Z"), records);
        var atGraceEnd = targetCase.EvaluateAt(Utc("2026-11-20T22:00:00Z"), records);

        Assert.Equal(new EntryEvaluation(EvaluationState.Pending, progress, Utc("2026-11-20T22:00:00Z"), false), atEnd);
        Assert.Equal(new EntryEvaluation(EvaluationState.Pending, progress, Utc("2026-11-20T22:00:00Z"), false), lastMoment);
        Assert.Equal(new EntryEvaluation(EvaluationState.Met, progress, Utc("2026-11-20T22:00:00Z"), true), atGraceEnd);
    }

    [Fact]
    public void US2_AS3_a_period_that_reaches_the_target_stays_Pending_until_its_grace_end()
    {
        var targetCase = Named("Build yes/no, 3 times a week");
        RecordValue[] records = [Yes, Yes, Yes];

        Assert.Equal(EvaluationState.Due, targetCase.EvaluateAt(Utc("2026-11-27T10:00:00Z"), records).State);
        Assert.Equal(EvaluationState.Pending, targetCase.EvaluateAt(Utc("2026-12-01T21:59:59Z"), records).State);
        Assert.Equal(EvaluationState.Met, targetCase.EvaluateAt(Utc("2026-12-01T22:00:00Z"), records).State);
    }

    public static TheoryData<DateOnly, string, string, string, string, string> KyivClockChanges => new()
    {
        // Saturday 24 October 2026: EEST (UTC+3) until 04:00 on Sunday; the grace end is 23:00 EET on Monday 26 October.
        { Date(2026, 10, 24), "2026-10-23T21:00:00Z", "2026-10-24T21:00:00Z", "2026-10-26T21:00:00Z", "2026-10-26T20:59:59Z", "2026-10-26T21:00:00Z" },
        // Saturday 27 March 2027: EET (UTC+2) until 03:00 on Sunday; the grace end is 01:00 EEST on Tuesday 30 March.
        { Date(2027, 3, 27), "2027-03-26T22:00:00Z", "2027-03-27T22:00:00Z", "2027-03-29T22:00:00Z", "2027-03-29T21:59:59Z", "2027-03-29T22:00:00Z" },
    };

    [Theory]
    [MemberData(nameof(KyivClockChanges))]
    public void US2_AS13_the_grace_period_is_48_elapsed_hours_across_a_clock_change(
        DateOnly saturday, string starts, string ends, string graceEnds, string pendingAt, string finalAt)
    {
        var goal = Goal(OnDays(DayOfWeek.Saturday), saturday, Kyiv);
        var entry = GoalTimeline.EntryOf(goal, saturday);
        Assert.NotNull(entry);
        var target = GoalTimeline.VersionOf(goal, saturday).Settings.Target;
        Assert.Equal(new CalendarEntry(saturday, saturday, Utc(starts), Utc(ends)), entry);

        var pending = Evaluator.Evaluate(target, entry.Starts, entry.Ends, [], Utc(pendingAt));
        var final = Evaluator.Evaluate(target, entry.Starts, entry.Ends, [], Utc(finalAt));

        Assert.Equal(EvaluationState.Pending, pending.State);
        Assert.False(pending.IsFinal);
        Assert.Equal(EvaluationState.Missed, final.State);
        Assert.True(final.IsFinal);
        Assert.Equal(Utc(graceEnds), final.GraceEnds);
        Assert.Equal(Utc(graceEnds), GracePeriod.EndOf(entry.Ends));
        Assert.Equal(TimeSpan.FromHours(48), final.GraceEnds - entry.Ends);
    }

    [Fact]
    public void FR_016_the_grace_period_is_48_hours_on_UTC_instants()
    {
        Assert.Equal(TimeSpan.FromHours(48), GracePeriod.Length);
        Assert.Equal(Utc("2026-11-20T22:00:00Z"), GracePeriod.EndOf(Utc("2026-11-18T22:00:00Z")));
        Assert.Throws<ArgumentException>(() => GracePeriod.EndOf(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.FromHours(2))));
    }

    [Theory]
    [MemberData(nameof(NamesAndSets), MemberType = typeof(EvaluatorCases))]
    public void SC_003_the_same_records_give_the_same_result_in_any_order(string name, string set)
    {
        var targetCase = Named(name);
        var records = targetCase.Set(set).Records;
        var expected = targetCase.EvaluateAt(targetCase.AfterGrace, records);

        for (var shift = 1; shift < records.Count; shift++)
        {
            var rotated = records.Skip(shift).Concat(records.Take(shift)).ToArray();
            Assert.Equal(expected, targetCase.EvaluateAt(targetCase.AfterGrace, rotated));
        }

        Assert.Equal(expected, targetCase.EvaluateAt(targetCase.AfterGrace, records.Reverse().ToArray()));
    }

    [Theory]
    [MemberData(nameof(NamesAndSets), MemberType = typeof(EvaluatorCases))]
    public void SC_003_a_final_result_does_not_depend_on_when_it_is_read(string name, string set)
    {
        var targetCase = Named(name);
        var records = targetCase.Set(set);

        var atGraceEnd = targetCase.EvaluateAt(targetCase.GraceEnds, records.Records);
        var aYearLater = targetCase.EvaluateAt(Utc("2027-12-01T12:00:00Z"), records.Records);

        Assert.Equal(records.Final, atGraceEnd.State);
        Assert.Equal(atGraceEnd, aYearLater);
    }
}
