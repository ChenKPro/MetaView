namespace MetaView.Core.Experiments;

/// <summary>
/// Represents the outcome of an experiment workflow.
/// </summary>
public sealed record ExperimentExecutionResult(
    ExperimentRecipe Recipe,
    IReadOnlyList<ExperimentStepRecord> Steps,
    IReadOnlyList<ExperimentDataProduct> DataProducts,
    string Message)
{
    /// <summary>
    /// Gets a compact workflow summary.
    /// </summary>
    public string Summary => $"{Message} ({Steps.Count(step => step.Success)}/{Steps.Count} steps)";
}
