using MetaView.Capability.DaqAndPreprocessing.GalvoScan;
using MetaView.Core.DataAcquisition;
using Vibronix.Foundation.Common.Results;
using Vibronix.Foundation.Hardware.DAQ;
using Vibronix.Foundation.Hardware.DAQ.Abstractions;

namespace MetaView.Capability.DaqAndPreprocessing;

/// <summary>
/// Adapts the foundation DAQ library to the MetaView platform DAQ contract.
/// </summary>
public sealed class FoundationDataAcquisitionCapability : IDataAcquisitionCapability, IDisposable
{
    private IDaq? _daq;

    /// <inheritdoc />
    public event EventHandler<DaqSamplesReceivedEventArgs>? SamplesReceived;

    /// <inheritdoc />
    public bool IsConfigured { get; private set; }

    /// <inheritdoc />
    public async Task<OperationResult> ConfigureAsync(
        DaqRuntimeConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (configuration.UseDemo)
        {
            IsConfigured = true;
            return OperationResult.Ok("Demo DAQ mode selected.");
        }

        if (configuration.GalvoScan.Enabled)
        {
            return ConfigureGalvoScan(configuration.GalvoScan);
        }

        if (string.IsNullOrWhiteSpace(configuration.ConfigurationPath))
        {
            return OperationResult.Error("DAQ configuration path is required.");
        }

        var deviceConfiguration = await DaqConfigurationLoader
            .LoadFromJsonAsync(configuration.ConfigurationPath, cancellationToken)
            .ConfigureAwait(false);

        DisposeDaq();
        _daq = new DaqFactory().Create(deviceConfiguration.Type);
        _daq.DataReceived += OnDataReceived;

        var result = _daq.Configure(deviceConfiguration);
        IsConfigured = result.Success;
        return result;
    }

    /// <inheritdoc />
    public Task<OperationResult> StartAsync(CancellationToken cancellationToken = default)
    {
        if (_daq is null)
        {
            return Task.FromResult(IsConfigured
                ? OperationResult.Ok("Demo DAQ acquisition started.")
                : OperationResult.Error("DAQ is not configured."));
        }

        return Task.FromResult(_daq.Start());
    }

    /// <inheritdoc />
    public Task<OperationResult> StopAsync(CancellationToken cancellationToken = default)
    {
        if (_daq is null)
        {
            return Task.FromResult(OperationResult.Ok("Demo DAQ acquisition stopped."));
        }

        var daq = _daq;
        _daq = null;
        daq.DataReceived -= OnDataReceived;

        var result = daq.Stop();
        daq.Dispose();
        IsConfigured = false;
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<OperationResult> WriteAnalogAsync(
        IReadOnlyList<double> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (_daq is null)
        {
            return Task.FromResult(OperationResult.Ok("Demo DAQ analog output written."));
        }

        return Task.FromResult(_daq.WriteAnalog(values));
    }

    /// <inheritdoc />
    public Task<OperationResult> WriteDigitalAsync(
        IReadOnlyList<bool> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (_daq is null)
        {
            return Task.FromResult(OperationResult.Ok("Demo DAQ digital output written."));
        }

        return Task.FromResult(_daq.WriteDigital(values));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeDaq();
    }

    private OperationResult ConfigureGalvoScan(GalvoScanRuntimeConfiguration settings)
    {
        try
        {
            var waveformSettings = CreateSingleFrameWaveformSettings(settings);
            var waveform = GalvoScanWaveformGenerator.Generate(waveformSettings);
            var deviceConfiguration = BuildGalvoScanDeviceConfiguration(settings, waveform.TotalSampleCount);

            DisposeDaq();
            _daq = new DaqFactory().Create(ParseEnum<DaqType>(settings.DaqType));
            _daq.DataReceived += OnDataReceived;

            var configureResult = _daq.Configure(deviceConfiguration);
            if (!configureResult.Success)
            {
                IsConfigured = false;
                return configureResult;
            }

            var writeResult = _daq.WriteAnalogSamples([waveform.XSamples, waveform.YSamples]);
            IsConfigured = writeResult.Success;
            return writeResult.Success
                ? OperationResult.Ok($"Galvo DAQ configured. Samples={waveform.TotalSampleCount}.")
                : writeResult;
        }
        catch (Exception ex)
        {
            IsConfigured = false;
            return OperationResult.Error(ex.Message);
        }
    }

    private void OnDataReceived(object? sender, DaqDataReceivedEventArgs e)
    {
        var packet = new DaqSamplePacket(
            e.Batch.Timestamp,
            e.Batch.Channels.Select(channel => channel.Name).ToArray(),
            e.Batch.Samples,
            e.Batch.SampleRate,
            e.Batch.TaskName);

        SamplesReceived?.Invoke(this, new DaqSamplesReceivedEventArgs(packet));
    }

    private static DaqDeviceConfiguration BuildGalvoScanDeviceConfiguration(
        GalvoScanRuntimeConfiguration settings,
        int totalSamples)
    {
        var daqType = ParseEnum<DaqType>(settings.DaqType);
        var terminal = ParseEnum<TerminalConfiguration>(settings.InputTerminalConfiguration);

        return new DaqDeviceConfiguration
        {
            Type = daqType,
            DeviceName = settings.DeviceName,
            Tasks =
            [
                new DaqTaskConfiguration
                {
                    Name = "GALVO_AI",
                    TaskType = DaqTaskType.AnalogInput,
                    SampleRate = settings.SampleRate,
                    SamplesPerCallback = totalSamples,
                    Continuous = true,
                    Channels =
                    [
                        CreateAiChannel("AI0_X", settings.PositionXInputChannel, settings, terminal),
                        CreateAiChannel("AI1_Y", settings.PositionYInputChannel, settings, terminal),
                        CreateAiChannel("AI2_Laser", settings.SignalAInputChannel, settings, terminal),
                        CreateAiChannel("AI3_Laser", settings.SignalBInputChannel, settings, terminal)
                    ]
                },
                new DaqTaskConfiguration
                {
                    Name = "GALVO_AO",
                    TaskType = DaqTaskType.AnalogOutput,
                    SampleRate = settings.SampleRate,
                    SamplesPerCallback = totalSamples,
                    Continuous = true,
                    ClockSource = settings.AnalogOutputClockSource,
                    StartTriggerSource = settings.AnalogOutputStartTriggerSource,
                    Channels =
                    [
                        new DaqChannel(
                            "AO_X",
                            settings.AnalogOutputXChannel,
                            DaqSignalType.AnalogVoltageOutput,
                            settings.VoltageMinimum,
                            settings.VoltageMaximum,
                            "V"),
                        new DaqChannel(
                            "AO_Y",
                            settings.AnalogOutputYChannel,
                            DaqSignalType.AnalogVoltageOutput,
                            settings.VoltageMinimum,
                            settings.VoltageMaximum,
                            "V")
                    ]
                }
            ]
        };
    }

    private static DaqChannel CreateAiChannel(
        string name,
        string physicalChannel,
        GalvoScanRuntimeConfiguration settings,
        TerminalConfiguration terminal)
    {
        return new DaqChannel(
            name,
            physicalChannel,
            DaqSignalType.AnalogVoltageInput,
            settings.VoltageMinimum,
            settings.VoltageMaximum,
            "V",
            terminal);
    }

    private static TEnum ParseEnum<TEnum>(string value)
        where TEnum : struct
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
            ? result
            : throw new InvalidOperationException($"Unsupported {typeof(TEnum).Name}: {value}");
    }

