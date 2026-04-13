namespace MetaView.Core.Algorithms;

/// <summary>
/// Describes scalar features extracted from spectral samples.
/// </summary>
public sealed record SpectralFeatureSet(
    double PeakValue,
    int PeakIndex,
    double Integral);
