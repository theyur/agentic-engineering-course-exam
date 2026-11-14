namespace HT.Domain.Habits;

/// <summary>
/// A habit's name: the trimmed text, 1 to 60 UTF-16 code units (research R-03). Uniqueness, order and search all
/// use <see cref="Key"/>, the invariant upper-cased text compared ordinally, so no current culture is read
/// (FR-004, constitution II).
/// </summary>
public readonly record struct HabitName
{
    public const int MaxLength = 60;

    private HabitName(string value)
    {
        Value = value;
        Key = value.ToUpperInvariant();
    }

    /// <summary>The trimmed name as entered.</summary>
    public string Value { get; }

    /// <summary>The key that uniqueness, order and search compare: <c>Value.ToUpperInvariant()</c>.</summary>
    public string Key { get; }

    /// <summary>Name order: the keys compared with <see cref="string.CompareOrdinal(string, string)"/>.</summary>
    public static IComparer<HabitName> Order { get; } = new KeyOrder();

    /// <summary>Trims the text; empty is <see cref="HabitFieldProblem.NameMissing"/>, over 60 is <see cref="HabitFieldProblem.NameTooLong"/>.</summary>
    public static bool TryCreate(string? text, out HabitName name, out HabitFieldProblem problem)
    {
        name = default;
        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            problem = HabitFieldProblem.NameMissing;
            return false;
        }

        if (trimmed.Length > MaxLength)
        {
            problem = HabitFieldProblem.NameTooLong;
            return false;
        }

        name = new HabitName(trimmed);
        problem = HabitFieldProblem.None;
        return true;
    }

    /// <summary>The key of a text as a name would have it, for comparing a text with stored keys.</summary>
    public static string KeyOf(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// True for a missing or blank search; otherwise true when the key contains the trimmed search in its key
    /// form, compared ordinally (FR-009).
    /// </summary>
    public bool Matches(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return (Key ?? string.Empty).Contains(search.Trim().ToUpperInvariant(), StringComparison.Ordinal);
    }

    public bool Equals(HabitName other) => string.Equals(Key, other.Key, StringComparison.Ordinal);

    public override int GetHashCode() => Key is null ? 0 : StringComparer.Ordinal.GetHashCode(Key);

    public override string ToString() => Value ?? string.Empty;

    private sealed class KeyOrder : IComparer<HabitName>
    {
        public int Compare(HabitName x, HabitName y) => string.CompareOrdinal(x.Key, y.Key);
    }
}
