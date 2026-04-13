using MetaView.Capability.ParameterManagement.Sources;
using MetaView.Core.Imaging.Brightfield;

namespace MetaView.Capability.ParameterManagement.Defaults;

/// <summary>
/// Creates brightfield camera settings from the configured parameter source.
/// </summary>
internal static class BrightfieldCameraParameterDefaults
{
    /// <summary>
    /// Creates brightfield camera settings.
    /// </summary>
    public static BrightfieldCameraSettings Create(EnvironmentParameterReader reader)
    {
        return new BrightfieldCameraSettings
        {
            CameraType = reader.GetString("METAVIEW_BRIGHTFIELD_CAMERA_TYPE", "Demo"),
            CameraId = reader.GetString("METAVIEW_BRIGHTFIELD_CAMERA_ID", string.Empty),
            ExposureTime = reader.GetUInt32("METAVIEW_BRIGHTFIELD_EXPOSURE_US", 10000),
            Gain = reader.GetSingle("METAVIEW_BRIGHTFIELD_GAIN", 1),
            Gamma = reader.GetSingle("METAVIEW_BRIGHTFIELD_GAMMA", 1),
            FrameRate = reader.GetUInt32("METAVIEW_BRIGHTFIELD_FRAME_RATE", 10),
            RoiOffsetX = reader.GetInt32("METAVIEW_BRIGHTFIELD_ROI_X", 0),
            RoiOffsetY = reader.GetInt32("METAVIEW_BRIGHTFIELD_ROI_Y", 0),
            RoiWidth = reader.GetInt32("METAVIEW_BRIGHTFIELD_ROI_WIDTH", 0),
            RoiHeight = reader.GetInt32("METAVIEW_BRIGHTFIELD_ROI_HEIGHT", 0),
            TriggerEnabled = reader.GetBoolean("METAVIEW_BRIGHTFIELD_TRIGGER_ENABLED", false),
            TriggerSource = reader.GetString("METAVIEW_BRIGHTFIELD_TRIGGER_SOURCE", "Software")
        };
    }
}
