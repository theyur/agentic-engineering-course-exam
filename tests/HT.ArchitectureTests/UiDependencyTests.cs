using ArchUnitNET.xUnit;
using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Application.Operator;
using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HT.ArchitectureTests;

public sealed class UiDependencyTests
{
    [Fact]
    public void BlazorComponentsMustNotDependOnPersistenceTypes()
    {
        var components = Types().That()
            .ResideInNamespaceMatching(@"^HT\.Ui\.Components(\..*)?$");

        var persistence = Types(true).That()
            .ResideInNamespaceMatching(@"^HT\.Persistence(\..*)?$");

        Types().That().Are(components)
            .Should().NotDependOnAny(persistence)
            .Because("Blazor components must use Application abstractions, not Persistence implementations")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void BlazorComponentsMustNotDependOnIdentityManagers()
    {
        Types().That().ResideInNamespaceMatching(@"^HT\.Ui\.Components(\..*)?$")
            .Should().NotDependOnAny(Types(true).That()
                .HaveFullNameMatching(@"^Microsoft\.AspNetCore\.Identity\.(SignInManager|UserManager)`1$"))
            .Because("only AccountSession writes the cookie; components never touch Identity's managers (research R-13)")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void NoUiTypeMayDependOnTheAccountStorePort()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Ui)
            .Should().NotDependOnAny(Types(true).That().Are(typeof(IAccountStore)))
            .Because("the port is persistence plumbing, not a use case; the host reaches accounts only through the use cases (ADR 0010)")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void NoUiTypeMayDependOnTheCatalogHabitStorePort()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Ui)
            .Should().NotDependOnAny(Types(true).That().Are(typeof(ICatalogHabitStore)))
            .Because("the port is persistence plumbing; pages reach catalog habits only through the use cases (ADR 0010, research R-12)")
            .Check(ArchitectureModel.Architecture);
    }

    [Fact]
    public void OnlyTheHostsOperatorTypesMayDependOnTheOperatorServices()
    {
        Types().That().ResideInAssembly(ArchitectureModel.Ui)
            .And().DoNotResideInNamespaceMatching(@"^HT\.Ui\.Operator(\..*)?$")
            .Should().NotDependOnAny(Types(true).That().Are(typeof(OperatorService), typeof(DemoDataService)))
            .Because("no page offers administrator creation or demo loading; only the startup tasks run them (FR-014, FR-016, research R-13)")
            .Check(ArchitectureModel.Architecture);
    }
}
