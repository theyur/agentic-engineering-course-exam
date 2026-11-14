using HT.Domain.Habits;

namespace HT.UnitTests.Domain.Habits;

/// <summary>FR-005: each field problem, the check order, and both sides of every length limit.</summary>
public class HabitDefinitionTests
{
    private static HabitFieldProblem Problem(
        string? name = "Reading", string? description = null, Category? category = Category.Mind,
        Intent? intent = Intent.Build, string? icon = "book-open")
    {
        Assert.False(HabitDefinition.TryCreate(name, description, category, intent, icon, out var definition, out var problem));
        Assert.Null(definition);
        return problem;
    }

    [Fact]
    public void Accepts_complete_fields_and_trims_them()
    {
        Assert.True(HabitDefinition.TryCreate("  Reading ", "  Books and articles. ", Category.Mind, Intent.Build, "book-open", out var definition, out var problem));
        Assert.Equal(HabitFieldProblem.None, problem);
        Assert.Equal("Reading", definition.Name.Value);
        Assert.Equal("Books and articles.", definition.Description);
        Assert.Equal(Category.Mind, definition.Category);
        Assert.Equal(Intent.Build, definition.Intent);
        Assert.Equal("book-open", definition.Icon.Name);
    }

    [Fact]
    public void Each_problem_alone()
    {
        Assert.Equal(HabitFieldProblem.NameMissing, Problem(name: "   "));
        Assert.Equal(HabitFieldProblem.NameTooLong, Problem(name: new string('x', 61)));
        Assert.Equal(HabitFieldProblem.DescriptionTooLong, Problem(description: new string('t', 501)));
        Assert.Equal(HabitFieldProblem.CategoryMissing, Problem(category: null));
        Assert.Equal(HabitFieldProblem.IntentMissing, Problem(intent: null));
        Assert.Equal(HabitFieldProblem.IconMissing, Problem(icon: null));
    }

    [Fact]
    public void Gives_the_first_problem_in_FR_005_order()
    {
        Assert.Equal(HabitFieldProblem.NameMissing, Problem(name: " ", icon: null));
        Assert.Equal(HabitFieldProblem.NameTooLong, Problem(name: new string('x', 61), description: new string('t', 501)));
        Assert.Equal(HabitFieldProblem.DescriptionTooLong, Problem(description: new string('t', 501), category: null));
        Assert.Equal(HabitFieldProblem.CategoryMissing, Problem(category: null, intent: null, icon: null));
        Assert.Equal(HabitFieldProblem.IntentMissing, Problem(intent: null, icon: "menu"));
    }

    [Fact]
    public void Accepts_a_description_of_500_and_refuses_501_after_trimming()
    {
        Assert.True(HabitDefinition.TryCreate("Reading", "  " + new string('t', 500) + "  ", Category.Mind, Intent.Build, "book-open", out var definition, out _));
        Assert.Equal(500, definition.Description!.Length);
        Assert.Equal(HabitFieldProblem.DescriptionTooLong, Problem(description: new string('t', 501)));
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    [InlineData(null)]
    public void A_blank_description_is_no_description(string? description)
    {
        Assert.True(HabitDefinition.TryCreate("Reading", description, Category.Mind, Intent.Build, "book-open", out var definition, out _));
        Assert.Null(definition.Description);
    }

    [Theory]
    [InlineData("menu")]
    [InlineData("trash-2")]
    [InlineData("Book-open")]
    [InlineData("")]
    public void An_icon_not_in_FR_003_is_missing(string icon)
    {
        Assert.Equal(HabitFieldProblem.IconMissing, Problem(icon: icon));
    }

    [Fact]
    public void An_undefined_category_or_intent_is_missing()
    {
        Assert.Equal(HabitFieldProblem.CategoryMissing, Problem(category: (Category)99));
        Assert.Equal(HabitFieldProblem.IntentMissing, Problem(intent: (Intent)99));
    }
}
