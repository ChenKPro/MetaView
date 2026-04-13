using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging.Signal;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Coordinates real-time DAQ sample processing and publishes image and trace frames.
/// </summary>
public interface IRealtimeSignalImagingService
{
    /// <summary>
    /// Processes one DAQ sample packet and publishes derived UI frames.
    /// </summary>
    void ProcessPacket(DaqSamplePacket packet, ScanGridSettings? settings = null);

    /// <summary>
    /// Generates and processes one synthetic four-channel frame for the demo workflow.
    /// </summary>
    void ProcessDemoFrame(ScanGridSettings? settings = null);
}
