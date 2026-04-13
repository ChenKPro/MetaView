using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MetaView.Presentation.Core;

namespace MetaView.Presentation.Infrastructure;

/// <summary>
/// Converts Boolean values to visibility.
/// </summary>
public sealed class BooleanToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}

/// <summary>
/// Converts acquisition states to status colors.
/// </summary>
public sealed class StateBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            AcquisitionState.Acquiring or AcquisitionState.LivePreview => new SolidColorBrush(Color.FromRgb(32, 199, 181)),
            AcquisitionState.Capturing => new SolidColorBrush(Color.FromRgb(45, 140, 255)),
            AcquisitionState.Aborting or AcquisitionState.Error or AcquisitionState.EmergencyStopped => new SolidColorBrush(Color.FromRgb(229, 72, 77)),
            _ => new SolidColorBrush(Color.FromRgb(155, 163, 175))
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

