namespace MetaView.Capabilities.Algorithms.Spectral;

/// <summary>
/// Generates spatial feature maps from spectral cube values.
/// </summary>
public static class SpectralMapGenerator
{
    /// <summary>
    /// Extracts the maximum spectral intensity at each X/Y position.
    /// </summary>
    /// <param name="values">Flat cube values in X/Y/Spectral order.</param>
    /// <param name="xLength">X axis length.</param>
    /// <param name="yLength">Y axis length.</param>
    /// <param name="spectralLength">Spectral axis length.</param>
    /// <returns>Flat X/Y feature map values.</returns>
    public static IReadOnlyList<double> ExtractPeakIntensityMap(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int spectralStartIndex = 0,
        int? spectralEndIndex = null)
    {
        ValidateCube(values, xLength, yLength, spectralLength);
        var endIndex = ValidateSpectralRange(spectralLength, spectralStartIndex, spectralEndIndex);

        var map = new double[xLength * yLength];
        for (var yIndex = 0; yIndex < yLength; yIndex++)
        {
            for (var xIndex = 0; xIndex < xLength; xIndex++)
            {
                var baseIndex = ((yIndex * xLength) + xIndex) * spectralLength;
                var peak = values[baseIndex + spectralStartIndex];
                for (var spectralIndex = spectralStartIndex + 1; spectralIndex <= endIndex; spectralIndex++)
                {
                    peak = Math.Max(peak, values[baseIndex + spectralIndex]);
                }

                map[(yIndex * xLength) + xIndex] = peak;
            }
        }

        return map;
    }

    /// <summary>
    /// Extracts the spectral integral at each X/Y position.
    /// </summary>
    /// <param name="values">Flat cube values in X/Y/Spectral order.</param>
    /// <param name="xLength">X axis length.</param>
    /// <param name="yLength">Y axis length.</param>
    /// <param name="spectralLength">Spectral axis length.</param>
    /// <returns>Flat X/Y feature map values.</returns>
    public static IReadOnlyList<double> ExtractIntegralMap(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int spectralStartIndex = 0,
        int? spectralEndIndex = null)
    {
        ValidateCube(values, xLength, yLength, spectralLength);
        var endIndex = ValidateSpectralRange(spectralLength, spectralStartIndex, spectralEndIndex);

        var map = new double[xLength * yLength];
        for (var yIndex = 0; yIndex < yLength; yIndex++)
        {
            for (var xIndex = 0; xIndex < xLength; xIndex++)
            {
                var baseIndex = ((yIndex * xLength) + xIndex) * spectralLength;
                var integral = 0.0;
                for (var spectralIndex = spectralStartIndex; spectralIndex <= endIndex; spectralIndex++)
                {
                    integral += values[baseIndex + spectralIndex];
                }

                map[(yIndex * xLength) + xIndex] = integral;
            }
        }

        return map;
    }

    private static void ValidateCube(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (xLength <= 0 || yLength <= 0 || spectralLength <= 0)
        {
            throw new ArgumentException("Spectral map generation requires positive X, Y and Spectral axis lengths.");
        }

        var expected = xLength * yLength * spectralLength;
        if (values.Count != expected)
        {
            throw new ArgumentException(
                $"Spectral cube value count must match X/Y/Spectral axis product. Expected {expected}, got {values.Count}.",
            nameof(values));
        }
    }

    private static int ValidateSpectralRange(
        int spectralLength,
        int spectralStartIndex,
        int? spectralEndIndex)
    {
        var endIndex = spectralEndIndex ?? spectralLength - 1;
        if (spectralStartIndex < 0 || endIndex < spectralStartIndex || endIndex >= spectralLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(spectralStartIndex),
                "Spectral range must be within the spectral axis and start before or at end.");
        }

        return endIndex;
    }
}

