using HT.Domain.Habits;

namespace HT.UnitTests.Domain.Habits;

/// <summary>FR-011: the six cells of the transition table.</summary>
public class CatalogHabitStatesTests
{
    [Theory]
    [InlineData(CatalogHabitState.Draft)]
    [InlineData(CatalogHabitState.Published)]
    [InlineData(CatalogHabitState.Unpublished)]
    public void Publish_always_gives_Published(CatalogHabitState state)
    {
        var transition = CatalogHabitStates.Publish(state);
        Assert.False(transition.IsNotPublished);
        Assert.Equal(CatalogHabitState.Published, transition.Next);
    }

    [Fact]
    public void Unpublish_of_a_Draft_is_refused_as_not_published()
    {
        var transition = CatalogHabitStates.Unpublish(CatalogHabitState.Draft);
        Assert.True(transition.IsNotPublished);
        Assert.Null(transition.Next);
    }

    [Theory]
    [InlineData(CatalogHabitState.Published)]
    [InlineData(CatalogHabitState.Unpublished)]
    public void Unpublish_of_a_Published_or_Unpublished_habit_gives_Unpublished(CatalogHabitState state)
    {
        var transition = CatalogHabitStates.Unpublish(state);
        Assert.False(transition.IsNotPublished);
        Assert.Equal(CatalogHabitState.Unpublished, transition.Next);
    }
}
