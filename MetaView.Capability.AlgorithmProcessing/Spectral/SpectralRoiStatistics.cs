namespace MetaView.Capabilities.Algorithms.Spectral;

/// <summary>
/// Describes per-channel statistics calculated from a spectral ROI.
/// </summary>
/// <param name="PixelCount">Number of pixels included in the ROI.</param>
/// <param name="AverageSpectrum">Average spectrum across all ROI pixels.</param>
/// <param name="MinimumSpectrum">Minimum value for each spectral channel.</param>
/// <param name="MaximumSpectrum">Maximum value for each spectral channel.</param>
/// <param name="StandardDeviationSpectrum">Standard deviation for each spectral channel.</param>
public sealed record SpectralRoiStatistics(
    int PixelCount,
    IReadOnlyList<double> AverageSpectrum,
    IReadOnlyList<double> MinimumSpectrum,
    IReadOnlyList<double> MaximumSpectrum,
    IReadOnlyList<double> StandardDeviationSpectrum);
