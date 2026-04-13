using Vibronix.Foundation.Common.Results;

namespace MetaView.Core.MotionControl;

/// <summary>
/// Defines the platform-level motion control contract consumed by presentation and workflows.
/// </summary>
public interface IMotionControlCapability
{
    /// <summary>
    /// Gets the current logical stage position.
    /// </summary>
    MotionPosition CurrentPosition { get; }

    /// <summary>
    /// Initializes the motion subsystem.
    /// </summary>
    Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts periodic status monitoring in the motion capability.
    /// </summary>
    Task<OperationResult> StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops periodic status monitoring in the motion capability.
    /// </summary>
    Task<OperationResult> StopMonitoringAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Homes one logical axis.
    /// </summary>
    Task<OperationResult> HomeAsync(MotionAxis axis, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves one logical axis by a relative distance.
    /// </summary>
    Task<OperationResult> MoveRelativeAsync(MotionAxis axis, double distance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves one logical axis to an absolute position.
    /// </summary>
    Task<OperationResult> MoveAbsoluteAsync(MotionAxis axis, double position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops all motion.
    /// </summary>
    Task<OperationResult> StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status for one logical axis.
    /// </summary>
    Task<OperationResult<MotionAxisStatus>> GetStatusAsync(MotionAxis axis, CancellationToken cancellationToken = default);
}
