using System.Windows;

namespace ImageViewer2D.Controls.Models;

/// <summary>
/// Manages the view transform between image pixel coordinates and control screen coordinates.
/// </summary>
public sealed class ImageViewport
{
    /// <summary>
    /// Gets the current image size in pixels.
    /// </summary>
    public Size ImageSize { get; private set; }

    /// <summary>
    /// Gets the current viewport size in device-independent pixels.
    /// </summary>
    public Size ViewportSize { get; private set; }

    /// <summary>
    /// Gets the current scale from image pixels to screen units.
    /// </summary>
    public double Scale { get; private set; } = 1.0;

    /// <summary>
    /// Gets the scale required to fit the full image inside the viewport.
    /// </summary>
    public double FitScale
    {
        get
        {
            if (!HasImage || ViewportSize.Width <= 0 || ViewportSize.Height <= 0)
            {
                return 1.0;
            }

            return Math.Min(ViewportSize.Width / ImageSize.Width, ViewportSize.Height / ImageSize.Height);
        }
    }

    /// <summary>
    /// Gets the current image origin offset in screen coordinates.
    /// </summary>
    public Point Offset { get; private set; }

    /// <summary>
    /// Sets the image size used by the viewport transform.
    /// </summary>
    /// <param name="imageSize">The image size in pixels.</param>
    public void SetImageSize(Size imageSize)
    {
        ImageSize = imageSize;
    }

    /// <summary>
    /// Sets the visible viewport size used by the viewport transform.
    /// </summary>
    /// <param name="viewportSize">The viewport size in screen units.</param>
    public void SetViewportSize(Size viewportSize)
    {
        ViewportSize = viewportSize;
    }

    /// <summary>
    /// Fits the full image inside the viewport while preserving aspect ratio.
    /// </summary>
    public void FitToView()
    {
        if (!HasImage || ViewportSize.Width <= 0 || ViewportSize.Height <= 0)
        {
            Scale = 1.0;
            Offset = new Point();
            return;
        }

        Scale = FitScale;
        var scaledWidth = ImageSize.Width * Scale;
        var scaledHeight = ImageSize.Height * Scale;

        Offset = new Point(
            (ViewportSize.Width - scaledWidth) / 2.0,
            (ViewportSize.Height - scaledHeight) / 2.0);
    }

    /// <summary>
    /// Converts an image pixel coordinate to a screen coordinate.
    /// </summary>
    /// <param name="imagePoint">The image pixel coordinate.</param>
    /// <returns>The screen coordinate.</returns>
    public Point ImageToScreen(Point imagePoint)
    {
        return new Point(
            imagePoint.X * Scale + Offset.X,
            imagePoint.Y * Scale + Offset.Y);
    }

    /// <summary>
    /// Converts a screen coordinate to an image pixel coordinate.
    /// </summary>
    /// <param name="screenPoint">The screen coordinate.</param>
    /// <returns>The image pixel coordinate.</returns>
    public Point ScreenToImage(Point screenPoint)
    {
        if (Scale <= 0)
        {
            return new Point();
        }

        return new Point(
            (screenPoint.X - Offset.X) / Scale,
            (screenPoint.Y - Offset.Y) / Scale);
    }

    /// <summary>
    /// Pans the image by the given screen-space delta.
    /// </summary>
    /// <param name="delta">The screen-space pan delta.</param>
    public void Pan(Vector delta)
    {
        Offset = new Point(Offset.X + delta.X, Offset.Y + delta.Y);
        ConstrainOffset();
    }

    /// <summary>
    /// Zooms around a screen-space anchor point.
    /// </summary>
    /// <param name="anchor">The screen-space anchor point.</param>
    /// <param name="factor">The multiplicative zoom factor.</param>
    /// <param name="maximumScale">The maximum allowed scale.</param>
    /// <param name="minimumScale">The minimum allowed scale.</param>
    public void ZoomAt(Point anchor, double factor, double maximumScale, double minimumScale = 0.01)
    {
        if (!HasImage || factor <= 0 || Scale <= 0)
        {
            return;
        }

        var imageAnchor = ScreenToImage(anchor);
        var effectiveMinimumScale = Math.Max(minimumScale, FitScale);
        var nextScale = Math.Clamp(Scale * factor, effectiveMinimumScale, maximumScale);
        Scale = nextScale;

        Offset = new Point(
            anchor.X - imageAnchor.X * Scale,
            anchor.Y - imageAnchor.Y * Scale);
        ConstrainOffset(0);
    }

    /// <summary>
    /// Moves the viewport so the given image point appears at the center of the visible area.
    /// </summary>
    /// <param name="imagePoint">The image pixel coordinate to center.</param>
    public void CenterOnImagePoint(Point imagePoint)
    {
        if (!HasImage || ViewportSize.Width <= 0 || ViewportSize.Height <= 0 || Scale <= 0)
        {
            return;
        }

        Offset = new Point(
            ViewportSize.Width / 2.0 - imagePoint.X * Scale,
            ViewportSize.Height / 2.0 - imagePoint.Y * Scale);
        ConstrainOffset(0);
    }

    /// <summary>
    /// Gets the screen-space rectangle occupied by the current image.
    /// </summary>
    /// <returns>The screen-space image rectangle.</returns>
    public Rect GetImageScreenBounds()
    {
        return new Rect(Offset, new Size(ImageSize.Width * Scale, ImageSize.Height * Scale));
    }

    /// <summary>
    /// Gets the current viewport rectangle in image pixel coordinates.
    /// </summary>
    /// <returns>The current visible image rectangle.</returns>
    public Rect GetVisibleImageRect()
    {
        var topLeft = ScreenToImage(new Point(0, 0));
        var bottomRight = ScreenToImage(new Point(ViewportSize.Width, ViewportSize.Height));
        var visible = Rect.Intersect(
            new Rect(topLeft, bottomRight),
            new Rect(new Point(), ImageSize));

        return visible.IsEmpty ? new Rect() : visible;
    }

    private bool HasImage => ImageSize.Width > 0 && ImageSize.Height > 0;

    private void ConstrainOffset(double overscroll = 120.0)
    {
        if (!HasImage || ViewportSize.Width <= 0 || ViewportSize.Height <= 0)
        {
            return;
        }

        var scaledWidth = ImageSize.Width * Scale;
        var scaledHeight = ImageSize.Height * Scale;
        var x = ConstrainAxis(Offset.X, scaledWidth, ViewportSize.Width, overscroll);
        var y = ConstrainAxis(Offset.Y, scaledHeight, ViewportSize.Height, overscroll);

        Offset = new Point(x, y);
    }

    private static double ConstrainAxis(double offset, double contentSize, double viewportSize, double overscroll)
    {
        if (contentSize <= viewportSize)
        {
            return (viewportSize - contentSize) / 2.0;
        }

        var minimum = viewportSize - contentSize - overscroll;
        var maximum = overscroll;
        return Math.Clamp(offset, minimum, maximum);
    }
}
