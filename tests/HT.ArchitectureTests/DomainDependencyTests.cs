using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HT.ArchitectureTests;

public sealed class DomainDependencyTests
{
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
}
