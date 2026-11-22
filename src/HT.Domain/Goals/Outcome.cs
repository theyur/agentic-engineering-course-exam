namespace HT.Domain.Goals;

/// <summary>
/// How an attempt of a goal ended (FR-007): never an evaluation state and never counted as a result. Its user-facing
/// texts are "Achieved", "Abandoned" and "No longer relevant".
/// </summary>
public enum Outcome
{
    Achieved,
    Abandoned,
    NoLongerRelevant,
}
