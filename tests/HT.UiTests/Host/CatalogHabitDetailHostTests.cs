using System.Net;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>US2-AS8, HP-006: the detail page and its Not found page through the real host, for every audience.</summary>
[Collection(HostCollection.Name)]
public sealed class CatalogHabitDetailHostTests(HostFixture host)
{
    private static readonly SemaphoreSlim Arranged = new(1, 1);
    private static (HabitId Published, HabitId Draft, HabitId Unpublished)? _habits;

    private async Task<(HabitId Published, HabitId Draft, HabitId Unpublished)> HabitsAsync()
    {
        await Arranged.WaitAsync();
        try
        {
            _habits ??= (
                await host.ArrangeCatalogHabitAsync(Definition("Ui-De Reading", description: "Books and articles."), CatalogHabitState.Published,
                    Goals(0, Count(20, "pages", 3, SchedulePeriod.Week))),
                await host.ArrangeCatalogHabitAsync(Definition("Ui-De Draft habit"), CatalogHabitState.Draft),
                await host.ArrangeCatalogHabitAsync(Definition("Ui-De Old habit"), CatalogHabitState.Unpublished));
            return _habits.Value;
        }
        finally
        {
            Arranged.Release();
        }
    }

    private async Task<HttpClient> BrowserAsync(string audience) => audience switch
    {
        "visitor" => host.CreateBrowser(),
        "user" => await host.SignedInBrowserAsync($"ui-de-user-{Guid.NewGuid():N}@example.com", Role.RegisteredUser),
        _ => await host.SignedInBrowserAsync($"ui-de-admin-{Guid.NewGuid():N}@example.com", Role.Administrator),
    };

    [Fact]
    public async Task A_Published_habit_answers_200_with_its_content()
    {
        var habits = await HabitsAsync();
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync($"/catalog/{habits.Published.Value}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h1>Ui-De Reading</h1>", html, StringComparison.Ordinal);
        Assert.Contains("Books and articles.", html, StringComparison.Ordinal);
        Assert.Contains("20 pages, 3 times a week", html, StringComparison.Ordinal);
        Assert.Contains("Register to use this habit", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("visitor")]
    [InlineData("user")]
    [InlineData("admin")]
    public async Task US2_AS8_a_Draft_an_Unpublished_and_an_unknown_habit_show_the_Not_found_page(string audience)
    {
        var habits = await HabitsAsync();
        using var browser = await BrowserAsync(audience);

        foreach (var id in new[] { habits.Draft.Value, habits.Unpublished.Value, Guid.NewGuid() })
        {
            var (status, html) = await browser.GetPageAsync($"/catalog/{id}");

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Ui-De", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task An_address_that_is_no_identifier_is_not_found()
    {
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync("/catalog/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
    }
}
