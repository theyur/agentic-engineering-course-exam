using System.Xml.Linq;
using Bunit;
using HT.Domain.Habits;
using HT.Ui.Components.Shared;

namespace HT.UiTests.Components.Shared;

/// <summary>
/// Research R-16: every drawing HtIcon knows equals its design file, and every habit icon has a drawing. A drawing
/// that differs from design/icons fails here, as the CSS does in DesignSyncTests.
/// </summary>
public sealed class HtIconDrawingTests : BunitContext
{
    public static TheoryData<string> KnownNames => new(HtIcon.KnownNames);

    [Theory]
    [MemberData(nameof(KnownNames))]
    public void The_drawing_equals_its_design_file(string name)
    {
        var path = Path.Combine(RepositoryRoot(), "design", "icons", name + ".svg");
        Assert.True(File.Exists(path), $"design/icons/{name}.svg is missing.");

        var design = XDocument.Load(path).Root!.Elements()
            .Select(e => Normalise(e.Name.LocalName, e.Attributes().Select(a => (a.Name.LocalName, a.Value))))
            .ToList();

        var rendered = Render<HtIcon>(p => p.Add(x => x.Name, name)).Find("svg").Children
            .Select(e => Normalise(e.LocalName, e.Attributes.Select(a => (a.LocalName, a.Value))))
            .ToList();

        Assert.Equal(design, rendered);
    }

    [Fact]
    public void Every_habit_icon_has_a_drawing()
    {
        Assert.Empty(HabitIcon.All.Select(i => i.Name).Except(HtIcon.KnownNames, StringComparer.Ordinal));
    }

    [Fact]
    public void Every_known_name_is_listed_once()
    {
        Assert.Equal(HtIcon.KnownNames.Count, HtIcon.KnownNames.Distinct(StringComparer.Ordinal).Count());
    }

    private static string Normalise(string element, IEnumerable<(string Name, string Value)> attributes) =>
        element + "(" + string.Join(" ", attributes.OrderBy(a => a.Name, StringComparer.Ordinal).Select(a => $"{a.Name}={a.Value.Trim()}")) + ")";

    private static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "HabitTracker.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("The repository root (the folder with HabitTracker.slnx) was not found.");
    }
}
