namespace MetaView.Core.Experiments;

/// <summary>
/// Defines the path pattern used to sample the requested acquisition dimension.
/// </summary>
public enum ScanPattern
{
    /// <summary>
    /// Single point acquisition.
    /// </summary>
    Point,

    /// <summary>
    /// One-dimensional line scan.
    /// </summary>
    Line,

    /// <summary>
    /// Two-dimensional raster scan.
    /// </summary>
    Raster,

    /// <summary>
    /// Repeated XY planes across Z.
    /// </summary>
    ZStack,

    /// <summary>
    /// Tiled raster acquisition for large-area imaging.
    /// </summary>
    TileGrid,

    /// <summary>
    /// Repeated acquisition over time.
    /// </summary>
    TimeSeries
}
