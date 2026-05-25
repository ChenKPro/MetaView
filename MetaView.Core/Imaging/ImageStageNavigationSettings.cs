namespace MetaView.Core.Imaging;

/// <summary>
/// Defines how image-viewer mouse interaction maps to stage motion.
/// </summary>
public sealed record ImageStageNavigationSettings
{
    /// <summary>
    /// Gets the sample-space distance represented by one image pixel on X.
    /// </summary>
    public double MicronsPerPixelX { get; init; } = 0.43;

    /// <summary>
    /// Gets the sample-space distance represented by one image pixel on Y.
    /// </summary>
    public double MicronsPerPixelY { get; init; } = 0.43;

    /// <summary>
    /// Gets the Z motion distance for one mouse-wheel notch.
    /// </summary>
    public double WheelStepMicronsZ { get; init; } = 0.5;

    /// <summary>
    /// Gets a value indicating whether image X movement should be inverted before stage motion.
    /// </summary>
    public bool InvertX { get; init; }

    /// <summary>
    /// Gets a value indicating whether image Y movement should be inverted before stage motion.
    /// </summary>
    public bool InvertY { get; init; }

    /// <summary>
    /// Gets a value indicating whether wheel movement should be inverted before Z motion.
    /// </summary>
    public bool InvertZ { get; init; }
}
