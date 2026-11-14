using HT.Application.Operator;
using HT.Domain.Habits;

namespace HT.UnitTests.Application.Operator;

/// <summary>FR-030 and FR-031 as executed checks of the fixed demo data (research R-10, constitution V).</summary>
public sealed class SeedCatalogTests
{
    private static IReadOnlyList<SeedCatalogHabit> Habits => SeedCatalog.Habits;

    private static List<SeedCatalogHabit> Published => Habits.Where(h => h.State == CatalogHabitState.Published).ToList();

    private static List<SuggestedStartingGoal> AllGoals => Habits.SelectMany(h => h.SuggestedStartingGoals.Items).ToList();

    [Fact]
    public void FR_030_has_22_habits_20_Published_one_Draft_and_one_Unpublished()
    {
        Assert.Equal(22, Habits.Count);
        Assert.Equal(20, Published.Count);
        Assert.Single(Habits, h => h.State == CatalogHabitState.Draft);
        Assert.Single(Habits, h => h.State == CatalogHabitState.Unpublished);
    }

    [Fact]
    public void FR_030_the_Published_habits_cover_all_six_categories()
    {
        Assert.Equal(Enum.GetValues<Category>(), Published.Select(h => h.Definition.Category).Distinct().Order());
    }

    [Fact]
    public void FR_030_at_least_4_Published_habits_are_Reduce()
    {
        Assert.True(Published.Count(h => h.Definition.Intent == Intent.Reduce) >= 4);
    }

    [Fact]
    public void FR_030_every_habit_has_a_description()
    {
        Assert.All(Habits, h => Assert.False(string.IsNullOrWhiteSpace(h.Definition.Description)));
    }

    [Fact]
    public void FR_030_every_habit_has_an_icon_no_other_seed_habit_has()
    {
        var icons = Habits.Select(h => h.Definition.Icon).ToList();

        Assert.Equal(22, icons.Distinct().Count());
        Assert.All(icons, icon => Assert.Contains(icon, HabitIcon.All));
    }

    [Fact]
    public void FR_004_every_habit_has_a_name_key_of_its_own()
    {
        Assert.Equal(Habits.Count, Habits.Select(h => h.Definition.Name.Key).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void FR_030_the_suggested_starting_goals_use_all_three_measurements_and_both_schedule_kinds()
    {
        Assert.Equal(Enum.GetValues<Measurement>(), AllGoals.Select(g => g.Measurement).Distinct().Order());
        Assert.Equal(Enum.GetValues<ScheduleKind>(), AllGoals.Select(g => g.Schedule.Kind).Distinct().Order());
    }

    [Fact]
    public void FR_030_at_least_one_Published_habit_has_no_suggested_starting_goal_and_one_has_three_or_more()
    {
        Assert.Contains(Published, h => h.SuggestedStartingGoals.Count == 0);
        Assert.Contains(Published, h => h.SuggestedStartingGoals.Count >= 3);
    }

    [Fact]
    public void Every_habit_passes_the_Domain_checks_again_from_its_own_values()
    {
        Assert.All(Habits, habit =>
        {
            var definition = habit.Definition;
            Assert.True(HabitDefinition.TryCreate(
                definition.Name.Value, definition.Description, definition.Category, definition.Intent, definition.Icon.Name,
                out var again, out var problem), $"{definition.Name.Value}: {problem}");
            Assert.Equal(definition.Name, again.Name);
            Assert.Equal(definition.Description, again.Description);

            var goals = habit.SuggestedStartingGoals;
            var items = goals.Items.Select((goal, index) => (Fields(goal), index == goals.MarkedIndex)).ToList();
            Assert.True(SuggestedStartingGoals.TryCreate(items, out var goalsAgain, out var goalsProblem), $"{definition.Name.Value}: {goalsProblem}");
            Assert.Equal(goals.Count, goalsAgain.Count);
            Assert.Equal(goals.MarkedIndex, goalsAgain.MarkedIndex);
            Assert.Equal(
                goals.Items.Select(g => SuggestedStartingGoalText.Format(g)),
                goalsAgain.Items.Select(g => SuggestedStartingGoalText.Format(g)));
        });
    }

    [Fact]
    public void HP_014_names_and_descriptions_follow_the_brand_book_no_exclamation_marks()
    {
        Assert.All(Habits, h =>
        {
            Assert.DoesNotContain("!", h.Definition.Name.Value, StringComparison.Ordinal);
            Assert.DoesNotContain("!", h.Definition.Description ?? string.Empty, StringComparison.Ordinal);
        });
    }

    [Theory]
    [InlineData("demo1@example.com", "Cold shower", Category.Body, Intent.Build, "droplet", "No phone in bed", Category.Home, Intent.Reduce, "smartphone")]
    [InlineData("demo2@example.com", "Evening stretch", Category.Body, Intent.Build, "person-standing", "Plan tomorrow", Category.Mind, Intent.Build, "pen-line")]
    [InlineData("demo3@example.com", "Late snacks", Category.Health, Intent.Reduce, "candy", "Water the plants", Category.Home, Intent.Build, "sprout")]
    public void FR_031_each_demo_email_has_its_two_demo_custom_habits_without_a_description(
        string email,
        string firstName, Category firstCategory, Intent firstIntent, string firstIcon,
        string secondName, Category secondCategory, Intent secondIntent, string secondIcon)
    {
        var habits = DemoCustomHabits.For(email);

        var expected = new (string Name, Category Category, Intent Intent, string Icon, string? Description)[]
        {
            (firstName, firstCategory, firstIntent, firstIcon, null),
            (secondName, secondCategory, secondIntent, secondIcon, null),
        };
        Assert.Equal(expected, habits.Select(h => (h.Name.Value, h.Category, h.Intent, h.Icon.Name, h.Description)));
    }

    [Fact]
    public void FR_031_every_demo_email_has_demo_custom_habits_and_no_other_email_has()
    {
        Assert.All(DemoDataService.DemoEmails, email => Assert.Equal(2, DemoCustomHabits.For(email).Count));
        Assert.Throws<ArgumentException>(() => DemoCustomHabits.For("demo4@example.com"));
    }

    private static SuggestedStartingGoalFields Fields(SuggestedStartingGoal goal) => new()
    {
        BuildOrReduce = goal.BuildOrReduce,
        Measurement = goal.Measurement,
        Amount = goal.Amount,
        CountUnit = goal.CountUnit,
        DurationUnit = goal.DurationUnit,
        ScheduleKind = goal.Schedule.Kind,
        Days = goal.Schedule.Kind == ScheduleKind.FixedDays ? goal.Schedule.Days : null,
        Times = goal.Schedule.Times,
        Period = goal.Schedule.Period,
    };
}
