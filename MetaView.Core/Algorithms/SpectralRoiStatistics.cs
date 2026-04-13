namespace MetaView.Core.Algorithms;

/// <summary>
/// Describes per-channel statistics calculated from a spectral ROI.
/// </summary>
public sealed record SpectralRoiStatistics(
    int PixelCount,
    IReadOnlyList<double> AverageSpectrum,
    IReadOnlyList<double> MinimumSpectrum,
    IReadOnlyList<double> MaximumSpectrum,
    IReadOnlyList<double> StandardDeviationSpectrum);
