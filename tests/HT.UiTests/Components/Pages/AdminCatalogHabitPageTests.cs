using Bunit;
using HT.Domain.Habits;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>HP-009: the administrator's catalog habit page over the real service and the fake store.</summary>
public sealed class AdminCatalogHabitPageTests : BunitContext
{
    private readonly FakeCatalogHabitStore _store;

    public AdminCatalogHabitPageTests()
    {
        AddAuthorization().SignIn("Administrator", "admin@example.com");
        _store = this.AddCatalog();
    }

    [Fact]
    public void A_new_Id_on_the_open_page_loads_that_habit_and_no_Id_gives_an_empty_form()
    {
        var reading = _store.Add(Catalog("Reading", CatalogHabitState.Published,
            goals: Goals(0, Count(20, "pages", 3, SchedulePeriod.Week))));
        var sugar = _store.Add(Catalog("Sugar", CatalogHabitState.Draft, Category.Health, Intent.Reduce, "candy"));
        var cut = Render<AdminCatalogHabit>(p => p.Add(x => x.Id, reading.Id.Value));
        Assert.Equal("Reading", cut.Find("#catalog-habit-name").GetAttribute("value"));
        Assert.Single(cut.FindAll(".ht-editor-item"));

        cut.Render(p => p.Add(x => x.Id, sugar.Id.Value));

        Assert.Equal("Edit catalog habit", cut.Find("h1").TextContent);
        Assert.Equal("Sugar · Draft. Saving keeps its state.", cut.Find(".ht-page-head .ht-muted").TextContent);
        Assert.Equal("Sugar", cut.Find("#catalog-habit-name").GetAttribute("value"));
        Assert.Empty(cut.FindAll(".ht-editor-item"));

        cut.Render(p => p.Add(x => x.Id, (Guid?)null));

        Assert.Equal("New catalog habit", cut.Find("h1").TextContent);
        Assert.Equal("Created as a Draft. Publish it from the catalog table.", cut.Find(".ht-page-head .ht-muted").TextContent);
        Assert.True(string.IsNullOrEmpty(cut.Find("#catalog-habit-name").GetAttribute("value")));
    }
}
