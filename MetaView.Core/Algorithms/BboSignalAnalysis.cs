namespace MetaView.Core.Algorithms;

/// <summary>
/// Describes BBO signal peak and width analysis.
/// </summary>
public sealed record BboSignalAnalysis(
    int PeakIndex,
    double PeakValue,
    double Baseline,
    double SignalToBaseline,
    double HalfMaximumLevel,
    double? LeftHalfMaximumIndex,
    double? RightHalfMaximumIndex,
    double? FullWidthHalfMaximum);
