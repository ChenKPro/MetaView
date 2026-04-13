using MetaView.Core.DataAcquisition;

namespace MetaView.Core.Imaging.Signal;

/// <summary>
/// Converts four-channel DAQ samples into a grid image and live signal traces.
/// </summary>
public interface ISignalImagingProcessor
{
    /// <summary>
    /// Processes one DAQ sample packet using AI0/AI1 as XY position and AI2/AI3 as laser signal.
    /// </summary>
    SignalImagingResult Process(DaqSamplePacket packet, ScanGridSettings settings);
}
