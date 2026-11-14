using HT.Application.Habits;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UnitTests.Application.Habits;

/// <summary>US5: the Home preview is the first four habits of browsing with no filters, and the Published count (FR-008).</summary>
public sealed class HomePreviewTests
{
    private readonly FakeCatalogHabitStore _store = new();
    private CatalogService Service => new(_store);

    /// <summary>Adds Published habits with the given names, plus a Draft and an Unpublished one that sort first.</summary>
    private void Arrange(params string[] publishedNames)
    {
        _store.Add(Catalog("Hp0 Draft", CatalogHabitState.Draft));
        _store.Add(Catalog("Hp0 Unpublished", CatalogHabitState.Unpublished));
        foreach (var name in publishedNames)
        {
            _store.Add(Catalog(name));
        }
    }

    [Fact]
    public async Task No_Published_habit_gives_an_empty_preview_and_a_count_of_zero()
    {
        Arrange();

        var result = await Service.GetHomePreviewAsync(CancellationToken.None);

        Assert.Equal(HomePreviewOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        Assert.Empty(result.Habits);
        Assert.Equal(0, result.PublishedCount);
    }

    [Fact]
    public async Task Fewer_than_four_Published_habits_gives_all_of_them_in_name_order()
    {
        Arrange("Hp3 Walking", "Hp3 Breathing", "Hp3 cycling");

        var result = await Service.GetHomePreviewAsync(CancellationToken.None);

        Assert.Equal(["Hp3 Breathing", "Hp3 cycling", "Hp3 Walking"], result.Habits.Select(h => h.Name));
        Assert.Equal(3, result.PublishedCount);
    }

    [Fact]
    public async Task Four_Published_habits_gives_all_four()
    {
        Arrange("Hp4 Yoga", "Hp4 Reading", "Hp4 Journaling", "Hp4 Running");

        var result = await Service.GetHomePreviewAsync(CancellationToken.None);

        Assert.Equal(["Hp4 Journaling", "Hp4 Reading", "Hp4 Running", "Hp4 Yoga"], result.Habits.Select(h => h.Name));
        Assert.Equal(4, result.PublishedCount);
    }

    [Fact]
    public async Task Five_Published_habits_gives_the_first_four_of_browsing_and_counts_all_five()
    {
        Arrange("Hp5 Yoga", "Hp5 Reading", "Hp5 Journaling", "Hp5 Running", "Hp5 Cycling");

        var result = await Service.GetHomePreviewAsync(CancellationToken.None);
        var browsing = await Service.BrowseAsync(new CatalogFilter(), CancellationToken.None);

        Assert.Equal(["Hp5 Cycling", "Hp5 Journaling", "Hp5 Reading", "Hp5 Running"], result.Habits.Select(h => h.Name));
        Assert.Equal(browsing.Habits.Take(4), result.Habits);
        Assert.Equal(5, result.PublishedCount);
        Assert.DoesNotContain(result.Habits, h => h.Name is "Hp0 Draft" or "Hp0 Unpublished");
    }
}
