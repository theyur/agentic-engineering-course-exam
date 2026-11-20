using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// The shared recording formatter (research R-12, HP-002, HP-004, HP-011): every text as the design's slice-5 state
/// words it, units as stored, durations in minutes, and the same results under a non-English culture.
/// </summary>
public sealed class RecordingTextTests
{
    private static readonly DateOnly Wednesday = new(2026, 10, 28);
    private static readonly DateOnly Friday = new(2026, 10, 30);
    private static readonly DateOnly WeekMonday = new(2026, 10, 26);
    private static readonly DateOnly WeekSunday = new(2026, 11, 1);
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 8, 0, 0, TimeSpan.Zero);

    private static GoalHistory History(GoalTarget target) => new()
    {
        GoalId = new GoalId(Guid.Parse("0b7e5f2a-0000-4000-8000-000000000002")),
        GoalScheduleKind = target.ScheduleKindOf,
        Period = target.PeriodLength,
        BuildOrReduce = target.BuildOrReduce,
        Measurement = target.Measurement,
        Amount = target.Amount,
        CountUnit = target.CountUnit,
        DurationUnit = target.DurationUnit,
        Times = target.Schedule.Times,
        CurrentDate = Friday,
        Target = target,
        ReadAt = Now,
        Entries = [],
    };

    private static HistoryEntry Entry(DateOnly first, DateOnly last, EvaluationState state, DateTimeOffset? graceEnds = null) => new()
    {
        FirstDate = first,
        LastDate = last,
        Starts = new DateTimeOffset(first.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
        Ends = new DateTimeOffset(last.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
        State = state,
        GraceEnds = graceEnds,
        Records = [],
        Progress = new Progress(ProgressKind.RecordedValue, null, 1),
        IsCorrection = false,
    };

    private static GoalTarget Reading20MinDaily() => Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));

    private static GoalTarget CoffeeAtMost2Cups() => Target(CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));

    private static GoalTarget SugarAtMost1Sweet() => Target(CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));

    private static GoalTarget Pages20ThreeTimesAWeek() => Target(CountTimes(20, "pages", 3, SchedulePeriod.Week));

    // State (HP-004)

    [Theory]
    [InlineData(EvaluationState.Met, "Met")]
    [InlineData(EvaluationState.Failed, "Failed")]
    [InlineData(EvaluationState.Missed, "Missed")]
    [InlineData(EvaluationState.Pending, "Pending")]
    [InlineData(EvaluationState.Due, "Due today")]
    public void State_of_a_commitment(EvaluationState state, string expected) =>
        Assert.Equal(expected, RecordingText.State(state, GoalScheduleKind.CommitmentBased, null));

    [Fact]
    public void Due_is_Due_today_for_a_per_day_period_and_Due_this_week_for_a_per_week_period()
    {
        Assert.Equal("Due today", RecordingText.State(EvaluationState.Due, GoalScheduleKind.PeriodBased, SchedulePeriod.Day));
        Assert.Equal("Due this week", RecordingText.State(EvaluationState.Due, GoalScheduleKind.PeriodBased, SchedulePeriod.Week));
        Assert.Equal("Pending", RecordingText.State(EvaluationState.Pending, GoalScheduleKind.PeriodBased, SchedulePeriod.Week));
    }

    [Fact]
    public void A_Reduce_goal_that_is_met_reads_Met()
    {
        var history = History(CoffeeAtMost2Cups());

        Assert.Equal("Met", RecordingText.State(EvaluationState.Met, history.GoalScheduleKind, history.Period));
    }

    [Fact]
    public void Upcoming_has_no_text_in_this_slice() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => RecordingText.State(EvaluationState.Upcoming, GoalScheduleKind.CommitmentBased, null));

    // Progress (FR-014)

    [Fact]
    public void Progress_of_a_Build_duration_commitment()
    {
        var history = History(Reading20MinDaily());

        Assert.Equal("12 of 20 min", RecordingText.Progress(history, new Progress(ProgressKind.RecordedValue, 12, 20)));
        Assert.Equal("25 of 20 min", RecordingText.Progress(history, new Progress(ProgressKind.RecordedValue, 25, 20)));
    }

    [Fact]
    public void Progress_of_a_goal_in_hours_is_in_minutes()
    {
        var history = History(Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, AllDays)));

        Assert.Equal("45 of 60 min", RecordingText.Progress(history, new Progress(ProgressKind.RecordedValue, 45, 60)));
    }

    [Fact]
    public void Progress_of_a_Build_count_commitment_keeps_the_unit_as_stored()
    {
        var history = History(Target(CountDays(20, "pages", BuildOrReduce.Build, Weekdays)));

        Assert.Equal("15 of 20 pages", RecordingText.Progress(history, new Progress(ProgressKind.RecordedValue, 15, 20)));
    }

    [Fact]
    public void Progress_of_a_Reduce_count_or_duration_commitment()
    {
        var cups = History(Target(CountDays(2, "cups", BuildOrReduce.Reduce, AllDays)));
        var minutes = History(Target(DurationDays(60, DurationUnit.Minutes, BuildOrReduce.Reduce, AllDays)));

        Assert.Equal("1 of at most 2 cups", RecordingText.Progress(cups, new Progress(ProgressKind.RecordedValue, 1, 2)));
        Assert.Equal("45 of at most 60 min", RecordingText.Progress(minutes, new Progress(ProgressKind.RecordedValue, 45, 60)));
    }

    [Fact]
    public void A_yes_no_commitment_has_no_progress_line()
    {
        Assert.Null(RecordingText.Progress(History(OnDays(MonWedFri)), new Progress(ProgressKind.RecordedValue, 1, 1)));
        Assert.Null(RecordingText.Progress(History(Target(YesNoDays(BuildOrReduce.Reduce, AllDays))), new Progress(ProgressKind.RecordedValue, 0, 1)));
    }

    [Fact]
    public void A_count_or_duration_commitment_with_no_value_has_no_progress_line()
    {
        Assert.Null(RecordingText.Progress(History(Target(CountDays(20, "pages", BuildOrReduce.Build, Weekdays))), new Progress(ProgressKind.RecordedValue, null, 20)));
        Assert.Null(RecordingText.Progress(History(Reading20MinDaily()), new Progress(ProgressKind.RecordedValue, null, 20)));
    }

    [Fact]
    public void Progress_of_a_Build_period_counts_times()
    {
        var history = History(Pages20ThreeTimesAWeek());

        Assert.Equal("2 of 3 times", RecordingText.Progress(history, new Progress(ProgressKind.RecordsReachingAmount, 2, 3)));
        Assert.Equal("0 of 1 time", RecordingText.Progress(History(PerDay()), new Progress(ProgressKind.RecordsReachingAmount, 0, 1)));
    }

    [Fact]
    public void Progress_of_a_Reduce_yes_no_period()
    {
        var history = History(Target(YesNoTimes(2, SchedulePeriod.Day, BuildOrReduce.Reduce)));

        Assert.Equal("1 of at most 2 times", RecordingText.Progress(history, new Progress(ProgressKind.YesRecords, 1, 2)));
    }

    [Fact]
    public void Progress_of_a_Reduce_count_period_keeps_singular_and_plural_units_as_stored()
    {
        Assert.Equal("1 of at most 2 cups", RecordingText.Progress(History(CoffeeAtMost2Cups()), new Progress(ProgressKind.Total, 1, 2)));
        Assert.Equal("0 of at most 1 sweet", RecordingText.Progress(History(SugarAtMost1Sweet()), new Progress(ProgressKind.Total, 0, 1)));
        Assert.Equal("3 of at most 1 sweet", RecordingText.Progress(History(SugarAtMost1Sweet()), new Progress(ProgressKind.Total, 3, 1)));
    }

    [Fact]
    public void Progress_of_a_Reduce_duration_period_in_hours_is_in_minutes()
    {
        var history = History(Target(DurationTimes(1, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce)));

        Assert.Equal("45 of at most 60 min", RecordingText.Progress(history, new Progress(ProgressKind.Total, 45, 60)));
    }

    [Theory]
    [InlineData(null, 20, 0)]
    [InlineData(0, 20, 0)]
    [InlineData(12, 20, 60)]
    [InlineData(20, 20, 100)]
    [InlineData(25, 20, 100)]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 100)]
    [InlineData(1, 3, 33)]
    public void ProgressFill_is_the_value_against_the_target_capped_at_full(int? value, int of, int expected) =>
        Assert.Equal(expected, RecordingText.ProgressFill(new Progress(ProgressKind.RecordedValue, value, of)));

    // Amount and Recorded

    [Theory]
    [InlineData(Measurement.Duration, null, 12, "12 min")]
    [InlineData(Measurement.Count, "sweet", 0, "0 sweet")]
    [InlineData(Measurement.Count, "sweet", 3, "3 sweet")]
    [InlineData(Measurement.Count, "cups", 1, "1 cups")]
    [InlineData(Measurement.Count, "cups", 2, "2 cups")]
    public void Amount_keeps_the_unit_as_stored(Measurement measurement, string? unit, int amount, string expected) =>
        Assert.Equal(expected, RecordingText.Amount(measurement, unit, amount));

    [Fact]
    public void A_yes_no_measurement_has_no_amount() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => RecordingText.Amount(Measurement.YesNo, null, 1));

    [Fact]
    public void Recorded_gives_the_answer_or_the_amount()
    {
        var id = new RecordId(Guid.NewGuid());

        Assert.Equal("Yes", RecordingText.Recorded(History(OnDays(MonWedFri)), new RecordView(id, Wednesday, YesNoAnswer.Yes, null)));
        Assert.Equal("No", RecordingText.Recorded(History(OnDays(MonWedFri)), new RecordView(id, Wednesday, YesNoAnswer.No, null)));
        Assert.Equal("12 min", RecordingText.Recorded(History(Reading20MinDaily()), new RecordView(id, Wednesday, null, 12)));
        Assert.Equal("3 sweet", RecordingText.Recorded(History(SugarAtMost1Sweet()), new RecordView(id, Wednesday, null, 3)));
        Assert.Equal("90 min", RecordingText.Recorded(History(Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, AllDays))), new RecordView(id, Wednesday, null, 90)));
    }

    // Dates

    [Fact]
    public void EntryDate_is_the_short_date_or_the_week_range()
    {
        Assert.Equal("Wed 28 Oct", RecordingText.EntryDate(Entry(Wednesday, Wednesday, EvaluationState.Met)));
        Assert.Equal("26 Oct – 1 Nov", RecordingText.EntryDate(Entry(WeekMonday, WeekSunday, EvaluationState.Met)));
        Assert.Equal("10 – 16 Aug", RecordingText.EntryDate(Entry(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 16), EvaluationState.Met)));
    }

    // Dialog texts

    [Fact]
    public void Title_names_the_goal() =>
        Assert.Equal("Record · Reading, 20 min daily", RecordingText.Title("Reading, 20 min daily"));

    [Fact]
    public void FieldLabel_by_measurement()
    {
        Assert.Equal("Did you do it?", RecordingText.FieldLabel(History(OnDays(MonWedFri))));
        Assert.Equal("Amount", RecordingText.FieldLabel(History(CoffeeAtMost2Cups())));
        Assert.Equal("Time spent", RecordingText.FieldLabel(History(Reading20MinDaily())));
    }

    [Fact]
    public void ReduceHelp_for_count_duration_and_yes_no()
    {
        Assert.Equal(
            "At most 2 cups. Enter 0 if you had none: an empty day cannot prove you stayed under the limit.",
            RecordingText.ReduceHelp(History(CoffeeAtMost2Cups())));
        Assert.Equal(
            "At most 1 h. Enter 0 if you had none: an empty day cannot prove you stayed under the limit.",
            RecordingText.ReduceHelp(History(Target(DurationTimes(1, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce)))));
        Assert.Equal(
            "Choose No if you avoided it: an empty day cannot prove you stayed under the limit.",
            RecordingText.ReduceHelp(History(Target(YesNoDays(BuildOrReduce.Reduce, AllDays)))));
    }

    [Fact]
    public void A_Build_goal_has_no_Reduce_help()
    {
        Assert.Null(RecordingText.ReduceHelp(History(Reading20MinDaily())));
        Assert.Null(RecordingText.ReduceHelp(History(OnDays(MonWedFri))));
    }

    [Fact]
    public void HP_003_NoteHelp_of_a_commitment_based_goal_says_that_clearing_the_field_removes_the_note()
    {
        Assert.Equal("Clear the field to remove the note.", RecordingText.NoteHelp(History(Reading20MinDaily())));
        Assert.Equal("Clear the field to remove the note.", RecordingText.NoteHelp(History(OnDays(MonWedFri))));
        Assert.Equal(
            "Clear the field to remove the note.",
            RecordingText.NoteHelp(History(Target(YesNoDays(BuildOrReduce.Reduce, AllDays)))));
    }

    [Fact]
    public void HP_003_NoteHelp_of_a_per_day_period_says_that_the_note_is_for_the_whole_day()
    {
        Assert.Equal(
            "This note is for the whole day. Clear the field to remove it.",
            RecordingText.NoteHelp(History(CoffeeAtMost2Cups())));
        Assert.Equal(
            "This note is for the whole day. Clear the field to remove it.",
            RecordingText.NoteHelp(History(Target(DurationTimes(20, DurationUnit.Minutes, 2, SchedulePeriod.Day)))));
    }

    [Fact]
    public void HP_003_NoteHelp_of_a_per_week_period_says_that_the_note_is_for_the_whole_week()
    {
        Assert.Equal(
            "This note is for the whole week. Clear the field to remove it.",
            RecordingText.NoteHelp(History(Pages20ThreeTimesAWeek())));
    }

    public static TheoryData<GoalTarget, string> TargetLines() => new()
    {
        { Reading20MinDaily(), "Build · at least 20 min · every day" },
        { OnDays(MonWedFri), "Build · Mon · Wed · Fri" },
        { PerWeek(), "Build · 3 times a week" },
        { OnDays(AllDays), "Build · every day" },
        { PerDay(), "Build · every day" },
        { Target(YesNoTimes(1, SchedulePeriod.Week)), "Build · once a week" },
        { Target(YesNoTimes(2, SchedulePeriod.Day)), "Build · 2 times a day" },
        { Target(CountDays(20, "pages", BuildOrReduce.Build, Weekdays)), "Build · at least 20 pages · Mon–Fri" },
        { Pages20ThreeTimesAWeek(), "Build · at least 20 pages · 3 times a week" },
        { Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Saturday)), "Build · at least 1 h · Mon–Wed · Sat" },
        { Target(DurationTimes(10, DurationUnit.Minutes, 2, SchedulePeriod.Day)), "Build · at least 10 min · 2 times a day" },
        { CoffeeAtMost2Cups(), "Reduce · at most 2 cups a day" },
        { SugarAtMost1Sweet(), "Reduce · at most 1 sweet a day" },
        { Target(YesNoTimes(2, SchedulePeriod.Day, BuildOrReduce.Reduce)), "Reduce · at most 2 times a day" },
        { Target(YesNoDays(BuildOrReduce.Reduce, AllDays)), "Reduce · avoided every day" },
    };

    [Theory]
    [MemberData(nameof(TargetLines))]
    public void TargetLine_is_the_sub_line(GoalTarget target, string expected) =>
        Assert.Equal(expected, RecordingText.TargetLine(History(target)));

    public static TheoryData<GoalTarget, string> PartialNotes() => new()
    {
        { Reading20MinDaily(), "Partial time is kept. Below 20 min the day counts as Failed when it is finalized." },
        {
            Target(CountTimes(20, "pages", 1, SchedulePeriod.Day)),
            "Partial amounts are kept. Without a record of at least 20 pages the day counts as Failed when it is finalized."
        },
        {
            Target(DurationTimes(10, DurationUnit.Minutes, 2, SchedulePeriod.Day)),
            "Partial time is kept. With fewer than 2 records of at least 10 min the day counts as Failed when it is finalized."
        },
        {
            Target(DurationTimes(1, DurationUnit.Hours, 1, SchedulePeriod.Week)),
            "Partial time is kept. Without a record of at least 1 h the week counts as Failed when it is finalized."
        },
        {
            Pages20ThreeTimesAWeek(),
            "Partial amounts are kept. With fewer than 3 records of at least 20 pages the week counts as Failed when it is finalized."
        },
    };

    [Theory]
    [MemberData(nameof(PartialNotes))]
    public void PartialNote_for_each_Build_count_and_duration_schedule_kind(GoalTarget target, string expected) =>
        Assert.Equal(expected, RecordingText.PartialNote(History(target)));

    [Fact]
    public void Yes_no_and_Reduce_goals_have_no_partial_note()
    {
        Assert.Null(RecordingText.PartialNote(History(OnDays(MonWedFri))));
        Assert.Null(RecordingText.PartialNote(History(PerWeek())));
        Assert.Null(RecordingText.PartialNote(History(CoffeeAtMost2Cups())));
        Assert.Null(RecordingText.PartialNote(History(Target(YesNoDays(BuildOrReduce.Reduce, AllDays)))));
    }

    // "For" labels (FR-011)

    [Fact]
    public void ForLabel_of_the_Due_entry_on_the_current_date_starts_with_Today()
    {
        var history = History(Reading20MinDaily());

        Assert.Equal("Today, Fri 30 Oct", RecordingText.ForLabel(Entry(Friday, Friday, EvaluationState.Due), history.CurrentDate!.Value, history.ReadAt));
    }

    [Fact]
    public void ForLabel_of_another_Due_single_date_entry_is_its_short_date() =>
        Assert.Equal("Thu 29 Oct", RecordingText.ForLabel(Entry(Friday.AddDays(-1), Friday.AddDays(-1), EvaluationState.Due), Friday, Now));

    [Fact]
    public void ForLabel_of_a_Due_week_is_its_range_never_Today() =>
        Assert.Equal("26 Oct – 1 Nov", RecordingText.ForLabel(Entry(WeekMonday, WeekSunday, EvaluationState.Due), Friday, Now));

    [Fact]
    public void ForLabel_of_a_Pending_week_adds_the_hours_left()
    {
        var entry = Entry(new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 25), EvaluationState.Pending, Now.AddHours(22));

        Assert.Equal("19 – 25 Oct · Pending, 22 h left", RecordingText.ForLabel(entry, Friday, Now));
    }

    [Theory]
    [InlineData(22 * 60, "Wed 28 Oct · Pending, 22 h left")]
    [InlineData((21 * 60) + 59, "Wed 28 Oct · Pending, 22 h left")]
    [InlineData(30, "Wed 28 Oct · Pending, 1 h left")]
    [InlineData(1, "Wed 28 Oct · Pending, 1 h left")]
    public void ForLabel_of_a_Pending_entry_rounds_the_hours_left_up(int minutesLeft, string expected)
    {
        var entry = Entry(Wednesday, Wednesday, EvaluationState.Pending, Now.AddMinutes(minutesLeft));

        Assert.Equal(expected, RecordingText.ForLabel(entry, Friday, Now));
    }

    // Hours left (slice 6 research R-13)

    [Theory]
    [InlineData(5 * 3600, 5)]
    [InlineData((4 * 3600) + 1, 5)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(-3600, 0)]
    public void HoursLeft_is_the_whole_hours_to_the_grace_end_rounded_up_and_0_from_the_end_on(int secondsLeft, long expected) =>
        Assert.Equal(expected, RecordingText.HoursLeft(Now.AddSeconds(secondsLeft), Now));

    // Culture (constitution II)

    [Fact]
    public void Every_text_is_the_same_under_a_non_English_culture()
    {
        var hours = History(Target(DurationTimes(1, DurationUnit.Hours, 1, SchedulePeriod.Week)));
        var pages = History(Pages20ThreeTimesAWeek());
        var cups = History(CoffeeAtMost2Cups());
        var pending = Entry(Wednesday, Wednesday, EvaluationState.Pending, Now.AddMinutes((21 * 60) + 59));
        var week = Entry(WeekMonday, WeekSunday, EvaluationState.Due);

        string[] Texts() =>
        [
            RecordingText.State(EvaluationState.Due, pages.GoalScheduleKind, pages.Period),
            RecordingText.Progress(hours, new Progress(ProgressKind.RecordsReachingAmount, 1234, 5000))!,
            RecordingText.Progress(cups, new Progress(ProgressKind.Total, 1500, 2))!,
            RecordingText.Amount(Measurement.Duration, null, 1440),
            RecordingText.Amount(Measurement.Count, "pages", 9999),
            RecordingText.EntryDate(week),
            RecordingText.ForLabel(pending, Friday, Now),
            RecordingText.ForLabel(Entry(Friday, Friday, EvaluationState.Due), Friday, Now),
            RecordingText.TargetLine(pages),
            RecordingText.PartialNote(hours)!,
            RecordingText.ReduceHelp(cups)!,
            RecordingText.FieldLabel(hours),
            RecordingText.ProgressFill(new Progress(ProgressKind.Total, 1, 3)).ToString(System.Globalization.CultureInfo.InvariantCulture),
        ];

        string[] expected =
        [
            "Due this week",
            "1234 of 5000 times",
            "1500 of at most 2 cups",
            "1440 min",
            "9999 pages",
            "26 Oct – 1 Nov",
            "Wed 28 Oct · Pending, 22 h left",
            "Today, Fri 30 Oct",
            "Build · at least 20 pages · 3 times a week",
            "Partial time is kept. Without a record of at least 1 h the week counts as Failed when it is finalized.",
            "At most 2 cups. Enter 0 if you had none: an empty day cannot prove you stayed under the limit.",
            "Time spent",
            "33",
        ];

        Assert.Equal(expected, Texts());
        using (new CultureScope("ar-SA"))
        {
            Assert.Equal(expected, Texts());
        }

        using (new CultureScope("uk-UA"))
        {
            Assert.Equal(expected, Texts());
        }
    }
}
