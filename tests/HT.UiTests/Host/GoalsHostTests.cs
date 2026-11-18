using System.Net;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>
/// contracts/host.md "Routes", HP-001, FR-021: who reaches the goal pages through the real host. US2 adds
/// <c>/goals</c> and <c>/goals/{id}</c>.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class GoalsHostTests(HostFixture host)
{
    private const string NewGoalAddress = "/goals/new";

    private static string Email(string who) => $"ui-go-{who}-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task A_signed_out_visitor_is_sent_to_sign_in_from_the_New_goal_page()
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri(NewGoalAddress, UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=" + Uri.EscapeDataString(NewGoalAddress), response.RedirectTarget(browser));
        Assert.DoesNotContain("<h1>New goal</h1>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_administrator_gets_the_Not_found_page_for_the_New_goal_page()
    {
        using var browser = await host.SignedInBrowserAsync(Email("admin"), Role.Administrator);

        var (status, html) = await browser.GetPageAsync(NewGoalAddress);

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<h1>New goal</h1>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Create goal", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_registered_user_gets_the_New_goal_page()
    {
        using var browser = await host.SignedInBrowserAsync(Email("user"), Role.RegisteredUser);

        var (status, html) = await browser.GetPageAsync(NewGoalAddress);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h1>New goal</h1>", html, StringComparison.Ordinal);
        Assert.Contains("Start from a catalog habit or one of your custom habits.", html, StringComparison.Ordinal);
        Assert.Contains(">Choose a habit</button>", html, StringComparison.Ordinal);
        Assert.Contains("Complete the form to see your goal here.", html, StringComparison.Ordinal);
        Assert.Contains(">Create goal</button>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_registered_user_gets_the_New_goal_page_from_a_catalog_habit()
    {
        var name = $"Ui-Go Reading {Guid.NewGuid():N}"[..30];
        var habit = await host.ArrangeCatalogHabitAsync(Definition(name), goals: HabitSamples.Goals(0, DurationDays(15, DurationUnit.Minutes, Weekdays)));
        using var browser = await host.SignedInBrowserAsync(Email("habit"), Role.RegisteredUser);

        var (status, html) = await browser.GetPageAsync($"{NewGoalAddress}?habit={habit.Value}&kind=catalog");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"Based on the catalog habit {name}. Change anything you like.", html, StringComparison.Ordinal);
        Assert.Contains($"href=\"/catalog/{habit.Value}\"", html, StringComparison.Ordinal);
        Assert.Contains($"{name}: at least 15 min, 5 days a week, starting ", html, StringComparison.Ordinal);
    }
}
