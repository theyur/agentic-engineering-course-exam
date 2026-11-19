using ArchUnitNET.xUnit;
using HT.Application.Goals;
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
    public void ApplicationMustNotDependOnNodaTime()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .Should().NotDependOnAny(Types(true).That().ResideInNamespaceMatching(@"^NodaTime(\..*)?$"))
            .Because("NodaTime stays inside HT.Domain; Application uses NamedTimeZone and BCL dates (research R-01)")
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

    [Fact]
    public void OnlyTheGoalTypesTheThreeHabitServicesTheProfileServiceAndTheRecordingServiceMayDependOnTheGoalStorePort()
    {
        // Nested types (async state machines, closures) belong to their service, so the names allow a nested suffix.
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .And().DoNotResideInNamespaceMatching(@"^HT\.Application\.Goals(\..*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Habits\.(CatalogService|CatalogAdministrationService|CustomHabitService)([+/].*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Accounts\.ProfileService([+/].*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Records\.RecordingService([+/].*)?$")
            .Should().NotDependOnAny(Types(true).That().Are(typeof(IGoalStore)))
            .Because("goals are private and reached only through owner-scoped use cases; the habit services need counts and the delete rule, the profile service the time-zone change, the recording service the owner's goal (constitution III, slice 4 research R-13, slice 5 research R-13)")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void NoOperatorTypeOfApplicationMayDependOnTheGoalStorePort()
    {
        Types().That().ResideInNamespaceMatching(@"^HT\.Application\.Operator(\..*)?$")
            .Should().NotDependOnAny(Types(true).That().Are(typeof(IGoalStore)))
            .Because("demo goals go through the demo data port, not the goal store (ADR 0013, slice 4 research R-13)")
            .Check(ArchitectureModel.Architecture);
    }
}
