namespace ImageViewer2D.Controls.Models;

/// <summary>
/// Specifies the active interaction mode for the image viewer.
/// </summary>
public enum RoiToolMode
{
    /// <summary>
    /// Allows image panning.
    /// </summary>
    Pan,

    /// <summary>
    /// Allows ROI selection.
    /// </summary>
    Select,

    /// <summary>
    /// Creates rectangle ROI shapes.
    /// </summary>
    Rectangle,

    /// <summary>
    /// Creates ellipse ROI shapes.
    /// </summary>
    Ellipse,

    /// <summary>
    /// Places or edits the red and blue crosshair lines.
    /// </summary>
    Crosshair,

    /// <summary>
    /// Reserves mouse interaction for stage navigation in the host application.
    /// </summary>
    StageNavigation,
}
