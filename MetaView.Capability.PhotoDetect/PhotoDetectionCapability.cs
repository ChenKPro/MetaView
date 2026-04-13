using MetaView.Core.PhotoDetection;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Capabilities.PhotoDetection;

/// <summary>
/// Placeholder photo-detection capability for platform composition.
/// </summary>
public sealed class PhotoDetectionCapability : IPhotoDetectionCapability
{
    /// <inheritdoc />
    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperationResult.ErrorFunctionNotImplemented());
    }

    /// <inheritdoc />
    public Task<OperationResult<IReadOnlyList<double>>> ReadSamplesAsync(
        int sampleCount,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperationResult<IReadOnlyList<double>>.ErrorFunctionNotImplemented());
    }
}
