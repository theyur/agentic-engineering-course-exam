using HT.Domain.Habits;
using static HT.Domain.Habits.BuildOrReduce;

namespace HT.Application.Operator;

/// <summary>
/// The seed catalog (FR-030, research R-10): the catalog habits that demo loading creates in an empty catalog. 20
/// Published habits in all six categories, one Draft and one Unpublished, each with an icon of its own. The texts
/// follow design/brand-book.md: neutral descriptions, and suggested starting goals that are examples, never advice
/// (HP-014). The first suggested starting goal of a habit is the one its card shows.
/// </summary>
public static class SeedCatalog
{
    // Declared before Habits: static initializers run in text order.
    private static readonly Plan Daily = OnDays(
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
        DayOfWeek.Saturday, DayOfWeek.Sunday);

    private static readonly Plan Weekdays = OnDays(
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday);

    /// <summary>The 22 seed habits, grouped by category, then the Draft and the Unpublished habit.</summary>
    public static IReadOnlyList<SeedCatalogHabit> Habits { get; } =
    [
        // Body
        Published("Yoga", Category.Body, Intent.Build, "person-standing",
            "Any yoga practice, at home or in class.",
            YesNo(Build, TimesAWeek(3)),
            Minutes(Build, 20, OnDays(DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday))),
        Published("Walking", Category.Body, Intent.Build, "footprints",
            "Walks outside, at your own pace.",
            Minutes(Build, 30, Daily)),
        Published("Strength training", Category.Body, Intent.Build, "dumbbell",
            "Exercises with weights, bands or your own body weight.",
            YesNo(Build, OnDays(DayOfWeek.Tuesday, DayOfWeek.Thursday)),
            Minutes(Build, 20, TimesAWeek(2))),
        Published("Cycling", Category.Body, Intent.Build, "bike",
            "Rides on a bike or an exercise bike, to get somewhere or just for the ride.",
            Minutes(Build, 30, TimesAWeek(3)),
            Count(Build, 10, "km", TimesAWeek(2))),

        // Mind
        Published("Reading", Category.Mind, Intent.Build, "book-open",
            "Read books, articles or papers, on paper or on a screen. Count minutes or pages; both work.",
            Minutes(Build, 15, Weekdays),
            Count(Build, 20, "pages", TimesAWeek(3)),
            Count(Build, 10, "pages", OnDays(DayOfWeek.Saturday))),
        Published("Meditation", Category.Mind, Intent.Build, "brain",
            "Sit quietly for a few minutes, with or without a guide.",
            Minutes(Build, 10, Daily),
            Minutes(Build, 5, TimesADay(2))),
        Published("Journaling", Category.Mind, Intent.Build, "pen-line",
            "Write a few lines about your day, on paper or in an app.",
            YesNo(Build, Daily),
            Count(Build, 1, "page", TimesAWeek(3))),
        Published("Language practice", Category.Mind, Intent.Build, "languages",
            "Practice a language with an app, a book or another person.",
            Minutes(Build, 15, Daily),
            Count(Build, 10, "new words", Weekdays)),
        Published("Screen time", Category.Mind, Intent.Reduce, "smartphone",
            "Time on phone and social apps outside work.",
            Hours(Reduce, 2, TimesADay(1)),
            Minutes(Reduce, 90, TimesADay(1))),

        // Health
        Published("Sugar", Category.Health, Intent.Reduce, "candy",
            "Sweets and sugary drinks.",
            Count(Reduce, 1, "sweet", TimesADay(1))),
        Published("Drinking water", Category.Health, Intent.Build, "droplet",
            "Glasses of water through the day.",
            Count(Build, 6, "glasses", Daily)),
        Published("Regular bedtime", Category.Health, Intent.Build, "moon",
            "Go to bed at about the same time each night.",
            YesNo(Build, Daily),
            YesNo(Build, OnDays(DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday))),
        Published("Vegetables", Category.Health, Intent.Build, "salad",
            "Vegetables or salad as part of a meal.",
            YesNo(Build, TimesADay(1))),
        Published("Smoking", Category.Health, Intent.Reduce, "cigarette-off",
            "Cigarettes and other tobacco products.",
            Count(Reduce, 5, "cigarettes", TimesADay(1))),
        Published("Alcohol", Category.Health, Intent.Reduce, "wine-off",
            "Beer, wine and other alcoholic drinks.",
            YesNo(Reduce, TimesAWeek(2)),
            Count(Reduce, 1, "drink", TimesADay(1))),

        // Money
        Published("Saving money", Category.Money, Intent.Build, "piggy-bank",
            "Put a planned amount aside.",
            YesNo(Build, TimesAWeek(1))),
        Published("Tracking spending", Category.Money, Intent.Build, "wallet",
            "Write down what you spend, in a notebook or an app.",
            YesNo(Build, Daily),
            YesNo(Build, OnDays(DayOfWeek.Sunday))),

        // Home
        Published("Cooking at home", Category.Home, Intent.Build, "utensils",
            "Cook a meal at home instead of ordering in or eating out.",
            YesNo(Build, TimesAWeek(4))),
        Published("Tidying", Category.Home, Intent.Build, "sparkles",
            "A short tidy of one room or one surface."),

        // Other
        Published("Watching TV", Category.Other, Intent.Reduce, "tv",
            "Shows, films and series, on a TV or a streaming service.",
            Hours(Reduce, 1, TimesADay(1))),

        // Not public: one Draft and one Unpublished habit, for the administrator's list.
        Habit("Music practice", Category.Mind, Intent.Build, "music", CatalogHabitState.Draft,
            "Play an instrument or sing, alone or with others.",
            Minutes(Build, 20, TimesAWeek(4))),
        Habit("Coffee", Category.Health, Intent.Reduce, "coffee", CatalogHabitState.Unpublished,
            "Cups of coffee and other drinks with caffeine.",
            Count(Reduce, 2, "cups", TimesADay(1))),
    ];

