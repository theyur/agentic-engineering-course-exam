using HT.Domain.Goals;

namespace HT.UnitTests.Domain.Goals;

/// <summary>FR-009: a goal's own name is trimmed, 1 to 60 code units, and empty means none.</summary>
public class GoalNameTests
{
    [Fact]
    public void The_name_is_trimmed()
    {
        Assert.True(GoalName.TryCreate("  Morning pages  ", out var name, out var tooLong));
        Assert.False(tooLong);
        Assert.Equal("Morning pages", name!.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Spaces_only_is_no_name(string? text)
    {
        Assert.True(GoalName.TryCreate(text, out var name, out var tooLong));
        Assert.False(tooLong);
        Assert.Null(name);
    }

    [Fact]
    public void Sixty_code_units_are_accepted()
    {
        var sixty = new string('g', GoalName.MaxLength);

        Assert.True(GoalName.TryCreate(" " + sixty + " ", out var name, out var tooLong));
        Assert.False(tooLong);
        Assert.Equal(sixty, name!.Value.Value);
    }

    [Fact]
    public void Sixty_one_code_units_are_too_long()
    {
        Assert.False(GoalName.TryCreate(new string('g', 61), out var name, out var tooLong));
        Assert.True(tooLong);
        Assert.Null(name);
    }
}
