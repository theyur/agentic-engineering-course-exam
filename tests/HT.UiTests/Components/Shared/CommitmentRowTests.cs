using Bunit;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Ui.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace HT.UiTests.Components.Shared;

/// <summary>
/// CommitmentRow (HP-003, D-2, design/components/CommitmentRow "Slice 6 state"): each state's meta line, chip, bar and
/// action, from a TodayEntry built by hand. The dashboard was read at 19:00Z on Wednesday 18 November 2026, in UTC.
/// </summary>
public sealed class CommitmentRowTests : BunitContext
{
    private static readonly DateTimeOffset ReadAt = new(2026, 11, 18, 19, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Wed18 = new(2026, 11, 18);

    private static GoalTarget TargetOf(GoalTargetFields fields)
    {
        Assert.True(GoalTarget.TryCreate(fields, out var target, out _));
        return target;
    }

    private static GoalTarget Reading20MinOnMonWedFri => TargetOf(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
    });

    private static GoalTarget CoffeeAtMost2Cups => TargetOf(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 2, CountUnit = "cups",
        ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
    });

    private static GoalTarget RunningTwiceAWeek => TargetOf(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Week,
    });

    /// <summary>A row of the goal with <paramref name="target"/> for <paramref name="first"/> to <paramref name="last"/>.</summary>
    private static TodayEntry EntryOf(
        string name, GoalTarget target, string schedule, DateOnly first, DateOnly last, EvaluationState state, Progress progress,
        DateTimeOffset? graceEnds = null, int records = 0, string? note = null)
    {
        var starts = new DateTimeOffset(first.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var entry = new HistoryEntry
        {
            FirstDate = first, LastDate = last, Starts = starts, Ends = starts.AddDays(last.DayNumber - first.DayNumber + 1),
            State = state, GraceEnds = graceEnds,
            Records = Enumerable.Range(0, records).Select(_ => new RecordView(new RecordId(Guid.NewGuid()), first, null, progress.Value)).ToList(),
            Progress = progress, Note = note, IsCorrection = false,
        };
        var goal = new GoalHistory
        {
            GoalId = new GoalId(Guid.Parse("7c1d0c9e-0000-4000-8000-000000000001")), GoalScheduleKind = target.ScheduleKindOf,
            Period = target.PeriodLength, BuildOrReduce = target.BuildOrReduce, Measurement = target.Measurement, Amount = target.Amount,
            CountUnit = target.CountUnit, DurationUnit = target.DurationUnit, CurrentDate = Wed18, Target = target, ReadAt = ReadAt,
            Entries = state is EvaluationState.Due or EvaluationState.Pending ? [entry] : [],
        };
        return new TodayEntry
        {
            GoalId = goal.GoalId, GoalName = name, BuildOrReduce = target.BuildOrReduce, ScheduleText = schedule, Goal = goal, Entry = entry,
        };
    }

    private static TodayEntry Reading(DateOnly date, EvaluationState state, int? minutes = null, DateTimeOffset? graceEnds = null, string? note = null) =>
        EntryOf("Reading, 20 min, 3 days a week", Reading20MinOnMonWedFri, "Mon · Wed · Fri", date, date, state,
            new Progress(ProgressKind.RecordedValue, minutes, 20), graceEnds, minutes is null ? 0 : 1, note);

    private IRenderedComponent<CommitmentRow> RenderRow(TodayEntry entry, Action<CommitmentRow.ActionKind>? onAction = null) =>
        Render<CommitmentRow>(p => p
            .Add(x => x.Entry, entry)
            .Add(x => x.Now, ReadAt)
            .Add(x => x.OnAction, (CommitmentRow.ActionKind kind) => onAction?.Invoke(kind)));

    /// <summary>
    /// The row's meta line, chip, and its actions' texts and accessible names, joined with " + " in their order; null
    /// without an action. Slice 7 (T051): a Pending commitment without a record has two actions, "Record" and "Excuse".
    /// </summary>
    private static (string Meta, string Chip, string? Action, string? Label) Parts(IRenderedComponent<CommitmentRow> cut)
    {
        var buttons = cut.FindAll("button");
        return (
            cut.Find(".ht-row__meta").TextContent,
            cut.Find(".ht-status").TextContent,
            buttons.Count == 0 ? null : string.Join(" + ", buttons.Select(b => b.TextContent)),
            buttons.Count == 0 ? null : string.Join(" + ", buttons.Select(b => b.GetAttribute("aria-label"))));
    }

    [Fact]
    public void A_Due_commitment_without_a_record_shows_its_schedule_and_Record()
    {
        var cut = RenderRow(Reading(Wed18, EvaluationState.Due, graceEnds: new DateTimeOffset(2026, 11, 21, 0, 0, 0, TimeSpan.Zero)));

        Assert.Equal("Reading, 20 min, 3 days a week", cut.Find(".ht-row__name").TextContent);
        Assert.Empty(cut.FindAll(".ht-row__name .ht-chip"));
        Assert.Equal(("Mon · Wed · Fri", "Due today", "Record", "Record for Reading, 20 min, 3 days a week, Wed 18 Nov"), Parts(cut));
        Assert.Equal(["ht-btn", "ht-btn--secondary", "ht-btn--sm"], cut.Find("button").ClassList);
        Assert.Equal(("button", "dialog"), (cut.Find("button").GetAttribute("type"), cut.Find("button").GetAttribute("aria-haspopup")));
        Assert.Empty(cut.FindAll(".ht-progress"));
    }

    [Fact]
    public void A_Due_commitment_with_a_record_shows_its_progress_and_bar_and_Edit()
    {
        var cut = RenderRow(Reading(Wed18, EvaluationState.Due, minutes: 15, graceEnds: new DateTimeOffset(2026, 11, 21, 0, 0, 0, TimeSpan.Zero)));

        Assert.Equal(("15 of 20 min today", "Due today", "Edit", "Edit record of Reading, 20 min, 3 days a week, Wed 18 Nov"), Parts(cut));
        var bar = cut.Find(".ht-row__main .ht-progress");
        Assert.Equal(
            ("progressbar", "15", "0", "20", "Reading, 20 min, 3 days a week progress"),
            (bar.GetAttribute("role"), bar.GetAttribute("aria-valuenow"), bar.GetAttribute("aria-valuemin"), bar.GetAttribute("aria-valuemax"), bar.GetAttribute("aria-label")));
        Assert.Equal("width:75%", bar.QuerySelector("span")!.GetAttribute("style"));
    }

    [Fact]
    public void A_Due_Reduce_count_day_with_a_record_has_the_Reduce_tag_its_progress_and_bar_and_Record()
    {
        var cut = RenderRow(EntryOf("Coffee, at most 2 cups a day", CoffeeAtMost2Cups, "Every day", Wed18, Wed18, EvaluationState.Due,
            new Progress(ProgressKind.Total, 1, 2), new DateTimeOffset(2026, 11, 21, 0, 0, 0, TimeSpan.Zero), records: 1));

        Assert.Equal("Reduce", cut.Find(".ht-row__name .ht-chip").TextContent);
        Assert.Equal("Coffee, at most 2 cups a day Reduce", cut.Find(".ht-row__name").TextContent);
        Assert.Equal(("1 of at most 2 cups today", "Due today", "Record", "Record for Coffee, at most 2 cups a day, Wed 18 Nov"), Parts(cut));
        Assert.Equal("width:50%", cut.Find(".ht-progress span").GetAttribute("style"));
    }

    [Fact]
    public void A_Due_yes_no_week_with_a_record_shows_its_progress_without_a_bar()
    {
        var cut = RenderRow(EntryOf("Running, 2 times a week", RunningTwiceAWeek, "Flexible · 2 times a week", new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 22),
            EvaluationState.Due, new Progress(ProgressKind.RecordsReachingAmount, 1, 2), new DateTimeOffset(2026, 11, 25, 0, 0, 0, TimeSpan.Zero), records: 1));

        Assert.Equal(("1 of 2 this week", "Due this week", "Record", "Record for Running, 2 times a week, 16 – 22 Nov"), Parts(cut));
        Assert.Empty(cut.FindAll(".ht-progress"));
    }

    [Fact]
    public void A_Pending_commitment_says_how_many_hours_are_left_rounded_up()
    {
        // Monday 16's grace period ends on Thursday 19 at 00:00Z, 5 hours after 19:00Z on Wednesday 18; 4 h 30 min gives 5.
        var cut = RenderRow(Reading(new DateOnly(2026, 11, 16), EvaluationState.Pending, graceEnds: ReadAt.AddHours(4.5)));

        Assert.Equal(
            ("Mon 16 Nov · 5 h left to record", "Pending", "Record + Excuse",
                "Record for Reading, 20 min, 3 days a week, Mon 16 Nov + Excuse Reading, 20 min, 3 days a week, Mon 16 Nov"),
            Parts(cut));
    }

    [Fact]
    public void A_Pending_commitment_with_a_record_has_Edit()
    {
        var cut = RenderRow(Reading(new DateOnly(2026, 11, 16), EvaluationState.Pending, minutes: 10, graceEnds: ReadAt.AddHours(5)));

        Assert.Equal(("Mon 16 Nov · 5 h left to record", "Pending", "Edit", "Edit record of Reading, 20 min, 3 days a week, Mon 16 Nov"), Parts(cut));
        Assert.Empty(cut.FindAll(".ht-progress"));
    }

    [Fact]
    public void A_Pending_week_names_its_dates_and_has_Record()
    {
        var cut = RenderRow(EntryOf("Running, 2 times a week", RunningTwiceAWeek, "Flexible · 2 times a week", new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 15),
            EvaluationState.Pending, new Progress(ProgressKind.RecordsReachingAmount, 1, 2), ReadAt.AddHours(5), records: 1));

        Assert.Equal(("9 – 15 Nov · 5 h left to record", "Pending", "Record", "Record for Running, 2 times a week, 9 – 15 Nov"), Parts(cut));
    }

    [Theory]
    [InlineData(null, "Add note", "Add note for Reading, 20 min, 3 days a week, Fri 13 Nov")]
    [InlineData("Away.", "Edit note", "Edit note of Reading, 20 min, 3 days a week, Fri 13 Nov")]
    public void A_Missed_commitment_says_no_record_and_has_a_note_action(string? note, string action, string label)
    {
        var cut = RenderRow(Reading(new DateOnly(2026, 11, 13), EvaluationState.Missed, note: note));

        Assert.Equal(("Fri 13 Nov · no record", "Missed", action, label), Parts(cut));
        Assert.Equal(["ht-btn", "ht-btn--ghost", "ht-btn--sm"], cut.Find("button").ClassList);
    }

    [Fact]
    public void An_Upcoming_row_has_its_date_the_Upcoming_chip_and_no_action()
    {
        var cut = RenderRow(Reading(new DateOnly(2026, 11, 20), EvaluationState.Upcoming));

        Assert.Equal(("Fri 20 Nov", "Upcoming", null, null), Parts(cut));
        Assert.Equal(["ht-chip", "ht-status", "ht-status--due"], cut.Find(".ht-status").ClassList);
    }

    [Theory]
    [InlineData(EvaluationState.Due, null, CommitmentRow.ActionKind.Record)]
    [InlineData(EvaluationState.Due, 15, CommitmentRow.ActionKind.Edit)]
    [InlineData(EvaluationState.Missed, null, CommitmentRow.ActionKind.Note)]
    public void The_action_raises_OnAction_with_its_kind(EvaluationState state, int? minutes, CommitmentRow.ActionKind expected)
    {
        CommitmentRow.ActionKind? chosen = null;
        var cut = RenderRow(Reading(Wed18, state, minutes, state == EvaluationState.Due ? ReadAt.AddHours(53) : null), kind => chosen = kind);

        cut.Find("button").Click();

        Assert.Equal(expected, chosen);
    }

    [Fact]
    public async Task FocusAsync_focuses_the_action()
    {
        var cut = RenderRow(Reading(Wed18, EvaluationState.Due, graceEnds: ReadAt.AddHours(53)));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        var focused = Assert.IsType<ElementReference>(JSInterop.VerifyFocusAsyncInvoke().Arguments[0]);
        Assert.Equal(cut.Find("button").GetAttribute("blazor:elementReference"), focused.Id);
    }

    [Fact]
    public async Task FocusAsync_does_nothing_for_an_Upcoming_row()
    {
        var cut = RenderRow(Reading(new DateOnly(2026, 11, 20), EvaluationState.Upcoming));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Assert.Empty(JSInterop.Invocations);
    }

    // ---- slice 7: "Excuse" (HP-005, US1-AS12; design/components/CommitmentRow, slice 7 state) ----

    [Fact]
    public void Slice_7_HP_005_a_Pending_commitment_without_a_record_has_the_ghost_Excuse_after_Record()
    {
        var cut = RenderRow(Reading(new DateOnly(2026, 11, 16), EvaluationState.Pending, graceEnds: ReadAt.AddHours(5)));

        var buttons = cut.FindAll("button");
        Assert.Equal(["Record", "Excuse"], buttons.Select(b => b.TextContent));
        var excuse = buttons[1];
        Assert.Equal(["ht-btn", "ht-btn--ghost", "ht-btn--sm"], excuse.ClassList);
        Assert.Equal(("button", "dialog"), (excuse.GetAttribute("type"), excuse.GetAttribute("aria-haspopup")));
        Assert.Equal("Excuse Reading, 20 min, 3 days a week, Mon 16 Nov", excuse.GetAttribute("aria-label"));
    }

    [Fact]
    public void Slice_7_HP_005_no_Excuse_on_a_Due_commitment_a_period_or_a_commitment_with_a_record()
    {
        var due = RenderRow(Reading(Wed18, EvaluationState.Due, graceEnds: ReadAt.AddHours(53)));
        var recorded = RenderRow(Reading(new DateOnly(2026, 11, 16), EvaluationState.Pending, minutes: 10, graceEnds: ReadAt.AddHours(5)));
        var week = RenderRow(EntryOf("Running, 2 times a week", RunningTwiceAWeek, "Flexible · 2 times a week", new DateOnly(2026, 11, 9),
            new DateOnly(2026, 11, 15), EvaluationState.Pending, new Progress(ProgressKind.RecordsReachingAmount, 0, 2), ReadAt.AddHours(5)));
        var day = RenderRow(EntryOf("Coffee, at most 2 cups a day", CoffeeAtMost2Cups, "Every day", new DateOnly(2026, 11, 17), new DateOnly(2026, 11, 17),
            EvaluationState.Pending, new Progress(ProgressKind.Total, 0, 2), ReadAt.AddHours(5)));

        Assert.All(new[] { due, recorded, week, day }, cut => Assert.DoesNotContain(cut.FindAll("button"), b => b.TextContent == "Excuse"));
    }

    [Fact]
    public void Slice_7_HP_005_Excuse_raises_OnAction_with_Excuse()
    {
        CommitmentRow.ActionKind? chosen = null;
        var cut = RenderRow(Reading(new DateOnly(2026, 11, 16), EvaluationState.Pending, graceEnds: ReadAt.AddHours(5)), kind => chosen = kind);

        cut.FindAll("button")[1].Click();

        Assert.Equal(CommitmentRow.ActionKind.Excuse, chosen);
    }

    [Fact]
    public async Task Slice_7_FocusExcuseAsync_focuses_Excuse_and_on_a_row_without_it_the_action()
    {
        var pending = RenderRow(Reading(new DateOnly(2026, 11, 16), EvaluationState.Pending, graceEnds: ReadAt.AddHours(5)));
        var due = RenderRow(Reading(Wed18, EvaluationState.Due, graceEnds: ReadAt.AddHours(53)));

        await pending.InvokeAsync(() => pending.Instance.FocusExcuseAsync().AsTask());
        await due.InvokeAsync(() => due.Instance.FocusExcuseAsync().AsTask());

        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<ElementReference>(i.Arguments[0]).Id);
        Assert.Equal(
            [pending.FindAll("button")[1].GetAttribute("blazor:elementReference"), due.Find("button").GetAttribute("blazor:elementReference")],
            focused);
    }

    // ---- slice 8: the row's own settings (research R-07, AC-080, contracts/host.md "Signed-in Home"; T059) ----

    private static EntrySettings SettingsOf(GoalTarget target) => new()
    {
        GoalScheduleKind = target.ScheduleKindOf, Period = target.PeriodLength, BuildOrReduce = target.BuildOrReduce,
        Measurement = target.Measurement, Amount = target.Amount, CountUnit = target.CountUnit, DurationUnit = target.DurationUnit,
        Times = target.ScheduleKindOf == GoalScheduleKind.PeriodBased ? target.Schedule.Times : null, Target = target,
    };

    /// <summary>
    /// A row of attempt 1, a "Reading, 20 min, 3 days a week" commitment with its own settings, of a goal whose latest
    /// attempt has <paramref name="latest"/>: the goal-level settings of the row's <see cref="TodayEntry.Goal"/>, as the
    /// dashboard gives an earlier attempt's Pending entry (FR-019).
    /// </summary>
    private static TodayEntry EarlierReading(GoalTarget latest, DateOnly date, EvaluationState state, int? minutes, DateTimeOffset graceEnds)
    {
        var row = Reading(date, state, minutes, graceEnds);
        var entry = row.Entry with { AttemptNumber = 1, IsLatestAttempt = false, Settings = SettingsOf(Reading20MinOnMonWedFri) };
        var goal = row.Goal with
        {
            GoalScheduleKind = latest.ScheduleKindOf, Period = latest.PeriodLength, BuildOrReduce = latest.BuildOrReduce,
            Measurement = latest.Measurement, Amount = latest.Amount, CountUnit = latest.CountUnit, DurationUnit = latest.DurationUnit,
            Times = latest.ScheduleKindOf == GoalScheduleKind.PeriodBased ? latest.Schedule.Times : null, Target = latest,
            Entries = [entry],
        };
        return row with { Goal = goal, Entry = entry };
    }

    [Fact]
    public void AC_080_an_earlier_attempts_Pending_commitment_under_a_per_day_attempt_has_its_own_commitment_actions()
    {
        // Under a latest attempt of Reduce, at most 2 cups a day (period-based), attempt 1's commitment keeps "Excuse",
        // which only a commitment has, and with a record "Edit", where a period would have "Record".
        var cut = RenderRow(EarlierReading(CoffeeAtMost2Cups, new DateOnly(2026, 11, 16), EvaluationState.Pending, null, ReadAt.AddHours(5)));
        var recorded = RenderRow(EarlierReading(CoffeeAtMost2Cups, new DateOnly(2026, 11, 16), EvaluationState.Pending, 10, ReadAt.AddHours(5)));

        Assert.Equal(
            ("Mon 16 Nov · 5 h left to record", "Pending", "Record + Excuse",
                "Record for Reading, 20 min, 3 days a week, Mon 16 Nov + Excuse Reading, 20 min, 3 days a week, Mon 16 Nov"),
            Parts(cut));
        Assert.Equal(("Mon 16 Nov · 5 h left to record", "Pending", "Edit", "Edit record of Reading, 20 min, 3 days a week, Mon 16 Nov"), Parts(recorded));
        Assert.Empty(cut.FindAll(".ht-row__name .ht-chip"));
    }

    [Fact]
    public void A_Due_commitment_under_goal_level_settings_of_a_yes_no_week_shows_its_own_state_word_progress_and_bar()
    {
        // The row's own settings, a duration commitment, give "Due today", "15 of 20 min today" and the bar; the goal-level
        // yes/no week would give "Due this week" and no bar.
        var cut = RenderRow(EarlierReading(RunningTwiceAWeek, Wed18, EvaluationState.Due, 15, ReadAt.AddHours(53)));

        Assert.Equal(("15 of 20 min today", "Due today", "Edit", "Edit record of Reading, 20 min, 3 days a week, Wed 18 Nov"), Parts(cut));
        Assert.Equal("width:75%", cut.Find(".ht-row__main .ht-progress span").GetAttribute("style"));
    }
}
