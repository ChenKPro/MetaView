namespace MetaView.Core.Algorithms;

/// <summary>
/// Represents one raw numeric frame used by algorithm capabilities.
/// </summary>
/// <param name="FrameIndex">Frame index in the acquisition sequence.</param>
/// <param name="Samples">Frame sample values.</param>
public sealed record AlgorithmRawFrame(int FrameIndex, IReadOnlyList<double> Samples);
