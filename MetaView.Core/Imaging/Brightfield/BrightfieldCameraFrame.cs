namespace MetaView.Core.Imaging.Brightfield;

/// <summary>
/// Represents a single brightfield camera frame.
/// </summary>
public sealed record BrightfieldCameraFrame(
    string CameraId,
    int Width,
    int Height,
    BrightfieldCameraPixelFormat PixelFormat,
    byte[] Pixels,
    DateTimeOffset Timestamp);
