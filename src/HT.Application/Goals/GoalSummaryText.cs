using System.Globalization;
using HT.Application.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Goals;

/// <summary>One row of the summary card's list: its label and its value ("Evaluated", "Each scheduled day").</summary>
public sealed record GoalSummaryRow(string Label, string Value);

/// <summary>
/// What the New goal summary card shows (HP-002). While the form is not complete, only <see cref="IncompleteText"/>:
/// no sentence, no rows and no line. Otherwise the sentence, the four rows in order, and the optional first-week and
/// catalog suggestion lines.
/// </summary>
public sealed record GoalSummary
{
    /// <summary>The card's eyebrow, in both states.</summary>
    public string Title => GoalSummaryText.Title;

    /// <summary>True when the habit, the target and the start date make a goal the card can describe.</summary>
    public required bool IsComplete { get; init; }

    /// <summary><see cref="GoalSummaryText.Incomplete"/> while the form is not complete; null otherwise.</summary>
    public string? IncompleteText { get; init; }

    /// <summary>The goal as one sentence: "Reading: at least 15 min, 5 days a week, starting Tue 17 Nov.".</summary>
    public string? Sentence { get; init; }

    /// <summary>Evaluated, Met when, Streak counted in and Late records, in this order; empty while not complete.</summary>
    public IReadOnlyList<GoalSummaryRow> Rows { get; init; } = [];

    /// <summary>"Your first week starts on Mon 23 Nov. Days before it are not evaluated."; per-week goals not starting on a Monday only.</summary>
    public string? FirstWeekLine { get; init; }

    /// <summary>"Catalog suggestion: … An example to adjust, not a prescribed target."; only when a suggestion gave the starting values.</summary>
    public string? SuggestionLine { get; init; }
}

/// <summary>
/// The one formatter of the New goal summary card (HP-002, research R-11; texts of design/pages/PageNewGoal, "Summary
/// card"). Pure: the result depends only on its arguments, never on a clock or a culture; dates come from
/// <see cref="DateTexts"/>. The goal page's Settings card uses <see cref="EvaluatedOf"/> for its "Evaluated" row.
/// </summary>
public static class GoalSummaryText
{
    public const string Title = "Your goal";
    public const string Incomplete = "Complete the form to see your goal here.";
    public const string EvaluatedLabel = "Evaluated";
    public const string MetWhenLabel = "Met when";
    public const string StreakLabel = "Streak counted in";
    public const string LateRecordsLabel = "Late records";

    /// <summary>
    /// The card for the form's current values. It is complete when <paramref name="habitName"/> is set, the target
    /// passes the checks of FR-006 up to and including the Reduce N rule, and <paramref name="startDate"/> is set; a
    /// start date in the past or too far ahead, and a long name, do not make it incomplete. The suggestion line uses
    /// <paramref name="catalogSuggestionText"/>, the starting values' text, while it is not null.
    /// </summary>
    public static GoalSummary For(string? habitName, GoalTargetFields target, DateOnly? startDate, string? catalogSuggestionText)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (string.IsNullOrEmpty(habitName) || startDate is not { } start || !GoalTarget.TryCreate(target, out var checkedTarget, out _))
        {
            return new GoalSummary { IsComplete = false, IncompleteText = Incomplete };
        }

        return new GoalSummary
        {
            IsComplete = true,
            Sentence = Sentence(habitName, checkedTarget, start),
            Rows =
            [
                new(EvaluatedLabel, EvaluatedOf(checkedTarget)),
                new(MetWhenLabel, MetWhen(checkedTarget)),
                new(StreakLabel, StreakUnit(checkedTarget)),
                new(LateRecordsLabel, LateRecords(checkedTarget)),
            ],
            FirstWeekLine = GoalTimeline.FirstWeekStart(checkedTarget, start) is { } monday && monday != start
                ? "Your first week starts on " + DateTexts.ShortDate(monday) + ". Days before it are not evaluated."
                : null,
            SuggestionLine = catalogSuggestionText is null
                ? null
                : "Catalog suggestion: " + catalogSuggestionText + ". An example to adjust, not a prescribed target.",
        };
    }

    /// <summary>"Each scheduled day" (fixed days), "Each day" (per day) or "Each week" (per week).</summary>
    public static string EvaluatedOf(GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return EvaluatedOf(target.ScheduleKindOf, target.PeriodLength);
    }

    /// <summary>
    /// The same text from a goal view's schedule kind and period (the goal page's Settings card, HP-004): "Each
    /// scheduled day" for a commitment-based goal, otherwise "Each day" or "Each week" by <paramref name="period"/>.
    /// </summary>
    public static string EvaluatedOf(GoalScheduleKind kind, SchedulePeriod? period) =>
        kind == GoalScheduleKind.CommitmentBased
            ? "Each scheduled day"
            : period == SchedulePeriod.Day ? "Each day" : "Each week";

    /// <summary>
    /// The habit's name, ": ", "at least " for a Build count or duration goal only, the target text, ", starting ", the
    /// start date as "Tue 17 Nov", and "." (HP-002).
    /// </summary>
    private static string Sentence(string habitName, GoalTarget target, DateOnly start)
    {
        var atLeast = target is { BuildOrReduce: BuildOrReduce.Build, Measurement: not Measurement.YesNo } ? "at least " : string.Empty;
        return habitName + ": " + atLeast + GoalText.TargetText(target) + ", starting " + DateTexts.ShortDate(start) + ".";
    }

    /// <summary>"Met when" per schedule kind (FR-004, design/pages/PageNewGoal).</summary>
    private static string MetWhen(GoalTarget target)
    {
        var build = target.BuildOrReduce == BuildOrReduce.Build;
        var amount = GoalText.AmountText(target);
        if (target.PeriodLength is not { } period)
        {
            if (amount is null)
            {
                return build ? "Done that day" : "Not done that day";
            }

            return (build ? "At least " : "At most ") + amount + " that day";
        }

        var that = period == SchedulePeriod.Day ? " that day" : " that week";
        var times = Times(target.Schedule.Times ?? 1);
        if (amount is null)
        {
            return (build ? "Done at least " : "Done at most ") + times + that;
        }

        return build ? amount + " or more, at least " + times + that : "At most " + amount + " in total" + that;
    }

    private static string StreakUnit(GoalTarget target) => target.PeriodLength switch
    {
        null => "Commitments",
        SchedulePeriod.Day => "Days",
        _ => "Weeks",
    };

    private static string LateRecords(GoalTarget target) =>
        target.PeriodLength == SchedulePeriod.Week ? "Up to 48 h after each week" : "Up to 48 h after each day";

    /// <summary>"once" for 1, "N times" otherwise.</summary>
    private static string Times(int times) =>
        times == 1 ? "once" : times.ToString(CultureInfo.InvariantCulture) + " times";
}
