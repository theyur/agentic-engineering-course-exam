namespace HT.Domain.Exemptions;

/// <summary>The identifier of a pause (FR-006, FR-011). Created by the record store, never by a use-case caller.</summary>
public readonly record struct PauseId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
