using System.Reflection;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HT.ArchitectureTests;

public sealed class DomainDependencyTests
{
    private const string NodaTime = "NodaTime";

    [Fact]
    public void DomainMustNotDependOnOtherHtAssemblies()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Domain)
            .Should().NotDependOnAny(Types(true).That().ResideInNamespaceMatching(@"^HT\.(Application|Persistence|Ui)(\..*)?$"))
            .Because("HT.Domain is the innermost layer")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void DomainMustNotDependOnMicrosoftTypes()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Domain)
            .Should().NotDependOnAny(Types(true).That().ResideInNamespaceMatching(@"^Microsoft(\..*)?$"))
            .Because("HT.Domain has no framework dependencies (research R-13)")
            .Check(ArchitectureModel.Architecture);
    }

    /// <summary>
    /// NodaTime is HT.Domain's only assembly outside the base class library (ADR 0017, research R-13). A base-library
    /// assembly is one the runtime loads from its own directory, the one <see cref="object"/> comes from.
    /// </summary>
    [Fact]
    public void NodaTimeIsDomainsOnlyNonBclAssembly()
    {
        var references = ArchitectureModel.Domain.GetReferencedAssemblies();
        var runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);

        var outsideTheBcl = references
            .Where(name => name.Name != NodaTime)
            .Where(name => Path.GetDirectoryName(Assembly.Load(name).Location) != runtimeDirectory)
            .Select(name => name.FullName)
            .ToList();

        Assert.Empty(outsideTheBcl);
        Assert.Contains(references, name => name.Name == NodaTime);
    }
}
