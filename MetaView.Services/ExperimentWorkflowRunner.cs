using MetaView.Core.Experiments;
using MetaView.Services.Interfaces;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Services;

/// <summary>
/// Runs experiment recipes through the planned workflow.
/// </summary>
public sealed class ExperimentWorkflowRunner(
    IEnumerable<IExperimentWorkflow> workflows,
    IExperimentPreflightValidator preflightValidator,
    IWorkflowLogPublisher logPublisher)
    : IExperimentWorkflowRunner
{
    private readonly IReadOnlyList<IExperimentWorkflow> _workflows = workflows.ToArray();

    /// <inheritdoc />
    public async Task<OperationResult<ExperimentExecutionResult>> RunAsync(
        ExperimentRecipe recipe,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var preflight = preflightValidator.Validate(recipe);
        foreach (var issue in preflight.Issues)
        {
            if (issue.IsBlocking)
            {
                logPublisher.Error($"Preflight: {issue.Message}");
            }
            else
            {
                logPublisher.Information($"Preflight: {issue.Message}");
            }
        }

        if (!preflight.CanRun)
        {
            return OperationResult<ExperimentExecutionResult>.Error("Workflow preflight failed.");
        }

        var workflow = _workflows.FirstOrDefault(candidate => candidate.CanRun(recipe));
        if (workflow is null)
        {
            return OperationResult<ExperimentExecutionResult>.Error(
                $"No workflow is registered for {recipe.Dimension} + {recipe.Modality}.");
        }

        return await workflow.RunAsync(recipe, cancellationToken).ConfigureAwait(false);
    }
}
