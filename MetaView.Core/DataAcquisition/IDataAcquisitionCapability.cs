using Vibronix.Foundation.Common.Results;

namespace MetaView.Core.DataAcquisition;

/// <summary>
/// Defines platform-level DAQ operations exposed to MetaView presentation and workflows.
/// </summary>
public interface IDataAcquisitionCapability
{
    /// <summary>
    /// Occurs when the DAQ service receives one sample packet.
    /// </summary>
    event EventHandler<DaqSamplesReceivedEventArgs>? SamplesReceived;

    /// <summary>
    /// Gets a value indicating whether the DAQ device has been configured.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Configures the active DAQ device.
    /// </summary>
    Task<OperationResult> ConfigureAsync(DaqRuntimeConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts DAQ acquisition.
    /// </summary>
    Task<OperationResult> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops DAQ acquisition.
    /// </summary>
    Task<OperationResult> StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes analog output values.
    /// </summary>
    Task<OperationResult> WriteAnalogAsync(IReadOnlyList<double> values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes digital output values.
    /// </summary>
    Task<OperationResult> WriteDigitalAsync(IReadOnlyList<bool> values, CancellationToken cancellationToken = default);
}
