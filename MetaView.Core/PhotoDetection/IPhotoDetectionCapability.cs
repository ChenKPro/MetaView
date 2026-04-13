using Vibronix.Foundation.Common.Results;

namespace MetaView.Core.PhotoDetection;

/// <summary>
/// Defines photo-detector operations exposed to experiment workflows.
/// </summary>
public interface IPhotoDetectionCapability
{
    /// <summary>
    /// Initializes the detector channel.
    /// </summary>
    Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads detector samples from the active detector.
    /// </summary>
    Task<OperationResult<IReadOnlyList<double>>> ReadSamplesAsync(
        int sampleCount,
        CancellationToken cancellationToken = default);
}
