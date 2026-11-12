using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace HT.ArchitectureTests;

/// <summary>
/// The AC reference export (export_refs.py) refuses an AC surface project that grants its internals to another
/// assembly. These checks fail the pre-commit gate first (ADR 0010).
/// </summary>
public sealed class AcSurfaceTests
{
    private const string Forbidden = "InternalsVisibleTo";

    private static readonly string[] SurfaceProjects = ["HT.Domain", "HT.Application", "HT.Persistence"];

    private static readonly string[] CheckedExtensions = [".cs", ".csproj", ".props", ".targets"];

    private static readonly string[] RootBuildFiles = ["Directory.Build.props", "Directory.Build.targets"];

    public static TheoryData<string> SurfaceAssemblies =>
        new(ArchitectureModel.Domain.GetName().Name!, ArchitectureModel.Application.GetName().Name!, ArchitectureModel.Persistence.GetName().Name!);

    [Theory]
    [MemberData(nameof(SurfaceAssemblies))]
    public void SurfaceAssemblyGrantsNoInternals(string assemblyName)
    {
        var assembly = new[] { ArchitectureModel.Domain, ArchitectureModel.Application, ArchitectureModel.Persistence }
            .Single(a => a.GetName().Name == assemblyName);

        Assert.Empty(assembly.GetCustomAttributes<InternalsVisibleToAttribute>());
    }

    [Fact]
    public void SurfaceSourcesAndRootBuildFilesDoNotMentionInternalsVisibleTo()
    {
        var root = RepositoryRoot();
        var files = SurfaceProjects
            .SelectMany(project => Directory.EnumerateFiles(Path.Combine(root, "src", project), "*", SearchOption.AllDirectories))
            .Where(path => CheckedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Where(path => !IsBuildOutput(root, path))
            .Concat(RootBuildFiles
                .Select(name => Path.Combine(root, name))
                .Where(File.Exists));

        var offenders = files
            .Where(path => File.ReadAllText(path).Contains(Forbidden, StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(root, path))
            .ToList();

        Assert.Empty(offenders);
    }

    private static bool IsBuildOutput(string root, string path)
    {
        var parts = Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Contains("bin", StringComparer.OrdinalIgnoreCase) || parts.Contains("obj", StringComparer.OrdinalIgnoreCase);
    }

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
