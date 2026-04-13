namespace MetaView.Core.Imaging.Signal;

/// <summary>
/// Represents one normalized grayscale image frame produced from AI2 and AI3.
/// </summary>
public sealed record SignalImageFrame(
    int Width,
    int Height,
    byte[] GrayPixels,
    double MinimumValue,
    double MaximumValue,
    DateTimeOffset Timestamp);
