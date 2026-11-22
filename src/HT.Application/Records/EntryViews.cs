using HT.Application.Goals;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// The entry evaluation that the history and the reports share (slice 6 research R-04): one entry as a
/// <see cref="HistoryEntry"/> at an instant, and the goal's settings as a <see cref="GoalHistory"/>. Moved from
/// <see cref="RecordingService"/> without a change of behaviour, so the goal page's history, what can be recorded now
/// and every report figure evaluate an entry the same way.
/// </summary>
internal static class EntryViews
{
    /// <summary>
    /// One entry as the history gives it at <paramref name="now"/> (FR-009, FR-010, FR-014, FR-015): evaluated with the
    /// target of the settings version in effect for its first date, its grace end only while it is Due or Pending, its
    /// records as views, its note, and whether a change now is a correction (its grace period has ended, FR-006).
    /// <paramref name="records"/> are the entry's records, in date order and then in the stored order.
    /// <para>
    /// Slice 7 (research R-06, FR-003, FR-007, FR-017): the entry's exemption comes from the goal's
    /// <paramref name="exemptions"/> (<see cref="GoalExemptions.ExemptionOf"/>, with the schedule kind of the entry's
    /// version). An Excused or Paused entry has that state, its records, progress and note as before, no grace end, and is
    /// never a correction; an Excused one also has its excusal's reason and whether the excusal can still be removed
    /// (<paramref name="now"/> before its grace end, FR-004).
    /// </para>
    /// <para>
    /// Slice 8 (research R-07, FR-016, FR-019): the entry carries the number of the attempt that contains it, whether
    /// that is the latest attempt, that attempt's name when it is not (<see cref="GoalText.AttemptName"/>, with the habit's
    /// current name), and the settings of its date's version (<see cref="HistoryEntry.Settings"/>).
    /// </para>
    /// </summary>
    internal static HistoryEntry Of(
        StoredGoal stored, CalendarEntry entry, IReadOnlyList<GoalRecord> records, NoteText? note, GoalExemptions exemptions, DateTimeOffset now)
    {
        var goal = stored.Goal;
        var target = TargetOf(goal, entry);
        var exemption = exemptions.ExemptionOf(entry, target.ScheduleKindOf);
        var evaluation = Evaluator.Evaluate(target, entry.Starts, entry.Ends, records.Select(record => record.Value).ToList(), now, exemption);
        var graceEnds = GracePeriod.EndOf(entry.Ends);
        var excused = exemption == EvaluationState.Excused;
        var number = AttemptEntries.NumberOf(goal, entry);
        var isLatest = number == goal.Attempt.Number;
        return new HistoryEntry
        {
            FirstDate = entry.FirstDate,
            LastDate = entry.LastDate,
            Starts = entry.Starts,
            Ends = entry.Ends,
            State = evaluation.State,
            GraceEnds = evaluation.State is EvaluationState.Due or EvaluationState.Pending ? evaluation.GraceEnds : null,
            Records = records.Select(record => new RecordView(record.Id, record.Date, record.Value.Answer, record.Value.Amount)).ToList(),
            Progress = evaluation.Progress,
            Note = note?.Value,
            IsCorrection = exemption is null && now >= graceEnds,
            ExcusalReason = excused ? exemptions.ExcusalOf(entry.Key)?.Reason?.Value : null,
            CanRemoveExcusal = excused && now < graceEnds,
            AttemptNumber = number,
            IsLatestAttempt = isLatest,
            AttemptName = isLatest ? null : GoalText.AttemptName(goal, number, stored.Habit.Name),
            Settings = EntrySettings.Of(target),
        };
    }

    /// <summary>
    /// The goal's settings as the pages need them to show <paramref name="entries"/> (FR-009, FR-011): the goal-level
    /// fields of <paramref name="version"/>, the goal's current date and the instant the clock was read. No goal or habit
    /// name. Slice 8 (research R-07, "The goal-level settings of an attempt's History"): every caller passes
    /// <see cref="GoalTimeline.VersionInEffect(Goal, int, DateTimeOffset)"/> of the attempt it shows, which for a goal
    /// with one attempt is the version of its current date, or its first version when it has none, as before.
    /// </summary>
    internal static GoalHistory HistoryOf(
        Goal goal, GoalSettingsVersion version, DateOnly? currentDate, IReadOnlyList<HistoryEntry> entries, DateTimeOffset now)
    {
        var target = version.Settings.Target;
        return new GoalHistory
        {
            GoalId = goal.Id,
            GoalScheduleKind = target.ScheduleKindOf,
            Period = target.PeriodLength,
            BuildOrReduce = target.BuildOrReduce,
            Measurement = target.Measurement,
            Amount = target.Amount,
            CountUnit = target.CountUnit,
            DurationUnit = target.DurationUnit,
            Times = target.ScheduleKindOf == GoalScheduleKind.PeriodBased ? target.Schedule.Times : null,
            CurrentDate = currentDate,
            Target = target,
            ReadAt = now,
            Entries = entries,
        };
    }

    /// <summary>The target of the settings version in effect for the entry's date (FR-015), in the attempt that contains it.</summary>
    internal static GoalTarget TargetOf(Goal goal, CalendarEntry entry) => GoalTimeline.VersionOf(goal, entry.FirstDate).Settings.Target;

    /// <summary>The records read with <paramref name="stored"/> that are dated in <paramref name="entry"/>, in their order.</summary>
    internal static List<GoalRecord> RecordsIn(StoredEntries stored, CalendarEntry entry) =>
        stored.Records.Where(record => record.Date >= entry.FirstDate && record.Date <= entry.LastDate).ToList();
}
