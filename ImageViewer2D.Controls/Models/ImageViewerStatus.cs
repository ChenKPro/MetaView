using System.Windows;

namespace ImageViewer2D.Controls.Models;

/// <summary>
/// Describes the current viewer status for display or event output.
/// </summary>
public sealed class ImageViewerStatus
{
    /// <summary>
    /// Gets or sets the current image size in pixels.
    /// </summary>
    public Size ImageSize { get; set; }

    /// <summary>
    /// Gets or sets the current zoom ratio.
    /// </summary>
    public double ZoomRatio { get; set; }

    /// <summary>
    /// Gets or sets the current mouse position in image pixel coordinates.
    /// </summary>
    public Point? MouseImagePosition { get; set; }

    /// <summary>
    /// Gets or sets the number of ROI shapes in the viewer.
    /// </summary>
    public int RoiCount { get; set; }

    /// <summary>
    /// Returns a concise status string.
    /// </summary>
    /// <returns>A status string for UI display.</returns>
    public override string ToString()
    {
        var position = MouseImagePosition is { } point
            ? $"X:{point.X:F1} Y:{point.Y:F1}"
            : "X:- Y:-";

        return $"Image: {ImageSize.Width:F0} x {ImageSize.Height:F0} | Zoom: {ZoomRatio:P0} | {position} | ROI: {RoiCount}";
    }
}
