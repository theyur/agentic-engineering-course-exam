using System.Net;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>US4-AS15, HP-007, HP-015: who reaches My custom habits and the custom habit page, through the real host.</summary>
[Collection(HostCollection.Name)]
public sealed class CustomHabitHostTests(HostFixture host)
{
    private const string HabitName = "Ui-Ch Cold shower";

    public static TheoryData<string> Addresses => new("/custom-habits", "/custom-habits/new", "/custom-habits/{id}");

    private static string Email(string who) => $"ui-ch-{who}-{Guid.NewGuid():N}@example.com";

    /// <summary>The address, with the identifier of a habit of a new owner for the edit address.</summary>
    private async Task<string> AddressAsync(string template)
    {
        if (!template.Contains("{id}", StringComparison.Ordinal))
        {
            return template;
        }

        var owner = await host.CreateAccountAsync(Email("owner"), "walking42");
        var id = await host.ArrangeCustomHabitAsync(owner.Id, Definition(HabitName, Category.Body));
        return template.Replace("{id}", id.Value.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Addresses))]
    public async Task US4_AS15_an_anonymous_visitor_is_sent_to_sign_in(string template)
    {
        var address = await AddressAsync(template);
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri(address, UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=" + Uri.EscapeDataString(address), response.RedirectTarget(browser));
        Assert.DoesNotContain(HabitName, html, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Addresses))]
    public async Task US4_AS15_an_administrator_gets_the_Not_found_page(string template)
    {
        var address = await AddressAsync(template);
        using var browser = await host.SignedInBrowserAsync(Email("admin"), Role.Administrator);

        var (status, html) = await browser.GetPageAsync(address);

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
        Assert.DoesNotContain(HabitName, html, StringComparison.Ordinal);
        Assert.DoesNotContain("custom habit</h1>", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("another user's")]
    [InlineData("deleted")]
    [InlineData("unknown")]
    public async Task US4_AS15_a_habit_that_is_not_the_owners_gives_the_Not_found_page(string which)
    {
        var signedIn = await host.SignedInAccountAsync(Email("user"), Role.RegisteredUser);
        using var browser = signedIn.Browser;
        var owner = signedIn.Account;
        HabitId id;
        switch (which)
        {
            case "another user's":
                var other = await host.CreateAccountAsync(Email("other"), "walking42");
                id = await host.ArrangeCustomHabitAsync(other.Id, Definition(HabitName, Category.Body));
                break;
            case "deleted":
                id = await host.ArrangeCustomHabitAsync(owner, Definition(HabitName, Category.Body));
                await host.DeleteCustomHabitAsync(owner, id);
                break;
            default:
                id = new HabitId(Guid.NewGuid());
                break;
        }

        var (status, html) = await browser.GetPageAsync($"/custom-habits/{id.Value}");

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
        Assert.DoesNotContain(HabitName, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_owner_gets_their_habit_the_list_and_the_new_form()
    {
        var signedIn = await host.SignedInAccountAsync(Email("owner"), Role.RegisteredUser);
        using var browser = signedIn.Browser;
        var owner = signedIn.Account;
        var id = await host.ArrangeCustomHabitAsync(owner, Definition(HabitName, Category.Body, icon: "droplet"));

        var (editStatus, edit) = await browser.GetPageAsync($"/custom-habits/{id.Value}");
        var (listStatus, list) = await browser.GetPageAsync("/custom-habits");
        var (newStatus, created) = await browser.GetPageAsync("/custom-habits/new");

        Assert.Equal(HttpStatusCode.OK, editStatus);
        Assert.Contains("<h1>Edit custom habit</h1>", edit, StringComparison.Ordinal);
        Assert.Contains($"value=\"{HabitName}\"", edit, StringComparison.Ordinal);
        Assert.Contains(">Delete</button>", edit, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, listStatus);
        Assert.Contains("<h1>Habit catalog</h1>", list, StringComparison.Ordinal);
        Assert.Contains($"<h3 class=\"ht-habit__name\">{HabitName}</h3>", list, StringComparison.Ordinal);
        Assert.Contains($"href=\"/custom-habits/{id.Value}\"", list, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, newStatus);
        Assert.Contains("<h1>New custom habit</h1>", created, StringComparison.Ordinal);
        Assert.DoesNotContain(">Delete</button>", created, StringComparison.Ordinal);
    }
}
