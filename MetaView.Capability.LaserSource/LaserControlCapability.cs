using MetaView.Core.Laser;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Capabilities.LaserControl;

/// <summary>
/// Placeholder laser control capability for platform composition.
/// </summary>
public sealed class LaserControlCapability : ILaserControlCapability
{
    /// <inheritdoc />
    public Task<OperationResult> InitializeAsync(
        LaserRuntimeSettings settings,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperationResult.ErrorFunctionNotImplemented());
    }

    /// <inheritdoc />
    public Task<OperationResult> SetEmissionAsync(
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperationResult.ErrorFunctionNotImplemented());
    }
}
