using HT.Application.Goals;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.UnitTests.Application.Goals;

namespace HT.UnitTests.Application.Operator;

/// <summary>
/// Slice 8 FR-031, research R-09: the three goals of <c>demo7@example.com</c>, with their settings versions, attempts,
/// endings, records and notes relative to the run date R ("R − k" is the date k days before R), and FR-031's figures
/// read through <see cref="ReportService"/>, <see cref="AttemptService"/> (with <see cref="AttemptService.GetGoalHistoryAsync"/>),
/// <see cref="GoalService"/> and <see cref="RecordingService"/> on <see cref="AttemptWorld"/>, with the goals built from
/// <see cref="DemoGoalChanges.For"/> through the <see cref="Goal"/> attempts constructor and the demo records stored
/// directly. The run dates are the seven days of the week of Monday 16 November 2026, so every weekday of R is
/// covered; the run time is 10:00 UTC on R, and the user and the default zone are UTC, as on the hosted demo.
/// </summary>
public sealed class DemoGoalChangesTests
{
    private const string ReadingName = "Reading, 20 min daily";
    private const string MeditationName = "Meditation, 5 min daily";
    private const string SugarName = "Sugar, at most 1 sweet a day";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly HashSet<DayOfWeek> EveryDay =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
        DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    public static TheoryData<DateOnly, DayOfWeek> RunDates => new()
    {
        { new DateOnly(2026, 11, 16), DayOfWeek.Monday },
        { new DateOnly(2026, 11, 17), DayOfWeek.Tuesday },
        { new DateOnly(2026, 11, 18), DayOfWeek.Wednesday },
        { new DateOnly(2026, 11, 19), DayOfWeek.Thursday },
        { new DateOnly(2026, 11, 20), DayOfWeek.Friday },
        { new DateOnly(2026, 11, 21), DayOfWeek.Saturday },
        { new DateOnly(2026, 11, 22), DayOfWeek.Sunday },
    };

