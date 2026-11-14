using ArchUnitNET.xUnit;
using HT.Application.Operator;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HT.ArchitectureTests;

public sealed class ApplicationDependencyTests
{
    [Fact]
    public void ApplicationMustDependOnlyOnDomainAmongHtAssemblies()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .Should().NotDependOnAny(Types(true).That().ResideInNamespaceMatching(@"^HT\.(Persistence|Ui)(\..*)?$"))
            .Because("HT.Application references only HT.Domain")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void ApplicationMustNotDependOnEfCoreAspNetCoreOrIdentity()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .Should().NotDependOnAny(Types(true).That().ResideInNamespaceMatching(
                @"^(Microsoft\.EntityFrameworkCore|Microsoft\.AspNetCore|Microsoft\.Extensions\.Identity)(\..*)?$"))
            .Because("no EF, SQL, web or Identity details belong in HT.Application (ADR 0004)")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void OnlyTheOperatorTypesOfApplicationMayDependOnTheDemoDataStorePort()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .And().DoNotResideInNamespaceMatching(@"^HT\.Application\.Operator(\..*)?$")
            .Should().NotDependOnAny(Types(true).That().Are(typeof(IDemoDataStore)))
            .Because("the demo data port writes private data without an acting user; only demo loading may use it (ADR 0013, research R-12)")
            .Check(ArchitectureModel.Architecture);
    }
}
