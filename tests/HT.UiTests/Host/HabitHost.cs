using System.Net;
using HT.Application.Habits;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Host;

/// <summary>Helpers for the habit host tests: signing in, redirects, and arranging catalog habits through the host's store.</summary>
public static class HabitHost
{
    /// <summary>Signs the browser in through the Sign in page; the response must be a redirect.</summary>
    public static async Task SignInAsync(this HttpClient browser, string email, string password)
    {
        var form = await browser.GetFormAsync("/sign-in", "sign-in");
        using var response = await browser.PostFormAsync("/sign-in", form, [new("Input.Email", email), new("Input.Password", password)]);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    /// <summary>A browser signed in as a new account of this role.</summary>
    public static async Task<HttpClient> SignedInBrowserAsync(this HostFixture host, string email, HT.Domain.Accounts.Role role, string password = "walking42")
    {
        ArgumentNullException.ThrowIfNull(host);
        await host.CreateAccountAsync(email, password, role);
        var browser = host.CreateBrowser();
        await browser.SignInAsync(email, password);
        return browser;
    }

    /// <summary>The absolute address a redirect points to; null when it is no redirect.</summary>
    public static string? RedirectTarget(this HttpResponseMessage response, HttpClient browser) =>
        response.Headers.Location is { } location ? new Uri(browser.BaseAddress!, location).AbsoluteUri : null;

    /// <summary>Creates a catalog habit through the host's store and moves it to <paramref name="state"/>.</summary>
    public static async Task<HabitId> ArrangeCatalogHabitAsync(
        this HostFixture host, HabitDefinition definition, CatalogHabitState state = CatalogHabitState.Published, SuggestedStartingGoals? goals = null)
    {
        ArgumentNullException.ThrowIfNull(host);
        await using var scope = host.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>();
        var created = await store.CreateAsync(definition, goals ?? SuggestedStartingGoals.None, CancellationToken.None);
        var id = created.Id ?? throw new InvalidOperationException("The catalog habit name is taken.");
        if (state != CatalogHabitState.Draft && !await store.TrySetStateAsync(id, CatalogHabitState.Draft, CatalogHabitState.Published, CancellationToken.None))
        {
            throw new InvalidOperationException("Publishing the arranged habit failed.");
        }

        if (state == CatalogHabitState.Unpublished && !await store.TrySetStateAsync(id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None))
        {
            throw new InvalidOperationException("Unpublishing the arranged habit failed.");
        }

        return id;
    }

    /// <summary>Creates a custom habit of <paramref name="owner"/> through the host's store.</summary>
    public static async Task<HabitId> ArrangeCustomHabitAsync(this HostFixture host, HT.Domain.Accounts.AccountId owner, HabitDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(host);
        await using var scope = host.Services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<ICustomHabitStore>().CreateAsync(owner, definition, CancellationToken.None);
        return created.Id ?? throw new InvalidOperationException("The custom habit name is taken.");
    }

    /// <summary>Deletes a custom habit of <paramref name="owner"/> through the host's store.</summary>
    public static async Task DeleteCustomHabitAsync(this HostFixture host, HT.Domain.Accounts.AccountId owner, HabitId id)
    {
        ArgumentNullException.ThrowIfNull(host);
        await using var scope = host.Services.CreateAsyncScope();
        var deleted = await scope.ServiceProvider.GetRequiredService<ICustomHabitStore>().DeleteAsync(owner, id, CancellationToken.None);
        Assert.Equal(DeleteCustomHabitOutcome.Deleted, deleted);
    }

    /// <summary>A new account of this role and a browser signed in as it.</summary>
    public static async Task<(HttpClient Browser, HT.Domain.Accounts.AccountId Account)> SignedInAccountAsync(
        this HostFixture host, string email, HT.Domain.Accounts.Role role, string password = "walking42")
    {
        ArgumentNullException.ThrowIfNull(host);
        var account = await host.CreateAccountAsync(email, password, role);
        var browser = host.CreateBrowser();
        await browser.SignInAsync(email, password);
        return (browser, account.Id);
    }

    /// <summary>GETs a page and returns its status and body.</summary>
    public static async Task<(HttpStatusCode Status, string Html)> GetPageAsync(this HttpClient browser, string path)
    {
        using var response = await browser.GetAsync(new Uri(path, UriKind.Relative));
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
