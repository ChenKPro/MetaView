namespace MetaView.Capabilities.Algorithms.Rendering;

/// <summary>
/// Builds lightweight 2D preview planes from spectral imaging buffers.
/// </summary>
public static class SpectralImagePreviewBuilder
{
    /// <summary>
    /// Uses an existing feature map as a 2D preview plane.
    /// </summary>
    public static IReadOnlyList<double> FromMap(
        IReadOnlyList<double> values,
        int xLength,
        int yLength)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateLength(values, xLength * yLength, "Spectral map");
        return values.ToArray();
    }

    /// <summary>
    /// Uses a spectral series buffer as a series/spectral preview plane.
    /// </summary>
    public static IReadOnlyList<double> FromSeries(
        IReadOnlyList<double> values,
        int seriesLength,
        int spectralLength)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateLength(values, seriesLength * spectralLength, "Spectral series");
        return values.ToArray();
    }

    /// <summary>
    /// Normalizes preview values into 8-bit grayscale intensities.
    /// </summary>
    public static IReadOnlyList<byte> ToGrayscaleBytes(IReadOnlyList<double> values)
    {
        return ToGrayscaleBytes(values, null, null, false);
    }

    /// <summary>
    /// Normalizes preview values into 8-bit grayscale intensities with optional display range.
    /// </summary>
    public static IReadOnlyList<byte> ToGrayscaleBytes(
        IReadOnlyList<double> values,
        double? minimum,
        double? maximum,
        bool invert)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0)
        {
            return Array.Empty<byte>();
        }

        var finiteValues = values.Where(double.IsFinite).ToArray();
        if (finiteValues.Length == 0)
        {
            return new byte[values.Count];
        }

        var min = minimum ?? finiteValues.Min();
        var max = maximum ?? finiteValues.Max();
        if (!double.IsFinite(min) || !double.IsFinite(max) || max < min)
        {
            throw new ArgumentException("Preview grayscale range must be finite and maximum must be greater than or equal to minimum.");
        }

        if (Math.Abs(max - min) <= double.Epsilon)
        {
            return values.Select(value =>
            {
                var intensity = double.IsFinite(value) ? (byte)255 : (byte)0;
                return invert ? (byte)(255 - intensity) : intensity;
            }).ToArray();
        }

        var scale = 255.0 / (max - min);
        return values
            .Select(value =>
            {
                var intensity = double.IsFinite(value)
                    ? (byte)Math.Clamp(Math.Round((value - min) * scale), 0.0, 255.0)
                    : (byte)0;
                return invert ? (byte)(255 - intensity) : intensity;
            })
            .ToArray();
    }

    /// <summary>
    /// Normalizes preview values into RGB24 heat-map pixels with optional display range.
    /// </summary>
    public static IReadOnlyList<byte> ToHeatMapRgbBytes(
        IReadOnlyList<double> values,
        double? minimum,
        double? maximum,
        bool invert)
    {
        var grayscale = ToGrayscaleBytes(values, minimum, maximum, invert);
        if (grayscale.Count == 0)
        {
            return Array.Empty<byte>();
        }

        var pixels = new byte[grayscale.Count * 3];
        for (var index = 0; index < grayscale.Count; index++)
        {
            var intensity = grayscale[index] / 255.0;
            var red = Math.Clamp(1.5 * intensity - 0.25, 0.0, 1.0);
            var green = Math.Clamp(1.5 - Math.Abs(3.0 * intensity - 1.5), 0.0, 1.0);
            var blue = Math.Clamp(1.25 - 1.5 * intensity, 0.0, 1.0);
            var offset = index * 3;
            pixels[offset] = (byte)Math.Round(red * 255.0);
            pixels[offset + 1] = (byte)Math.Round(green * 255.0);
            pixels[offset + 2] = (byte)Math.Round(blue * 255.0);
        }

        return pixels;
    }

    /// <summary>
    /// Renders spectrum samples into an RGB24 line plot.
    /// </summary>
    public static IReadOnlyList<byte> RenderSpectrumLinePlotRgbBytes(
        IReadOnlyList<double> values,
        int width,
        int height)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        var pixels = Enumerable.Repeat((byte)255, width * height * 3).ToArray();
        if (values.Count == 0)
        {
            return pixels;
        }

        var finiteValues = values.Where(double.IsFinite).ToArray();
        if (finiteValues.Length == 0)
        {
            return pixels;
        }

        var min = finiteValues.Min();
        var max = finiteValues.Max();
        var previousX = 0;
        var previousY = GetSpectrumPlotY(values[0], min, max, height);
        for (var index = 0; index < values.Count; index++)
        {
            var x = values.Count == 1
                ? width / 2
                : (int)Math.Round(index * (width - 1.0) / (values.Count - 1));
            var y = GetSpectrumPlotY(values[index], min, max, height);
            DrawLine(pixels, width, height, previousX, previousY, x, y);
            previousX = x;
            previousY = y;
        }

        return pixels;
    }

    /// <summary>
    /// Extracts one spectral channel from a cube in X/Y/Spectral axis order.
    /// </summary>
    public static IReadOnlyList<double> SliceCubeAtSpectralIndex(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int spectralIndex)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateLength(values, xLength * yLength * spectralLength, "Spectral cube");
        if (spectralIndex < 0 || spectralIndex >= spectralLength)
        {
            throw new ArgumentOutOfRangeException(nameof(spectralIndex));
        }

        var preview = new double[xLength * yLength];
        for (var xIndex = 0; xIndex < xLength; xIndex++)
        {
            for (var yIndex = 0; yIndex < yLength; yIndex++)
            {
                preview[(xIndex * yLength) + yIndex] =
                    values[((xIndex * yLength) + yIndex) * spectralLength + spectralIndex];
            }
        }

        return preview;
    }

    /// <summary>
    /// Projects a spectral volume to X/Y using maximum intensity across Z and spectral axes.
    /// </summary>
    public static IReadOnlyList<double> ProjectVolumeMaxIntensity(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int zLength,
        int spectralLength)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateLength(values, xLength * yLength * zLength * spectralLength, "Spectral volume");

        var preview = new double[xLength * yLength];
        for (var xIndex = 0; xIndex < xLength; xIndex++)
        {
            for (var yIndex = 0; yIndex < yLength; yIndex++)
            {
                var max = GetVolumeValue(values, xIndex, yIndex, 0, 0, yLength, zLength, spectralLength);
                for (var zIndex = 0; zIndex < zLength; zIndex++)
                {
                    for (var spectralIndex = 0; spectralIndex < spectralLength; spectralIndex++)
                    {
                        var value = GetVolumeValue(values, xIndex, yIndex, zIndex, spectralIndex, yLength, zLength, spectralLength);
                        if (value > max)
                        {
                            max = value;
                        }
                    }
                }

                preview[(xIndex * yLength) + yIndex] = max;
            }
        }

        return preview;
    }

    /// <summary>
    /// Projects a spectral volume to X/Y using average intensity across Z and spectral axes.
    /// </summary>
    public static IReadOnlyList<double> ProjectVolumeAverageIntensity(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int zLength,
        int spectralLength)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateLength(values, xLength * yLength * zLength * spectralLength, "Spectral volume");

        var preview = new double[xLength * yLength];
        var divisor = zLength * spectralLength;
        for (var xIndex = 0; xIndex < xLength; xIndex++)
        {
            for (var yIndex = 0; yIndex < yLength; yIndex++)
            {
                var sum = 0.0;
                for (var zIndex = 0; zIndex < zLength; zIndex++)
                {
                    for (var spectralIndex = 0; spectralIndex < spectralLength; spectralIndex++)
                    {
                        sum += GetVolumeValue(values, xIndex, yIndex, zIndex, spectralIndex, yLength, zLength, spectralLength);
                    }
                }

                preview[(xIndex * yLength) + yIndex] = sum / divisor;
            }
        }

        return preview;
    }

    private static double GetVolumeValue(
        IReadOnlyList<double> values,
        int xIndex,
        int yIndex,
        int zIndex,
        int spectralIndex,
        int yLength,
        int zLength,
        int spectralLength)
    {
        return values[(((xIndex * yLength) + yIndex) * zLength + zIndex) * spectralLength + spectralIndex];
    }

    private static int GetSpectrumPlotY(double value, double min, double max, int height)
    {
        if (!double.IsFinite(value))
        {
            return height - 1;
        }

        var normalized = Math.Abs(max - min) <= double.Epsilon
            ? 0.5
            : Math.Clamp((value - min) / (max - min), 0.0, 1.0);
        return height - 1 - (int)Math.Round(normalized * (height - 1));
    }

    private static void DrawLine(
        byte[] pixels,
        int width,
        int height,
        int x0,
        int y0,
        int x1,
        int y1)
    {
        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;
        while (true)
        {
            DrawPixel(pixels, width, height, x0, y0);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            var doubledError = 2 * error;
            if (doubledError >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (doubledError <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private static void DrawPixel(byte[] pixels, int width, int height, int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }

        var offset = ((y * width) + x) * 3;
        pixels[offset] = 24;
        pixels[offset + 1] = 84;
        pixels[offset + 2] = 160;
    }

    private static void ValidateLength(
        IReadOnlyList<double> values,
        int expectedLength,
        string productName)
    {
        if (values.Count != expectedLength)
        {
            throw new ArgumentException(
                $"{productName} buffer length must match expected preview shape. Expected {expectedLength}, got {values.Count}.",
                nameof(values));
        }
    }
}

