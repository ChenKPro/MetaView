using System.Windows.Input;
using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging.Signal;
using MetaView.Core.Parameters;
using MetaView.Presentation.Infrastructure;
using MetaView.Services.Interfaces;
using Prism.Events;
using AsyncDelegateCommand = MetaView.Presentation.Infrastructure.AsyncDelegateCommand;
using DelegateCommand = MetaView.Presentation.Infrastructure.DelegateCommand;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Coordinates the menu-driven galvo DAQ scan setup panel.
/// </summary>
public sealed class GalvoDaqScanSetupViewModel : MetaView.Presentation.Infrastructure.BindableBase, IDisposable
{
    private const long MaxPlannedSamples = 10_000_000;
    private readonly IDataAcquisitionCapability _daq;
    private readonly IRealtimeSignalImagingService _signalImagingService;
    private readonly IRuntimeParameterProvider _parameterProvider;
    private readonly IEventAggregator _eventAggregator;
    private bool _useDemo = true;
    private bool _isRunning;
    private int _receivedFrameCount;
    private string _statusText = "Ready";
    private string _daqType = "NationalInstruments";
    private string _deviceName = "Dev1";
    private string _aoX = "Dev1/ao0";
    private string _aoY = "Dev1/ao1";
    private string _aiX = "Dev1/ai0";
    private string _aiY = "Dev1/ai1";
    private string _aiSignalA = "Dev1/ai2";
    private string _aiSignalB = "Dev1/ai3";
    private string _terminal = "Differential";
    private string _aoClockSource = "/Dev1/ai/SampleClock";
    private string _aoStartTriggerSource = "/Dev1/ai/StartTrigger";
    private string _scanMode = nameof(GalvoScanRuntimeMode.UnidirectionalRaster);
    private double _sampleRate = 20000;
    private int _imageWidth = 100;
    private int _imageHeight = 100;
    private int _xExtraPixels;
    private int _frameCount = 1;
    private int _samplesPerPixel = 2;
    private double _centerXVoltage;
    private double _centerYVoltage;
    private double _amplitudeXVoltage = 1;
    private double _amplitudeYVoltage = 1;
    private double _xFeedbackScale = 1;
    private double _yFeedbackScale = 1;
    private double _fillFraction = 0.8;
    private double _retraceRatio = 0.25;
    private int _bidirectionalPhaseSamples;
    private int _detectorOffsetSamples;
    private bool _readXYFeedback = true;
    private bool _enableSlewLimit;
    private double _maxSlewRateVoltsPerSecond = 2000;
    private double _rampMilliseconds;
    private double _voltageMinimum = -10;
    private double _voltageMaximum = 10;
    private bool _continuous = true;
    private string _estimatedPlanText = string.Empty;

    public GalvoDaqScanSetupViewModel(
        IDataAcquisitionCapability daq,
        IRealtimeSignalImagingService signalImagingService,
        IRuntimeParameterProvider parameterProvider,
        IEventAggregator eventAggregator)
    {
        _daq = daq;
        _signalImagingService = signalImagingService;
        _parameterProvider = parameterProvider;
        _eventAggregator = eventAggregator;
        DaqTypes = ["NationalInstruments", "ART"];
        TerminalOptions = ["Default", "Differential", "Rse", "Nrse"];
        ScanModes =
        [
            nameof(GalvoScanRuntimeMode.UnidirectionalRaster),
            nameof(GalvoScanRuntimeMode.BidirectionalRaster),
            nameof(GalvoScanRuntimeMode.FeedbackResample),
            nameof(GalvoScanRuntimeMode.XFeedbackRaster)
        ];
        ApplyCommand = new DelegateCommand(ApplyConfiguration);
        RunCommand = new AsyncDelegateCommand(RunAsync, () => !IsRunning);
        StopCommand = new AsyncDelegateCommand(StopAsync, () => IsRunning);
        DemoPreviewCommand = new DelegateCommand(PublishDemoPreview);
        _daq.SamplesReceived += OnSamplesReceived;
        LoadCurrentConfiguration();
        RefreshEstimatedPlan();
    }

