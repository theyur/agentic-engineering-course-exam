using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using HT.Persistence;
using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HT.ArchitectureTests;

public sealed class UiDependencyTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(PersistenceAssemblyMarker).Assembly,
            typeof(HT.Ui.Components.App).Assembly)
        .Build();

    [Fact]
    public void BlazorComponentsMustNotDependOnPersistenceTypes()
    {
        var components = Types().That()
            .ResideInNamespaceMatching(@"^HT\.Ui\.Components(\..*)?$");

        var persistence = Types().That()
            .ResideInNamespaceMatching(@"^HT\.Persistence(\..*)?$");

        Types().That().Are(components)
            .Should().NotDependOnAny(persistence)
            .Because("Blazor components must use Application abstractions, not Persistence implementations")
            .Check(Architecture);
    }
}
