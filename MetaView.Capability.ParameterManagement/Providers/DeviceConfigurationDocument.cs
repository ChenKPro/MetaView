using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging;
using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.Laser;
using MetaView.Core.MotionControl;

namespace MetaView.Capability.ParameterManagement.Providers;

/// <summary>
/// Root JSON document for MetaView device runtime configuration.
/// </summary>
internal sealed record DeviceConfigurationDocument
{
    /// <summary>
    /// Gets the optional brightfield camera settings.
    /// </summary>
    public BrightfieldCameraSettings? BrightfieldCamera { get; init; }

    /// <summary>
    /// Gets the optional motion system configuration.
    /// </summary>
    public MotionSystemConfiguration? MotionSystem { get; init; }

    /// <summary>
    /// Gets the optional DAQ runtime configuration.
    /// </summary>
    public DaqRuntimeConfiguration? Daq { get; init; }

    /// <summary>
    /// Gets the optional laser runtime settings.
    /// </summary>
    public LaserRuntimeSettings? Laser { get; init; }

    /// <summary>
    /// Gets the optional image-to-stage navigation settings.
    /// </summary>
    public ImageStageNavigationSettings? ImageStageNavigation { get; init; }
}
