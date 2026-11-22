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

    /// <summary>
    /// Slice 5 (research R-13): the record store, its mapping and its rows are internal, so records and notes are reached
    /// only through the owner-scoped <c>IRecordStore</c> (constitution III, ADR 0010). The named types must exist, so a
    /// rename cannot make the rule pass on nothing.
    /// </summary>
    [Fact]
    public void TheRecordStoreItsMappingAndItsRowsAreInternal()
    {
        string[] named =
        [
            "HT.Persistence.Records.RecordStore",
            "HT.Persistence.Records.RecordMapping",
            "HT.Persistence.Records.GoalRecordRow",
            "HT.Persistence.Records.GoalNoteRow",
        ];
        var records = ArchitectureModel.Persistence.GetTypes()
            .Where(t => t.Namespace == "HT.Persistence.Records" && !t.IsNested)
            .ToList();

        Assert.All(named, name => Assert.Contains(records, t => t.FullName == name));
        Assert.All(records, t => Assert.False(t.IsPublic, $"{t.FullName} is public."));
        Assert.DoesNotContain(ArchitectureModel.Persistence.GetExportedTypes(), t => t.Namespace == "HT.Persistence.Records");
    }

    /// <summary>
    /// Slice 6 (research R-09, R-14), slice 7 (research R-08) and slice 8 (research R-09): the shared demo account writer,
    /// the demo report store, the demo pause store and the demo goal-change store are internal, so the demo report data
    /// is written only through <c>IDemoReportStore</c>, the demo pause data only through <c>IDemoPauseStore</c> and the
    /// demo goal-change data only through <c>IDemoGoalChangeStore</c> (ADR 0010, ADR 0013, ADR 0023, ADR 0026). The named
    /// types must exist, so a rename cannot make the rule pass on nothing.
    /// </summary>
    [Theory]
    [InlineData("HT.Persistence.Habits.DemoAccountWriter")]
    [InlineData("HT.Persistence.Habits.DemoReportStore")]
    [InlineData("HT.Persistence.Habits.DemoPauseStore")]
    [InlineData("HT.Persistence.Habits.DemoGoalChangeStore")]
    public void TheDemoAccountWriterTheDemoReportStoreTheDemoPauseStoreAndTheDemoGoalChangeStoreAreInternal(string name)
    {
        var type = ArchitectureModel.Persistence.GetType(name);

        Assert.NotNull(type);
        Assert.False(type.IsPublic, $"{name} is public.");
        Assert.False(type.IsNestedPublic, $"{name} is public.");
    }
}
