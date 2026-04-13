namespace MetaView.Core.Experiments;

/// <summary>
/// Defines the primary processing pipeline requested for an experiment.
/// </summary>
public enum ProcessingMode
{
    /// <summary>
    /// No algorithmic post-processing beyond raw data publication.
    /// </summary>
    Raw,

    /// <summary>
    /// Build a curve or spectrum product.
    /// </summary>
    Spectrum,

    /// <summary>
    /// Build a two-dimensional signal image.
    /// </summary>
    SignalImage,

    /// <summary>
    /// Build a spectral cube.
    /// </summary>
    SpectralCube,

    /// <summary>
    /// Build a volume product.
    /// </summary>
    Volume,

    /// <summary>
    /// Stitch multiple fields into a single product.
    /// </summary>
    Stitching
}
