namespace MetaView.Core.Laser;

/// <summary>
/// Contains runtime settings for laser-related controls.
/// </summary>
public sealed record LaserRuntimeSettings
{
    /// <summary>
    /// Gets the laser backend type. Use Demo when hardware is unavailable.
    /// </summary>
    public string LaserType { get; init; } = "Demo";

    /// <summary>
    /// Gets the device id or resource name.
    /// </summary>
    public string DeviceId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the requested laser power in milliwatts.
    /// </summary>
    public double PowerMilliWatts { get; init; } = 10;

    /// <summary>
    /// Gets the warmup timeout in milliseconds.
    /// </summary>
    public int WarmupTimeoutMs { get; init; } = 5000;

    /// <summary>
    /// Gets a value indicating whether laser emission should be enabled by default.
    /// </summary>
    public bool EmissionEnabled { get; init; } = true;
}
