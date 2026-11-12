namespace HT.UiTests;

/// <summary>
/// The app's stylesheets are copies of the design files (constitution IV). A style written only in the app copy
/// fails here, so the build gate catches it.
/// </summary>
public sealed class DesignSyncTests
{
    [Theory]
    [InlineData("tokens.css")]
    [InlineData("ht-components.css")]
    public void App_stylesheet_equals_its_design_source(string fileName)
    {
        var root = RepositoryRoot();

        var design = Normalise(File.ReadAllText(Path.Combine(root, "design", "css", fileName)));
        var app = Normalise(File.ReadAllText(Path.Combine(root, "src", "HT.Ui", "wwwroot", "css", fileName)));

        Assert.Equal(design, app);
    }

    private static string Normalise(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal);

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
