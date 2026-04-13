namespace MetaView.Capabilities.Algorithms.Rendering;

/// <summary>
/// Provides volume rendering projection algorithms for spectral volumes.
/// </summary>
public static class VolumeRenderer
{
    /// <summary>
    /// Projects a spectral volume along the Z axis using maximum intensity for each X/Y/spectral sample.
    /// </summary>
    /// <param name="values">Flat volume values in X/Y/Z/Spectral axis order.</param>
    /// <param name="xLength">X axis length.</param>
    /// <param name="yLength">Y axis length.</param>
    /// <param name="zLength">Z axis length.</param>
    /// <param name="spectralLength">Spectral axis length.</param>
    /// <returns>Projected flat values in X/Y/Spectral axis order.</returns>
    public static IReadOnlyList<double> ProjectMaxIntensityAlongZ(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int zLength,
        int spectralLength)
    {
        ArgumentNullException.ThrowIfNull(values);

        var expected = xLength * yLength * zLength * spectralLength;
        if (values.Count != expected)
        {
            throw new ArgumentException(
                $"Spectral volume buffer length must match X*Y*Z*Spectral. Expected {expected}, got {values.Count}.",
                nameof(values));
        }

        var projected = new double[xLength * yLength * spectralLength];
        for (var xIndex = 0; xIndex < xLength; xIndex++)
        {
            for (var yIndex = 0; yIndex < yLength; yIndex++)
            {
                for (var spectralIndex = 0; spectralIndex < spectralLength; spectralIndex++)
                {
                    var max = GetValue(values, xIndex, yIndex, 0, spectralIndex, yLength, zLength, spectralLength);
                    for (var zIndex = 1; zIndex < zLength; zIndex++)
                    {
                        var value = GetValue(values, xIndex, yIndex, zIndex, spectralIndex, yLength, zLength, spectralLength);
                        if (value > max)
                        {
                            max = value;
                        }
                    }

                    projected[GetProjectionIndex(xIndex, yIndex, spectralIndex, yLength, spectralLength)] = max;
                }
            }
        }

        return projected;
    }

    private static double GetValue(
        IReadOnlyList<double> values,
        int xIndex,
        int yIndex,
        int zIndex,
        int spectralIndex,
        int yLength,
        int zLength,
        int spectralLength)
    {
        var index = (((xIndex * yLength) + yIndex) * zLength + zIndex) * spectralLength + spectralIndex;
        return values[index];
    }

    private static int GetProjectionIndex(
        int xIndex,
        int yIndex,
        int spectralIndex,
        int yLength,
        int spectralLength)
    {
        return ((xIndex * yLength) + yIndex) * spectralLength + spectralIndex;
    }
}

