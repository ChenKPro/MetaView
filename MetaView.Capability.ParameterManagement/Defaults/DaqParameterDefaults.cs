using MetaView.Capability.ParameterManagement.Sources;
using MetaView.Core.DataAcquisition;

namespace MetaView.Capability.ParameterManagement.Defaults;

/// <summary>
/// Creates DAQ runtime configuration from the configured parameter source.
/// </summary>
internal static class DaqParameterDefaults
{
    /// <summary>
    /// Creates DAQ runtime configuration.
    /// </summary>
    public static DaqRuntimeConfiguration Create(EnvironmentParameterReader reader)
    {
        return new DaqRuntimeConfiguration
        {
            UseDemo = reader.GetBoolean("METAVIEW_DAQ_USE_DEMO", true),
            ConfigurationPath = reader.GetString("METAVIEW_DAQ_CONFIGURATION_PATH", string.Empty)
        };
    }
}
