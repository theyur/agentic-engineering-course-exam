namespace HT.Application.Operator;

/// <summary>
/// The record result of the demo account with history (FR-030); its text names no goal, habit, note, unit or amount
/// (HP-006). Only an account that demo history loading created in the run has one.
/// </summary>
public sealed class DemoRecordsResult
{
    private DemoRecordsResult(string message) => Message = message;

    /// <summary>The spec's text, written to the operator log.</summary>
    public string Message { get; }

    internal static DemoRecordsResult Added { get; } = new(OperatorTexts.DemoRecordsAdded);
}
