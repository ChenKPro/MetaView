namespace MetaView.Core.Experiments;

/// <summary>
/// Defines the output data product family produced by a workflow.
/// </summary>
public enum DataProductKind
{
    /// <summary>
    /// A curve, trace, or spectrum.
    /// </summary>
    Curve,

    /// <summary>
    /// A two-dimensional image.
    /// </summary>
    Image,

    /// <summary>
    /// A three-dimensional volume.
    /// </summary>
    Volume,

    /// <summary>
    /// A spectral cube or high-dimensional array.
    /// </summary>
    DataCube,

    /// <summary>
    /// Raw acquisition data.
    /// </summary>
    RawData
}
