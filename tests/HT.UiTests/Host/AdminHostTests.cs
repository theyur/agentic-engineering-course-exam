using System.Net;
using HT.Domain.Accounts;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>HP-010, US5-AS4: who reaches /admin and the administrator's catalog habit form, through the real host.</summary>
[Collection(HostCollection.Name)]
public sealed class AdminHostTests(HostFixture host)
{
    public static TheoryData<string> Addresses => new("/admin", "/admin/catalog/new", "/admin/catalog/{id}");

    private async Task<string> AddressAsync(string template)
    {
        if (!template.Contains("{id}", StringComparison.Ordinal))
        {
            return template;
        }

        var id = await host.ArrangeCatalogHabitAsync(Definition($"Ui-Ad {Guid.NewGuid():N}"[..20]));
        return template.Replace("{id}", id.Value.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Addresses))]
    public async Task An_anonymous_visitor_is_sent_to_sign_in(string template)
    {
        var address = await AddressAsync(template);
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri(address, UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=" + Uri.EscapeDataString(address), response.RedirectTarget(browser));
    }

    [Theory]
    [MemberData(nameof(Addresses))]
    public async Task A_registered_user_gets_the_Not_found_page(string template)
    {
        var address = await AddressAsync(template);
        using var browser = await host.SignedInBrowserAsync($"ui-ad-user-{Guid.NewGuid():N}@example.com", Role.RegisteredUser);

        var (status, html) = await browser.GetPageAsync(address);

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Administration", html, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Addresses))]
    public async Task An_administrator_gets_the_page(string template)
    {
        var address = await AddressAsync(template);
        using var browser = await host.SignedInBrowserAsync($"ui-ad-admin-{Guid.NewGuid():N}@example.com", Role.Administrator);

        var (status, html) = await browser.GetPageAsync(address);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(template == "/admin" ? "<h1>Administration</h1>" : "catalog habit</h1>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_unknown_catalog_habit_is_not_found_for_an_administrator()
    {
        using var browser = await host.SignedInBrowserAsync($"ui-ad-admin-{Guid.NewGuid():N}@example.com", Role.Administrator);

        var (status, html) = await browser.GetPageAsync($"/admin/catalog/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
    }
}
