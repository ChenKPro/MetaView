namespace MetaView.Capabilities.Algorithms.Spectral;

/// <summary>
/// Extracted scalar features from spectral samples.
/// </summary>
/// <param name="PeakValue">Maximum sample value.</param>
/// <param name="PeakIndex">Index of maximum sample value.</param>
/// <param name="Integral">Sum of all sample values.</param>
public sealed record SpectralFeatures(
    double PeakValue,
    int PeakIndex,
    double Integral);

/// <summary>
/// Provides common spectral feature extraction algorithms.
/// </summary>
public static class SpectralFeatureExtractor
{
    /// <summary>
    /// Extracts peak and integral features from spectral samples.
    /// </summary>
    /// <param name="samples">Spectral samples.</param>
    /// <returns>Extracted spectral features.</returns>
    public static SpectralFeatures Extract(IReadOnlyList<double> samples)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (samples.Count == 0)
        {
            throw new ArgumentException("Spectral feature extraction requires at least one sample.", nameof(samples));
        }

        var peakValue = samples[0];
        var peakIndex = 0;
        var integral = 0.0;

        for (var index = 0; index < samples.Count; index++)
        {
            var sample = samples[index];
            integral += sample;

            if (sample > peakValue)
            {
                peakValue = sample;
                peakIndex = index;
            }
        }

        return new SpectralFeatures(peakValue, peakIndex, integral);
    }
}

