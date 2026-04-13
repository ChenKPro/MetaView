namespace MetaView.Core.DataAcquisition;

/// <summary>
/// Describes the DAQ runtime configuration used by MetaView capability adapters.
/// </summary>
public sealed class DaqRuntimeConfiguration
{
    /// <summary>
    /// Gets or sets whether the platform should use the demo acquisition service.
    /// </summary>
    public bool UseDemo { get; set; } = true;

    /// <summary>
    /// Gets or sets the DAQ configuration JSON file path for the foundation DAQ service.
    /// </summary>
    public string ConfigurationPath { get; set; } = string.Empty;
}
