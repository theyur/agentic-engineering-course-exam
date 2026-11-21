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
    public void OnlyTheOperatorTypesOfApplicationMayDependOnTheDemoHistoryStorePort()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .And().DoNotResideInNamespaceMatching(@"^HT\.Application\.Operator(\..*)?$")
            .Should().NotDependOnAny(Types(true).That().Are(typeof(IDemoHistoryStore)))
            .Because("the demo history port writes private records and notes without an acting user; only demo history loading may use it (ADR 0013, slice 5 research R-13)")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void OnlyTheDemoReportServiceMayDependOnTheDemoReportStorePort()
    {
        // Nested types (async state machines, closures) belong to their service, so the name allows a nested suffix.
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Operator\.DemoReportService([+/].*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Operator\.IDemoReportStore$")
            .Should().NotDependOnAny(Types(true).That().Are(typeof(IDemoReportStore)))
            .Because("the demo report port writes private records and notes without an acting user; only demo report loading may use it (ADR 0013, ADR 0021, slice 6 research R-14)")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void OnlyTheDemoPauseServiceMayDependOnTheDemoPauseStorePort()
    {
        // Nested types (async state machines, closures) belong to their service, so the name allows a nested suffix.
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Operator\.DemoPauseService([+/].*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Operator\.IDemoPauseStore$")
            .Should().NotDependOnAny(Types(true).That().Are(typeof(IDemoPauseStore)))
            .Because("the demo pause port writes private records, notes, excusals and pauses without an acting user; only demo pause loading may use it (ADR 0013, ADR 0023, slice 7 research R-08)")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void OnlyTheGoalTypesTheThreeHabitServicesTheProfileServiceTheRecordingServiceTheReportServiceAndTheTwoExemptionServicesMayDependOnTheGoalStorePort()
    {
        // Nested types (async state machines, closures) belong to their service, so the names allow a nested suffix.
        Types().That().ResideInAssembly(ArchitectureModel.Application)
            .And().DoNotResideInNamespaceMatching(@"^HT\.Application\.Goals(\..*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Habits\.(CatalogService|CatalogAdministrationService|CustomHabitService)([+/].*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Accounts\.ProfileService([+/].*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Records\.RecordingService([+/].*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Reports\.ReportService([+/].*)?$")
            .And().DoNotHaveFullNameMatching(@"^HT\.Application\.Exemptions\.(ExcusalService|PauseService)([+/].*)?$")
            .Should().NotDependOnAny(Types(true).That().Are(typeof(IGoalStore)))
            .Because("goals are private and reached only through owner-scoped use cases; the habit services need counts and the delete rule, the profile service the time-zone change, the recording service the owner's goal, the report service the owner's goals, the excusal and pause services the owner's goal (constitution III, slice 4 research R-13, slice 5 research R-13, slice 6 research R-14, slice 7 research R-11)")
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
