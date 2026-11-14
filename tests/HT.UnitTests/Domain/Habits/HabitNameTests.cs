using HT.Domain.Habits;

namespace HT.UnitTests.Domain.Habits;

/// <summary>
/// FR-004 and research R-03: one key for uniqueness, order and search, invariant upper-casing compared ordinally.
/// Every test runs under tr-TR, where a culture-sensitive call would give a different result.
/// </summary>
public class HabitNameTests
{
    private static readonly string[] UnorderedNames = ["Zest", "b habit", "Ac2 walking", "Éclair", "A habit", "Ac2 Cycling", "Ac2 Breathing"];

    private static HabitName Name(string text)
    {
        Assert.True(HabitName.TryCreate(text, out var name, out var problem));
        Assert.Equal(HabitFieldProblem.None, problem);
        return name;
    }

    [Fact]
    public void Trims_the_name_and_two_spellings_share_one_key()
    {
        using var culture = new CultureScope("tr-TR");
        var spaced = Name(" ac17 reading ");
        var plain = Name("Ac17 Reading");

        Assert.Equal("ac17 reading", spaced.Value);
        Assert.Equal(spaced.Key, plain.Key);
        Assert.Equal(spaced, plain);
    }

    [Fact]
    public void Accepts_60_code_units_and_refuses_61_after_trimming()
    {
        using var culture = new CultureScope("tr-TR");
        Assert.True(HabitName.TryCreate("  " + new string('x', 60) + "  ", out var sixty, out _));
        Assert.Equal(60, sixty.Value.Length);

        Assert.False(HabitName.TryCreate(new string('x', 61), out _, out var problem));
        Assert.Equal(HabitFieldProblem.NameTooLong, problem);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    [InlineData(null)]
    public void A_blank_or_missing_name_is_missing(string? text)
    {
        using var culture = new CultureScope("tr-TR");
        Assert.False(HabitName.TryCreate(text, out _, out var problem));
        Assert.Equal(HabitFieldProblem.NameMissing, problem);
    }

    [Fact]
    public void Orders_by_key_ignoring_case_and_ordinally()
    {
        using var culture = new CultureScope("tr-TR");
        var names = UnorderedNames
            .Select(Name)
            .Order(HabitName.Order)
            .Select(n => n.Value)
            .ToList();

        Assert.Equal(["A habit", "Ac2 Breathing", "Ac2 Cycling", "Ac2 walking", "b habit", "Zest", "Éclair"], names);
    }

    [Fact]
    public void Accented_names_in_other_case_share_one_key()
    {
        using var culture = new CultureScope("tr-TR");
        Assert.Equal(Name("Éclair"), Name("éclair"));
    }

    [Fact]
    public void Long_s_and_S_share_one_key_and_sharp_s_keeps_its_key()
    {
        using var culture = new CultureScope("tr-TR");
        Assert.Equal(Name("ſun").Key, Name("Sun").Key);
        Assert.Equal("ß", Name("ß").Key);
    }

    [Fact]
    public void Turkish_letters_give_the_invariant_key_not_the_tr_TR_one()
    {
        using var culture = new CultureScope("tr-TR");
        Assert.Equal("AC2 WALKING", Name("Ac2 walking").Key);
        // Invariant upper-casing keeps the dotless ı; tr-TR would give I.
        Assert.Equal("ı", Name("ı").Key);
        Assert.Equal("I", "ı".ToUpper(System.Globalization.CultureInfo.GetCultureInfo("tr-TR")));
        Assert.Equal("İ", Name("İ").Key);
        Assert.Equal("AC2 WALKİNG", "Ac2 walking".ToUpper(System.Globalization.CultureInfo.GetCultureInfo("tr-TR")));
    }

    [Fact]
    public void Search_matches_the_trimmed_text_ignoring_case()
    {
        using var culture = new CultureScope("tr-TR");
        Assert.True(Name("Ac5 Morning pages").Matches(" morning "));
        Assert.False(Name("Ac5 Evening walk").Matches(" morning "));
        Assert.True(Name("Ac5 Walking").Matches("WALK"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void A_blank_or_missing_search_matches_everything(string? search)
    {
        using var culture = new CultureScope("tr-TR");
        Assert.True(Name("Ac5 Evening walk").Matches(search));
    }
}
