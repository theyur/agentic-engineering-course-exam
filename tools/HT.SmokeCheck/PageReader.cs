using System.Net;
using System.Text.RegularExpressions;

namespace HT.SmokeCheck;

/// <summary>
/// Reads what the smoke check needs from a server-rendered page (contracts/smoke-check.md): the antiforgery token
/// and handler names of a form, the first heading, the names on habit cards and the linked stylesheets and scripts.
/// Every value is HTML-decoded. Names are compared in memory and never printed.
/// </summary>
public static partial class PageReader
{
    /// <summary>The antiforgery request token of the page's form.</summary>
    /// <exception cref="InvalidOperationException">The page has no token; the message names only the path.</exception>
    public static string ReadToken(string html, string path)
    {
        ArgumentNullException.ThrowIfNull(html);
        var token = TokenPattern().Match(html);
        if (!token.Success)
        {
            throw new InvalidOperationException($"No antiforgery token on {path}.");
        }

        return WebUtility.HtmlDecode(token.Groups["value"].Value);
    }

    /// <summary>The <c>_handler</c> value of every form on the page, in document order.</summary>
    public static IReadOnlyList<string> ReadHandlers(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return [.. HandlerPattern().Matches(html).Select(m => WebUtility.HtmlDecode(m.Groups["value"].Value))];
    }

    /// <summary>The text of the first <c>h1</c>, without inner tags, or null when the page has none.</summary>
    public static string? ReadFirstHeading(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        var heading = FirstHeadingPattern().Match(html);
        return heading.Success ? Text(heading.Groups["inner"].Value) : null;
    }

    /// <summary>
    /// The visible name of every habit card, in document order: <c>HabitCard</c> renders it as
    /// <c>&lt;h3 class="ht-habit__name"&gt;</c>.
    /// </summary>
    public static IReadOnlyList<string> ReadHabitNames(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return [.. HabitNamePattern().Matches(html).Select(m => Text(m.Groups["inner"].Value))];
    }

    /// <summary>
    /// The <c>href</c> of every <c>&lt;link rel="stylesheet"&gt;</c> and the <c>src</c> of every
    /// <c>&lt;script src&gt;</c>, in document order, as written on the page; inline scripts have no <c>src</c> and are
    /// left out.
    /// </summary>
    public static IReadOnlyList<string> ReadAssetUrls(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        var urls = new List<string>();
        foreach (Match tag in AssetTagPattern().Matches(html))
        {
            var attributes = tag.Groups["attributes"].Value;
            var isLink = tag.Groups["name"].Value.Equals("link", StringComparison.OrdinalIgnoreCase);
            if (isLink && !IsStylesheet(attributes))
            {
                continue;
            }

            var url = Attribute(attributes, isLink ? "href" : "src");
            if (!string.IsNullOrEmpty(url))
            {
                urls.Add(url);
            }
        }

        return urls;
    }

    /// <summary>True when every expected name is among the found ones (ordinal), whatever else is found.</summary>
    public static bool ContainsAll(IEnumerable<string> found, IEnumerable<string> expected)
    {
        ArgumentNullException.ThrowIfNull(found);
        ArgumentNullException.ThrowIfNull(expected);
        var present = found.ToHashSet(StringComparer.Ordinal);
        return expected.All(present.Contains);
    }

    private static bool IsStylesheet(string attributes) =>
        (Attribute(attributes, "rel") ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Contains("stylesheet", StringComparer.OrdinalIgnoreCase);

    private static string? Attribute(string attributes, string name)
    {
        foreach (Match attribute in AttributePattern().Matches(attributes))
        {
            if (attribute.Groups["name"].Value.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return WebUtility.HtmlDecode(attribute.Groups["value"].Value);
            }
        }

        return null;
    }

    private static string Text(string inner) => WebUtility.HtmlDecode(InnerTagPattern().Replace(inner, string.Empty)).Trim();

    [GeneratedRegex("""name="__RequestVerificationToken"[^>]*value="(?<value>[^"]*)"|value="(?<value>[^"]*)"[^>]*name="__RequestVerificationToken"(?=[\s/>])""")]
    private static partial Regex TokenPattern();

    [GeneratedRegex("""name="_handler"[^>]*value="(?<value>[^"]*)"|value="(?<value>[^"]*)"[^>]*name="_handler"(?=[\s/>])""")]
    private static partial Regex HandlerPattern();

    [GeneratedRegex("""<h1\b[^>]*>(?<inner>.*?)</h1\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FirstHeadingPattern();

    [GeneratedRegex("""<h3\b[^>]*\bclass="(?:[^"]*\s)?ht-habit__name(?:\s[^"]*)?"[^>]*>(?<inner>.*?)</h3\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex HabitNamePattern();

    [GeneratedRegex("""<(?<name>link|script)\b(?<attributes>[^>]*)>""", RegexOptions.IgnoreCase)]
    private static partial Regex AssetTagPattern();

    [GeneratedRegex("""(?<name>[\w-]+)\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)'|(?<value>[^\s"'>]+))""")]
    private static partial Regex AttributePattern();

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex InnerTagPattern();
}
