namespace MetaView.Core.DataAcquisition;

/// <summary>
/// Provides DAQ samples received by the active DAQ service.
/// </summary>
public sealed class DaqSamplesReceivedEventArgs(DaqSamplePacket packet) : EventArgs
{
    /// <summary>
    /// Gets the received DAQ sample packet.
    /// </summary>
    public DaqSamplePacket Packet { get; } = packet;
}
