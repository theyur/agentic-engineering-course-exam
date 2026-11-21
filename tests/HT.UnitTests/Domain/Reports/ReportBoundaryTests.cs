using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using HT.Domain.Time;
using HT.UnitTests.Domain.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Reports;

/// <summary>
/// Constitution II, SC-003, D-1: the states <see cref="Evaluator.Evaluate"/> gives every entry of
/// <see cref="GoalTimeline.EntriesThrough"/>, folded by <see cref="Consistency.Of"/> and <see cref="Streaks.Of"/>, on
/// both sides of each boundary: just before, at and just after a grace end; at, below and above a Build target or a
/// Reduce limit; for each schedule kind and for Build and Reduce; and around the clock changes of Europe/Kyiv and
/// Europe/Berlin, where a day of 23 or 25 hours counts once. Every instant is a UTC literal worked out by hand.
/// </summary>
public class ReportBoundaryTests
{
    private const string Below = "below";
    private const string At = "at";
    private const string Above = "above";
    private const string NoRecord = "no record";

    private const string BeforeGraceEnd = "grace end - 1 s";
    private const string AtGraceEnd = "grace end";
    private const string AfterGraceEnd = "grace end + 1 s";

    private static readonly RecordValue Yes = RecordValue.FromAnswer(YesNoAnswer.Yes);
    private static readonly RecordValue No = RecordValue.FromAnswer(YesNoAnswer.No);
    private static readonly NamedTimeZone Berlin = Zone("Europe/Berlin");

    private static RecordValue[] Amounts(params int[] amounts) => amounts.Select(RecordValue.FromAmount).ToArray();

    /// <summary>The records of one level and the final state they give.</summary>
    private sealed record Level(RecordValue[] Records, EvaluationState Final);

    /// <summary>
    /// A goal in UTC with the grace end of its first entry and of its fifth, and its records below, at and above its
    /// target; <paramref name="Failing"/> names the level that fails (below for Build, above for Reduce).
    /// </summary>
    private sealed record BoundaryCase(
        string Name,
        GoalTarget Target,
        DateOnly Start,
        string GraceEndLiterals,
        string FifthGraceEndLiterals,
        IReadOnlyDictionary<string, Level> Levels,
        string Failing)
    {
        public Goal Goal { get; } = GoalSamples.Goal(Target, Start, NamedTimeZone.Utc);

        /// <summary>"before, at, after": the three instants around a grace end.</summary>
        public static DateTimeOffset Moment(string literals, string moment)
        {
            var parts = literals.Split(", ");
            return moment switch
            {
                BeforeGraceEnd => Utc(parts[0]),
                AtGraceEnd => Utc(parts[1]),
                AfterGraceEnd => Utc(parts[2]),
                _ => throw new ArgumentOutOfRangeException(nameof(moment)),
            };
        }
    }

    private static Level Met(params RecordValue[] records) => new(records, EvaluationState.Met);

    private static Level Failed(params RecordValue[] records) => new(records, EvaluationState.Failed);

    private static Dictionary<string, Level> Levels(Level below, Level at, Level above) =>
        new Dictionary<string, Level>
        {
            [Below] = below,
            [At] = at,
            [Above] = above,
            [NoRecord] = new([], EvaluationState.Missed),
        };