    [Theory]
    [MemberData(nameof(RunDates))]
    public void The_three_goals_are_Reading_Meditation_and_Sugar_on_Published_habits_with_FR_031s_versions_and_attempts(DateOnly r, DayOfWeek day)
    {
        Assert.Equal(day, r.DayOfWeek);

        var goals = DemoGoalChanges.For(r);

        Assert.Equal(["Reading", "Meditation", "Sugar"], goals.Select(g => g.CatalogHabitName));
        Assert.All(goals, g => Assert.Equal(new[] { CatalogHabitState.Published }, g.AcceptedStates));

        var reading = Assert.Single(goals[0].Attempts);
        Assert.Equal(1, reading.Number);
        Assert.Null(reading.Ending);
        Assert.Equal(
            [(r.AddDays(-28), Minutes(15)), (r.AddDays(-13), Minutes(20)), (r.AddDays(1), Minutes(25))],
            reading.Versions.Select(v => (v.AppliesFrom, v.Target)));
        Assert.Equal(r.AddDays(-28), goals[0].StartDate);

        Assert.Equal(2, goals[1].Attempts.Count);
        var (first, second) = (goals[1].Attempts[0], goals[1].Attempts[1]);
        Assert.Equal(1, first.Number);
        Assert.Equal([(r.AddDays(-42), Minutes(10))], first.Versions.Select(v => (v.AppliesFrom, v.Target)));
        Assert.Equal(r.AddDays(-21), first.Ending!.Date);
        Assert.Equal(Outcome.Abandoned, first.Ending.Outcome);
        Assert.Equal("Too busy.", first.Ending.Note!.Value.Value);
        Assert.Equal(2, second.Number);
        Assert.Equal([(r.AddDays(-14), Minutes(5))], second.Versions.Select(v => (v.AppliesFrom, v.Target)));
        Assert.Equal(r.AddDays(-14), second.StartDate);
        Assert.Null(second.Ending);
        Assert.Equal(r.AddDays(-42), goals[1].StartDate);

        var sugar = Assert.Single(goals[2].Attempts);
        Assert.Equal(1, sugar.Number);
        Assert.Equal([(r.AddDays(-30), SugarTarget())], sugar.Versions.Select(v => (v.AppliesFrom, v.Target)));
        Assert.Equal(r.AddDays(-7), sugar.Ending!.Date);
        Assert.Equal(Outcome.Achieved, sugar.Ending.Outcome);
        Assert.Equal("Three weeks without extra sweets.", sugar.Ending.Note!.Value.Value);
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void The_records_and_notes_are_FR_031s(DateOnly r, DayOfWeek day)
    {
        Assert.Equal(day, r.DayOfWeek);

        var goals = DemoGoalChanges.For(r);

        List<(DateOnly, int?)> reading = [];
        for (var k = 28; k >= 2; k--)
        {
            reading.Add((r.AddDays(-k), k >= 14 || k == 9 ? 15 : 20));
        }

        Assert.Equal(reading, goals[0].Records.Select(x => (x.Date, x.Value.Amount)));
        Assert.Equal([(r.AddDays(-9), "Short on time.")], goals[0].Notes.Select(n => (n.EntryDate, n.Text.Value)));

        List<(DateOnly, int?)> meditation = [];
        for (var k = 42; k >= 32; k--)
        {
            meditation.Add((r.AddDays(-k), 10));
        }

        for (var k = 14; k >= 2; k--)
        {
            meditation.Add((r.AddDays(-k), 5));
        }

        Assert.Equal(meditation, goals[1].Records.Select(x => (x.Date, x.Value.Amount)));
        Assert.Empty(goals[1].Notes);

        Assert.Equal(Enumerable.Range(8, 23).Reverse().Select(k => (r.AddDays(-k), (int?)0)), goals[2].Records.Select(x => (x.Date, x.Value.Amount)));
        Assert.Empty(goals[2].Notes);

        foreach (var goal in goals)
        {
            Assert.All(goal.Records, x => Assert.Null(x.Value.Answer));
            Assert.All(goal.Records, x => Assert.InRange(x.Date, goal.StartDate, r.AddDays(-1)));
            Assert.Equal(goal.Records.Select(x => x.Date).Order(), goal.Records.Select(x => x.Date));
            Assert.Equal(goal.Records.Count, goal.Records.Select(x => x.Date).Distinct().Count());
        }
    }

    [Fact]
    public void On_Wednesday_18_November_the_dates_are_those_of_the_acceptance_criteria()
    {
        var goals = DemoGoalChanges.For(new DateOnly(2026, 11, 18));

        Assert.Equal(
            [new DateOnly(2026, 10, 21), new DateOnly(2026, 11, 5), new DateOnly(2026, 11, 19)],
            goals[0].Attempts[0].Versions.Select(v => v.AppliesFrom));
        Assert.Equal(new DateOnly(2026, 11, 9), Assert.Single(goals[0].Notes).EntryDate);
        Assert.Equal(new DateOnly(2026, 10, 7), goals[1].StartDate);
        Assert.Equal(new DateOnly(2026, 10, 28), goals[1].Attempts[0].Ending!.Date);
        Assert.Equal(new DateOnly(2026, 11, 4), goals[1].Attempts[1].StartDate);
        Assert.Equal(new DateOnly(2026, 10, 19), goals[2].StartDate);
        Assert.Equal(new DateOnly(2026, 11, 11), goals[2].Attempts[0].Ending!.Date);
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Every_goal_passes_the_Goal_constructors_invariants_when_built_from_it(DateOnly r, DayOfWeek day)
    {
        Assert.Equal(day, r.DayOfWeek);

        foreach (var demo in DemoGoalChanges.For(r))
        {
            var goal = new Goal(
                new GoalId(Guid.NewGuid()),
                new HT.Domain.Accounts.AccountId(Guid.NewGuid()),
                new GoalHabit(new HabitId(Guid.NewGuid()), HabitKind.Catalog),
                null,
                demo.AttemptsIn(NamedTimeZone.Utc));
            Assert.Equal(demo.StartDate, goal.StartDate);
            Assert.Equal(demo.Attempts.Count, goal.Attempts.Count);
            Assert.All(goal.Attempts.SelectMany(a => a.Versions), v => Assert.Equal(NamedTimeZone.Utc, v.Settings.TimeZone));
        }
    }

    [Fact]
    public void Every_note_passes_NoteText_and_every_ending_note_passes_EndingNote_unchanged()
    {
        Assert.Equal("Short on time.", DemoGoalChanges.ShortOnTimeNote);
        Assert.Equal("Too busy.", DemoGoalChanges.TooBusyEndingNote);
        Assert.Equal("Three weeks without extra sweets.", DemoGoalChanges.SugarEndingNote);

        var goals = DemoGoalChanges.For(new DateOnly(2026, 11, 16));
        Assert.All(goals.SelectMany(g => g.Notes), n =>
        {
            Assert.True(NoteText.TryCreate(n.Text.Value, out var again, out _));
            Assert.Equal(n.Text, again);
        });
        var endingNotes = goals.SelectMany(g => g.Attempts).Select(a => a.Ending?.Note).OfType<EndingNote>().ToList();
        Assert.Equal(2, endingNotes.Count);
        Assert.All(endingNotes, n =>
        {
            Assert.True(EndingNote.TryCreate(n.Value, out var again, out var tooLong));
            Assert.False(tooLong);
            Assert.Equal(n, again);
        });
    }

    [Fact]
    public void Each_record_value_passes_the_value_checks_of_its_goals_measurement()
    {
        foreach (var goal in DemoGoalChanges.For(new DateOnly(2026, 11, 16)))
        {
            var measurement = goal.Attempts[0].Versions[0].Target.Measurement;
            Assert.All(goal.Records, x =>
            {
                Assert.True(RecordValue.TryCreate(measurement, x.Value.Answer, x.Value.Amount, out var value, out _));
                Assert.Equal(x.Value, value);
            });
        }
    }

    [Fact]
    public void The_goal_names_the_earlier_attempts_name_and_the_waiting_target_text_are_FR_031s()
    {
        Assert.Equal([ReadingName, MeditationName, SugarName], DemoGoalChanges.GoalNames);
        Assert.Equal("Meditation, 10 min daily", DemoGoalChanges.EarlierAttemptName);
        Assert.Equal("25 min daily", DemoGoalChanges.WaitingTargetText);
    }

    [Fact]
    public void A_demo_goal_change_goal_refuses_null_parts_and_attempts_that_fail_the_goal_invariants()
    {
        var r = new DateOnly(2026, 11, 16);
        var states = new[] { CatalogHabitState.Published };
        DemoAttempt Open(int number, int k) => new(number, [new DemoVersion(r.AddDays(-k), Minutes(5))], null);
        DemoAttempt Ended(int number, int k, int endK) =>
            new(number, [new DemoVersion(r.AddDays(-k), Minutes(5))], new GoalEnding(r.AddDays(-endK), Outcome.Abandoned, null));

        Assert.Throws<ArgumentException>(() => new DemoGoalChangeGoal(" ", states, [Open(1, 3)], [], []));
        Assert.Throws<ArgumentException>(() => new DemoGoalChangeGoal("Reading", [], [Open(1, 3)], [], []));
        Assert.Throws<ArgumentException>(() => new DemoGoalChangeGoal("Reading", [(CatalogHabitState)99], [Open(1, 3)], [], []));
        Assert.Throws<ArgumentException>(() => new DemoGoalChangeGoal("Reading", states, [null!], [], []));
        Assert.Throws<ArgumentException>(() => new DemoGoalChangeGoal("Reading", states, [Open(1, 3)], [null!], []));
        Assert.Throws<ArgumentException>(() => new DemoGoalChangeGoal("Reading", states, [Open(1, 3)], [], [null!]));

        // No attempt; a gap in the numbers; an attempt before the latest that is not ended; an attempt that starts on
        // the previous ending date; versions out of order.
        string?[] failures =
        [
            Refusal([]),
            Refusal([Open(2, 3)]),
            Refusal([Open(1, 10), Open(2, 3)]),
            Refusal([Ended(1, 10, 5), Open(2, 5)]),
            Refusal([new DemoAttempt(1, [new DemoVersion(r, Minutes(5)), new DemoVersion(r.AddDays(-1), Minutes(10))], null)]),
        ];
        Assert.All(failures, f => Assert.Equal("attempts", f));

        Assert.Equal(2, new DemoGoalChangeGoal("Reading", states, [Ended(1, 10, 5), Open(2, 4)], [], []).Attempts.Count);

        string? Refusal(DemoAttempt[] attempts) =>
            Assert.Throws<ArgumentException>(() => new DemoGoalChangeGoal("Reading", states, attempts, [], [])).ParamName;
    }

    [Fact]
    public void A_demo_attempt_refuses_a_number_below_1_no_version_or_a_null_version()
    {
        var version = new DemoVersion(new DateOnly(2026, 11, 16), Minutes(5));

        Assert.Throws<ArgumentOutOfRangeException>(() => new DemoAttempt(0, [version], null));
        Assert.Throws<ArgumentException>(() => new DemoAttempt(1, [], null));
        Assert.Throws<ArgumentException>(() => new DemoAttempt(1, [null!], null));
        Assert.Throws<ArgumentNullException>(() => new DemoVersion(version.AppliesFrom, null!));
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public async Task FR_031_Reading_has_96_percent_its_streaks_its_newest_states_and_its_waiting_change(DateOnly r, DayOfWeek day)
    {
        Assert.Equal(day, r.DayOfWeek);
        var (world, ids) = Arrange(r);

        var report = await ReportAsync(world, ids[0]);
        Assert.Equal(ReadingName, report.Name);
        Assert.Equal("96% (25 of 26)", report.Stats.ConsistencyText);
        Assert.Equal(("6 commitments", "19 commitments"), (report.Stats.CurrentStreakText, report.Stats.LongestStreakText));

        // Counted R − 28 to R − 3: R − 9 Failed (15 of 20 minutes), R − 14 Met under version 1 with 15 minutes.
        var history = await HistoryAsync(world, ids[0], r.AddDays(-28), r);
        Assert.Equal(29, history.Count);
        Assert.Equal(
            [(r.AddDays(-9), EvaluationState.Failed), (r.AddDays(-2), EvaluationState.Pending), (r.AddDays(-1), EvaluationState.Pending), (r, EvaluationState.Due)],
            history.Where(e => e.State != EvaluationState.Met).Select(e => (e.FirstDate, e.State)).OrderBy(e => e.FirstDate));
        Assert.Equal(15, Assert.Single(history.Single(e => e.FirstDate == r.AddDays(-14)).Records).Amount);
        Assert.Equal("Short on time.", history.Single(e => e.FirstDate == r.AddDays(-9)).Note);

        var details = await DetailsAsync(world, ids[0]);
        Assert.Equal("20 min daily", details.TargetText);
        Assert.Equal(r.AddDays(-13), details.TimeZoneAppliesFrom);
        Assert.Equal("25 min daily", details.Later!.TargetText);
        Assert.Equal(r.AddDays(1), details.LaterTimeZoneAppliesFrom);
        Assert.Equal(1, details.AttemptNumber);
        Assert.False(details.IsEnded);
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public async Task FR_031_Meditation_is_on_attempt_2_with_100_percent_and_its_attempt_1_ended_Abandoned_with_52_percent(DateOnly r, DayOfWeek day)
    {
        Assert.Equal(day, r.DayOfWeek);
        var (world, ids) = Arrange(r);

        var report = await ReportAsync(world, ids[1]);
        Assert.Equal(MeditationName, report.Name);
        Assert.Equal("100% (12 of 12)", report.Stats.ConsistencyText);
        Assert.Equal(("12 commitments", "12 commitments"), (report.Stats.CurrentStreakText, report.Stats.LongestStreakText));
        Assert.Equal(
            [(r.AddDays(-2), EvaluationState.Pending), (r.AddDays(-1), EvaluationState.Pending), (r, EvaluationState.Due)],
            (await HistoryAsync(world, ids[1], r.AddDays(-2), r)).Select(e => (e.FirstDate, e.State)).OrderBy(e => e.FirstDate));

        var details = await DetailsAsync(world, ids[1]);
        Assert.Equal((2, r.AddDays(-14)), (details.AttemptNumber, details.AttemptStartDate));
        Assert.False(details.IsEnded);

        var attempts = await AttemptsAsync(world, ids[1]);
        Assert.Equal([2, 1], attempts.Select(a => a.Number));
        var latest = attempts[0];
        Assert.True(latest.IsLatest);
        Assert.Equal((MeditationName, r.AddDays(-14)), (latest.Name, latest.StartDate));
        Assert.Equal("100% (12 of 12)", latest.Stats.ConsistencyText);
        Assert.Equal(("12 commitments", "12 commitments"), (latest.Stats.CurrentStreakText, latest.Stats.LongestStreakText));
        var earlier = attempts[1];
        Assert.False(earlier.IsLatest);
        Assert.Equal("Meditation, 10 min daily", earlier.Name);
        Assert.Equal((r.AddDays(-42), r.AddDays(-21), r.AddDays(-22)), (earlier.StartDate, earlier.EndingDate!.Value, earlier.LastDate!.Value));
        Assert.Equal((Outcome.Abandoned, "Too busy."), (earlier.Outcome!.Value, earlier.EndingNote));
        Assert.False(earlier.HasNoDate);
        Assert.Equal("52% (11 of 21)", earlier.Stats.ConsistencyText);
        Assert.Equal("11 commitments", earlier.Stats.LongestStreakText);
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public async Task FR_031_Sugar_ended_Achieved_with_100_percent_and_23_days(DateOnly r, DayOfWeek day)
    {
        Assert.Equal(day, r.DayOfWeek);
        var (world, ids) = Arrange(r);

        var details = await DetailsAsync(world, ids[2]);
        Assert.Equal(SugarName, details.Name);
        Assert.True(details.IsEnded);
        Assert.Equal((r.AddDays(-7), r.AddDays(-8), Outcome.Achieved), (details.EndingDate!.Value, details.LastDate!.Value, details.Outcome!.Value));
        Assert.Equal("Three weeks without extra sweets.", details.EndingNote);

        var attempt = Assert.Single(await AttemptsAsync(world, ids[2]));
        Assert.Equal("100% (23 of 23)", attempt.Stats.ConsistencyText);
        Assert.Equal(("23 days", "23 days"), (attempt.Stats.CurrentStreakText, attempt.Stats.LongestStreakText));

        var report = await ReportAsync(world, ids[2]);
        Assert.Equal("100% (23 of 23)", report.Stats.ConsistencyText);
        Assert.Equal(("23 days", "23 days"), (report.Stats.CurrentStreakText, report.Stats.LongestStreakText));
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public async Task FR_031_the_dashboard_has_2_Due_4_Pending_0_Missed_and_2_goals_and_the_goal_history_Sugar_then_Meditation(DateOnly r, DayOfWeek day)
    {
        Assert.Equal(day, r.DayOfWeek);
        var (world, ids) = Arrange(r);

        var dashboard = await world.DashboardAsync();
        Assert.Equal(new DashboardCounts { Due = 2, Pending = 4, MissedThisWeek = 0, Goals = 2 }, dashboard.Counts);

        var list = await world.ListAsync();
        Assert.Equal(
            [(ReadingName, false), (MeditationName, false), (SugarName, true)],
            ids.Select(id => list.Single(g => g.Id == id)).Select(g => (g.Name, g.IsEnded)));

        var result = await world.Attempts.GetGoalHistoryAsync(world.User, None);
        Assert.Equal(GoalHistoryListOutcome.Found, result.Outcome);
        Assert.Equal(
            [
                (ids[2], SugarName, 1, r.AddDays(-7), Outcome.Achieved, "Three weeks without extra sweets.", true),
                (ids[1], "Meditation, 10 min daily", 1, r.AddDays(-21), Outcome.Abandoned, "Too busy.", false),
            ],
            result.Entries.Select(e => (e.GoalId, e.Name, e.AttemptNumber, e.EndingDate, e.Outcome, e.EndingNote, e.IsLatestAttempt)));
    }

    /// <summary>
    /// A world at 10:00 UTC on <paramref name="r"/> with the three goals of <see cref="DemoGoalChanges.For"/> stored for
    /// its user, in UTC, on views of their catalog habits, and their records and notes; the goal ids in creation order.
    /// </summary>
    private static (AttemptWorld World, GoalId[] Ids) Arrange(DateOnly r)
    {
        var world = new AttemptWorld(new DateTimeOffset(r, new TimeOnly(10, 0), TimeSpan.Zero));
        var owner = world.User.AccountId!.Value;
        var ids = new List<GoalId>();
        foreach (var demo in DemoGoalChanges.For(r))
        {
            var view = new GoalHabitView(new HabitId(Guid.NewGuid()), HabitKind.Catalog, demo.CatalogHabitName, Category.Mind, true);
            var goal = new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(view.Id, view.Kind), null, demo.AttemptsIn(NamedTimeZone.Utc));
            world.Goals.Add(owner, goal, view);
            foreach (var record in demo.Records)
            {
                world.Records.Seed(goal.Id, record.Date, record.Value);
            }

            foreach (var note in demo.Notes)
            {
                world.Records.SeedNote(goal.Id, note.EntryDate, note.Text);
            }

            ids.Add(goal.Id);
        }

        return (world, [.. ids]);
    }

    private static async Task<GoalReport> ReportAsync(AttemptWorld world, GoalId goal)
    {
        var result = await world.Reports.GetGoalReportAsync(world.User, goal, None);
        Assert.Equal(GoalReportOutcome.Found, result.Outcome);
        return result.Report!;
    }

    private static async Task<IReadOnlyList<HistoryEntry>> HistoryAsync(AttemptWorld world, GoalId goal, DateOnly first, DateOnly last) =>
        (await world.HistoryAsync(goal, first, last)).Entries;

    private static async Task<GoalDetails> DetailsAsync(AttemptWorld world, GoalId goal) => await world.DetailsAsync(goal);

    private static async Task<IReadOnlyList<AttemptView>> AttemptsAsync(AttemptWorld world, GoalId goal)
    {
        var result = await world.Attempts.GetAttemptsAsync(world.User, goal, None);
        Assert.Equal(AttemptListOutcome.Found, result.Outcome);
        return result.Attempts;
    }

    private static GoalTarget Minutes(int minutes) => Target(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = EveryDay,
    });

    private static GoalTarget SugarTarget() => Target(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 1, CountUnit = "sweet",
        ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
    });

    private static GoalTarget Target(GoalTargetFields fields)
    {
        Assert.True(GoalTarget.TryCreate(fields, out var target, out _));
        return target;
    }
}
