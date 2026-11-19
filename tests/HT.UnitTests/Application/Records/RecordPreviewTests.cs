using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// The RecordDialog's live progress (HP-002, FR-003, FR-004, FR-014): the entry's other records and the candidate,
/// passed to <see cref="Evaluator.ProgressOf"/> with the goal's target. A commitment never gets more than one value.
/// </summary>
public sealed class RecordPreviewTests
{
    private static readonly DateOnly Monday = new(2026, 11, 16);
    private static readonly DateOnly Tuesday = new(2026, 11, 17);
    private static readonly DateOnly Wednesday = new(2026, 11, 18);

    private static GoalHistory History(GoalTarget target) => new()
    {
        GoalId = new GoalId(Guid.Parse("0b7e5f2a-0000-4000-8000-000000000001")),
        GoalScheduleKind = target.ScheduleKindOf,
        Period = target.PeriodLength,
        BuildOrReduce = target.BuildOrReduce,
        Measurement = target.Measurement,
        Amount = target.Amount,
        CountUnit = target.CountUnit,
        DurationUnit = target.DurationUnit,
        Times = target.Schedule.Times,
        CurrentDate = Wednesday,
        Target = target,
        ReadAt = new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero),
        Entries = [],
    };

    private static HistoryEntry Entry(DateOnly first, DateOnly last, params RecordView[] records) => new()
    {
        FirstDate = first,
        LastDate = last,
        Starts = new DateTimeOffset(first.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
        Ends = new DateTimeOffset(last.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
        State = EvaluationState.Due,
        Records = records,
        Progress = new Progress(ProgressKind.RecordedValue, null, 0),
        IsCorrection = false,
    };

    private static RecordView Amount(DateOnly date, int amount) => new(new RecordId(Guid.NewGuid()), date, null, amount);

    private static RecordView Answer(DateOnly date, YesNoAnswer answer) => new(new RecordId(Guid.NewGuid()), date, answer, null);

    private static GoalTarget DailyReading20Min() => Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));

    private static GoalTarget Pages20ThreeTimesAWeek() => Target(CountTimes(20, "pages", 3, SchedulePeriod.Week));

    [Fact]
    public void A_commitment_without_a_record_has_no_value_until_one_is_entered()
    {
        var history = History(DailyReading20Min());
        var entry = Entry(Tuesday, Tuesday);

        Assert.Equal(new Progress(ProgressKind.RecordedValue, null, 20), RecordPreview.Progress(history, entry, null, null, null));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), RecordPreview.Progress(history, entry, null, null, 12));
    }

    [Fact]
    public void US1_AS2_a_commitment_with_12_minutes_and_a_candidate_of_25_gives_25_of_20_without_replacing()
    {
        var history = History(DailyReading20Min());
        var entry = Entry(Tuesday, Tuesday, Amount(Tuesday, 12));

        var progress = RecordPreview.Progress(history, entry, null, null, 25);

        Assert.Equal(new Progress(ProgressKind.RecordedValue, 25, 20), progress);
    }

    [Fact]
    public void A_commitment_record_being_edited_is_replaced_by_the_candidate()
    {
        var history = History(DailyReading20Min());
        var existing = Amount(Tuesday, 12);
        var entry = Entry(Tuesday, Tuesday, existing);

        Assert.Equal(new Progress(ProgressKind.RecordedValue, 25, 20), RecordPreview.Progress(history, entry, existing.Id, null, 25));
    }

    [Fact]
    public void A_commitment_leaves_its_existing_record_out_even_without_a_valid_candidate()
    {
        // Saving replaces the commitment's record (FR-003), so the preview shows only what would be saved.
        var history = History(DailyReading20Min());
        var entry = Entry(Tuesday, Tuesday, Amount(Tuesday, 12));

        Assert.Equal(new Progress(ProgressKind.RecordedValue, null, 20), RecordPreview.Progress(history, entry, null, null, 1441));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, null, 20), RecordPreview.Progress(history, entry, null, null, null));
    }

    [Fact]
    public void A_yes_no_commitment_counts_Yes_1_and_No_0_against_1()
    {
        var history = History(OnDays(MonWedFri));
        var entry = Entry(Wednesday, Wednesday, Answer(Wednesday, YesNoAnswer.No));

        Assert.Equal(new Progress(ProgressKind.RecordedValue, 1, 1), RecordPreview.Progress(history, entry, null, YesNoAnswer.Yes, null));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 0, 1), RecordPreview.Progress(history, entry, null, YesNoAnswer.No, null));
    }

    [Fact]
    public void A_duration_in_hours_is_compared_in_minutes()
    {
        var history = History(Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, AllDays)));

        Assert.Equal(new Progress(ProgressKind.RecordedValue, 45, 60), RecordPreview.Progress(history, Entry(Tuesday, Tuesday), null, null, 45));
    }

    [Fact]
    public void A_Build_period_keeps_its_other_records_and_adds_the_candidate()
    {
        var history = History(Pages20ThreeTimesAWeek());
        var entry = Entry(Monday, Monday.AddDays(6), Amount(Monday, 20), Amount(Tuesday, 25), Amount(Tuesday, 12));

        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), RecordPreview.Progress(history, entry, null, null, null));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), RecordPreview.Progress(history, entry, null, null, 12));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 3, 3), RecordPreview.Progress(history, entry, null, null, 30));
    }

    [Fact]
    public void A_Build_period_leaves_out_the_record_being_edited()
    {
        var history = History(Pages20ThreeTimesAWeek());
        var edited = Amount(Tuesday, 25);
        var entry = Entry(Monday, Monday.AddDays(6), Amount(Monday, 20), edited, Amount(Tuesday, 12));

        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 1, 3), RecordPreview.Progress(history, entry, edited.Id, null, 12));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), RecordPreview.Progress(history, entry, edited.Id, null, 20));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 1, 3), RecordPreview.Progress(history, entry, edited.Id, null, null));
    }

    [Fact]
    public void A_Build_yes_no_period_counts_its_Yes_records_and_drops_an_amount_given_with_the_answer()
    {
        var history = History(PerWeek());
        var entry = Entry(Monday, Monday.AddDays(6), Answer(Monday, YesNoAnswer.Yes), Answer(Tuesday, YesNoAnswer.No), Answer(Tuesday, YesNoAnswer.Yes));

        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 3, 3), RecordPreview.Progress(history, entry, null, YesNoAnswer.Yes, 5));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), RecordPreview.Progress(history, entry, null, null, 5));
    }

    [Fact]
    public void A_Reduce_count_period_gives_its_total_with_the_candidate()
    {
        var history = History(Target(CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce)));
        var entry = Entry(Tuesday, Tuesday, Amount(Tuesday, 1));

        Assert.Equal(new Progress(ProgressKind.Total, 1, 2), RecordPreview.Progress(history, entry, null, null, null));
        Assert.Equal(new Progress(ProgressKind.Total, 3, 2), RecordPreview.Progress(history, entry, null, null, 2));
        Assert.Equal(new Progress(ProgressKind.Total, 1, 2), RecordPreview.Progress(history, entry, null, null, 0));
    }

    [Fact]
    public void A_Reduce_yes_no_period_counts_its_Yes_records_against_at_most_N()
    {
        var history = History(Target(YesNoTimes(2, SchedulePeriod.Day, BuildOrReduce.Reduce)));
        var entry = Entry(Tuesday, Tuesday, Answer(Tuesday, YesNoAnswer.Yes));

        Assert.Equal(new Progress(ProgressKind.YesRecords, 1, 2), RecordPreview.Progress(history, entry, null, YesNoAnswer.No, null));
        Assert.Equal(new Progress(ProgressKind.YesRecords, 2, 2), RecordPreview.Progress(history, entry, null, YesNoAnswer.Yes, null));
    }

    [Theory]
    [InlineData(10000)]
    [InlineData(-1)]
    public void An_out_of_range_count_candidate_gives_the_progress_without_it(int candidate)
    {
        var history = History(Pages20ThreeTimesAWeek());
        var entry = Entry(Monday, Monday.AddDays(6), Amount(Monday, 20), Amount(Tuesday, 25));

        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), RecordPreview.Progress(history, entry, null, null, candidate));
    }

    [Fact]
    public void A_candidate_out_of_the_duration_range_or_of_the_wrong_kind_gives_the_progress_without_it()
    {
        var history = History(Target(DurationTimes(20, DurationUnit.Minutes, 2, SchedulePeriod.Day)));
        var entry = Entry(Tuesday, Tuesday, Amount(Tuesday, 30));

        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 1, 2), RecordPreview.Progress(history, entry, null, null, 1441));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 1, 2), RecordPreview.Progress(history, entry, null, YesNoAnswer.Yes, null));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 2), RecordPreview.Progress(history, entry, null, null, 1440));
    }
}
