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
        Assert.Equal(4, CardName().Count(html));
        var text = WebUtility.HtmlDecode(html);
        Assert.Equal(preview.Habits.Select(h => h.Name), CardName().Matches(text).Select(m => m.Groups[1].Value));
        Assert.Contains($"<a href=\"/catalog\">See all {preview.PublishedCount} →</a>", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Ho Hidden draft", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HP_004_the_visitor_home_with_Published_habits_has_the_example_card_and_is_the_same_for_two_visitors()
    {
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Ho Example reading", Category.Mind));
        using var first = host.CreateBrowser();
        using var second = host.CreateBrowser();

        var (status, html) = await first.GetPageAsync("/");
        var (_, again) = await second.GetPageAsync("/");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h2>Popular in the catalog</h2>", html, StringComparison.Ordinal);
        AssertExampleCard(html);
        Assert.Equal(HeroOf(html), HeroOf(again));
    }

    /// <summary>
    /// The hero's example card (HP-004): "Example goal", "Yoga, 3 days a week", "90% (18 of 20)", the stat labels and the
    /// 21 strip cells, inside the hero and before "How it works".
    /// </summary>
    internal static void AssertExampleCard(string html)
    {
        var text = WebUtility.HtmlDecode(html);
        var hero = text.IndexOf("<section class=\"ht-hero\">", StringComparison.Ordinal);
        var card = text.IndexOf("<div class=\"ht-eyebrow\">Example goal</div>", StringComparison.Ordinal);
        var steps = text.IndexOf("<h2>How it works</h2>", StringComparison.Ordinal);
        Assert.True(hero >= 0 && card > hero && steps > card, "The example card is not in the hero.");
        Assert.Contains("<h2 class=\"ht-card__title\">Yoga, 3 days a week</h2>", text, StringComparison.Ordinal);
        Assert.Contains("<div class=\"ht-stat__value ht-stat__value--text\">90% (18 of 20)</div><div class=\"ht-stat__label\">Consistency</div>", text, StringComparison.Ordinal);
        Assert.Contains("<div class=\"ht-stat__value\">6</div><div class=\"ht-stat__label\">Streak, commitments</div>", text, StringComparison.Ordinal);
        Assert.Contains("<div class=\"ht-stat__value\">11</div><div class=\"ht-stat__label\">Longest, commitments</div>", text, StringComparison.Ordinal);
        var strip = text[text.IndexOf("<div class=\"ht-strip\"", card, StringComparison.Ordinal)..];
        strip = strip[..strip.IndexOf("</div>", StringComparison.Ordinal)];
        Assert.Equal(21, strip.Split("<i class=\"ht-strip__cell").Length - 1);
    }

    /// <summary>
    /// The hero with the example card, up to "How it works": the part of the page that holds no per-request value (the
    /// Blazor prerender markers do).
    /// </summary>
    private static string HeroOf(string html)
    {
        var start = html.IndexOf("<section class=\"ht-hero\">", StringComparison.Ordinal);
        var end = html.IndexOf("<h2>How it works</h2>", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "The page has no hero before How it works.");
        return html[start..end];
    }

    private async Task<HomePreviewResult> PreviewAsync()
    {
        await using var scope = host.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<CatalogService>().GetHomePreviewAsync(CancellationToken.None);
    }

    [GeneratedRegex("<h3 class=\"ht-habit__name\">([^<]*)</h3>")]
    private static partial Regex CardName();
}

/// <summary>
/// HP-004 (amended slice 2 US5-AS8): with no Published habit, the visitor Home still has the example card, through a
/// host over the derived database <c>HT-UI-KEYS</c>, reset to an empty catalog and started with no operator settings, so
/// demo loading adds no seed catalog.
/// </summary>
[Collection(DerivedDatabaseHostCollection.Name)]
public sealed class HomeExampleCardEmptyCatalogHostTests
{
    [Fact]
    public async Task HP_004_the_visitor_home_without_Published_habits_has_the_example_card_and_no_catalog_preview()
    {
        await DerivedDatabaseHost.ResetAsync("HT-UI-KEYS");
        await using var host = new DerivedDatabaseHost("HT-UI-KEYS", new Dictionary<string, string?>());
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync("/");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.DoesNotContain("Popular in the catalog", html, StringComparison.Ordinal);
        HomeCatalogHostTests.AssertExampleCard(html);
    }
}
