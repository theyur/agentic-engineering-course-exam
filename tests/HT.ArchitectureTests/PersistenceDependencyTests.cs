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
}
