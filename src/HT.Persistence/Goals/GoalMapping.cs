using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.Persistence.Habits;

namespace HT.Persistence.Goals;

/// <summary>
/// What the goal store's one read query returns per goal: the goal row, its versions and attempts, and the current
/// name, category and state of its habit from a left join on each habit table (research R-08).
/// </summary>
internal sealed class GoalReadRow
{
    public required GoalRow Goal { get; init; }

    public required List<GoalSettingsVersionRow> Versions { get; init; }

    public required List<GoalAttemptRow> Attempts { get; init; }

    public string? CatalogHabitName { get; init; }

    public Category? CatalogHabitCategory { get; init; }

    public CatalogHabitState? CatalogHabitState { get; init; }

    public string? CustomHabitName { get; init; }

    public Category? CustomHabitCategory { get; init; }
}

/// <summary>
/// The only mapping between goal rows and domain types (data-model.md). A stored target is rebuilt through
/// <see cref="GoalTarget.TryCreate"/> and a stored zone through <see cref="NamedTimeZone.TryParse"/>, and the goal
/// through its constructor, so a stored goal is checked like a new one. A row that fails a check is a fault whose
/// message carries no field value (HP-011). Days are a bit mask, Monday 1 to Sunday 64, as in
/// <see cref="HabitMapping"/>.
/// </summary>
internal static class GoalMapping
{
    /// <summary>The version row of a settings version of goal <paramref name="goalId"/>'s attempt 1.</summary>
    public static GoalSettingsVersionRow ToRow(Guid goalId, GoalSettingsVersion version) => ToRow(goalId, GoalAttempt.First, version);

