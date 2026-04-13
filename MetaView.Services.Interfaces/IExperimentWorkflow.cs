using MetaView.Core.Experiments;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Executes one family of experiment recipes.
/// </summary>
public interface IExperimentWorkflow
{
    /// <summary>
    /// Gets the stable workflow identifier.
    /// </summary>
    string WorkflowId { get; }

    /// <summary>
    /// Determines whether this workflow can execute the recipe.
    /// </summary>
    bool CanRun(ExperimentRecipe recipe);

    /// <summary>
    /// Executes the recipe.
    /// </summary>
    Task<OperationResult<ExperimentExecutionResult>> RunAsync(
        ExperimentRecipe recipe,
        CancellationToken cancellationToken = default);
}
