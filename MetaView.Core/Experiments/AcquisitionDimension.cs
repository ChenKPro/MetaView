namespace MetaView.Core.Experiments;

/// <summary>
/// Defines the dimensionality of an acquisition data product.
/// </summary>
public enum AcquisitionDimension
{
    /// <summary>
    /// One-dimensional data such as spectra, line scans, or time traces.
    /// </summary>
    OneD,

    /// <summary>
    /// Two-dimensional data such as XY images.
    /// </summary>
    TwoD,

    /// <summary>
    /// Three-dimensional data such as XYZ stacks or XY-spectral cubes.
    /// </summary>
    ThreeD,

    /// <summary>
    /// Four-dimensional data such as XYZ-time or XY-Z-spectral acquisitions.
    /// </summary>
    FourD
}
