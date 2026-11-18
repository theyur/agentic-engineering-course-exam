using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Operator;

/// <summary>
/// The demo goals of FR-040 (research R-10): two or three per demo email, on catalog habits of the seed catalog and on
/// the account's own demo custom habits (<see cref="DemoCustomHabits"/>), never on "Cold shower", "Evening stretch" or
/// "Plan tomorrow". Demo loading writes them only together with the demo account they belong to (ADR 0013). Pure: the
/// start dates depend only on the day passed in.
/// </summary>
public static class DemoGoals
{
    private static readonly CatalogHabitState[] Published = [CatalogHabitState.Published];

    // "Coffee" is loaded Unpublished by the seed catalog, so that a goal on an Unpublished habit can be seen (FR-034).
    private static readonly CatalogHabitState[] PublishedOrUnpublished = [CatalogHabitState.Published, CatalogHabitState.Unpublished];

    private static readonly DayOfWeek[] Weekdays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    private static readonly DayOfWeek[] EveryDay = [.. Weekdays, DayOfWeek.Saturday, DayOfWeek.Sunday];

    private static readonly Dictionary<string, IReadOnlyList<Spec>> ByEmail = new(StringComparer.Ordinal)
    {
        ["demo1@example.com"] =
        [
            Catalog("Reading", Published, Duration(BuildOrReduce.Build, 15, OnDays(Weekdays))),
            Custom("demo1@example.com", "No phone in bed", YesNo(BuildOrReduce.Reduce, OnDays(EveryDay))),
        ],
        ["demo2@example.com"] =
        [
            Catalog("Yoga", Published, YesNo(BuildOrReduce.Build, TimesA(3, SchedulePeriod.Week)), ownName: "Morning yoga"),
            Catalog("Meditation", Published, Duration(BuildOrReduce.Build, 10, OnDays(DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday)), startsAfterDays: 7),
        ],
        ["demo3@example.com"] =
        [
            Catalog("Sugar", Published, Count(BuildOrReduce.Reduce, 1, "sweet", TimesA(1, SchedulePeriod.Day))),
            Custom("demo3@example.com", "Late snacks", YesNo(BuildOrReduce.Reduce, TimesA(1, SchedulePeriod.Day))),
            Catalog("Coffee", PublishedOrUnpublished, Count(BuildOrReduce.Reduce, 2, "cups", TimesA(1, SchedulePeriod.Day))),
        ],
    };

    /// <summary>
    /// The demo goals of one of <see cref="DemoDataService.DemoEmails"/>, in normal form (lower case), in creation
    /// order. A goal starts on <paramref name="today"/>, the day of the run in the installation's default time zone,
    /// except Meditation, which starts 7 days later. Any other email is a programming error.
    /// </summary>
    public static IReadOnlyList<DemoGoal> For(string demoEmail, DateOnly today) =>
        [.. SpecsOf(demoEmail).Select(spec => spec.ToGoal(today))];

    /// <summary>
    /// The names the Goals page shows for the demo goals of one demo email, in creation order (FR-010): the goal's own
    /// name, or the habit's name, ", " and the target text. The smoke check expects them (HP-018).
    /// </summary>
    public static IReadOnlyList<string> DisplayNames(string demoEmail) =>
        [.. SpecsOf(demoEmail).Select(spec => spec.OwnName?.Value ?? spec.HabitName + ", " + GoalText.TargetText(spec.Target))];

    private static IReadOnlyList<Spec> SpecsOf(string demoEmail)
    {
        ArgumentNullException.ThrowIfNull(demoEmail);
        return ByEmail.TryGetValue(demoEmail, out var specs)
            ? specs
            : throw new ArgumentException("Not a demo email.", nameof(demoEmail));
    }

    /// <summary>A goal on a seed catalog habit; the name is the seed habit's own.</summary>
    private static Spec Catalog(
        string habitName, CatalogHabitState[] states, GoalTargetFields fields, string? ownName = null, int startsAfterDays = 0)
    {
        var key = HabitName.KeyOf(habitName);
        var seed = SeedCatalog.Habits.SingleOrDefault(h => h.Definition.Name.Key == key)
            ?? throw new InvalidOperationException("A demo goal names a habit that is not in the seed catalog.");
        return new Spec(seed.Definition.Name.Value, null, states, TargetOf(fields), NameOf(ownName), startsAfterDays);
    }

    /// <summary>A goal on one of the account's demo custom habits, found by its position in <see cref="DemoCustomHabits.For"/>.</summary>
    private static Spec Custom(string demoEmail, string habitName, GoalTargetFields fields)
    {
        var key = HabitName.KeyOf(habitName);
        var habits = DemoCustomHabits.For(demoEmail);
        var position = habits.Select((habit, index) => (habit, index)).Single(h => h.habit.Name.Key == key).index;
        return new Spec(habits[position].Name.Value, position, [], TargetOf(fields), null, 0);
    }

    private static GoalTarget TargetOf(GoalTargetFields fields) =>
        GoalTarget.TryCreate(fields, out var target, out _)
            ? target
            : throw new InvalidOperationException("A demo goal fails the target checks.");

    private static GoalName? NameOf(string? text) =>
        GoalName.TryCreate(text, out var name, out _)
            ? name
            : throw new InvalidOperationException("A demo goal has a name that fails the name checks.");

    private static GoalTargetFields YesNo(BuildOrReduce side, GoalTargetFields schedule) =>
        schedule with { BuildOrReduce = side, Measurement = Measurement.YesNo };

    private static GoalTargetFields Count(BuildOrReduce side, int amount, string unit, GoalTargetFields schedule) =>
        schedule with { BuildOrReduce = side, Measurement = Measurement.Count, Amount = amount, CountUnit = unit };

    private static GoalTargetFields Duration(BuildOrReduce side, int minutes, GoalTargetFields schedule) =>
        schedule with
        {
            BuildOrReduce = side, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        };

    private static GoalTargetFields OnDays(params DayOfWeek[] days) =>
        new() { ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet() };

    private static GoalTargetFields TimesA(int times, SchedulePeriod period) =>
        new() { ScheduleKind = ScheduleKind.Times, Times = times, Period = period };

    /// <summary>One demo goal before its start date is known: the habit (a catalog name, or a custom-habit position) and its settings.</summary>
    private sealed record Spec(
        string HabitName, int? CustomHabitPosition, CatalogHabitState[] States, GoalTarget Target, GoalName? OwnName, int StartsAfterDays)
    {
        public DemoGoal ToGoal(DateOnly today) => CustomHabitPosition is { } position
            ? DemoGoal.OnCustomHabit(position, Target, OwnName, today.AddDays(StartsAfterDays))
            : DemoGoal.OnCatalogHabit(HabitName, States, Target, OwnName, today.AddDays(StartsAfterDays));
    }
}
