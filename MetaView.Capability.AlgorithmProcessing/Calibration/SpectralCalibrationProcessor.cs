namespace MetaView.Capabilities.Algorithms.Calibration;

/// <summary>
/// Provides calibration algorithms for spectral samples.
/// </summary>
public static class SpectralCalibrationProcessor
{
    /// <summary>
    /// Applies linear calibration to sample values.
    /// </summary>
    /// <param name="samples">Source samples.</param>
    /// <param name="scale">Multiplicative scale.</param>
    /// <param name="offset">Additive offset.</param>
    /// <returns>Calibrated samples.</returns>
    public static IReadOnlyList<double> ApplyLinear(
        IReadOnlyList<double> samples,
        double scale,
        double offset)
    {
        ArgumentNullException.ThrowIfNull(samples);

        var calibrated = new double[samples.Count];
        for (var index = 0; index < samples.Count; index++)
        {
            calibrated[index] = samples[index] * scale + offset;
        }

        return calibrated;
    }
}

