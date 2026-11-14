using System.Net;
using HT.Domain.Habits;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>US1-AS10, HP-001, HP-003: /catalog through the real host, prerendered.</summary>
[Collection(HostCollection.Name)]
public sealed class CatalogHostTests(HostFixture host)
{
    [Fact]
    public async Task An_anonymous_visitor_gets_the_prerendered_catalog()
    {
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Reading", Category.Mind));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Hidden draft", Category.Mind), CatalogHabitState.Draft);
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync("/catalog");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h1>Habit catalog</h1>", html, StringComparison.Ordinal);
        Assert.Contains("Ui-Br Reading", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Br Hidden draft", html, StringComparison.Ordinal);
        Assert.Contains("See details", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_address_filters_are_applied_when_prerendering()
    {
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Budget", Category.Money, Intent.Reduce, "wallet"));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Saving", Category.Money, Intent.Build, "piggy-bank"));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Sweets", Category.Health, Intent.Reduce, "candy"));
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync("/catalog?category=money&intent=reduce&q=ui-br");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("Ui-Br Budget", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Br Saving", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Br Sweets", html, StringComparison.Ordinal);
    }
}
