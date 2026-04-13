
namespace MetaView.Capabilities.Algorithms.Spectral;

/// <summary>
/// Extracts spectra from spatial regions in spectral cubes.
/// </summary>
public static class RoiSpectrumExtractor
{
    /// <summary>
    /// Extracts the average spectrum inside a rectangular X/Y ROI.
    /// </summary>
    /// <param name="values">Flat cube values in X/Y/Spectral order.</param>
    /// <param name="xLength">X axis length.</param>
    /// <param name="yLength">Y axis length.</param>
    /// <param name="spectralLength">Spectral axis length.</param>
    /// <param name="roiX">ROI X start index.</param>
    /// <param name="roiY">ROI Y start index.</param>
    /// <param name="roiWidth">ROI width.</param>
    /// <param name="roiHeight">ROI height.</param>
    /// <returns>Average spectrum across all pixels inside the ROI.</returns>
    public static IReadOnlyList<double> ExtractAverageSpectrum(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int roiX,
        int roiY,
        int roiWidth,
        int roiHeight)
    {
        return ExtractStatistics(
            values,
            xLength,
            yLength,
            spectralLength,
            roiX,
            roiY,
            roiWidth,
            roiHeight).AverageSpectrum;
    }

    /// <summary>
    /// Calculates per-channel statistics inside a rectangular X/Y ROI.
    /// </summary>
    /// <param name="values">Flat cube values in X/Y/Spectral order.</param>
    /// <param name="xLength">X axis length.</param>
    /// <param name="yLength">Y axis length.</param>
    /// <param name="spectralLength">Spectral axis length.</param>
    /// <param name="roiX">ROI X start index.</param>
    /// <param name="roiY">ROI Y start index.</param>
    /// <param name="roiWidth">ROI width.</param>
    /// <param name="roiHeight">ROI height.</param>
    /// <returns>Per-channel ROI statistics.</returns>
    public static SpectralRoiStatistics ExtractStatistics(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int roiX,
        int roiY,
        int roiWidth,
        int roiHeight)
    {
        ValidateCube(values, xLength, yLength, spectralLength);
        ValidateRoi(xLength, yLength, roiX, roiY, roiWidth, roiHeight);

        var sums = new double[spectralLength];
        var sumSquares = new double[spectralLength];
        var minimums = Enumerable.Repeat(double.PositiveInfinity, spectralLength).ToArray();
        var maximums = Enumerable.Repeat(double.NegativeInfinity, spectralLength).ToArray();
        for (var yIndex = roiY; yIndex < roiY + roiHeight; yIndex++)
        {
            for (var xIndex = roiX; xIndex < roiX + roiWidth; xIndex++)
            {
                var baseIndex = ((yIndex * xLength) + xIndex) * spectralLength;
                for (var spectralIndex = 0; spectralIndex < spectralLength; spectralIndex++)
                {
                    var value = values[baseIndex + spectralIndex];
                    sums[spectralIndex] += value;
                    sumSquares[spectralIndex] += value * value;
                    minimums[spectralIndex] = Math.Min(minimums[spectralIndex], value);
                    maximums[spectralIndex] = Math.Max(maximums[spectralIndex], value);
                }
            }
        }

        var pixelCount = roiWidth * roiHeight;
        var averages = new double[spectralLength];
        var standardDeviations = new double[spectralLength];
        for (var spectralIndex = 0; spectralIndex < spectralLength; spectralIndex++)
        {
            var average = sums[spectralIndex] / pixelCount;
            var variance = (sumSquares[spectralIndex] / pixelCount) - (average * average);
            averages[spectralIndex] = average;
            standardDeviations[spectralIndex] = Math.Sqrt(Math.Max(0.0, variance));
        }

        return new SpectralRoiStatistics(
            pixelCount,
            averages,
            minimums,
            maximums,
            standardDeviations);
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
            throw new ArgumentException("ROI spectrum extraction requires positive X, Y and Spectral axis lengths.");
        }

        var expected = xLength * yLength * spectralLength;
        if (values.Count != expected)
        {
            throw new ArgumentException(
                $"Spectral cube value count must match X/Y/Spectral axis product. Expected {expected}, got {values.Count}.",
                nameof(values));
        }
    }

    private static void ValidateRoi(
        int xLength,
        int yLength,
        int roiX,
        int roiY,
        int roiWidth,
        int roiHeight)
    {
        if (roiX < 0 || roiY < 0 || roiWidth <= 0 || roiHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roiX), "ROI must have non-negative origin and positive size.");
        }

        if (roiX + roiWidth > xLength || roiY + roiHeight > yLength)
        {
            throw new ArgumentOutOfRangeException(nameof(roiWidth), "ROI must stay within the spectral cube X/Y bounds.");
        }
    }
}