    public IReadOnlyList<string> DaqTypes { get; }
    public IReadOnlyList<string> TerminalOptions { get; }
    public IReadOnlyList<string> ScanModes { get; }
    public ICommand ApplyCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand DemoPreviewCommand { get; }

    public bool UseDemo { get => _useDemo; set => SetProperty(ref _useDemo, value); }
    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                (RunCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
                (StopCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    public string DaqType { get => _daqType; set => SetProperty(ref _daqType, value); }
    public string DeviceName { get => _deviceName; set { if (SetProperty(ref _deviceName, value)) UpdateRoutes(); } }
    public string AoX { get => _aoX; set => SetProperty(ref _aoX, value); }
    public string AoY { get => _aoY; set => SetProperty(ref _aoY, value); }
    public string AiX { get => _aiX; set => SetProperty(ref _aiX, value); }
    public string AiY { get => _aiY; set => SetProperty(ref _aiY, value); }
    public string AiSignalA { get => _aiSignalA; set => SetProperty(ref _aiSignalA, value); }
    public string AiSignalB { get => _aiSignalB; set => SetProperty(ref _aiSignalB, value); }
    public string Terminal { get => _terminal; set => SetProperty(ref _terminal, value); }
    public string AoClockSource { get => _aoClockSource; set => SetProperty(ref _aoClockSource, value); }
    public string AoStartTriggerSource { get => _aoStartTriggerSource; set => SetProperty(ref _aoStartTriggerSource, value); }
    public string ScanMode { get => _scanMode; set => SetProperty(ref _scanMode, value); }
    public double SampleRate { get => _sampleRate; set { if (SetProperty(ref _sampleRate, value)) RefreshEstimatedPlan(); } }
    public int ImageWidth { get => _imageWidth; set { if (SetProperty(ref _imageWidth, value)) RefreshEstimatedPlan(); } }
    public int ImageHeight { get => _imageHeight; set { if (SetProperty(ref _imageHeight, value)) RefreshEstimatedPlan(); } }
    public int XExtraPixels { get => _xExtraPixels; set { if (SetProperty(ref _xExtraPixels, value)) RefreshEstimatedPlan(); } }
    public int FrameCount { get => _frameCount; set { if (SetProperty(ref _frameCount, value)) RefreshEstimatedPlan(); } }
    public int SamplesPerPixel { get => _samplesPerPixel; set { if (SetProperty(ref _samplesPerPixel, value)) RefreshEstimatedPlan(); } }
    public double CenterXVoltage { get => _centerXVoltage; set => SetProperty(ref _centerXVoltage, value); }
    public double CenterYVoltage { get => _centerYVoltage; set => SetProperty(ref _centerYVoltage, value); }
    public double AmplitudeXVoltage { get => _amplitudeXVoltage; set => SetProperty(ref _amplitudeXVoltage, value); }
    public double AmplitudeYVoltage { get => _amplitudeYVoltage; set => SetProperty(ref _amplitudeYVoltage, value); }
    public double XFeedbackScale { get => _xFeedbackScale; set => SetProperty(ref _xFeedbackScale, value); }
    public double YFeedbackScale { get => _yFeedbackScale; set => SetProperty(ref _yFeedbackScale, value); }
    public double FillFraction { get => _fillFraction; set { if (SetProperty(ref _fillFraction, value)) RefreshEstimatedPlan(); } }
    public double RetraceRatio { get => _retraceRatio; set { if (SetProperty(ref _retraceRatio, value)) RefreshEstimatedPlan(); } }
    public int BidirectionalPhaseSamples { get => _bidirectionalPhaseSamples; set => SetProperty(ref _bidirectionalPhaseSamples, value); }
    public int DetectorOffsetSamples { get => _detectorOffsetSamples; set => SetProperty(ref _detectorOffsetSamples, value); }
    public bool ReadXYFeedback { get => _readXYFeedback; set => SetProperty(ref _readXYFeedback, value); }
    public bool EnableSlewLimit { get => _enableSlewLimit; set => SetProperty(ref _enableSlewLimit, value); }
    public double MaxSlewRateVoltsPerSecond { get => _maxSlewRateVoltsPerSecond; set => SetProperty(ref _maxSlewRateVoltsPerSecond, value); }
    public double RampMilliseconds { get => _rampMilliseconds; set { if (SetProperty(ref _rampMilliseconds, value)) RefreshEstimatedPlan(); } }
    public double VoltageMinimum { get => _voltageMinimum; set => SetProperty(ref _voltageMinimum, value); }
    public double VoltageMaximum { get => _voltageMaximum; set => SetProperty(ref _voltageMaximum, value); }
    public bool Continuous { get => _continuous; set => SetProperty(ref _continuous, value); }
    public string EstimatedPlanText { get => _estimatedPlanText; private set => SetProperty(ref _estimatedPlanText, value); }

    public void Dispose()
    {
        _daq.SamplesReceived -= OnSamplesReceived;
    }

    private void ApplyConfiguration()
    {
        if (!TryValidate(out var validationMessage))
        {
            StatusText = "Validation failed";
            PublishLog($"Galvo scan configuration invalid: {validationMessage}", true);
            return;
        }

        var configuration = CreateConfiguration();
        var result = _parameterProvider.SetDaqRuntimeConfiguration(configuration);
        _signalImagingService.SetGridSettings(new ScanGridSettings(ImageWidth, ImageHeight));
        _signalImagingService.SetGalvoScanSettings(configuration.GalvoScan);
        StatusText = result.Success ? "Configuration saved" : "Save failed";
        PublishLog(
            result.Success
                ? $"Galvo scan configured and saved. Grid={ImageWidth}x{ImageHeight}, DAQ={DaqType}, Device={DeviceName}"
                : $"Galvo scan configuration save failed: {result.Message}",
            !result.Success);
    }

    private async Task RunAsync()
    {
        if (!TryValidate(out var validationMessage))
        {
            StatusText = "Validation failed";
            PublishLog($"Galvo scan cannot start: {validationMessage}", true);
            return;
        }

        ApplyConfiguration();
        var configuration = CreateConfiguration();

        if (configuration.UseDemo)
        {
            PublishDemoPreview();
            StatusText = "Demo preview published";
            return;
        }

        IsRunning = true;
        _receivedFrameCount = 0;
        StatusText = "Configuring DAQ";
        PublishLog("Galvo scan: configuring synchronized AI/AO tasks");

        var configureResult = await _daq.ConfigureAsync(configuration, CancellationToken.None).ConfigureAwait(true);
        if (!configureResult.Success)
        {
            PublishLog($"Galvo scan configure failed: {configureResult.Message}", true);
            StatusText = "Configure failed";
            IsRunning = false;
            return;
        }

        PublishLog(configureResult.Message);
        StatusText = "Running";
        var startResult = await _daq.StartAsync(CancellationToken.None).ConfigureAwait(true);
        PublishLog(startResult.Success ? "Galvo scan started" : $"Galvo scan start failed: {startResult.Message}", !startResult.Success);
        if (!startResult.Success)
        {
            StatusText = "Start failed";
            IsRunning = false;
        }
    }

    private async Task StopAsync()
    {
        var result = await _daq.StopAsync(CancellationToken.None).ConfigureAwait(true);
        PublishLog(result.Success ? "Galvo scan stopped" : $"Galvo scan stop failed: {result.Message}", !result.Success);
        StatusText = result.Success ? "Stopped" : "Stop failed";
        IsRunning = false;
    }

    private void PublishDemoPreview()
    {
        _signalImagingService.SetGridSettings(new ScanGridSettings(ImageWidth, ImageHeight));
        _signalImagingService.SetGalvoScanSettings(CreateConfiguration().GalvoScan);
        _signalImagingService.ProcessDemoFrame(new ScanGridSettings(ImageWidth, ImageHeight));
        PublishLog($"Galvo demo preview published to ImageDisplay and Signal. Grid={ImageWidth}x{ImageHeight}");
    }

    private DaqRuntimeConfiguration CreateConfiguration()
    {
        return new DaqRuntimeConfiguration
        {
            UseDemo = UseDemo,
            GalvoScan = new GalvoScanRuntimeConfiguration
            {
                Enabled = true,
                DaqType = DaqType,
                DeviceName = DeviceName,
                AnalogOutputXChannel = AoX,
                AnalogOutputYChannel = AoY,
                PositionXInputChannel = AiX,
                PositionYInputChannel = AiY,
                SignalAInputChannel = AiSignalA,
                SignalBInputChannel = AiSignalB,
                InputTerminalConfiguration = Terminal,
                AnalogOutputClockSource = AoClockSource,
                AnalogOutputStartTriggerSource = AoStartTriggerSource,
                ScanMode = Enum.TryParse<GalvoScanRuntimeMode>(ScanMode, out var mode)
                    ? mode
                    : GalvoScanRuntimeMode.UnidirectionalRaster,
                ImageWidth = ImageWidth,
                ImageHeight = ImageHeight,
                XExtraPixels = XExtraPixels,
                FrameCount = FrameCount,
                SampleRate = SampleRate,
                SamplesPerPixel = SamplesPerPixel,
                CenterXVoltage = CenterXVoltage,
                CenterYVoltage = CenterYVoltage,
                AmplitudeXVoltage = AmplitudeXVoltage,
                AmplitudeYVoltage = AmplitudeYVoltage,
                XFeedbackScale = XFeedbackScale,
                YFeedbackScale = YFeedbackScale,
                VoltageMinimum = VoltageMinimum,
                VoltageMaximum = VoltageMaximum,
                FillFraction = FillFraction,
                RetraceRatio = RetraceRatio,
                BidirectionalPhaseSamples = BidirectionalPhaseSamples,
                DetectorSampleOffsetSamples = DetectorOffsetSamples,
                EnableSlewLimit = EnableSlewLimit,
                MaxSlewRateVoltsPerSecond = MaxSlewRateVoltsPerSecond,
                RampMilliseconds = RampMilliseconds,
                Continuous = Continuous
            }
        };
    }

    private bool TryValidate(out string message)
    {
        if (string.IsNullOrWhiteSpace(DeviceName))
        {
            message = "Device name is required.";
            return false;
        }

        if (HasEmptyChannel())
        {
            message = "AO/AI channel names cannot be empty.";
            return false;
        }

        if (ImageWidth < 2 || ImageHeight < 2)
        {
            message = "Image width and height must be at least 2.";
            return false;
        }

        if (FrameCount < 1)
        {
            message = "Frame count must be at least 1.";
            return false;
        }

        if (SampleRate <= 0)
        {
            message = "Sample rate must be greater than zero.";
            return false;
        }

        if (SamplesPerPixel < 1)
        {
            message = "Samples per pixel must be at least 1.";
            return false;
        }

        if (AmplitudeXVoltage <= 0 || AmplitudeYVoltage <= 0)
        {
            message = "X/Y amplitude must be greater than zero.";
            return false;
        }

        if (FillFraction <= 0 || FillFraction > 1)
        {
            message = "Fill fraction must be in the range (0, 1].";
            return false;
        }

        if (RetraceRatio < 0)
        {
            message = "Retrace ratio cannot be negative.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(AoClockSource) || string.IsNullOrWhiteSpace(AoStartTriggerSource))
        {
            message = "AO clock source and start trigger cannot be empty.";
            return false;
        }

        if (VoltageMinimum >= VoltageMaximum)
        {
            message = "Voltage minimum must be less than voltage maximum.";
            return false;
        }

        if (XExtraPixels < 0 || BidirectionalPhaseSamples < 0)
        {
            message = "Extra pixels and phase samples cannot be negative.";
            return false;
        }

        if (DetectorOffsetSamples is < -1_000_000 or > 1_000_000)
        {
            message = "Detector offset samples must be in the range [-1000000, 1000000].";
            return false;
        }

        if (XFeedbackScale <= 0 || YFeedbackScale <= 0)
        {
            message = "X/Y feedback scale must be greater than zero.";
            return false;
        }

        if (EnableSlewLimit && MaxSlewRateVoltsPerSecond <= 0)
        {
            message = "Max slew rate must be greater than zero when slew limit is enabled.";
            return false;
        }

        if (RampMilliseconds < 0)
        {
            message = "Ramp milliseconds cannot be negative.";
            return false;
        }

        if (CenterXVoltage - AmplitudeXVoltage < VoltageMinimum
            || CenterXVoltage + AmplitudeXVoltage > VoltageMaximum
            || CenterYVoltage - AmplitudeYVoltage < VoltageMinimum
            || CenterYVoltage + AmplitudeYVoltage > VoltageMaximum)
        {
            message = $"Scan voltage window must stay within {VoltageMinimum} V to {VoltageMaximum} V.";
            return false;
        }

        var plannedSamples = EstimatePlannedSamples();
        if (plannedSamples > MaxPlannedSamples)
        {
            message = $"Estimated AO plan is too large ({plannedSamples:N0} samples). Reduce image size, frames, samples/pixel, or retrace ratio.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private bool HasEmptyChannel()
    {
        return string.IsNullOrWhiteSpace(AoX)
            || string.IsNullOrWhiteSpace(AoY)
            || string.IsNullOrWhiteSpace(AiX)
            || string.IsNullOrWhiteSpace(AiY)
            || string.IsNullOrWhiteSpace(AiSignalA)
            || string.IsNullOrWhiteSpace(AiSignalB);
    }

    private void LoadCurrentConfiguration()
    {
        var result = _parameterProvider.GetDaqRuntimeConfiguration();
        if (!result.Success || result.Data is null)
        {
            return;
        }

        var configuration = result.Data;
        var settings = configuration.GalvoScan;
        UseDemo = configuration.UseDemo;
        DaqType = settings.DaqType;
        DeviceName = settings.DeviceName;
        AoX = settings.AnalogOutputXChannel;
        AoY = settings.AnalogOutputYChannel;
        AiX = settings.PositionXInputChannel;
        AiY = settings.PositionYInputChannel;
        AiSignalA = settings.SignalAInputChannel;
        AiSignalB = settings.SignalBInputChannel;
        Terminal = settings.InputTerminalConfiguration;
        AoClockSource = settings.AnalogOutputClockSource;
        AoStartTriggerSource = settings.AnalogOutputStartTriggerSource;
        ScanMode = settings.ScanMode.ToString();
        SampleRate = settings.SampleRate;
        ImageWidth = settings.ImageWidth;
        ImageHeight = settings.ImageHeight;
        XExtraPixels = settings.XExtraPixels;
        FrameCount = settings.FrameCount;
        SamplesPerPixel = settings.SamplesPerPixel;
        CenterXVoltage = settings.CenterXVoltage;
        CenterYVoltage = settings.CenterYVoltage;
        AmplitudeXVoltage = settings.AmplitudeXVoltage;
        AmplitudeYVoltage = settings.AmplitudeYVoltage;
        XFeedbackScale = settings.XFeedbackScale;
        YFeedbackScale = settings.YFeedbackScale;
        VoltageMinimum = settings.VoltageMinimum;
        VoltageMaximum = settings.VoltageMaximum;
        FillFraction = settings.FillFraction;
        RetraceRatio = settings.RetraceRatio;
        BidirectionalPhaseSamples = settings.BidirectionalPhaseSamples;
        DetectorOffsetSamples = settings.DetectorSampleOffsetSamples;
        EnableSlewLimit = settings.EnableSlewLimit;
        MaxSlewRateVoltsPerSecond = settings.MaxSlewRateVoltsPerSecond;
        RampMilliseconds = settings.RampMilliseconds;
        Continuous = settings.Continuous;
    }

    private void OnSamplesReceived(object? sender, DaqSamplesReceivedEventArgs e)
    {
        if (!IsRunning)
        {
            return;
        }

        var frameNumber = Interlocked.Increment(ref _receivedFrameCount);
        PublishLog($"Galvo scan frame received. Frame={frameNumber}, Task={e.Packet.TaskName}, Samples={e.Packet.Samples.FirstOrDefault()?.Length ?? 0}");
        if (!Continuous && frameNumber >= FrameCount)
        {
            StatusText = "Target frames received";
            IsRunning = false;
            _ = StopAfterTargetFramesAsync(frameNumber);
        }
    }

    private async Task StopAfterTargetFramesAsync(int frameNumber)
    {
        var result = await _daq.StopAsync(CancellationToken.None).ConfigureAwait(true);
        PublishLog(
            result.Success
                ? $"Galvo scan completed. Frames={frameNumber}"
                : $"Galvo scan target-frame stop failed: {result.Message}",
            !result.Success);
    }

    private void UpdateRoutes()
    {
        if (string.IsNullOrWhiteSpace(DeviceName))
        {
            return;
        }

        AoX = $"{DeviceName}/ao0";
        AoY = $"{DeviceName}/ao1";
        AiX = $"{DeviceName}/ai0";
        AiY = $"{DeviceName}/ai1";
        AiSignalA = $"{DeviceName}/ai2";
        AiSignalB = $"{DeviceName}/ai3";
        AoClockSource = $"/{DeviceName}/ai/SampleClock";
        AoStartTriggerSource = $"/{DeviceName}/ai/StartTrigger";
    }

    private void PublishLog(string message, bool isError = false)
    {
        var hint = isError
            ? "Check DAQ device name, channel mapping, terminal mode, clock/trigger route, and whether another task is occupying the device."
            : string.Empty;
        _eventAggregator
            .GetEvent<WorkflowLogPublishedEvent>()
            .Publish(new WorkflowLogEntry(DateTimeOffset.Now, message, isError, hint));
    }

    private void RefreshEstimatedPlan()
    {
        if (ImageWidth < 1 || ImageHeight < 1 || SamplesPerPixel < 1 || FrameCount < 1 || SampleRate <= 0)
        {
            EstimatedPlanText = "Estimated plan: waiting for valid scan parameters";
            return;
        }

        var plannedSamples = EstimatePlannedSamples();
        var seconds = plannedSamples / SampleRate;
        var warning = plannedSamples > MaxPlannedSamples ? "  Exceeds safe limit" : string.Empty;
        EstimatedPlanText = $"Estimated plan: {plannedSamples:N0} AO samples, {seconds:F2} s total{warning}";
    }

    private long EstimatePlannedSamples()
    {
        var activeSamples = checked((long)Math.Max(1, ImageWidth + XExtraPixels * 2) * Math.Max(1, SamplesPerPixel));
        var fillSamples = FillFraction <= 0 ? 1 : (long)Math.Ceiling(activeSamples / FillFraction) - activeSamples;
        var retraceSamples = (long)Math.Ceiling(activeSamples * Math.Max(0, RetraceRatio));
        var turnSamples = Math.Max(1, Math.Max(fillSamples, retraceSamples));
        var lineSamples = activeSamples + turnSamples;
        var rasterSamples = checked(lineSamples * Math.Max(1, ImageHeight) * Math.Max(1, FrameCount));
        var rampSamples = RampMilliseconds <= 0 || SampleRate <= 0
            ? 1
            : Math.Max(1, (long)Math.Round(SampleRate * RampMilliseconds / 1000d));
        return checked(rasterSamples + rampSamples * 2);
    }
}
