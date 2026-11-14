namespace HT.Domain.Habits;

/// <summary>One of the 24 habit icons of FR-003, by its Lucide name. Several habits may use the same icon.</summary>
public readonly record struct HabitIcon
{
    private HabitIcon(string name) => Name = name;

    /// <summary>The Lucide name, for example <c>book-open</c>.</summary>
    public string Name { get; }

    /// <summary>The habit icons in FR-003's order.</summary>
    public static IReadOnlyList<HabitIcon> All { get; } =
    [
        new("book-open"), new("person-standing"), new("smartphone"), new("footprints"), new("piggy-bank"),
        new("candy"), new("coffee"), new("moon"), new("dumbbell"), new("droplet"), new("bike"), new("brain"),
        new("pen-line"), new("languages"), new("music"), new("salad"), new("utensils"), new("cigarette-off"),
        new("wine-off"), new("wallet"), new("receipt"), new("sprout"), new("sparkles"), new("tv"),
    ];

    /// <summary>Accepts exactly one of the names of <see cref="All"/>, compared ordinally and case-sensitively.</summary>
    public static bool TryParse(string? name, out HabitIcon icon)
    {
        foreach (var candidate in All)
        {
            if (string.Equals(candidate.Name, name, StringComparison.Ordinal))
            {
                icon = candidate;
                return true;
            }
        }

        icon = default;
        return false;
    }

    public override string ToString() => Name ?? string.Empty;
}
