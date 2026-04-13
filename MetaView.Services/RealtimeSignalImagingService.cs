using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging.Signal;
using MetaView.Services.Interfaces;
using Prism.Events;

namespace MetaView.Services;

/// <summary>
/// Processes real-time DAQ packets into image and signal trace frames.
/// </summary>
public sealed class RealtimeSignalImagingService : IRealtimeSignalImagingService, IDisposable
{
    private readonly IDataAcquisitionCapability _dataAcquisitionCapability;
    private readonly ISignalImagingProcessor _signalImagingProcessor;
    private readonly IEventAggregator _eventAggregator;

    public RealtimeSignalImagingService(
        IDataAcquisitionCapability dataAcquisitionCapability,
        ISignalImagingProcessor signalImagingProcessor,
        IEventAggregator eventAggregator)
    {
        _dataAcquisitionCapability = dataAcquisitionCapability;
        _signalImagingProcessor = signalImagingProcessor;
        _eventAggregator = eventAggregator;
        _dataAcquisitionCapability.SamplesReceived += OnSamplesReceived;
    }

    /// <inheritdoc />
    public void ProcessPacket(DaqSamplePacket packet, ScanGridSettings? settings = null)
    {
        var result = _signalImagingProcessor.Process(packet, settings ?? ScanGridSettings.Default);
        _eventAggregator.GetEvent<SignalImageFramePublishedEvent>().Publish(result.ImageFrame);
        _eventAggregator.GetEvent<SignalTraceFramePublishedEvent>().Publish(result.TraceFrame);
    }

    /// <inheritdoc />
    public void ProcessDemoFrame(ScanGridSettings? settings = null)
    {
        ProcessPacket(CreateDemoPacket(), settings);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _dataAcquisitionCapability.SamplesReceived -= OnSamplesReceived;
    }

    private void OnSamplesReceived(object? sender, DaqSamplesReceivedEventArgs e)
    {
        ProcessPacket(e.Packet);
    }

    private static DaqSamplePacket CreateDemoPacket()
    {
        const int sampleCount = 20000;
        var ai0 = new double[sampleCount];
        var ai1 = new double[sampleCount];
        var ai2 = new double[sampleCount];
        var ai3 = new double[sampleCount];
        var random = new Random(42);

        for (var index = 0; index < sampleCount; index++)
        {
            var row = index / 200;
            var col = index % 200;
            var reverse = row % 2 == 1;
            var x = reverse ? 199 - col : col;
            var y = row % 100;
            var normalizedX = x / 199.0;
            var normalizedY = y / 99.0;
            var spot = Math.Exp(-Math.Pow(normalizedX - 0.58, 2) / 0.018 - Math.Pow(normalizedY - 0.44, 2) / 0.028);
            var ripple = 0.25 * Math.Sin(normalizedX * Math.PI * 8) * Math.Cos(normalizedY * Math.PI * 5);
            var noise = (random.NextDouble() - 0.5) * 0.04;

            ai0[index] = normalizedX;
            ai1[index] = normalizedY;
            ai2[index] = 0.4 + spot + ripple + noise;
            ai3[index] = 0.35 + spot * 0.82 - ripple * 0.35 + noise;
        }

        return new DaqSamplePacket(
            DateTimeOffset.Now,
            ["AI0", "AI1", "AI2", "AI3"],
            [ai0, ai1, ai2, ai3],
            20000,
            "Demo four-channel signal imaging");
    }
}
