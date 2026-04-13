namespace MetaView.Core.Imaging.Signal;

/// <summary>
/// Represents a downsampled four-channel signal trace for real-time display.
/// </summary>
public sealed record SignalTraceFrame(
    IReadOnlyList<double> Ai0,
    IReadOnlyList<double> Ai1,
    IReadOnlyList<double> Ai2,
    IReadOnlyList<double> Ai3,
    DateTimeOffset Timestamp);
