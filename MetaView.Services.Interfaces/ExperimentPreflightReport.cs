namespace MetaView.Services.Interfaces;

/// <summary>
/// Contains workflow preflight validation results.
/// </summary>
public sealed record ExperimentPreflightReport(IReadOnlyList<ExperimentPreflightIssue> Issues)
{
    /// <summary>
    /// Gets a value indicating whether workflow execution can continue.
    /// </summary>
    public bool CanRun => Issues.All(issue => !issue.IsBlocking);
}