    /// <summary>Slice 8: the version row of a settings version of attempt <paramref name="attemptNumber"/> of goal <paramref name="goalId"/>.</summary>
    public static GoalSettingsVersionRow ToRow(Guid goalId, int attemptNumber, GoalSettingsVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);
        var target = version.Settings.Target;
        return new GoalSettingsVersionRow
        {
            GoalId = goalId,
            AttemptNumber = attemptNumber,
            AppliesFrom = version.AppliesFrom,
            BuildOrReduce = target.BuildOrReduce,
            Measurement = target.Measurement,
            Amount = target.Amount,
            CountUnit = target.CountUnit,
            DurationUnit = target.DurationUnit,
            ScheduleKind = target.Schedule.Kind,
            Days = target.Schedule.Kind == ScheduleKind.FixedDays ? HabitMapping.DayMask(target.Schedule.Days) : null,
            Times = target.Schedule.Times,
            Period = target.Schedule.Period,
            TimeZone = version.Settings.TimeZone.Id,
        };
    }

    public static StoredGoal ToStoredGoal(GoalReadRow read)
    {
        ArgumentNullException.ThrowIfNull(read);
        var row = read.Goal;
        var habit = HabitOf(row);
        var view = habit.Kind == HabitKind.Catalog ? CatalogView(read, habit.Id) : CustomView(read, habit.Id);

        GoalName? ownName = null;
        if (row.Name is not null && (!GoalName.TryCreate(row.Name, out ownName, out _) || ownName is null))
        {
            throw new InvalidOperationException("A stored goal has a name that fails the name checks.");
        }

        var attempts = AttemptsOf(read);
        if (attempts[0].Versions[0].AppliesFrom != row.StartDate)
        {
            throw new InvalidOperationException("A stored goal's first version does not apply from its start date.");
        }

        Goal goal;
        try
        {
            goal = new Goal(new GoalId(row.Id), new AccountId(row.OwnerId), habit, ownName, attempts);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("A stored goal fails the goal invariants.");
        }

        return new StoredGoal(goal, view);
    }

    /// <summary>The stored ending of an attempt row; null while it is not ended. A row that fails a check is a fault naming no value (HP-010).</summary>
    private static GoalEnding? EndingOf(GoalAttemptRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        switch (row)
        {
            case { EndingDate: null, Outcome: null, EndingNote: null }:
                return null;
            case { EndingDate: { } date, Outcome: { } outcome } when Enum.IsDefined(outcome):
                EndingNote? note = null;
                if (row.EndingNote is not null && (!EndingNote.TryCreate(row.EndingNote, out note, out _) || note is null))
                {
                    throw new InvalidOperationException("A stored ending note fails the note checks.");
                }

                return new GoalEnding(date, outcome, note);
            default:
                throw new InvalidOperationException("A stored attempt has an incomplete ending.");
        }
    }

    /// <summary>
    /// Slice 8 (data-model.md "Tables"): the goal's attempts with their versions and endings. The numbers run 1, 2, …
    /// with no gap, every attempt has a version and every version an attempt, and every attempt but the latest is ended;
    /// the <see cref="Goal"/> constructor checks the rest (a later attempt's first version on its start date among
    /// them). Each failure is a fault that names no value (HP-010).
    /// </summary>
    private static List<GoalAttemptVersions> AttemptsOf(GoalReadRow read)
    {
        var rows = read.Attempts.OrderBy(a => a.Number).ToList();
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("A stored goal has no attempt.");
        }

        var attempts = new List<GoalAttemptVersions>(rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Number != i + GoalAttempt.First)
            {
                throw new InvalidOperationException("A stored goal's attempt numbers do not run 1, 2, … without a gap.");
            }

            var ending = EndingOf(row);
            if (ending is null && i < rows.Count - 1)
            {
                throw new InvalidOperationException("A stored goal has an attempt before its latest that is not ended.");
            }

            var versions = read.Versions.Where(v => v.AttemptNumber == row.Number).OrderBy(v => v.AppliesFrom).Select(ToVersion).ToList();
            if (versions.Count == 0)
            {
                throw new InvalidOperationException("A stored goal has an attempt without a settings version.");
            }

            attempts.Add(new GoalAttemptVersions(new GoalAttempt(row.Number, row.StartDate) { Ending = ending }, versions));
        }

        if (read.Versions.Any(v => v.AttemptNumber < GoalAttempt.First || v.AttemptNumber > rows.Count))
        {
            throw new InvalidOperationException("A stored goal has a settings version of no attempt.");
        }

        return attempts;
    }

    private static GoalHabit HabitOf(GoalRow row) => (row.CatalogHabitId, row.CustomHabitId) switch
    {
        ({ } catalog, null) => new GoalHabit(new HabitId(catalog), HabitKind.Catalog),
        (null, { } custom) => new GoalHabit(new HabitId(custom), HabitKind.Custom),
        _ => throw new InvalidOperationException("A stored goal must have exactly one habit."),
    };

    private static GoalHabitView CatalogView(GoalReadRow read, HabitId id) =>
        read is { CatalogHabitName: { } name, CatalogHabitCategory: { } category, CatalogHabitState: { } state }
            ? new GoalHabitView(id, HabitKind.Catalog, name, category, state == CatalogHabitState.Published)
            : throw new InvalidOperationException("A stored goal's catalog habit is missing.");

    private static GoalHabitView CustomView(GoalReadRow read, HabitId id) =>
        read is { CustomHabitName: { } name, CustomHabitCategory: { } category }
            ? new GoalHabitView(id, HabitKind.Custom, name, category, null)
            : throw new InvalidOperationException("A stored goal's custom habit is missing.");

    private static GoalSettingsVersion ToVersion(GoalSettingsVersionRow row)
    {
        var fields = new GoalTargetFields
        {
            BuildOrReduce = row.BuildOrReduce,
            Measurement = row.Measurement,
            Amount = row.Amount,
            CountUnit = row.CountUnit,
            DurationUnit = row.DurationUnit,
            ScheduleKind = row.ScheduleKind,
            Days = row.Days is { } mask ? HabitMapping.DaysOf(mask) : null,
            Times = row.Times,
            Period = row.Period,
        };
        if (!GoalTarget.TryCreate(fields, out var target, out _))
        {
            throw new InvalidOperationException("A stored goal has a target that fails the target checks.");
        }

        if (!NamedTimeZone.TryParse(row.TimeZone, out var zone))
        {
            throw new InvalidOperationException("A stored goal has a time zone that is not a named time zone.");
        }

        return new GoalSettingsVersion(new GoalSettings(target, zone), row.AppliesFrom);
    }
}
