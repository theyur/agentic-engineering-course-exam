using System.Net;
using System.Text.RegularExpressions;
using HT.Application.Habits;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>US5-AS2, HP-012: the visitor Home's "Popular in the catalog" through the real host.</summary>
[Collection(HostCollection.Name)]
public sealed partial class HomeCatalogHostTests(HostFixture host)
{
    [Fact]
    public async Task The_visitor_home_shows_four_cards_and_see_all_with_the_number_of_Published_habits()
    {
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Ho Reading", Category.Mind));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Ho Running", Category.Body, icon: "dumbbell"));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Ho Saving", Category.Money, icon: "piggy-bank"));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Ho Sweets", Category.Health, Intent.Reduce, "candy"));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Ho Hidden draft", Category.Mind), CatalogHabitState.Draft);
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync("/");
        var preview = await PreviewAsync();

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h2>Popular in the catalog</h2>", html, StringComparison.Ordinal);
        Assert.Equal(4, CardName().Matches(html).Count);
        var text = WebUtility.HtmlDecode(html);
        Assert.Equal(preview.Habits.Select(h => h.Name), CardName().Matches(text).Select(m => m.Groups[1].Value));
        Assert.Contains($"<a href=\"/catalog\">See all {preview.PublishedCount} →</a>", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Ho Hidden draft", text, StringComparison.Ordinal);
    }

    private async Task<HomePreviewResult> PreviewAsync()
    {
        await using var scope = host.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<CatalogService>().GetHomePreviewAsync(CancellationToken.None);
    }

    [GeneratedRegex("<h3 class=\"ht-habit__name\">([^<]*)</h3>")]
    private static partial Regex CardName();
}
