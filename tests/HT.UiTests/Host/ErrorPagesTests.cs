using System.Net;

namespace HT.UiTests.Host;

[Collection(HostCollection.Name)]
public sealed class ErrorPagesTests(HostFixture host)
{
    [Fact]
    public async Task An_unknown_path_gives_404_with_the_not_found_page_in_the_layout()
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri("/no-such-page", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("<main id=\"main\">", html, StringComparison.Ordinal);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
        Assert.Contains("class=\"ht-card ht-empty\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_error_page_shows_the_empty_state_without_technical_detail_or_secrets()
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri("/Error", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("<main id=\"main\">", html, StringComparison.Ordinal);
        Assert.Contains("Something went wrong", html, StringComparison.Ordinal);
        Assert.Contains("class=\"ht-card ht-empty\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Request ID", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Development Mode", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ASPNETCORE_ENVIRONMENT", html, StringComparison.Ordinal);
        Assert.DoesNotContain(host.ConnectionString, html, StringComparison.Ordinal);
        Assert.DoesNotContain(host.DatabaseName, html, StringComparison.Ordinal);
        Assert.DoesNotContain(HostFixture.AdministratorEmail, html, StringComparison.Ordinal);
        Assert.DoesNotContain(HostFixture.AdministratorPassword, html, StringComparison.Ordinal);
        Assert.DoesNotContain(HostFixture.DemoPassword, html, StringComparison.Ordinal);
    }
}
