using HT.Application.Habits;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UnitTests.Application.Habits;

/// <summary>US2: a Published habit's details; everything else is "not in the catalog" (FR-010).</summary>
public sealed class CatalogDetailsTests
{
    private readonly FakeCatalogHabitStore _store = new();
    private CatalogService Service => new(_store);

    [Fact]
    public async Task US2_AS1_all_suggested_starting_goals_in_order_with_fields_mark_texts_and_kinds()
    {
        var reading = _store.Add(Catalog("Ac7 Reading", category: Category.Mind, icon: "book-open", description: "Books and articles.",
            goals: Goals(1, DurationDays(15, DurationUnit.Minutes, Weekdays), Count(20, "pages", 3, SchedulePeriod.Week), YesNo(1, SchedulePeriod.Day))));

        var result = await Service.GetDetailsAsync(reading.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitDetailsOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        var details = result.Details!;
        Assert.Equal("Ac7 Reading", details.Name);
        Assert.Equal(Category.Mind, details.Category);
        Assert.Equal(Intent.Build, details.Intent);
        Assert.Equal("book-open", details.Icon);
        Assert.Equal("Books and articles.", details.Description);
        Assert.Equal([1, 2, 3], details.SuggestedStartingGoals.Select(g => g.Position));
        Assert.Equal([false, true, false], details.SuggestedStartingGoals.Select(g => g.IsMarked));
        Assert.Equal(["15 min, 5 days a week", "20 pages, 3 times a week", "once a day"], details.SuggestedStartingGoals.Select(g => g.Text));
        Assert.Equal(["Duration · fixed days Mon–Fri", "Count · flexible", "Yes/no · flexible"], details.SuggestedStartingGoals.Select(g => g.Kind));
        var first = details.SuggestedStartingGoals[0];
        Assert.Equal(Measurement.Duration, first.Measurement);
        Assert.Equal(15, first.Amount);
        Assert.Equal(DurationUnit.Minutes, first.DurationUnit);
        Assert.Equal(ScheduleKind.FixedDays, first.ScheduleKind);
        Assert.Equal(Weekdays.ToHashSet(), first.Days.ToHashSet());
        Assert.Equal(2, details.MarkedSuggestedStartingGoal!.Position);
    }

    [Fact]
    public async Task US2_AS2_a_habit_without_suggested_starting_goals()
    {
        var walking = _store.Add(Catalog("Ac7b Walking"));

        var details = (await Service.GetDetailsAsync(walking.Id, CancellationToken.None)).Details!;

        Assert.Empty(details.SuggestedStartingGoals);
        Assert.Null(details.MarkedSuggestedStartingGoal);
    }

    [Fact]
    public async Task US2_AS3_texts_through_the_service()
    {
        var habit = _store.Add(Catalog("Ac7c Mixed", goals: Goals(0,
            DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce),
            Count(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce))));

        var goals = (await Service.GetDetailsAsync(habit.Id, CancellationToken.None)).Details!.SuggestedStartingGoals;

        Assert.Equal(["at most 2 h a day", "at most 1 sweet a day"], goals.Select(g => g.Text));
        Assert.Equal(["Duration · flexible", "Count · flexible"], goals.Select(g => g.Kind));
    }

    [Theory]
    [InlineData(CatalogHabitState.Draft)]
    [InlineData(CatalogHabitState.Unpublished)]
    public async Task US2_AS4_a_Draft_or_Unpublished_habit_is_not_in_the_catalog(CatalogHabitState state)
    {
        var habit = _store.Add(Catalog("Ac8 Hidden", state));

        var result = await Service.GetDetailsAsync(habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitDetailsOutcome.NotInCatalog, result.Outcome);
        Assert.Equal("This habit is not in the catalog.", result.Message);
        Assert.Equal(CatalogTexts.NotInCatalog, result.Message);
        Assert.Null(result.Details);
    }

    [Fact]
    public async Task US2_AS4_an_unknown_identifier_is_not_in_the_catalog()
    {
        var result = await Service.GetDetailsAsync(new HabitId(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(CatalogHabitDetailsOutcome.NotInCatalog, result.Outcome);
        Assert.Equal("This habit is not in the catalog.", result.Message);
    }
}
