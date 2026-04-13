namespace MetaView.Capabilities.Algorithms.FrameProcessing;

/// <summary>
/// Represents one raw DAQ frame used by frame-level algorithms.
/// </summary>
/// <param name="FrameIndex">Frame index in the acquisition sequence.</param>
/// <param name="Samples">Raw sample values.</param>
public sealed record DaqRawFrame(int FrameIndex, IReadOnlyList<double> Samples);
