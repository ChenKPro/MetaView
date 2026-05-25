using MetaView.Capability.ParameterManagement.Sources;
using MetaView.Core.Imaging;

namespace MetaView.Capability.ParameterManagement.Defaults;

/// <summary>
/// Creates image-to-stage navigation settings from the configured parameter source.
/// </summary>
internal static class ImageStageNavigationParameterDefaults
{
    /// <summary>
    /// Creates the image-to-stage navigation settings.
    /// </summary>
    public static ImageStageNavigationSettings Create(EnvironmentParameterReader reader)
    {
        return new ImageStageNavigationSettings
        {
            MicronsPerPixelX = reader.GetDouble("METAVIEW_IMAGE_STAGE_MICRONS_PER_PIXEL_X", 0.43),
            MicronsPerPixelY = reader.GetDouble("METAVIEW_IMAGE_STAGE_MICRONS_PER_PIXEL_Y", 0.43),
            WheelStepMicronsZ = reader.GetDouble("METAVIEW_IMAGE_STAGE_WHEEL_STEP_MICRONS_Z", 0.5),
            InvertX = reader.GetBoolean("METAVIEW_IMAGE_STAGE_INVERT_X", false),
            InvertY = reader.GetBoolean("METAVIEW_IMAGE_STAGE_INVERT_Y", false),
            InvertZ = reader.GetBoolean("METAVIEW_IMAGE_STAGE_INVERT_Z", false)
        };
    }
}