    private static GalvoScanRuntimeConfiguration CreateSingleFrameWaveformSettings(GalvoScanRuntimeConfiguration settings)
    {
        return new GalvoScanRuntimeConfiguration
        {
            Enabled = settings.Enabled,
            DaqType = settings.DaqType,
            DeviceName = settings.DeviceName,
            AnalogOutputXChannel = settings.AnalogOutputXChannel,
            AnalogOutputYChannel = settings.AnalogOutputYChannel,
            PositionXInputChannel = settings.PositionXInputChannel,
            PositionYInputChannel = settings.PositionYInputChannel,
            SignalAInputChannel = settings.SignalAInputChannel,
            SignalBInputChannel = settings.SignalBInputChannel,
            InputTerminalConfiguration = settings.InputTerminalConfiguration,
            AnalogOutputClockSource = settings.AnalogOutputClockSource,
            AnalogOutputStartTriggerSource = settings.AnalogOutputStartTriggerSource,
            ScanMode = settings.ScanMode,
            ImageWidth = settings.ImageWidth,
            ImageHeight = settings.ImageHeight,
            XExtraPixels = settings.XExtraPixels,
            FrameCount = 1,
            SampleRate = settings.SampleRate,
            SamplesPerPixel = settings.SamplesPerPixel,
            CenterXVoltage = settings.CenterXVoltage,
            CenterYVoltage = settings.CenterYVoltage,
            AmplitudeXVoltage = settings.AmplitudeXVoltage,
            AmplitudeYVoltage = settings.AmplitudeYVoltage,
            XFeedbackScale = settings.XFeedbackScale,
            YFeedbackScale = settings.YFeedbackScale,
            VoltageMinimum = settings.VoltageMinimum,
            VoltageMaximum = settings.VoltageMaximum,
            FillFraction = settings.FillFraction,
            RetraceRatio = settings.RetraceRatio,
            BidirectionalPhaseSamples = settings.BidirectionalPhaseSamples,
            DetectorSampleOffsetSamples = settings.DetectorSampleOffsetSamples,
            EnableSlewLimit = settings.EnableSlewLimit,
            MaxSlewRateVoltsPerSecond = settings.MaxSlewRateVoltsPerSecond,
            RampMilliseconds = settings.RampMilliseconds,
            Continuous = settings.Continuous
        };
    }

    private void DisposeDaq()
    {
        if (_daq is null)
        {
            return;
        }

        _daq.DataReceived -= OnDataReceived;
        _daq.Dispose();
        _daq = null;
    }
}
