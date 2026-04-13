using System.Windows;
using System.Windows.Controls;

namespace MetaView.Presentation.Controls;

/// <summary>
/// Arranges a single child within the largest centered rectangle that matches a target aspect ratio.
/// </summary>
public sealed class RatioBox : Decorator
{
    public static readonly DependencyProperty AspectRatioProperty =
        DependencyProperty.Register(
            nameof(AspectRatio),
            typeof(double),
            typeof(RatioBox),
            new FrameworkPropertyMetadata(16.0 / 9.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double AspectRatio
    {
        get => (double)GetValue(AspectRatioProperty);
        set => SetValue(AspectRatioProperty, value);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        if (Child is null)
        {
            return default;
        }

        var size = GetRatioSize(constraint);
        Child.Measure(size);
        return size;
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (Child is null)
        {
            return arrangeSize;
        }

        var size = GetRatioSize(arrangeSize);
        var x = Math.Max(0, (arrangeSize.Width - size.Width) / 2);
        var y = Math.Max(0, (arrangeSize.Height - size.Height) / 2);
        Child.Arrange(new Rect(new Point(x, y), size));
        return arrangeSize;
    }

    private Size GetRatioSize(Size available)
    {
        var ratio = AspectRatio <= 0 ? 1 : AspectRatio;
        var width = double.IsInfinity(available.Width) ? 0 : available.Width;
        var height = double.IsInfinity(available.Height) ? 0 : available.Height;

        if (width <= 0 || height <= 0)
        {
            return new Size(Math.Max(0, width), Math.Max(0, height));
        }

        var targetHeight = width / ratio;
        if (targetHeight <= height)
        {
            return new Size(width, targetHeight);
        }

        return new Size(height * ratio, height);
    }
}

