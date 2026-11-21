using System.Globalization;
using HT.Application.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// The one formatter of the recording texts (research R-12, HP-002, HP-004, HP-011): the state words, the progress
/// line and its bar, the recorded values, the entry dates, the RecordDialog's title, sub line, "For" labels, field
/// label and help texts, worded as the design's slice-5 state words them (<c>design/components/RecordDialog.md</c>,
/// <c>RecordForms.md</c>, <c>StatusChip.md</c>). Pure and culture-invariant: numbers use the invariant culture, dates
/// come only from <see cref="DateTexts"/>, and the time is a parameter, never a clock. It takes the goal's measurement
/// and unit from <see cref="GoalHistory"/>, so no text but <see cref="Title"/> names a goal (HP-006). A count unit is
/// written as stored, never inflected ("3 sweet", "1 of at most 2 cups").
/// </summary>
public static class RecordingText
{
    private const string Separator = " · ";
    private const string EmptyDayCannotProve = "an empty day cannot prove you stayed under the limit.";
    private const string FailedWhenFinalized = " counts as Failed when it is finalized.";

    /// <summary>
    /// The StatusChip's word (HP-004): "Met" (Build and Reduce alike), "Failed", "Missed", "Pending", for Due
    /// "Due today" for a commitment or a per-day period and "Due this week" for a per-week period, and "Excused" and
    /// "Paused" (slice 7 HP-006).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Upcoming, which no page of this slice shows, or an undefined state.</exception>
    public static string State(EvaluationState state, GoalScheduleKind scheduleKind, SchedulePeriod? period) => state switch
    {
        EvaluationState.Met => "Met",
        EvaluationState.Failed => "Failed",
        EvaluationState.Missed => "Missed",
        EvaluationState.Pending => "Pending",
        EvaluationState.Due => scheduleKind == GoalScheduleKind.PeriodBased && period == SchedulePeriod.Week
            ? "Due this week"
            : "Due today",
        EvaluationState.Excused => "Excused",
        EvaluationState.Paused => "Paused",
        _ => throw new ArgumentOutOfRangeException(nameof(state), "The state has no text in this slice."),
    };

