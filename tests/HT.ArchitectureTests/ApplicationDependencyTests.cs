using ArchUnitNET.xUnit;
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
}
