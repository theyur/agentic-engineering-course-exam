namespace HT.Domain.Goals;

/// <summary>The identifier of a goal. Created by the goal store, never by a use-case caller.</summary>
public readonly record struct GoalId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
