using MetaView.Core.Reporting;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Capabilities.Reporting;

/// <summary>
/// Placeholder report generation capability for platform composition.
/// </summary>
public sealed class ReportGenerationCapability : IReportGenerationCapability
{
    /// <inheritdoc />
    public Task<OperationResult<string>> GenerateAsync(
        ReportGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperationResult<string>.ErrorFunctionNotImplemented());
    }
}
