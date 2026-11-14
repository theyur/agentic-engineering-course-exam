using HT.Domain.Habits;

namespace HT.Application.Operator;

/// <summary>
/// The demo custom habits of FR-031: two per demo email, with no description. Demo loading creates them only
/// together with the demo account they belong to (ADR 0013).
/// </summary>
public static class DemoCustomHabits
{
    private static readonly Dictionary<string, IReadOnlyList<HabitDefinition>> ByEmail = new(StringComparer.Ordinal)
    {
        ["demo1@example.com"] =
        [
            Habit("Cold shower", Category.Body, Intent.Build, "droplet"),
            Habit("No phone in bed", Category.Home, Intent.Reduce, "smartphone"),
        ],
        ["demo2@example.com"] =
        [
            Habit("Evening stretch", Category.Body, Intent.Build, "person-standing"),
            Habit("Plan tomorrow", Category.Mind, Intent.Build, "pen-line"),
        ],
        ["demo3@example.com"] =
        [
            Habit("Late snacks", Category.Health, Intent.Reduce, "candy"),
            Habit("Water the plants", Category.Home, Intent.Build, "sprout"),
        ],
    };

    /// <summary>
    /// The demo custom habits of one of <see cref="DemoDataService.DemoEmails"/>, in normal form (lower case).
    /// Any other email is a programming error.
    /// </summary>
    public static IReadOnlyList<HabitDefinition> For(string demoEmail)
    {
        ArgumentNullException.ThrowIfNull(demoEmail);
        return ByEmail.TryGetValue(demoEmail, out var habits)
            ? habits
            : throw new ArgumentException("Not a demo email.", nameof(demoEmail));
    }

    private static HabitDefinition Habit(string name, Category category, Intent intent, string icon) =>
        HabitDefinition.TryCreate(name, null, category, intent, icon, out var definition, out _)
            ? definition
            : throw new InvalidOperationException("A demo custom habit fails the habit checks.");
}
