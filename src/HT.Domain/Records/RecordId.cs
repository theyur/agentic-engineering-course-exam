namespace HT.Domain.Records;

/// <summary>The identifier of a record (FR-003). Created by the record store, never by a use-case caller.</summary>
public readonly record struct RecordId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
