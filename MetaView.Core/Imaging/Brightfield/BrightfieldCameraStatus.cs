namespace MetaView.Core.Imaging.Brightfield;

/// <summary>
/// Describes the current brightfield camera state.
/// </summary>
public sealed record BrightfieldCameraStatus(
    string CameraType,
    string CameraId,
    bool IsConnected,
    bool IsGrabbing,
    int FrameCount,
    string Message);
