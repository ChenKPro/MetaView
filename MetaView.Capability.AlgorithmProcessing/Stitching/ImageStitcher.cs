namespace MetaView.Capabilities.Algorithms.Stitching;

/// <summary>
/// Provides stitching algorithms for spectral image buffers.
/// </summary>
public static class ImageStitcher
{
    /// <summary>
    /// Stitches two spectral image buffers along the X axis.
    /// </summary>
    /// <param name="leftValues">Left image flat buffer.</param>
    /// <param name="rightValues">Right image flat buffer.</param>
    /// <param name="leftXLength">Left image X length.</param>
    /// <param name="rightXLength">Right image X length.</param>
    /// <param name="yLength">Shared Y length.</param>
    /// <param name="spectralLength">Shared spectral length.</param>
    /// <returns>Stitched flat buffer.</returns>
    public static IReadOnlyList<double> StitchAlongX(
        IReadOnlyList<double> leftValues,
        IReadOnlyList<double> rightValues,
        int leftXLength,
        int rightXLength,
        int yLength,
        int spectralLength)
    {
        ArgumentNullException.ThrowIfNull(leftValues);
        ArgumentNullException.ThrowIfNull(rightValues);

        ValidateLength(leftValues, leftXLength, yLength, spectralLength, nameof(leftValues));
        ValidateLength(rightValues, rightXLength, yLength, spectralLength, nameof(rightValues));

        var stitched = new double[(leftXLength + rightXLength) * yLength * spectralLength];
        var targetOffset = 0;
        for (var yIndex = 0; yIndex < yLength; yIndex++)
        {
            targetOffset = CopyRow(leftValues, stitched, yIndex, leftXLength, spectralLength, targetOffset);
            targetOffset = CopyRow(rightValues, stitched, yIndex, rightXLength, spectralLength, targetOffset);
        }

        return stitched;
    }

    private static int CopyRow(
        IReadOnlyList<double> source,
        double[] target,
        int yIndex,
        int xLength,
        int spectralLength,
        int targetOffset)
    {
        var sourceOffset = yIndex * xLength * spectralLength;
        var sourceCount = xLength * spectralLength;
        for (var index = 0; index < sourceCount; index++)
        {
            target[targetOffset + index] = source[sourceOffset + index];
        }

        return targetOffset + sourceCount;
    }

    private static void ValidateLength(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        string parameterName)
    {
        var expected = xLength * yLength * spectralLength;
        if (values.Count != expected)
        {
            throw new ArgumentException(
                $"Spectral image buffer length must match X*Y*Spectral. Expected {expected}, got {values.Count}.",
                parameterName);
        }
    }
}

