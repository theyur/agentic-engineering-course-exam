using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HT.ArchitectureTests;

/// <summary>
/// The smoke check <c>tools/HT.SmokeCheck</c> (slice 3, research R-06, R-11) is an operational tool outside the four
/// layers: it reads its expected demo data from <c>HT.Application</c> and reaches the app only over HTTP, never through
/// the host or the database.
/// </summary>
public sealed class SmokeCheckDependencyTests
{
    private static readonly string[] AllowedHtReferences = ["HT.Application", "HT.Domain"];

    [Fact]
    public void SmokeCheckMustNotDependOnPersistenceOrUi()
    {
        Types().That().ResideInAssembly(ArchitectureModel.SmokeCheck)
            .Should().NotDependOnAny(Types(true).That().ResideInNamespaceMatching(@"^HT\.(Persistence|Ui)(\..*)?$"))
            .Because("the smoke check talks to the app over HTTP only and takes its expected values from HT.Application")
            .Check(ArchitectureModel.Architecture);
    }

    /// <summary>
    /// The compiler records a reference only when a type of it is used, so the set grows as the checks use
    /// <c>HT.Application</c> (and through it <c>HT.Domain</c>); it never includes another solution assembly.
    /// </summary>
    [Fact]
    public void SmokeCheckReferencesOnlyApplicationAndDomainAmongSolutionAssemblies()
    {
        var offenders = ArchitectureModel.SmokeCheck.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => name.StartsWith("HT.", StringComparison.Ordinal))
            .Where(name => !AllowedHtReferences.Contains(name, StringComparer.Ordinal))
            .ToList();

        Assert.Empty(offenders);
    }
}
