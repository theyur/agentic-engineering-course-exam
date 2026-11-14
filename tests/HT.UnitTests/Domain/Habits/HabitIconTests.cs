using HT.Domain.Habits;

namespace HT.UnitTests.Domain.Habits;

/// <summary>FR-003: exactly the 24 habit icons, in order; any other name is not a habit icon.</summary>
public class HabitIconTests
{
    private static readonly string[] Fr003 =
    [
        "book-open", "person-standing", "smartphone", "footprints", "piggy-bank", "candy", "coffee", "moon",
        "dumbbell", "droplet", "bike", "brain", "pen-line", "languages", "music", "salad", "utensils",
        "cigarette-off", "wine-off", "wallet", "receipt", "sprout", "sparkles", "tv",
    ];

    [Fact]
    public void All_has_the_24_names_of_FR_003_in_order_without_duplicates()
    {
        Assert.Equal(Fr003, HabitIcon.All.Select(i => i.Name));
        Assert.Equal(24, HabitIcon.All.Select(i => i.Name).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Each_name_parses()
    {
        foreach (var name in Fr003)
        {
            Assert.True(HabitIcon.TryParse(name, out var icon));
            Assert.Equal(name, icon.Name);
        }
    }

    [Theory]
    [InlineData("menu")]
    [InlineData("trash-2")]
    [InlineData("Book-open")]
    [InlineData(" book-open")]
    [InlineData("")]
    [InlineData(null)]
    public void Any_other_name_fails(string? name)
    {
        Assert.False(HabitIcon.TryParse(name, out _));
    }
}