    /// <summary>
    /// Commitment-based goals start on Monday 2 November 2026: its commitment ends at 2026-11-03T00:00Z, so its grace
    /// period ends at 2026-11-05T00:00Z; the fifth, Wednesday 11 November, at 2026-11-14T00:00Z. Per-day goals start on
    /// Tuesday 10 November 2026 (D-1's example): grace end 2026-11-13T00:00Z, the fifth day's (14 November)
    /// 2026-11-17T00:00Z. Per-week goals start on Monday 2 November 2026: the week ends at 2026-11-09T00:00Z, grace end
    /// 2026-11-11T00:00Z; the fifth week (30 November to 6 December) 2026-12-09T00:00Z.
    /// </summary>
    private static readonly BoundaryCase[] Cases =
    [
        new(
            "commitment-based Build, 5 pages on Mon Wed Fri",
            Target(CountDays(5, "pages", BuildOrReduce.Build, MonWedFri)),
            Date(2026, 11, 2),
            "2026-11-04T23:59:59Z, 2026-11-05T00:00:00Z, 2026-11-05T00:00:01Z",
            "2026-11-13T23:59:59Z, 2026-11-14T00:00:00Z, 2026-11-14T00:00:01Z",
            Levels(Failed(Amounts(4)), Met(Amounts(5)), Met(Amounts(6))),
            Below),
        new(
            "commitment-based Reduce, at most 2 sweets on Mon Wed Fri",
            Target(CountDays(2, "sweets", BuildOrReduce.Reduce, MonWedFri)),
            Date(2026, 11, 2),
            "2026-11-04T23:59:59Z, 2026-11-05T00:00:00Z, 2026-11-05T00:00:01Z",
            "2026-11-13T23:59:59Z, 2026-11-14T00:00:00Z, 2026-11-14T00:00:01Z",
            Levels(Met(Amounts(1)), Met(Amounts(2)), Failed(Amounts(3))),
            Above),
        new(
            "per-day Build, yes 2 times a day",
            Target(YesNoTimes(2, SchedulePeriod.Day)),
            Date(2026, 11, 10),
            "2026-11-12T23:59:59Z, 2026-11-13T00:00:00Z, 2026-11-13T00:00:01Z",
            "2026-11-16T23:59:59Z, 2026-11-17T00:00:00Z, 2026-11-17T00:00:01Z",
            Levels(Failed(Yes, No), Met(Yes, Yes), Met(Yes, Yes, Yes)),
            Below),
        new(
            "per-day Reduce, at most 120 min a day",
            Target(DurationTimes(120, DurationUnit.Minutes, 1, SchedulePeriod.Day, BuildOrReduce.Reduce)),
            Date(2026, 11, 10),
            "2026-11-12T23:59:59Z, 2026-11-13T00:00:00Z, 2026-11-13T00:00:01Z",
            "2026-11-16T23:59:59Z, 2026-11-17T00:00:00Z, 2026-11-17T00:00:01Z",
            Levels(Met(Amounts(119)), Met(Amounts(100, 20)), Failed(Amounts(100, 21))),
            Above),
        new(
            "per-week Build, yes 3 times a week",
            Target(YesNoTimes(3, SchedulePeriod.Week)),
            Date(2026, 11, 2),
            "2026-11-10T23:59:59Z, 2026-11-11T00:00:00Z, 2026-11-11T00:00:01Z",
            "2026-12-08T23:59:59Z, 2026-12-09T00:00:00Z, 2026-12-09T00:00:01Z",
            Levels(Failed(Yes, Yes, No), Met(Yes, Yes, Yes), Met(Yes, Yes, Yes, Yes)),
            Below),
        new(
            "per-week Reduce, yes at most 2 times a week",
            Target(YesNoTimes(2, SchedulePeriod.Week, BuildOrReduce.Reduce)),
            Date(2026, 11, 2),
            "2026-11-10T23:59:59Z, 2026-11-11T00:00:00Z, 2026-11-11T00:00:01Z",
            "2026-12-08T23:59:59Z, 2026-12-09T00:00:00Z, 2026-12-09T00:00:01Z",
            Levels(Met(Yes), Met(Yes, Yes), Failed(Yes, Yes, Yes)),
            Above),
    ];

    private static BoundaryCase Named(string name) => Cases.Single(c => c.Name == name);

    /// <summary>The state of every entry of the goal through its current date at <paramref name="now"/>, each with its own version's target.</summary>
    private static List<EvaluationState> StatesAt(Goal goal, IReadOnlyDictionary<DateOnly, RecordValue[]> records, DateTimeOffset now)
    {
        if (GoalTimeline.CurrentDate(goal, now) is not { } current)
        {
            return [];
        }

        return GoalTimeline.EntriesThrough(goal, current)
            .Select(entry => Evaluator.Evaluate(
                GoalTimeline.VersionOf(goal, entry.FirstDate).Settings.Target,
                entry.Starts,
                entry.Ends,
                records.GetValueOrDefault(entry.FirstDate, []),
                now).State)
            .ToList();
    }

