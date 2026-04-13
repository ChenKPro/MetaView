using System.Windows;
using System.Windows.Input;

namespace ImageViewer2D.Controls.Models;

/// <summary>
/// Provides image viewer mouse event data in both control and image coordinates.
/// </summary>
public class ImageViewerMouseEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewerMouseEventArgs" /> class.
    /// </summary>
    /// <param name="controlPosition">The mouse position in control coordinates.</param>
    /// <param name="imagePosition">The mouse position in image pixel coordinates.</param>
    /// <param name="modifiers">The active keyboard modifiers.</param>
    /// <param name="zoomRatio">The current zoom ratio.</param>
    /// <param name="toolMode">The active viewer tool mode.</param>
    /// <param name="hitRoi">The ROI under the mouse pointer, if any.</param>
    public ImageViewerMouseEventArgs(
        Point controlPosition,
        Point imagePosition,
        ModifierKeys modifiers,
        double zoomRatio,
        RoiToolMode toolMode,
        RoiShape? hitRoi)
    {
        ControlPosition = controlPosition;
        ImagePosition = imagePosition;
        Modifiers = modifiers;
        ZoomRatio = zoomRatio;
        ToolMode = toolMode;
        HitRoi = hitRoi;
    }

    /// <summary>
    /// Gets the mouse position in control coordinates.
    /// </summary>
    public Point ControlPosition { get; }

    /// <summary>
    /// Gets the mouse position in image pixel coordinates.
    /// </summary>
    public Point ImagePosition { get; }

    /// <summary>
    /// Gets the active keyboard modifiers.
    /// </summary>
    public ModifierKeys Modifiers { get; }

    /// <summary>
    /// Gets the current zoom ratio.
    /// </summary>
    public double ZoomRatio { get; }

    /// <summary>
    /// Gets the active viewer tool mode.
    /// </summary>
    public RoiToolMode ToolMode { get; }

    /// <summary>
    /// Gets the ROI under the mouse pointer, if any.
    /// </summary>
    public RoiShape? HitRoi { get; }
}

/// <summary>
/// Provides image viewer mouse button event data.
/// </summary>
public sealed class ImageViewerMouseButtonEventArgs : ImageViewerMouseEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewerMouseButtonEventArgs" /> class.
    /// </summary>
    /// <param name="controlPosition">The mouse position in control coordinates.</param>
    /// <param name="imagePosition">The mouse position in image pixel coordinates.</param>
    /// <param name="changedButton">The changed mouse button.</param>
    /// <param name="clickCount">The number of clicks associated with the event.</param>
    /// <param name="modifiers">The active keyboard modifiers.</param>
    /// <param name="zoomRatio">The current zoom ratio.</param>
    /// <param name="toolMode">The active viewer tool mode.</param>
    /// <param name="hitRoi">The ROI under the mouse pointer, if any.</param>
    public ImageViewerMouseButtonEventArgs(
        Point controlPosition,
        Point imagePosition,
        MouseButton changedButton,
        int clickCount,
        ModifierKeys modifiers,
        double zoomRatio,
        RoiToolMode toolMode,
        RoiShape? hitRoi)
        : base(controlPosition, imagePosition, modifiers, zoomRatio, toolMode, hitRoi)
    {
        ChangedButton = changedButton;
        ClickCount = clickCount;
    }

    /// <summary>
    /// Gets the changed mouse button.
    /// </summary>
    public MouseButton ChangedButton { get; }

    /// <summary>
    /// Gets the number of clicks associated with the event.
    /// </summary>
    public int ClickCount { get; }
}

/// <summary>
/// Provides image viewer mouse wheel event data.
/// </summary>
public sealed class ImageViewerMouseWheelEventArgs : ImageViewerMouseEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewerMouseWheelEventArgs" /> class.
    /// </summary>
    /// <param name="controlPosition">The mouse position in control coordinates.</param>
    /// <param name="imagePosition">The mouse position in image pixel coordinates.</param>
    /// <param name="delta">The wheel delta.</param>
    /// <param name="modifiers">The active keyboard modifiers.</param>
    /// <param name="zoomRatio">The current zoom ratio.</param>
    /// <param name="toolMode">The active viewer tool mode.</param>
    /// <param name="hitRoi">The ROI under the mouse pointer, if any.</param>
    public ImageViewerMouseWheelEventArgs(
        Point controlPosition,
        Point imagePosition,
        int delta,
        ModifierKeys modifiers,
        double zoomRatio,
        RoiToolMode toolMode,
        RoiShape? hitRoi)
        : base(controlPosition, imagePosition, modifiers, zoomRatio, toolMode, hitRoi)
    {
        Delta = delta;
    }

    /// <summary>
    /// Gets the wheel delta.
    /// </summary>
    public int Delta { get; }
}