    /// <summary>
    /// The progress line (FR-014): "12 of 20 min", "1 of at most 2 cups", "2 of 3 times", "0 of 1 time",
    /// "1 of at most 2 times". Durations are in minutes. Null for a yes/no commitment, whose field already shows the
    /// answer, and for a commitment with no value (<see cref="HT.Domain.Evaluation.Progress.Value"/> null), so the dialog
    /// then shows neither the line nor the bar.
    /// </summary>
    public static string? Progress(GoalHistory history, Progress progress)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(progress);
        var reduce = history.BuildOrReduce == BuildOrReduce.Reduce;
        switch (progress.Kind)
        {
            case ProgressKind.RecordedValue:
                if (history.Measurement == Measurement.YesNo || progress.Value is not { } value)
                {
                    return null;
                }

                return Number(value) + Of(reduce) + Amount(history.Measurement, history.CountUnit, progress.Of);
            case ProgressKind.RecordsReachingAmount:
            case ProgressKind.YesRecords:
                return Number(progress.Value ?? 0) + Of(reduce) + Times(progress.Of);
            case ProgressKind.Total:
                return Number(progress.Value ?? 0) + Of(reduce) + Amount(history.Measurement, history.CountUnit, progress.Of);
            default:
                throw new ArgumentOutOfRangeException(nameof(progress), "The progress kind is not defined.");
        }
    }

    /// <summary>
    /// How full the progress bar is, 0 to 100: the value against the target, capped at full. A target of 0 (a Reduce
    /// "at most 0") is full once the value is above 0 and empty at 0; no value is empty.
    /// </summary>
    public static int ProgressFill(Progress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        var value = Math.Max(progress.Value ?? 0, 0);
        if (progress.Of <= 0)
        {
            return value > 0 ? 100 : 0;
        }

        return (int)Math.Min(100L, value * 100L / progress.Of);
    }

    /// <summary>An amount with its unit: "12 min" for a duration (always minutes), "3 sweet" or "2 cups" for a count, with the unit as stored.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A yes/no measurement, which has no amount.</exception>
    public static string Amount(Measurement measurement, string? countUnit, int amount) => measurement switch
    {
        Measurement.Count => Number(amount) + " " + countUnit,
        Measurement.Duration => Number(amount) + " min",
        _ => throw new ArgumentOutOfRangeException(nameof(measurement), "A yes/no measurement has no amount."),
    };

    /// <summary>A record as the History card's Recorded cell shows it: "Yes", "No", or its <see cref="Amount"/>.</summary>
    /// <exception cref="ArgumentException">The record has neither an answer nor an amount; the message names no value.</exception>
    public static string Recorded(GoalHistory history, RecordView record)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(record);
        return record.Answer is { } answer ? (answer == YesNoAnswer.Yes ? "Yes" : "No")
            : record.Amount is { } amount ? Amount(history.Measurement, history.CountUnit, amount)
            : throw new ArgumentException("The record has no value.", nameof(record));
    }

    /// <summary>An entry's date: "Tue 17 Nov" for a single date, "10 – 16 Aug" for a week.</summary>
    public static string EntryDate(HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return DateOf(entry.FirstDate, entry.LastDate);
    }

    /// <summary>The RecordDialog's title, the same for "Edit": "Record · Reading, 20 min daily".</summary>
    public static string Title(string goalName)
    {
        ArgumentNullException.ThrowIfNull(goalName);
        return "Record" + Separator + goalName;
    }

    /// <summary>The field's label by measurement: "Did you do it?" (yes/no), "Amount" (count), "Time spent" (duration).</summary>
    public static string FieldLabel(GoalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        return history.Measurement switch
        {
            Measurement.YesNo => "Did you do it?",
            Measurement.Count => "Amount",
            Measurement.Duration => "Time spent",
            _ => throw new ArgumentOutOfRangeException(nameof(history), "The measurement is not defined."),
        };
    }

    /// <summary>
    /// The Reduce help under the field (FR-013): "At most 2 cups. Enter 0 if you had none: an empty day cannot prove you
    /// stayed under the limit." for count and duration; "Choose No if you avoided it: an empty day cannot prove you
    /// stayed under the limit." for yes/no; null for a Build goal.
    /// </summary>
    public static string? ReduceHelp(GoalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.BuildOrReduce != BuildOrReduce.Reduce)
        {
            return null;
        }

        return history.Target.Measurement == Measurement.YesNo
            ? "Choose No if you avoided it: " + EmptyDayCannotProve
            : "At most " + GoalText.AmountText(history.Target) + ". Enter 0 if you had none: " + EmptyDayCannotProve;
    }

    /// <summary>
    /// The help under the RecordDialog's note field (HP-003, research R-07), from the goal's schedule kind and period:
    /// "Clear the field to remove the note." for a commitment-based goal; "This note is for the whole day. Clear the
    /// field to remove it." for a per-day period; "This note is for the whole week. Clear the field to remove it." for
    /// a per-week period.
    /// </summary>
    public static string NoteHelp(GoalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        return history.GoalScheduleKind switch
        {
            GoalScheduleKind.CommitmentBased => "Clear the field to remove the note.",
            GoalScheduleKind.PeriodBased => history.Period == SchedulePeriod.Week
                ? "This note is for the whole week. Clear the field to remove it."
                : "This note is for the whole day. Clear the field to remove it.",
            _ => throw new ArgumentOutOfRangeException(nameof(history), "The schedule kind is not defined."),
        };
    }

    /// <summary>
    /// A "For" label of the RecordDialog (FR-011), called with <see cref="GoalHistory.CurrentDate"/> and
    /// <see cref="GoalHistory.ReadAt"/>: "Today, Fri 30 Oct" for the Due entry whose single date is the current date,
    /// otherwise the entry's date ("Wed 28 Oct", "26 Oct – 1 Nov"), then for a Pending entry " · Pending, N h left",
    /// N being the whole hours from <paramref name="now"/> to its grace end, rounded up ("1 h left" in the last hour).
    /// </summary>
    /// <exception cref="ArgumentException">A Pending entry without a grace end; the message names no value.</exception>
    public static string ForLabel(HistoryEntry entry, DateOnly currentDate, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var date = EntryDate(entry);
        if (entry.State == EvaluationState.Due && entry.FirstDate == entry.LastDate && entry.FirstDate == currentDate)
        {
            return "Today, " + date;
        }

        if (entry.State != EvaluationState.Pending)
        {
            return date;
        }

        if (entry.GraceEnds is not { } graceEnds)
        {
            throw new ArgumentException("A Pending entry has a grace end.", nameof(entry));
        }

        return date + Separator + "Pending, " + HoursLeft(graceEnds, now).ToString(CultureInfo.InvariantCulture) + " h left";
    }

    /// <summary>
    /// The whole hours from <paramref name="now"/> to <paramref name="graceEnds"/>, rounded up: 5 for exactly 5 hours
    /// and for 4 hours and 1 second, 1 in the last hour, 0 at or after the end. <see cref="ForLabel"/> and the reports'
    /// "{N} h left to record" share this one rule.
    /// </summary>
    public static long HoursLeft(DateTimeOffset graceEnds, DateTimeOffset now)
    {
        var ticks = Math.Max((graceEnds - now).Ticks, 0L);
        return (ticks + TimeSpan.TicksPerHour - 1) / TimeSpan.TicksPerHour;
    }

    /// <summary>
    /// The RecordDialog's sub line, from <see cref="GoalHistory.Target"/>: "Reduce · " and the target text for a Reduce
    /// goal ("Reduce · at most 2 cups a day", "Reduce · avoided every day"); "Build · at least ", the amount, " · " and
    /// the schedule part for a Build count or duration goal ("Build · at least 20 min · every day"); "Build · " and
    /// the schedule part for a Build yes/no goal ("Build · Mon · Wed · Fri", "Build · 3 times a week").
    /// </summary>
    public static string TargetLine(GoalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        var target = history.Target;
        if (target.BuildOrReduce == BuildOrReduce.Reduce)
        {
            return "Reduce" + Separator + GoalText.TargetText(target);
        }

        return target.Measurement == Measurement.YesNo
            ? "Build" + Separator + SchedulePart(target)
            : "Build" + Separator + "at least " + GoalText.AmountText(target) + Separator + SchedulePart(target);
    }

    /// <summary>
    /// The partial-progress sentence of a Build count or duration goal ("Partial time is kept. Below 20 min the day
    /// counts as Failed when it is finalized."), by schedule kind; null for a yes/no or a Reduce goal.
    /// </summary>
    public static string? PartialNote(GoalHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        var target = history.Target;
        if (target.BuildOrReduce != BuildOrReduce.Build || target.Measurement == Measurement.YesNo)
        {
            return null;
        }

        var kept = target.Measurement == Measurement.Duration ? "Partial time is kept. " : "Partial amounts are kept. ";
        var amount = GoalText.AmountText(target);
        var schedule = target.Schedule;
        if (schedule.Kind == ScheduleKind.FixedDays)
        {
            return kept + "Below " + amount + " the day" + FailedWhenFinalized;
        }

        var times = schedule.Times ?? 1;
        var unit = schedule.Period == SchedulePeriod.Week ? " the week" : " the day";
        var rest = times == 1
            ? "Without a record of at least " + amount
            : "With fewer than " + Number(times) + " records of at least " + amount;
        return kept + rest + unit + FailedWhenFinalized;
    }

    /// <summary>
    /// "every day" for fixed days on all seven days and for 1 time a day; the day groups of
    /// <see cref="GoalText.ScheduleText"/> as they are for other fixed days; "N times a day"; "once a week" or
    /// "N times a week". Nothing is lower-cased: every part is written as it is.
    /// </summary>
    private static string SchedulePart(GoalTarget target)
    {
        var schedule = target.Schedule;
        if (schedule.Kind == ScheduleKind.FixedDays)
        {
            return schedule.Days.Count == 7 ? "every day" : GoalText.ScheduleText(target);
        }

        var times = schedule.Times ?? 1;
        if (schedule.Period == SchedulePeriod.Week)
        {
            return times == 1 ? "once a week" : Number(times) + " times a week";
        }

        return times == 1 ? "every day" : Number(times) + " times a day";
    }

    private static string DateOf(DateOnly first, DateOnly last) =>
        first == last ? DateTexts.ShortDate(first) : DateTexts.WeekRange(first, last);

    private static string Of(bool reduce) => reduce ? " of at most " : " of ";

    private static string Times(int count) => Number(count) + (count == 1 ? " time" : " times");

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
