namespace MetaView.Capabilities.Algorithms.Corrections;

/// <summary>
/// Provides sample-wise shadow correction algorithms for spectral data.
/// </summary>
public static class ShadowCorrectionProcessor
{
    /// <summary>
    /// Applies dark subtraction and reference normalization.
    /// </summary>
    /// <param name="sourceSamples">Measured source samples.</param>
    /// <param name="darkSamples">Dark reference samples.</param>
    /// <param name="referenceSamples">Reference normalization samples.</param>
    /// <returns>Corrected samples.</returns>
    public static IReadOnlyList<double> Correct(
        IReadOnlyList<double> sourceSamples,
        IReadOnlyList<double> darkSamples,
        IReadOnlyList<double> referenceSamples)
    {
        ArgumentNullException.ThrowIfNull(sourceSamples);
        ArgumentNullException.ThrowIfNull(darkSamples);
        ArgumentNullException.ThrowIfNull(referenceSamples);

        if (sourceSamples.Count != darkSamples.Count || sourceSamples.Count != referenceSamples.Count)
        {
            throw new ArgumentException("Shadow correction inputs must have the same sample length.");
        }

        var corrected = new double[sourceSamples.Count];
        for (var index = 0; index < sourceSamples.Count; index++)
        {
            if (Math.Abs(referenceSamples[index]) < double.Epsilon)
            {
                throw new ArgumentException("Shadow correction reference samples must not contain zero.");
            }

            corrected[index] = (sourceSamples[index] - darkSamples[index]) / referenceSamples[index];
        }

        return corrected;
    }
}

