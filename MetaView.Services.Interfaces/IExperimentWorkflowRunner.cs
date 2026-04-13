using MetaView.Core.Experiments;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Selects and runs experiment workflows for recipes.
/// </summary>
public interface IExperimentWorkflowRunner
{
    /// <summary>
    /// Runs the workflow that matches the supplied recipe.
    /// </summary>
    Task<OperationResult<ExperimentExecutionResult>> RunAsync(
        ExperimentRecipe recipe,
        CancellationToken cancellationToken = default);
}
