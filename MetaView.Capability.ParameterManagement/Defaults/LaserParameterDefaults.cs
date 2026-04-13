using MetaView.Capability.ParameterManagement.Sources;
using MetaView.Core.Laser;

namespace MetaView.Capability.ParameterManagement.Defaults;

/// <summary>
/// Creates laser runtime settings from the configured parameter source.
/// </summary>
internal static class LaserParameterDefaults
{
    /// <summary>
    /// Creates laser runtime settings.
    /// </summary>
    public static LaserRuntimeSettings Create(EnvironmentParameterReader reader)
    {
        return new LaserRuntimeSettings
        {
            LaserType = reader.GetString("METAVIEW_LASER_TYPE", "Demo"),
            DeviceId = reader.GetString("METAVIEW_LASER_DEVICE_ID", string.Empty),
            PowerMilliWatts = reader.GetDouble("METAVIEW_LASER_POWER_MW", 10),
            WarmupTimeoutMs = reader.GetInt32("METAVIEW_LASER_WARMUP_TIMEOUT_MS", 5000),
            EmissionEnabled = reader.GetBoolean("METAVIEW_LASER_EMISSION_ENABLED", true)
        };
    }
}
