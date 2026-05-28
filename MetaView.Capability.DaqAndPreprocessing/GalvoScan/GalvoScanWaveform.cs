namespace MetaView.Capability.DaqAndPreprocessing.GalvoScan;

/// <summary>
/// Describes one active imaging line inside the generated AO waveform.
/// </summary>
internal sealed record GalvoScanLineDescriptor(
    int FrameIndex,
    int RowIndex,
    int ActiveStartSampleIndex,
    int ActiveSampleCount,
    bool IsReverse);

/// <summary>
/// Buffered AO samples and geometry metadata for one synchronized galvo scan.
/// </summary>
internal sealed record GalvoScanWaveform(
    double[] XSamples,
    double[] YSamples,
    int StartRampSampleCount,
    int EndRampSampleCount,
    int ForwardSampleCount,
    int TurnSampleCount,
    IReadOnlyList<GalvoScanLineDescriptor> Lines)
{
    public int TotalSampleCount => Math.Min(XSamples.Length, YSamples.Length);
}
