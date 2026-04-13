namespace MetaView.Core.Imaging.Brightfield;

/// <summary>
/// Contains runtime settings for the brightfield camera.
/// </summary>
public sealed record BrightfieldCameraSettings
{
    /// <summary>
    /// Gets the camera backend type. Use Demo when hardware is unavailable.
    /// </summary>
    public string CameraType { get; init; } = "Demo";

    /// <summary>
    /// Gets the requested camera id. When empty, the first detected hardware camera is used.
    /// </summary>
    public string CameraId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the exposure time in microseconds.
    /// </summary>
    public uint? ExposureTime { get; init; } = 10000;

    /// <summary>
    /// Gets the analog gain.
    /// </summary>
    public float? Gain { get; init; } = 1;

    /// <summary>
    /// Gets the gamma value.
    /// </summary>
    public float? Gamma { get; init; } = 1;

    /// <summary>
    /// Gets the acquisition frame rate.
    /// </summary>
    public uint? FrameRate { get; init; } = 30;

    /// <summary>
    /// Gets the ROI offset in pixels on X.
    /// </summary>
    public int RoiOffsetX { get; init; }

    /// <summary>
    /// Gets the ROI offset in pixels on Y.
    /// </summary>
    public int RoiOffsetY { get; init; }

    /// <summary>
    /// Gets the ROI width in pixels. A value less than or equal to zero keeps the hardware default.
    /// </summary>
    public int RoiWidth { get; init; }

    /// <summary>
    /// Gets the ROI height in pixels. A value less than or equal to zero keeps the hardware default.
    /// </summary>
    public int RoiHeight { get; init; }

    /// <summary>
    /// Gets a value indicating whether hardware trigger mode is enabled.
    /// </summary>
    public bool TriggerEnabled { get; init; }

    /// <summary>
    /// Gets the trigger source name supported by the Foundation camera SDK.
    /// </summary>
    public string TriggerSource { get; init; } = "Software";
}
