namespace MetaView.Core.Experiments;

/// <summary>
/// Captures one executed experiment workflow step.
/// </summary>
public sealed record ExperimentStepRecord(
    string StepId,
    string DisplayName,
    bool Success,
    string Message,
    TimeSpan Elapsed);
