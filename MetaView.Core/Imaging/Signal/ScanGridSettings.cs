namespace MetaView.Core.Imaging.Signal;

/// <summary>
/// Defines the XY grid used to convert analog position and laser signals into an image.
/// </summary>
public sealed record ScanGridSettings(int Width = 100, int Height = 100)
{
    /// <summary>
    /// Gets the default grid used by the demo pipeline.
    /// </summary>
    public static ScanGridSettings Default { get; } = new();
}
