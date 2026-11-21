using Bunit;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Shared;

namespace HT.UiTests.Components.Shared;

/// <summary>
/// StatusChip (HP-004, design/components/StatusChip slice-5 state; slice 7 HP-006): each state's word from RecordingText
/// and the design's classes, and no other, "Excused" and "Paused" included; the word is always there, so colour is never
/// the only signal.
/// </summary>
public sealed class StatusChipTests : BunitContext
{
    [Theory]
    [InlineData(EvaluationState.Met, "Met", "ht-status--met")]
    [InlineData(EvaluationState.Failed, "Failed", "ht-status--failed")]
    [InlineData(EvaluationState.Missed, "Missed", "ht-status--missed")]
    [InlineData(EvaluationState.Pending, "Pending", "ht-status--pending")]
    [InlineData(EvaluationState.Due, "Due today", "ht-status--due")]
    public void A_commitment_chip_has_the_state_word_and_the_design_classes(EvaluationState state, string text, string modifier)
    {
        var cut = Render<StatusChip>(p => p
            .Add(x => x.State, state)
            .Add(x => x.ScheduleKind, GoalScheduleKind.CommitmentBased));

        var chip = cut.Find("span");
        Assert.Equal(text, chip.TextContent);
        Assert.Equal(["ht-chip", "ht-status", modifier], chip.ClassList.ToArray());
    }

    public static TheoryData<EvaluationState, GoalScheduleKind, SchedulePeriod?> EveryExemption()
    {
        var data = new TheoryData<EvaluationState, GoalScheduleKind, SchedulePeriod?>();
        foreach (var state in new[] { EvaluationState.Excused, EvaluationState.Paused })
        {
            data.Add(state, GoalScheduleKind.CommitmentBased, null);
            data.Add(state, GoalScheduleKind.PeriodBased, SchedulePeriod.Day);
            data.Add(state, GoalScheduleKind.PeriodBased, SchedulePeriod.Week);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(EveryExemption))]
    public void Slice_7_an_Excused_or_Paused_chip_has_its_word_and_the_design_classes(EvaluationState state, GoalScheduleKind kind, SchedulePeriod? period)
    {
        // Slice 7 (HP-006, design/components/StatusChip slice-7 state): the word from ReportText.State, never colour alone.
        var (text, modifier) = state == EvaluationState.Excused ? ("Excused", "ht-status--excused") : ("Paused", "ht-status--paused");

        var cut = Render<StatusChip>(p => p
            .Add(x => x.State, state)
            .Add(x => x.ScheduleKind, kind)
            .Add(x => x.Period, period));

        cut.MarkupMatches($"<span class=\"ht-chip ht-status {modifier}\">{text}</span>");
    }

    [Fact]
    public void A_per_day_period_that_is_due_reads_Due_today()
    {
        var cut = Render<StatusChip>(p => p
            .Add(x => x.State, EvaluationState.Due)
            .Add(x => x.ScheduleKind, GoalScheduleKind.PeriodBased)
            .Add(x => x.Period, SchedulePeriod.Day));

        Assert.Equal("Due today", cut.Find("span.ht-chip.ht-status.ht-status--due").TextContent);
    }

    [Fact]
    public void A_per_week_period_that_is_due_reads_Due_this_week()
    {
        var cut = Render<StatusChip>(p => p
            .Add(x => x.State, EvaluationState.Due)
            .Add(x => x.ScheduleKind, GoalScheduleKind.PeriodBased)
            .Add(x => x.Period, SchedulePeriod.Week));

        var chip = cut.Find("span");
        Assert.Equal("Due this week", chip.TextContent);
        Assert.Equal(["ht-chip", "ht-status", "ht-status--due"], chip.ClassList.ToArray());
    }

    [Fact]
    public void A_Reduce_goal_that_is_met_reads_Met()
    {
        // A chip knows no Build or Reduce: a Reduce goal's met period is "Met" like any other (constitution I).
        var cut = Render<StatusChip>(p => p
            .Add(x => x.State, EvaluationState.Met)
            .Add(x => x.ScheduleKind, GoalScheduleKind.PeriodBased)
            .Add(x => x.Period, SchedulePeriod.Day));

        Assert.Equal("Met", cut.Find("span.ht-status--met").TextContent);
        cut.MarkupMatches("<span class=\"ht-chip ht-status ht-status--met\">Met</span>");
    }

    [Theory]
    [InlineData(GoalScheduleKind.CommitmentBased, null)]
    [InlineData(GoalScheduleKind.PeriodBased, SchedulePeriod.Day)]
    [InlineData(GoalScheduleKind.PeriodBased, SchedulePeriod.Week)]
    public void An_upcoming_chip_reads_Upcoming_with_the_due_class(GoalScheduleKind kind, SchedulePeriod? period)
    {
        // Slice 6 (HP-003, design/components/StatusChip slice-6 state): "Upcoming", never "Due Mon"; the date belongs
        // to the row's meta line, and the word tells it apart from Due, which shares its colour.
        var cut = Render<StatusChip>(p => p
            .Add(x => x.State, EvaluationState.Upcoming)
            .Add(x => x.ScheduleKind, kind)
            .Add(x => x.Period, period));

        cut.MarkupMatches("<span class=\"ht-chip ht-status ht-status--due\">Upcoming</span>");
    }

    public static TheoryData<EvaluationState, GoalScheduleKind, SchedulePeriod?> EveryShownState()
    {
        var data = new TheoryData<EvaluationState, GoalScheduleKind, SchedulePeriod?>();
        foreach (var state in new[] { EvaluationState.Met, EvaluationState.Failed, EvaluationState.Missed, EvaluationState.Pending, EvaluationState.Due })
        {
            data.Add(state, GoalScheduleKind.CommitmentBased, null);
            data.Add(state, GoalScheduleKind.PeriodBased, SchedulePeriod.Day);
            data.Add(state, GoalScheduleKind.PeriodBased, SchedulePeriod.Week);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(EveryShownState))]
    public void The_word_is_always_shown_with_the_colour(EvaluationState state, GoalScheduleKind kind, SchedulePeriod? period)
    {
        var cut = Render<StatusChip>(p => p
            .Add(x => x.State, state)
            .Add(x => x.ScheduleKind, kind)
            .Add(x => x.Period, period));

        var chip = cut.Find("span");
        Assert.False(string.IsNullOrWhiteSpace(chip.TextContent));
        Assert.Equal(3, chip.ClassList.Length);
        Assert.Single(chip.ClassList, c => c.StartsWith("ht-status--", StringComparison.Ordinal));
    }
}
