using Vibronix.Foundation.Common.Results;

namespace MetaView.Core.Laser;

/// <summary>
/// Defines laser source operations exposed to experiment workflows.
/// </summary>
public interface ILaserControlCapability
{
    /// <summary>
    /// Initializes the laser source from runtime settings.
    /// </summary>
    Task<OperationResult> InitializeAsync(
        LaserRuntimeSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables laser emission.
    /// </summary>
    Task<OperationResult> SetEmissionAsync(
        bool enabled,
        CancellationToken cancellationToken = default);
}
