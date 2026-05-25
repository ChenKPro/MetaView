using MetaView.Capability.ParameterManagement.Defaults;
using MetaView.Capability.ParameterManagement.Sources;
using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging;
using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.Laser;
using MetaView.Core.MotionControl;
using MetaView.Core.Parameters;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Capability.ParameterManagement.Providers;

/// <summary>
/// Provides MetaView runtime parameters from environment variables with in-memory overrides.
/// </summary>
public sealed class EnvironmentRuntimeParameterProvider : IRuntimeParameterProvider
{
    private readonly object _syncRoot = new();
    private readonly EnvironmentParameterReader _reader = new();
    private BrightfieldCameraSettings? _brightfieldCameraSettings;
    private MotionSystemConfiguration? _motionSystemConfiguration;
    private DaqRuntimeConfiguration? _daqRuntimeConfiguration;
    private LaserRuntimeSettings? _laserRuntimeSettings;
    private ImageStageNavigationSettings? _imageStageNavigationSettings;

    /// <inheritdoc />
    public OperationResult<BrightfieldCameraSettings> GetBrightfieldCameraSettings()
    {
        lock (_syncRoot)
        {
            _brightfieldCameraSettings ??= BrightfieldCameraParameterDefaults.Create(_reader);
            return OperationResult<BrightfieldCameraSettings>.Ok(_brightfieldCameraSettings, "Brightfield camera settings loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetBrightfieldCameraSettings(BrightfieldCameraSettings settings)
    {
        lock (_syncRoot)
        {
            _brightfieldCameraSettings = settings;
        }

        return OperationResult.Ok("Brightfield camera settings updated.");
    }

    /// <inheritdoc />
    public OperationResult<MotionSystemConfiguration> GetMotionSystemConfiguration()
    {
        lock (_syncRoot)
        {
            _motionSystemConfiguration ??= MotionParameterDefaults.CreateSystem(_reader);
            return OperationResult<MotionSystemConfiguration>.Ok(_motionSystemConfiguration, "Motion system configuration loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetMotionSystemConfiguration(MotionSystemConfiguration configuration)
    {
        lock (_syncRoot)
        {
            _motionSystemConfiguration = configuration;
        }

        return OperationResult.Ok("Motion system configuration updated.");
    }

    /// <inheritdoc />
    public OperationResult<DaqRuntimeConfiguration> GetDaqRuntimeConfiguration()
    {
        lock (_syncRoot)
        {
            _daqRuntimeConfiguration ??= DaqParameterDefaults.Create(_reader);
            return OperationResult<DaqRuntimeConfiguration>.Ok(_daqRuntimeConfiguration, "DAQ runtime configuration loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetDaqRuntimeConfiguration(DaqRuntimeConfiguration configuration)
    {
        lock (_syncRoot)
        {
            _daqRuntimeConfiguration = configuration;
        }

        return OperationResult.Ok("DAQ runtime configuration updated.");
    }

    /// <inheritdoc />
    public OperationResult<LaserRuntimeSettings> GetLaserRuntimeSettings()
    {
        lock (_syncRoot)
        {
            _laserRuntimeSettings ??= LaserParameterDefaults.Create(_reader);
            return OperationResult<LaserRuntimeSettings>.Ok(_laserRuntimeSettings, "Laser runtime settings loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetLaserRuntimeSettings(LaserRuntimeSettings settings)
    {
        lock (_syncRoot)
        {
            _laserRuntimeSettings = settings;
        }

        return OperationResult.Ok("Laser runtime settings updated.");
    }

    /// <inheritdoc />
    public OperationResult<ImageStageNavigationSettings> GetImageStageNavigationSettings()
    {
        lock (_syncRoot)
        {
            _imageStageNavigationSettings ??= ImageStageNavigationParameterDefaults.Create(_reader);
            return OperationResult<ImageStageNavigationSettings>.Ok(_imageStageNavigationSettings, "Image-stage navigation settings loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetImageStageNavigationSettings(ImageStageNavigationSettings settings)
    {
        lock (_syncRoot)
        {
            _imageStageNavigationSettings = settings;
        }

        return OperationResult.Ok("Image-stage navigation settings updated.");
    }
}
