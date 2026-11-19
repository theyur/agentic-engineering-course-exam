using System.Reflection;
using System.Text.RegularExpressions;
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

    /// <summary>The namespaces of slice 5's records and evaluation (research R-13).</summary>
    internal static readonly string[] RecordsAndEvaluation = ["HT.Domain.Records", "HT.Domain.Evaluation"];

    /// <summary>The types of one of <see cref="RecordsAndEvaluation"/>; a test fails when there are none, so moving them out fails.</summary>
    internal static List<ArchUnitNET.Domain.IType> TypesIn(string ns)
    {
        var types = ArchitectureModel.Architecture.Types
            .Where(t => t.Assembly.FullName == ArchitectureModel.Domain.FullName && t.Namespace.FullName == ns)
            .ToList();
        Assert.True(types.Count > 0, $"{ns} has no types");
        return types;
    }

    /// <summary>
    /// Slice 5 research R-13: <c>HT.Domain.Records</c> and <c>HT.Domain.Evaluation</c> depend only on HT.Domain and
    /// the base class library, not even on NodaTime.
    /// </summary>
    [Fact]
    public void RecordsAndEvaluationDependOnlyOnTheDomainAndTheBcl()
    {
        foreach (var ns in RecordsAndEvaluation)
        {
            Assert.NotEmpty(TypesIn(ns));
            Types().That().ResideInNamespaceMatching($"^{Regex.Escape(ns)}$")
                .Should().NotDependOnAny(Types(true).That().DoNotResideInNamespaceMatching(@"^(HT\.Domain|System)(\..*)?$"))
                .Because("records and evaluation use only the domain and the base class library (research R-13)")
                .Check(ArchitectureModel.Architecture);
        }
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