    public static TheoryData<string, string, string> CaseLevelAndMoment()
    {
        var data = new TheoryData<string, string, string>();
        foreach (var boundaryCase in Cases)
        {
            foreach (var level in new[] { Below, At, Above, NoRecord })
            {
                foreach (var moment in new[] { BeforeGraceEnd, AtGraceEnd, AfterGraceEnd })
                {
                    data.Add(boundaryCase.Name, level, moment);
                }
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(CaseLevelAndMoment))]
    public void SC_003_the_first_result_counts_from_its_grace_end_on_by_its_target(string name, string level, string moment)
    {
        var boundaryCase = Named(name);
        var records = boundaryCase.Levels[level];
        var now = BoundaryCase.Moment(boundaryCase.GraceEndLiterals, moment);

        var states = StatesAt(boundaryCase.Goal, new Dictionary<DateOnly, RecordValue[]> { [boundaryCase.Start] = records.Records }, now);

        if (moment == BeforeGraceEnd)
        {
            Assert.Equal(EvaluationState.Pending, states[0]);
            Assert.Equal(new Consistency(0, 0), Consistency.Of(states));
            Assert.Null(Consistency.Of(states).Percentage);
            Assert.Equal(new Streaks(0, 0), Streaks.Of(states));
            return;
        }

        Assert.Equal(records.Final, states[0]);
        var met = records.Final == EvaluationState.Met;
        Assert.Equal(met ? new Consistency(1, 1) : new Consistency(0, 1), Consistency.Of(states));
        Assert.Equal(met ? 100 : 0, Consistency.Of(states).Percentage);
        Assert.Equal(met ? new Streaks(1, 1) : new Streaks(0, 0), Streaks.Of(states));
    }

    [Fact]
    public void The_cases_cover_every_schedule_kind_for_Build_and_Reduce_with_the_expected_states()
    {
        Assert.Equal(6, Cases.Length);
        foreach (var boundaryCase in Cases)
        {
            var build = boundaryCase.Target.BuildOrReduce == BuildOrReduce.Build;
            Assert.Equal(build ? Below : Above, boundaryCase.Failing);
            Assert.Equal(build ? EvaluationState.Failed : EvaluationState.Met, boundaryCase.Levels[Below].Final);
            Assert.Equal(EvaluationState.Met, boundaryCase.Levels[At].Final);
            Assert.Equal(build ? EvaluationState.Met : EvaluationState.Failed, boundaryCase.Levels[Above].Final);
        }
    }

    public static TheoryData<string, string> CaseAndFailure()
    {
        var data = new TheoryData<string, string>();
        foreach (var boundaryCase in Cases)
        {
            data.Add(boundaryCase.Name, boundaryCase.Failing);
            data.Add(boundaryCase.Name, NoRecord);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(CaseAndFailure))]
    public void Met_Met_Met_a_failure_and_Met_give_80_of_4_and_5_with_streaks_1_and_3_from_the_fifth_grace_end_on(string name, string failure)
    {
        var boundaryCase = Named(name);
        var entries = GoalTimeline.EntriesThrough(boundaryCase.Goal, boundaryCase.Start.AddDays(40)).Take(5).ToList();
        string[] levels = [At, At, At, failure, At];
        var records = entries.Zip(levels).ToDictionary(pair => pair.First.FirstDate, pair => boundaryCase.Levels[pair.Second].Records);

        var before = StatesAt(boundaryCase.Goal, records, BoundaryCase.Moment(boundaryCase.FifthGraceEndLiterals, BeforeGraceEnd));
        Assert.Equal(new Consistency(3, 4), Consistency.Of(before));
        Assert.Equal(75, Consistency.Of(before).Percentage);
        Assert.Equal(new Streaks(0, 3), Streaks.Of(before));

        foreach (var moment in new[] { AtGraceEnd, AfterGraceEnd })
        {
            var states = StatesAt(boundaryCase.Goal, records, BoundaryCase.Moment(boundaryCase.FifthGraceEndLiterals, moment));
            Assert.Equal(new Consistency(4, 5), Consistency.Of(states));
            Assert.Equal(80, Consistency.Of(states).Percentage);
            Assert.Equal(new Streaks(1, 3), Streaks.Of(states));
        }
    }

    // Clock changes.

    private static NamedTimeZone ZoneNamed(string zone) => zone == "Europe/Berlin" ? Berlin : Kyiv;

    private static GoalTarget ClockTarget(string kind, BuildOrReduce side) => kind switch
    {
        "commitment-based" => Target(YesNoDays(side, DayOfWeek.Friday)),
        "per-day" => Target(YesNoTimes(1, SchedulePeriod.Day, side)),
        "per-week" => Target(YesNoTimes(2, SchedulePeriod.Week, side)),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    /// <summary>Records that meet the target: "Yes" for Build (twice a week for per-week), "No" for Reduce.</summary>
    private static RecordValue[] MetRecords(string kind, BuildOrReduce side) =>
        side == BuildOrReduce.Reduce ? [No] : kind == "per-week" ? [Yes, Yes] : [Yes];

    /// <summary>
    /// The first entry's grace end around each clock change, with the instant one second before it. Friday 23 October
    /// 2026 ends at Saturday midnight before the change back (Kyiv UTC+3, Berlin UTC+2), and its grace end falls on
    /// Sunday 23:00 after it. Friday 26 March 2027 ends before the change forward (Kyiv UTC+2, Berlin UTC+1), and its
    /// grace end falls on Monday 01:00 after it. The weeks of 19 to 25 October 2026 and 22 to 28 March 2027 contain
    /// the change and end at the next Monday's midnight, after it.
    /// </summary>
    public static TheoryData<string, string, string, BuildOrReduce, string, string> ClockChangeGraceEnds()
    {
        var data = new TheoryData<string, string, string, BuildOrReduce, string, string>();
        (string Zone, string Kind, string Start, string Before, string GraceEnd)[] rows =
        [
            ("Europe/Kyiv", "commitment-based", "2026-10-23", "2026-10-25T20:59:59Z", "2026-10-25T21:00:00Z"),
            ("Europe/Kyiv", "per-day", "2026-10-23", "2026-10-25T20:59:59Z", "2026-10-25T21:00:00Z"),
            ("Europe/Kyiv", "per-week", "2026-10-19", "2026-10-27T21:59:59Z", "2026-10-27T22:00:00Z"),
            ("Europe/Kyiv", "commitment-based", "2027-03-26", "2027-03-28T21:59:59Z", "2027-03-28T22:00:00Z"),
            ("Europe/Kyiv", "per-day", "2027-03-26", "2027-03-28T21:59:59Z", "2027-03-28T22:00:00Z"),
            ("Europe/Kyiv", "per-week", "2027-03-22", "2027-03-30T20:59:59Z", "2027-03-30T21:00:00Z"),
            ("Europe/Berlin", "commitment-based", "2026-10-23", "2026-10-25T21:59:59Z", "2026-10-25T22:00:00Z"),
            ("Europe/Berlin", "per-day", "2026-10-23", "2026-10-25T21:59:59Z", "2026-10-25T22:00:00Z"),
            ("Europe/Berlin", "per-week", "2026-10-19", "2026-10-27T22:59:59Z", "2026-10-27T23:00:00Z"),
            ("Europe/Berlin", "commitment-based", "2027-03-26", "2027-03-28T22:59:59Z", "2027-03-28T23:00:00Z"),
            ("Europe/Berlin", "per-day", "2027-03-26", "2027-03-28T22:59:59Z", "2027-03-28T23:00:00Z"),
            ("Europe/Berlin", "per-week", "2027-03-22", "2027-03-30T21:59:59Z", "2027-03-30T22:00:00Z"),
        ];
        foreach (var row in rows)
        {
            foreach (var side in new[] { BuildOrReduce.Build, BuildOrReduce.Reduce })
            {
                data.Add(row.Zone, row.Kind, row.Start, side, row.Before, row.GraceEnd);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ClockChangeGraceEnds))]
    public void SC_003_around_a_clock_change_a_result_is_final_from_its_grace_end_on(
        string zone, string kind, string start, BuildOrReduce side, string before, string graceEnd)
    {
        var startDate = DateOnly.ParseExact(start, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var goal = Goal(ClockTarget(kind, side), startDate, ZoneNamed(zone));
        var met = new Dictionary<DateOnly, RecordValue[]> { [startDate] = MetRecords(kind, side) };
        var none = new Dictionary<DateOnly, RecordValue[]>();

        var pending = StatesAt(goal, met, Utc(before));
        Assert.Equal(EvaluationState.Pending, pending[0]);
        Assert.Equal(new Consistency(0, 0), Consistency.Of(pending));
        Assert.Equal(new Streaks(0, 0), Streaks.Of(pending));

        var final = StatesAt(goal, met, Utc(graceEnd));
        Assert.Equal(EvaluationState.Met, final[0]);
        Assert.Equal(new Consistency(1, 1), Consistency.Of(final));
        Assert.Equal(new Streaks(1, 1), Streaks.Of(final));

        var missed = StatesAt(goal, none, Utc(graceEnd));
        Assert.Equal(EvaluationState.Missed, missed[0]);
        Assert.Equal(new Consistency(0, 1), Consistency.Of(missed));
        Assert.Equal(new Streaks(0, 0), Streaks.Of(missed));

        Assert.Equal(new Consistency(0, 0), Consistency.Of(StatesAt(goal, none, Utc(before))));
    }

    /// <summary>
    /// Friday to Monday around each change, with "Yes" on all four days: the Sunday lasts 25 hours (change back) or 23
    /// (change forward). Read at the Monday's grace end, the four count once each: Tuesday and Wednesday are Pending,
    /// Thursday is Due. One second earlier the Monday is still Pending.
    /// </summary>
    public static TheoryData<string, string, string, string, string, string> ClockChangeDays()
    {
        var data = new TheoryData<string, string, string, string, string, string>();
        (string Zone, string Friday, string SundayStarts, string SundayEnds, string Before, string MondayGraceEnd)[] rows =
        [
            ("Europe/Kyiv", "2026-10-23", "2026-10-24T21:00:00Z", "2026-10-25T22:00:00Z", "2026-10-28T21:59:59Z", "2026-10-28T22:00:00Z"),
            ("Europe/Kyiv", "2027-03-26", "2027-03-27T22:00:00Z", "2027-03-28T21:00:00Z", "2027-03-31T20:59:59Z", "2027-03-31T21:00:00Z"),
            ("Europe/Berlin", "2026-10-23", "2026-10-24T22:00:00Z", "2026-10-25T23:00:00Z", "2026-10-28T22:59:59Z", "2026-10-28T23:00:00Z"),
            ("Europe/Berlin", "2027-03-26", "2027-03-27T23:00:00Z", "2027-03-28T22:00:00Z", "2027-03-31T21:59:59Z", "2027-03-31T22:00:00Z"),
        ];
        foreach (var row in rows)
        {
            foreach (var kind in new[] { "commitment-based", "per-day" })
            {
                data.Add(row.Zone, kind, row.Friday, row.SundayStarts, row.SundayEnds, row.Before + " " + row.MondayGraceEnd);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ClockChangeDays))]
    public void Edge_case_a_23_or_25_hour_day_counts_once(
        string zone, string kind, string friday, string sundayStarts, string sundayEnds, string beforeAndGraceEnd)
    {
        var fridayDate = DateOnly.ParseExact(friday, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var target = kind == "per-day" ? PerDay() : OnDays(AllDays);
        var goal = Goal(target, fridayDate, ZoneNamed(zone));
        var mondayDate = fridayDate.AddDays(3);
        var instants = beforeAndGraceEnd.Split(' ');

        var fridayToMonday = GoalTimeline.EntriesThrough(goal, mondayDate);
        Assert.Equal(4, fridayToMonday.Count);
        var sunday = fridayToMonday[2];
        Assert.Equal(DayOfWeek.Sunday, sunday.FirstDate.DayOfWeek);
        Assert.Equal(Utc(sundayStarts), sunday.Starts);
        Assert.Equal(Utc(sundayEnds), sunday.Ends);

        var records = fridayToMonday.ToDictionary(entry => entry.FirstDate, _ => new[] { Yes });

        var before = StatesAt(goal, records, Utc(instants[0]));
        Assert.Equal(new Consistency(3, 3), Consistency.Of(before));
        Assert.Equal(new Streaks(3, 3), Streaks.Of(before));

        var states = StatesAt(goal, records, Utc(instants[1]));
        Assert.Equal(
            [EvaluationState.Met, EvaluationState.Met, EvaluationState.Met, EvaluationState.Met, EvaluationState.Pending, EvaluationState.Pending, EvaluationState.Due],
            states);
        Assert.Equal(new Consistency(4, 4), Consistency.Of(states));
        Assert.Equal(new Streaks(4, 4), Streaks.Of(states));
    }
}
