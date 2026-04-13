using MetaView.Core.Experiments;
using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.Imaging.Signal;
using MetaView.Core.MotionControl;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Invokes platform capabilities for experiment workflows without exposing hardware implementations.
/// </summary>
public interface IExperimentCapabilityInvoker
{
    /// <summary>
    /// Initializes the capabilities required by a plan.
    /// </summary>
    Task<OperationResult> InitializeAsync(
        CapabilityPlan plan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops capabilities that should be stopped after a plan finishes.
    /// </summary>
    Task<OperationResult> StopAsync(
        CapabilityPlan plan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a logical motion axis by a relative distance.
    /// </summary>
    Task<OperationResult> MoveRelativeAsync(
        MotionAxis axis,
        double distance,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts DAQ acquisition.
    /// </summary>
    Task<OperationResult> StartDaqAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for DAQ samples and optionally publishes a demo signal image frame.
    /// </summary>
    Task<OperationResult> AcquireDaqAsync(
        TimeSpan duration,
        ScanGridSettings gridSettings,
        bool publishDemoFrame,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops DAQ acquisition.
    /// </summary>
    Task<OperationResult> StopDaqAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts brightfield live acquisition.
    /// </summary>
    Task<OperationResult> StartBrightfieldLiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops brightfield live acquisition.
    /// </summary>
    Task<OperationResult> StopBrightfieldLiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures one brightfield frame.
    /// </summary>
    Task<OperationResult<BrightfieldCameraFrame>> CaptureBrightfieldAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a demo signal image and trace frame through the realtime imaging path.
    /// </summary>
    OperationResult PublishSignalPreview(ScanGridSettings gridSettings);

    /// <summary>
    /// Runs a small algorithm smoke check for workflows that require algorithm capability.
    /// </summary>
    OperationResult RunAlgorithmPreview();
}
