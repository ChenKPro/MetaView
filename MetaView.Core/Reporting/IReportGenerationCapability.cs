using Vibronix.Foundation.Common.Results;

namespace MetaView.Core.Reporting;

/// <summary>
/// Defines report generation operations for acquisition results.
/// </summary>
public interface IReportGenerationCapability
{
    /// <summary>
    /// Generates a report from a completed experiment.
    /// </summary>
    Task<OperationResult<string>> GenerateAsync(
        ReportGenerationRequest request,
        CancellationToken cancellationToken = default);
}
