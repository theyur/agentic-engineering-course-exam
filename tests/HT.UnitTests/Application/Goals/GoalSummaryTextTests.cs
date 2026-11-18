using HT.Application.Goals;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// HP-002 and design/pages/PageNewGoal "Summary card": the sentence, the four rows per schedule kind, "Met when" for
/// every measurement and both intents, the first-week and suggestion lines, and the incomplete-form state.
/// </summary>
public sealed class GoalSummaryTextTests
{
    /// <summary>Tuesday 17 November 2026, the preview's today.</summary>
    private static readonly DateOnly Tuesday = new(2026, 11, 17);

    private static readonly DateOnly Monday = new(2026, 11, 23);

    private static GoalSummary For(GoalTargetFields fields, DateOnly? start = null, string habit = "Reading", string? suggestion = null) =>
        GoalSummaryText.For(habit, fields, start ?? Tuesday, suggestion);

    private static string? Row(GoalSummary summary, string label) => summary.Rows.SingleOrDefault(r => r.Label == label)?.Value;

    public static TheoryData<string, GoalTargetFields, string> Sentences => new()
    {
        { "Reading", DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), "Reading: at least 15 min, 5 days a week, starting Tue 17 Nov." },
        { "Sugar", CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "Sugar: at most 1 sweet a day, starting Tue 17 Nov." },
        { "Cold shower", YesNoDays(BuildOrReduce.Build, AllDays), "Cold shower: daily, starting Tue 17 Nov." },
        { "Yoga", YesNoTimes(3, SchedulePeriod.Week), "Yoga: 3 times a week, starting Tue 17 Nov." },
        { "Reading", CountTimes(20, "pages", 3, SchedulePeriod.Week), "Reading: at least 20 pages, 3 times a week, starting Tue 17 Nov." },
        { "No phone in bed", YesNoDays(BuildOrReduce.Reduce, AllDays), "No phone in bed: avoided every day, starting Tue 17 Nov." },
        { "Sweets", YesNoTimes(1, SchedulePeriod.Day, BuildOrReduce.Reduce), "Sweets: at most once a day, starting Tue 17 Nov." },
    };

    [Theory]
    [MemberData(nameof(Sentences))]
    public void HP_002_the_goal_as_one_sentence(string habit, GoalTargetFields fields, string sentence)
    {
        var summary = For(fields, habit: habit);

        Assert.True(summary.IsComplete);
        Assert.Equal(sentence, summary.Sentence);
        Assert.Equal("Your goal", summary.Title);
        Assert.Null(summary.IncompleteText);
    }

    public static TheoryData<GoalTargetFields, string, string, string> Kinds => new()
    {
        { DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), "Each scheduled day", "Commitments", "Up to 48 h after each day" },
        { CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "Each day", "Days", "Up to 48 h after each day" },
        { YesNoTimes(3, SchedulePeriod.Week), "Each week", "Weeks", "Up to 48 h after each week" },
    };

    [Theory]
    [MemberData(nameof(Kinds))]
    public void The_rows_for_each_schedule_kind_in_order(GoalTargetFields fields, string evaluated, string streak, string late)
    {
        var summary = For(fields);

        Assert.Equal(["Evaluated", "Met when", "Streak counted in", "Late records"], summary.Rows.Select(r => r.Label));
        Assert.Equal(evaluated, Row(summary, "Evaluated"));
        Assert.Equal(streak, Row(summary, "Streak counted in"));
        Assert.Equal(late, Row(summary, "Late records"));
        Assert.Equal(evaluated, GoalSummaryText.EvaluatedOf(Target(fields)));
        Assert.Equal(evaluated, GoalSummaryText.EvaluatedOf(Target(fields).ScheduleKindOf, Target(fields).PeriodLength));
    }

    public static TheoryData<GoalTargetFields, string> MetWhen => new()
    {
        { YesNoDays(BuildOrReduce.Build, Weekdays), "Done that day" },
        { YesNoDays(BuildOrReduce.Reduce, AllDays), "Not done that day" },
        { DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), "At least 15 min that day" },
        { DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, MonWedFri), "At least 1 h that day" },
        { CountDays(2, "cups", BuildOrReduce.Reduce, AllDays), "At most 2 cups that day" },
        { CountDays(0, "cigarettes", BuildOrReduce.Reduce, Weekdays), "At most 0 cigarettes that day" },
        { YesNoTimes(3, SchedulePeriod.Week), "Done at least 3 times that week" },
        { YesNoTimes(1, SchedulePeriod.Day), "Done at least once that day" },
        { YesNoTimes(1, SchedulePeriod.Day, BuildOrReduce.Reduce), "Done at most once that day" },
        { YesNoTimes(2, SchedulePeriod.Week, BuildOrReduce.Reduce), "Done at most 2 times that week" },
        { CountTimes(20, "pages", 3, SchedulePeriod.Week), "20 pages or more, at least 3 times that week" },
        { DurationTimes(10, DurationUnit.Minutes, 1, SchedulePeriod.Day), "10 min or more, at least once that day" },
        { CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "At most 1 sweet in total that day" },
        { DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Week, BuildOrReduce.Reduce), "At most 2 h in total that week" },
    };

    [Theory]
    [MemberData(nameof(MetWhen))]
    public void FR_004_met_when_for_every_measurement_and_both_intents(GoalTargetFields fields, string metWhen)
    {
        Assert.Equal(metWhen, Row(For(fields), "Met when"));
    }

    [Fact]
    public void The_catalog_suggestion_line_only_when_a_suggestion_was_used()
    {
        var fields = DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays);

        Assert.Equal(
            "Catalog suggestion: 15 min, 5 days a week. An example to adjust, not a prescribed target.",
            For(fields, suggestion: "15 min, 5 days a week").SuggestionLine);
        Assert.Equal(
            "Catalog suggestion: 15 min, 5 days a week. An example to adjust, not a prescribed target.",
            For(fields with { Amount = 30 }, suggestion: "15 min, 5 days a week").SuggestionLine);
        Assert.Null(For(fields).SuggestionLine);
    }

    [Fact]
    public void A_per_week_goal_starting_on_a_Tuesday_has_the_first_week_line()
    {
        Assert.Equal(
            "Your first week starts on Mon 23 Nov. Days before it are not evaluated.",
            For(YesNoTimes(3, SchedulePeriod.Week)).FirstWeekLine);
    }

    [Fact]
    public void A_per_week_goal_starting_on_a_Monday_and_other_goals_have_no_first_week_line()
    {
        Assert.Null(For(YesNoTimes(3, SchedulePeriod.Week), Monday).FirstWeekLine);
        Assert.Null(For(YesNoTimes(3, SchedulePeriod.Day)).FirstWeekLine);
        Assert.Null(For(YesNoDays(BuildOrReduce.Build, MonWedFri)).FirstWeekLine);
    }

    public static TheoryData<string?, GoalTargetFields, DateOnly?> Incomplete => new()
    {
        { null, DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), Tuesday },
        { "", DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), Tuesday },
        { "Reading", new GoalTargetFields(), Tuesday },
        { "Reading", DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays) with { DurationUnit = null }, Tuesday },
        { "Reading", YesNoTimes(3, SchedulePeriod.Week) with { Period = null }, Tuesday },
        { "Sugar", CountTimes(1, "sweet", 2, SchedulePeriod.Day, BuildOrReduce.Reduce), Tuesday },
        { "Reading", DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), null },
    };

    [Theory]
    [MemberData(nameof(Incomplete))]
    public void An_incomplete_form_shows_only_the_title_and_one_line(string? habit, GoalTargetFields fields, DateOnly? start)
    {
        var summary = GoalSummaryText.For(habit, fields, start, "15 min, 5 days a week");

        Assert.False(summary.IsComplete);
        Assert.Equal("Your goal", summary.Title);
        Assert.Equal("Complete the form to see your goal here.", summary.IncompleteText);
        Assert.Null(summary.Sentence);
        Assert.Empty(summary.Rows);
        Assert.Null(summary.FirstWeekLine);
        Assert.Null(summary.SuggestionLine);
    }

    [Fact]
    public void A_start_date_in_the_past_or_too_far_ahead_does_not_make_it_incomplete()
    {
        var fields = YesNoDays(BuildOrReduce.Build, AllDays);

        Assert.Equal("Reading: daily, starting Mon 16 Nov.", For(fields, new DateOnly(2026, 11, 16)).Sentence);
        Assert.Equal("Reading: daily, starting Fri 31 Dec.", For(fields, new DateOnly(2027, 12, 31)).Sentence);
    }

    [Fact]
    public void No_culture_is_read()
    {
        using var culture = new CultureScope("ar-SA");

        var summary = For(YesNoTimes(12, SchedulePeriod.Week));

        Assert.Equal("Reading: 12 times a week, starting Tue 17 Nov.", summary.Sentence);
        Assert.Equal("Done at least 12 times that week", Row(summary, "Met when"));
        Assert.Equal("Your first week starts on Mon 23 Nov. Days before it are not evaluated.", summary.FirstWeekLine);
    }
}
