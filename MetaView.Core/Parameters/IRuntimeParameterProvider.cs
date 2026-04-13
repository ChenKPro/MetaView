using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.DataAcquisition;
using MetaView.Core.Laser;
using MetaView.Core.MotionControl;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Core.Parameters;

/// <summary>
/// Provides runtime parameter access for MetaView capabilities.
/// </summary>
public interface IRuntimeParameterProvider
{
    /// <summary>
    /// Gets the current brightfield camera settings.
    /// </summary>
    OperationResult<BrightfieldCameraSettings> GetBrightfieldCameraSettings();

    /// <summary>
    /// Updates the current brightfield camera settings.
    /// </summary>
    OperationResult SetBrightfieldCameraSettings(BrightfieldCameraSettings settings);

    /// <summary>
    /// Gets the current multi-controller motion system configuration.
    /// </summary>
    OperationResult<MotionSystemConfiguration> GetMotionSystemConfiguration();

    /// <summary>
    /// Updates the current multi-controller motion system configuration.
    /// </summary>
    OperationResult SetMotionSystemConfiguration(MotionSystemConfiguration configuration);

    /// <summary>
    /// Gets the current DAQ runtime configuration.
    /// </summary>
    OperationResult<DaqRuntimeConfiguration> GetDaqRuntimeConfiguration();

    /// <summary>
    /// Updates the current DAQ runtime configuration.
    /// </summary>
    OperationResult SetDaqRuntimeConfiguration(DaqRuntimeConfiguration configuration);

    /// <summary>
    /// Gets the current laser runtime settings.
    /// </summary>
    OperationResult<LaserRuntimeSettings> GetLaserRuntimeSettings();

    /// <summary>
    /// Updates the current laser runtime settings.
    /// </summary>
    OperationResult SetLaserRuntimeSettings(LaserRuntimeSettings settings);
}
