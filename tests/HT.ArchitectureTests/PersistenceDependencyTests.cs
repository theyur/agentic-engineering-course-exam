using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HT.ArchitectureTests;

public sealed class PersistenceDependencyTests
{
    [Fact]
    public void PersistenceMustNotDependOnUi()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Persistence)
            .Should().NotDependOnAny(Types(true).That().ResideInNamespaceMatching(@"^HT\.Ui(\..*)?$"))
            .Because("HT.Persistence implements inward contracts and knows nothing of the host")
            .Check(ArchitectureModel.Architecture);
    }
}
