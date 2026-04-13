using System.Diagnostics;
using MetaView.Core.Experiments;
using MetaView.Services.Interfaces;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Services.Workflows;

/// <summary>
/// Base implementation for experiment workflows composed from executable steps.
/// </summary>
public abstract class ExperimentWorkflowBase(IWorkflowLogPublisher logPublisher) : IExperimentWorkflow
{
    /// <inheritdoc />
    public abstract string WorkflowId { get; }

    /// <inheritdoc />
    public abstract bool CanRun(ExperimentRecipe recipe);

    /// <inheritdoc />
    public async Task<OperationResult<ExperimentExecutionResult>> RunAsync(
        ExperimentRecipe recipe,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var context = new WorkflowExecutionContext(recipe);
        var records = new List<ExperimentStepRecord>();

        logPublisher.Information($"{WorkflowId} workflow started.");
        foreach (var step in BuildSteps(recipe))
        {
            cancellationToken.ThrowIfCancellationRequested();
            logPublisher.Information($"{step.DisplayName} started.");

            var stopwatch = Stopwatch.StartNew();
            var result = await step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            records.Add(new ExperimentStepRecord(
                step.StepId,
                step.DisplayName,
                result.Success,
                result.Message,
                stopwatch.Elapsed));

            logPublisher.Information(
                $"{step.DisplayName} {(result.Success ? "completed" : "failed")}: {result.Message}");

            if (!result.Success)
            {
                logPublisher.Error($"{WorkflowId} workflow failed.");
                var failedResult = new ExperimentExecutionResult(
                    recipe,
                    records,
                    context.DataProducts,
                    $"{WorkflowId} workflow failed.");
                return OperationResult<ExperimentExecutionResult>.Error(failedResult, failedResult.Message);
            }
        }

        var successResult = new ExperimentExecutionResult(
            recipe,
            records,
            context.DataProducts,
            $"{WorkflowId} workflow completed.");
        logPublisher.Information(successResult.Message);
        return OperationResult<ExperimentExecutionResult>.Ok(successResult, successResult.Message);
    }

    /// <summary>
    /// Builds the ordered workflow steps for a recipe.
    /// </summary>
    protected abstract IReadOnlyList<WorkflowStep> BuildSteps(ExperimentRecipe recipe);
}
