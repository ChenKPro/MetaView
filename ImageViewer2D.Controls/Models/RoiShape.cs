using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace ImageViewer2D.Controls.Models;

/// <summary>
/// Represents an ROI shape stored in original image pixel coordinates.
/// </summary>
public abstract class RoiShape : INotifyPropertyChanged
{
    private Point _startPoint;
    private Point _endPoint;
    private Brush _stroke = Brushes.LimeGreen;
    private double _strokeThickness = 2.0;
    private bool _isSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoiShape" /> class.
    /// </summary>
    /// <param name="startPoint">The first image-space point.</param>
    /// <param name="endPoint">The second image-space point.</param>
    protected RoiShape(Point startPoint, Point endPoint)
    {
        _startPoint = startPoint;
        _endPoint = endPoint;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the stable ROI identifier.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the first image-space point.
    /// </summary>
    public Point StartPoint
    {
        get => _startPoint;
        set
        {
            _startPoint = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Bounds));
        }
    }

    /// <summary>
    /// Gets or sets the second image-space point.
    /// </summary>
    public Point EndPoint
    {
        get => _endPoint;
        set
        {
            _endPoint = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Bounds));
        }
    }

    /// <summary>
    /// Gets or sets the ROI stroke brush.
    /// </summary>
    public Brush Stroke
    {
        get => _stroke;
        set
        {
            _stroke = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the ROI stroke thickness in screen units.
    /// </summary>
    public double StrokeThickness
    {
        get => _strokeThickness;
        set
        {
            _strokeThickness = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the ROI is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the normalized ROI bounds in image pixel coordinates.
    /// </summary>
    public Rect Bounds
    {
        get
        {
            var x = Math.Min(StartPoint.X, EndPoint.X);
            var y = Math.Min(StartPoint.Y, EndPoint.Y);
            var width = Math.Abs(StartPoint.X - EndPoint.X);
            var height = Math.Abs(StartPoint.Y - EndPoint.Y);
            return new Rect(x, y, width, height);
        }
    }

    /// <summary>
    /// Gets a value that identifies the visual ROI kind.
    /// </summary>
    public abstract RoiShapeKind Kind { get; }

    /// <summary>
    /// Moves the ROI by an image-space delta.
    /// </summary>
    /// <param name="delta">The image-space delta.</param>
    public void MoveBy(Vector delta)
    {
        StartPoint += delta;
        EndPoint += delta;
    }

    /// <summary>
    /// Replaces the ROI bounds in image pixel coordinates.
    /// </summary>
    /// <param name="bounds">The new image-space bounds.</param>
    public void SetBounds(Rect bounds)
    {
        StartPoint = bounds.TopLeft;
        EndPoint = bounds.BottomRight;
    }

    /// <summary>
    /// Determines whether the ROI contains an image-space point.
    /// </summary>
    /// <param name="point">The image-space point.</param>
    /// <returns><see langword="true" /> if the ROI contains the point; otherwise, <see langword="false" />.</returns>
    public abstract bool Contains(Point point);

    /// <summary>
    /// Determines whether an image-space point is close to the ROI boundary.
    /// </summary>
    /// <param name="point">The image-space point.</param>
    /// <param name="tolerance">The image-space hit tolerance.</param>
    /// <returns><see langword="true" /> if the point is near the ROI boundary; otherwise, <see langword="false" />.</returns>
    public abstract bool IsNearBoundary(Point point, double tolerance);

    /// <summary>
    /// Raises the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="propertyName">The changed property name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Identifies supported ROI visual shapes.
/// </summary>
public enum RoiShapeKind
{
    /// <summary>
    /// A rectangle ROI.
    /// </summary>
    Rectangle,

    /// <summary>
    /// An ellipse ROI.
    /// </summary>
    Ellipse,
}

/// <summary>
/// Represents a rectangle ROI stored in image pixel coordinates.
/// </summary>
public sealed class RectangleRoi : RoiShape
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleRoi" /> class.
    /// </summary>
    /// <param name="startPoint">The first image-space point.</param>
    /// <param name="endPoint">The second image-space point.</param>
    public RectangleRoi(Point startPoint, Point endPoint)
        : base(startPoint, endPoint)
    {
    }

    /// <inheritdoc />
    public override RoiShapeKind Kind => RoiShapeKind.Rectangle;

    /// <inheritdoc />
    public override bool Contains(Point point)
    {
        return Bounds.Contains(point);
    }

    /// <inheritdoc />
    public override bool IsNearBoundary(Point point, double tolerance)
    {
        var bounds = Bounds;
        var outer = bounds;
        outer.Inflate(tolerance, tolerance);

        if (!outer.Contains(point))
        {
            return false;
        }

        var inner = bounds;
        inner.Inflate(-tolerance, -tolerance);

        return inner.Width <= 0 || inner.Height <= 0 || !inner.Contains(point);
    }
}

/// <summary>
/// Represents an ellipse ROI stored in image pixel coordinates.
/// </summary>
public sealed class EllipseRoi : RoiShape
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EllipseRoi" /> class.
    /// </summary>
    /// <param name="startPoint">The first image-space point.</param>
    /// <param name="endPoint">The second image-space point.</param>
    public EllipseRoi(Point startPoint, Point endPoint)
        : base(startPoint, endPoint)
    {
    }

    /// <inheritdoc />
    public override RoiShapeKind Kind => RoiShapeKind.Ellipse;

    /// <inheritdoc />
    public override bool Contains(Point point)
    {
        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return false;
        }

        var radiusX = bounds.Width / 2.0;
        var radiusY = bounds.Height / 2.0;
        var centerX = bounds.X + radiusX;
        var centerY = bounds.Y + radiusY;
        var normalizedX = (point.X - centerX) / radiusX;
        var normalizedY = (point.Y - centerY) / radiusY;

        return normalizedX * normalizedX + normalizedY * normalizedY <= 1.0;
    }

    /// <inheritdoc />
    public override bool IsNearBoundary(Point point, double tolerance)
    {
        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return false;
        }

        var radiusX = bounds.Width / 2.0;
        var radiusY = bounds.Height / 2.0;
        var centerX = bounds.X + radiusX;
        var centerY = bounds.Y + radiusY;
        var normalizedX = (point.X - centerX) / radiusX;
        var normalizedY = (point.Y - centerY) / radiusY;
        var normalizedDistance = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
        var normalizedTolerance = Math.Max(tolerance / radiusX, tolerance / radiusY);

        return Math.Abs(normalizedDistance - 1.0) <= normalizedTolerance;
    }
}
