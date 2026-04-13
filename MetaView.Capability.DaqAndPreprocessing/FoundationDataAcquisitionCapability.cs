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

        if (string.IsNullOrWhiteSpace(configuration.ConfigurationPath))
        {
            return OperationResult.Error("DAQ configuration path is required.");
        }

        var deviceConfiguration = await DaqConfigurationLoader
            .LoadFromJsonAsync(configuration.ConfigurationPath, cancellationToken)
            .ConfigureAwait(false);

        _daq?.Dispose();
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

        return Task.FromResult(_daq.Stop());
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
        if (_daq is not null)
        {
            _daq.DataReceived -= OnDataReceived;
            _daq.Dispose();
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
}
