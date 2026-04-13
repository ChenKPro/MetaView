using Vibronix.Foundation.Common.Results;

namespace MetaView.Services.Workflows;

/// <summary>
/// Describes one executable workflow step.
/// </summary>
public sealed class WorkflowStep(
    string stepId,
    string displayName,
    Func<WorkflowExecutionContext, CancellationToken, Task<OperationResult>> executeAsync)
{
    /// <summary>
    /// Gets the stable step identifier.
    /// </summary>
    public string StepId { get; } = stepId;

    /// <summary>
    /// Gets the user-facing step name.
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// Executes the step.
    /// </summary>
    public Task<OperationResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return executeAsync(context, cancellationToken);
    }
}
