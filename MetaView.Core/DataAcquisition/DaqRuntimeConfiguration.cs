namespace MetaView.Core.DataAcquisition;

/// <summary>
/// Describes the DAQ runtime configuration used by MetaView capability adapters.
/// </summary>
public sealed class DaqRuntimeConfiguration
{
    /// <summary>
    /// Gets or sets whether the platform should use the demo acquisition service.
    /// </summary>
    public bool UseDemo { get; set; } = true;

    /// <summary>
    /// Gets or sets the DAQ configuration JSON file path for the foundation DAQ service.
    /// </summary>
    public string ConfigurationPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets synchronized galvo scan settings.
    /// </summary>
    public GalvoScanRuntimeConfiguration GalvoScan { get; set; } = new();
}

/// <summary>
/// Defines the scan reconstruction and waveform mode used by a synchronized galvo scan.
/// </summary>
public enum GalvoScanRuntimeMode
{
    /// <summary>
    /// Forward raster only.
    /// </summary>
    UnidirectionalRaster,

    /// <summary>
    /// Alternating forward and reverse raster lines.
    /// </summary>
    BidirectionalRaster,

    /// <summary>
    /// Use measured X/Y feedback to resample detector data into pixels.
    /// </summary>
    FeedbackResample,

    /// <summary>
    /// Use measured X feedback for columns while rows follow the planned raster.
    /// </summary>
    XFeedbackRaster
}

/// <summary>
/// Describes a synchronized AI/AO galvo scan for Foundation DAQ.
/// </summary>
public sealed class GalvoScanRuntimeConfiguration
{
    /// <summary>
    /// Gets or sets whether this configuration should build synchronized galvo AI/AO tasks.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the DAQ vendor type name, such as NationalInstruments or ART.
    /// </summary>
    public string DaqType { get; set; } = "NationalInstruments";

    /// <summary>
    /// Gets or sets the physical device name, such as Dev1.
    /// </summary>
    public string DeviceName { get; set; } = "Dev1";

    public string AnalogOutputXChannel { get; set; } = "Dev1/ao0";

    public string AnalogOutputYChannel { get; set; } = "Dev1/ao1";

    public string PositionXInputChannel { get; set; } = "Dev1/ai0";

    public string PositionYInputChannel { get; set; } = "Dev1/ai1";

    public string SignalAInputChannel { get; set; } = "Dev1/ai2";

    public string SignalBInputChannel { get; set; } = "Dev1/ai3";

    public string InputTerminalConfiguration { get; set; } = "Differential";

    public string AnalogOutputClockSource { get; set; } = "/Dev1/ai/SampleClock";

    public string AnalogOutputStartTriggerSource { get; set; } = "/Dev1/ai/StartTrigger";

    public GalvoScanRuntimeMode ScanMode { get; set; } = GalvoScanRuntimeMode.UnidirectionalRaster;

    public int ImageWidth { get; set; } = 100;

    public int ImageHeight { get; set; } = 100;

    public int XExtraPixels { get; set; }

    public int FrameCount { get; set; } = 1;

    public double SampleRate { get; set; } = 20000;

    public int SamplesPerPixel { get; set; } = 2;

    public double CenterXVoltage { get; set; }

    public double CenterYVoltage { get; set; }

    public double AmplitudeXVoltage { get; set; } = 1;

    public double AmplitudeYVoltage { get; set; } = 1;

    public double XFeedbackScale { get; set; } = 1;

    public double YFeedbackScale { get; set; } = 1;

    public double VoltageMinimum { get; set; } = -10;

    public double VoltageMaximum { get; set; } = 10;

    public double FillFraction { get; set; } = 0.8;

    public double RetraceRatio { get; set; } = 0.25;

    public int BidirectionalPhaseSamples { get; set; }

    public int DetectorSampleOffsetSamples { get; set; }

    public bool EnableSlewLimit { get; set; }

    public double MaxSlewRateVoltsPerSecond { get; set; } = 2000;

    public double RampMilliseconds { get; set; }

    public bool Continuous { get; set; } = true;

    public int CommandLinePixelCount => checked(ImageWidth + XExtraPixels * 2);

    public double CommandAmplitudeXVoltage => ImageWidth <= 0
        ? AmplitudeXVoltage
        : AmplitudeXVoltage * CommandLinePixelCount / ImageWidth;
}