    private static SeedCatalogHabit Published(
        string name, Category category, Intent intent, string icon, string description, params SuggestedStartingGoalFields[] goals) =>
        Habit(name, category, intent, icon, CatalogHabitState.Published, description, goals);

    /// <summary>Builds one seed habit through the Domain checks; the first suggested starting goal is the marked one.</summary>
    private static SeedCatalogHabit Habit(
        string name,
        Category category,
        Intent intent,
        string icon,
        CatalogHabitState state,
        string description,
        params SuggestedStartingGoalFields[] goals)
    {
        if (!HabitDefinition.TryCreate(name, description, category, intent, icon, out var definition, out var problem))
        {
            throw new InvalidOperationException($"The seed catalog habit {name} fails the habit checks: {problem}.");
        }

        var items = goals.Select((fields, index) => (fields, index == 0)).ToList();
        if (!SuggestedStartingGoals.TryCreate(items, out var suggestedStartingGoals, out var goalsProblem))
        {
            throw new InvalidOperationException($"The seed catalog habit {name} has suggested starting goals that fail their checks: {goalsProblem}.");
        }

        return new SeedCatalogHabit(definition, suggestedStartingGoals, state);
    }

    private static SuggestedStartingGoalFields YesNo(BuildOrReduce side, Plan plan) =>
        plan.Apply(new SuggestedStartingGoalFields { BuildOrReduce = side, Measurement = Measurement.YesNo });

    private static SuggestedStartingGoalFields Count(BuildOrReduce side, int amount, string unit, Plan plan) =>
        plan.Apply(new SuggestedStartingGoalFields
        {
            BuildOrReduce = side, Measurement = Measurement.Count, Amount = amount, CountUnit = unit,
        });

    private static SuggestedStartingGoalFields Minutes(BuildOrReduce side, int amount, Plan plan) =>
        Duration(side, amount, DurationUnit.Minutes, plan);

    private static SuggestedStartingGoalFields Hours(BuildOrReduce side, int amount, Plan plan) =>
        Duration(side, amount, DurationUnit.Hours, plan);

    private static SuggestedStartingGoalFields Duration(BuildOrReduce side, int amount, DurationUnit unit, Plan plan) =>
        plan.Apply(new SuggestedStartingGoalFields
        {
            BuildOrReduce = side, Measurement = Measurement.Duration, Amount = amount, DurationUnit = unit,
        });

    private static Plan OnDays(params DayOfWeek[] days) => new(ScheduleKind.FixedDays, days.ToHashSet(), null, null);

    private static Plan TimesAWeek(int times) => new(ScheduleKind.Times, null, times, SchedulePeriod.Week);

    private static Plan TimesADay(int times) => new(ScheduleKind.Times, null, times, SchedulePeriod.Day);

    /// <summary>The schedule part of a suggested starting goal: fixed days, or N times per day or per week.</summary>
    private sealed record Plan(ScheduleKind Kind, IReadOnlySet<DayOfWeek>? Days, int? Times, SchedulePeriod? Period)
    {
        public SuggestedStartingGoalFields Apply(SuggestedStartingGoalFields fields) =>
            fields with { ScheduleKind = Kind, Days = Days, Times = Times, Period = Period };
    }
}
