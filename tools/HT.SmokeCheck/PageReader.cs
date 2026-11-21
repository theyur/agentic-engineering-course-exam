using System.Net;
using System.Text.RegularExpressions;

namespace HT.SmokeCheck;

/// <summary>
/// Reads what the smoke check needs from a server-rendered page (contracts/smoke-check.md): the antiforgery token
/// and handler names of a form, the first heading, the names on habit cards and goal items, the goal items' links, the
/// text of a card, the stat labels, tile labels and status chips (slice 6), the goal items' meta texts (slice 7), and the
/// linked stylesheets and scripts.
/// Every value is HTML-decoded. Names and texts are compared in memory and never printed.
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
    /// The visible name of every goal on the Goals page, in document order: <c>GoalItem</c> renders it as
    /// <c>&lt;a class="ht-goalitem__name" href="/goals/…"&gt;</c>.
    /// </summary>
    public static IReadOnlyList<string> ReadGoalNames(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return [.. GoalNamePattern().Matches(html).Select(m => Text(m.Groups["inner"].Value))];
    }

    /// <summary>
    /// Every goal item on the Goals page with its link, in document order: the name of each
    /// <c>&lt;a class="ht-goalitem__name"&gt;</c>, as <see cref="ReadGoalNames"/> reads it, paired with its decoded
    /// <c>href</c>, or null when the anchor has none (smoke check 6, slice 5 research R-10).
    /// </summary>
    public static IReadOnlyList<GoalLink> ReadGoalLinks(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return [.. GoalNamePattern().Matches(html).Select(m => new GoalLink(Text(m.Groups["inner"].Value), Attribute(m.Groups["attributes"].Value, "href")))];
    }

    /// <summary>
    /// Every goal item on the Goals page with its meta text, in document order (smoke check 8, slice 7 HP-011): the name
    /// of each <c>&lt;a class="ht-goalitem__name"&gt;</c>, as <see cref="ReadGoalNames"/> reads it, paired with the text of
    /// the first <c>&lt;div class="ht-goalitem__meta"&gt;</c> after it and before the next goal item's name, up to that
    /// div's <c>&lt;/div&gt;</c>, without its chips (every element whose class list holds <c>ht-chip</c>: the Build or
    /// Reduce Tag and a StatusChip): tags removed, entities decoded and every run of white space made one space, such as
    /// "Paused until Sun 22 Nov" or "Every day · next today". Null when the item has no meta line.
    /// </summary>
    public static IReadOnlyList<GoalMeta> ReadGoalMetas(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        var names = GoalNamePattern().Matches(html);
        var metas = new List<GoalMeta>(names.Count);
        for (var i = 0; i < names.Count; i++)
        {
            var start = names[i].Index + names[i].Length;
            var end = i + 1 < names.Count ? names[i + 1].Index : html.Length;
            metas.Add(new GoalMeta(Text(names[i].Groups["inner"].Value), MetaText(html[start..end])));
        }

        return metas;
    }

    /// <summary>
    /// The text of the first card <c>&lt;section class="ht-card"&gt;</c> whose first
    /// <c>&lt;h2 class="ht-card__title"&gt;</c> is <paramref name="title"/> (ordinal), up to that section's
    /// <c>&lt;/section&gt;</c>: tags removed, entities decoded and every run of white space made one space. Null when no
    /// such card is on the page, or when a card has no <c>&lt;/section&gt;</c>. Another card's words are never part of it
    /// (smoke check 6, slice 5 research R-10).
    /// </summary>
    public static string? ReadCardText(string html, string title)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(title);
        foreach (Match section in CardSectionPattern().Matches(html))
        {
            var start = section.Index + section.Length;
            var end = html.IndexOf("</section", start, StringComparison.OrdinalIgnoreCase);
            if (end < 0)
            {
                return null;
            }

            var body = html[start..end];
            var heading = CardTitlePattern().Match(body);
            if (heading.Success && string.Equals(Text(heading.Groups["inner"].Value), title, StringComparison.Ordinal))
            {
                var text = WebUtility.HtmlDecode(InnerTagPattern().Replace(body, string.Empty));
                return WhiteSpacePattern().Replace(text, " ").Trim();
            }
        }

        return null;
    }

    /// <summary>
    /// The text of every GoalStats stat label, in document order: each element whose class list holds
    /// <c>ht-stat__label</c>, such as "Consistency" or "Streak, commitments" (smoke checks 6 and 7, slice 6 research R-11).
    /// </summary>
    public static IReadOnlyList<string> ReadStatLabels(string html) => ReadLeafTexts(html, StatLabelPattern());

    /// <summary>
    /// The text of every StatTile label, in document order: each element whose class list holds <c>ht-tile__label</c>,
    /// such as "Due now" or "Missed this week" (smoke checks 6 and 7, slice 6 research R-11).
    /// </summary>
    public static IReadOnlyList<string> ReadTileLabels(string html) => ReadLeafTexts(html, TileLabelPattern());

    /// <summary>
    /// The text of every StatusChip, in document order: each element whose class list holds <c>ht-status</c> (not only a
    /// modifier such as <c>ht-status--missed</c>), such as "Missed" (smoke check 7, slice 6 research R-11).
    /// </summary>
    public static IReadOnlyList<string> ReadStatusChips(string html) => ReadLeafTexts(html, StatusChipPattern());

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

    /// <summary>The text of the first meta line in <paramref name="item"/> without its chips, or null when it has none.</summary>
    private static string? MetaText(string item)
    {
        var meta = GoalMetaPattern().Match(item);
        if (!meta.Success)
        {
            return null;
        }

        var start = meta.Index + meta.Length;
        var end = item.IndexOf("</div", start, StringComparison.OrdinalIgnoreCase);
        if (end < 0)
        {
            return null;
        }

        var inner = ChipPattern().Replace(item[start..end], " ");
        return WhiteSpacePattern().Replace(WebUtility.HtmlDecode(InnerTagPattern().Replace(inner, " ")), " ").Trim();
    }

    /// <summary>The decoded text of every leaf element the pattern matches, in document order; inner tags are removed.</summary>
    private static List<string> ReadLeafTexts(string html, Regex pattern)
    {
        ArgumentNullException.ThrowIfNull(html);
        return [.. pattern.Matches(html).Select(m => Text(m.Groups["inner"].Value))];
    }

    [GeneratedRegex("""<(?<tag>[a-z][a-z0-9]*)\b[^>]*\bclass="(?:[^"]*\s)?ht-stat__label(?:\s[^"]*)?"[^>]*>(?<inner>.*?)</\k<tag>\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StatLabelPattern();

    [GeneratedRegex("""<(?<tag>[a-z][a-z0-9]*)\b[^>]*\bclass="(?:[^"]*\s)?ht-tile__label(?:\s[^"]*)?"[^>]*>(?<inner>.*?)</\k<tag>\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TileLabelPattern();

    [GeneratedRegex("""<(?<tag>[a-z][a-z0-9]*)\b[^>]*\bclass="(?:[^"]*\s)?ht-status(?:\s[^"]*)?"[^>]*>(?<inner>.*?)</\k<tag>\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StatusChipPattern();

    [GeneratedRegex("""name="__RequestVerificationToken"[^>]*value="(?<value>[^"]*)"|value="(?<value>[^"]*)"[^>]*name="__RequestVerificationToken"(?=[\s/>])""")]
    private static partial Regex TokenPattern();

    [GeneratedRegex("""name="_handler"[^>]*value="(?<value>[^"]*)"|value="(?<value>[^"]*)"[^>]*name="_handler"(?=[\s/>])""")]
    private static partial Regex HandlerPattern();

    [GeneratedRegex("""<h1\b[^>]*>(?<inner>.*?)</h1\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FirstHeadingPattern();

    [GeneratedRegex("""<h3\b[^>]*\bclass="(?:[^"]*\s)?ht-habit__name(?:\s[^"]*)?"[^>]*>(?<inner>.*?)</h3\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex HabitNamePattern();

    [GeneratedRegex("""<a\b(?<attributes>[^>]*\bclass="(?:[^"]*\s)?ht-goalitem__name(?:\s[^"]*)?"[^>]*)>(?<inner>.*?)</a\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex GoalNamePattern();

    [GeneratedRegex("""<div\b[^>]*\bclass="(?:[^"]*\s)?ht-goalitem__meta(?:\s[^"]*)?"[^>]*>""", RegexOptions.IgnoreCase)]
    private static partial Regex GoalMetaPattern();

    [GeneratedRegex("""<(?<tag>[a-z][a-z0-9]*)\b[^>]*\bclass="(?:[^"]*\s)?ht-chip(?:\s[^"]*)?"[^>]*>.*?</\k<tag>\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ChipPattern();

    [GeneratedRegex("""<section\b[^>]*\bclass="(?:[^"]*\s)?ht-card(?:\s[^"]*)?"[^>]*>""", RegexOptions.IgnoreCase)]
    private static partial Regex CardSectionPattern();

    [GeneratedRegex("""<h2\b[^>]*\bclass="(?:[^"]*\s)?ht-card__title(?:\s[^"]*)?"[^>]*>(?<inner>.*?)</h2\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex CardTitlePattern();

    [GeneratedRegex("""\s+""")]
    private static partial Regex WhiteSpacePattern();

    [GeneratedRegex("""<(?<name>link|script)\b(?<attributes>[^>]*)>""", RegexOptions.IgnoreCase)]
    private static partial Regex AssetTagPattern();

    [GeneratedRegex("""(?<name>[\w-]+)\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)'|(?<value>[^\s"'>]+))""")]
    private static partial Regex AttributePattern();

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex InnerTagPattern();
}

/// <summary>A goal item of the Goals page: its visible name and its link (smoke check 6).</summary>
/// <param name="Name">The goal's visible name.</param>
/// <param name="Href">The anchor's decoded <c>href</c> as written on the page, or null when it has none.</param>
public sealed record GoalLink(string Name, string? Href);

/// <summary>A goal item of the Goals page: its visible name and its meta text (smoke check 8, slice 7 HP-011).</summary>
/// <param name="Name">The goal's visible name.</param>
/// <param name="Meta">The meta line's text without its chips, such as "Paused until Sun 22 Nov", or null when the item has none.</param>
public sealed record GoalMeta(string Name, string? Meta);
