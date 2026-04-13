namespace MetaView.Core.DataAcquisition;

/// <summary>
/// Represents one platform-level DAQ sample packet.
/// </summary>
public sealed record DaqSamplePacket(
    DateTimeOffset Timestamp,
    IReadOnlyList<string> Channels,
    IReadOnlyList<double[]> Samples,
    double SampleRate,
    string TaskName);
