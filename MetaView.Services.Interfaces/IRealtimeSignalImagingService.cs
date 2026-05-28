using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging.Signal;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Coordinates real-time DAQ sample processing and publishes image and trace frames.
/// </summary>
public interface IRealtimeSignalImagingService
{
    /// <summary>
    /// Sets the default grid used for live DAQ sample callbacks.
    /// </summary>
    void SetGridSettings(ScanGridSettings settings);

    /// <summary>
    /// Sets the synchronized galvo scan settings used for frame reconstruction.
    /// </summary>
    void SetGalvoScanSettings(GalvoScanRuntimeConfiguration settings);

    /// <summary>
    /// Processes one DAQ sample packet and publishes derived UI frames.
    /// </summary>
    void ProcessPacket(DaqSamplePacket packet, ScanGridSettings? settings = null);

    /// <summary>
    /// Generates and processes one synthetic four-channel frame for the demo workflow.
    /// </summary>
    void ProcessDemoFrame(ScanGridSettings? settings = null);
}
