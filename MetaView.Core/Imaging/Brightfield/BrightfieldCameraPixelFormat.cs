namespace MetaView.Core.Imaging.Brightfield;

/// <summary>
/// Describes the pixel layout used by a brightfield camera frame.
/// </summary>
public enum BrightfieldCameraPixelFormat
{
    /// <summary>
    /// Pixel format is not known. Consumers should infer a safe fallback from frame dimensions.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 8-bit monochrome pixels.
    /// </summary>
    Mono8,

    /// <summary>
    /// 16-bit monochrome pixels.
    /// </summary>
    Mono16,

    /// <summary>
    /// 24-bit BGR pixels.
    /// </summary>
    Bgr24,

    /// <summary>
    /// 24-bit RGB pixels.
    /// </summary>
    Rgb24,

    /// <summary>
    /// 32-bit BGRA pixels.
    /// </summary>
    Bgra32,

    /// <summary>
    /// 8-bit Bayer RG mosaic pixels.
    /// </summary>
    BayerRG8,

    /// <summary>
    /// 8-bit Bayer GB mosaic pixels.
    /// </summary>
    BayerGB8,

    /// <summary>
    /// 8-bit Bayer GR mosaic pixels.
    /// </summary>
    BayerGR8,

    /// <summary>
    /// 8-bit Bayer BG mosaic pixels.
    /// </summary>
    BayerBG8
}
