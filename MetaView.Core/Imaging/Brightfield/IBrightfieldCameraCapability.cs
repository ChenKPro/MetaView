using Vibronix.Foundation.Common.Results;

namespace MetaView.Core.Imaging.Brightfield;

/// <summary>
/// Defines brightfield camera acquisition capabilities for the MetaView platform.
/// </summary>
public interface IBrightfieldCameraCapability
{
    /// <summary>
    /// Gets the current brightfield camera status.
    /// </summary>
    OperationResult<BrightfieldCameraStatus> GetStatus();

    /// <summary>
    /// Enumerates camera ids available for the selected backend.
    /// </summary>
    OperationResult<IReadOnlyList<string>> GetAvailableCameraIds();

    /// <summary>
    /// Initializes and connects the selected camera.
    /// </summary>
    Task<OperationResult> InitializeAsync(BrightfieldCameraSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies camera parameters such as exposure, gain, frame rate, ROI, and trigger mode.
    /// </summary>
    Task<OperationResult> ApplySettingsAsync(BrightfieldCameraSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts continuous live acquisition.
    /// </summary>
    Task<OperationResult> StartLiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops continuous live acquisition.
    /// </summary>
    Task<OperationResult> StopLiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures or retrieves a single frame.
    /// </summary>
    Task<OperationResult<BrightfieldCameraFrame>> CaptureSingleAsync(CancellationToken cancellationToken = default);
}
