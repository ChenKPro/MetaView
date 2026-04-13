using MetaView.Core.Algorithms;
using MetaView.Core.DataAcquisition;
using MetaView.Core.Experiments;
using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.Imaging.Signal;
using MetaView.Core.Laser;
using MetaView.Core.MotionControl;
using MetaView.Core.Parameters;
using MetaView.Core.PhotoDetection;
using MetaView.Services.Interfaces;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Services;

/// <summary>
/// Default workflow-facing invoker for platform capabilities.
/// </summary>
public sealed class ExperimentCapabilityInvoker(
    IMotionControlCapability motionControlCapability,
    IDataAcquisitionCapability dataAcquisitionCapability,
    IBrightfieldCameraCapability brightfieldCameraCapability,
    ILaserControlCapability laserControlCapability,
    IPhotoDetectionCapability photoDetectionCapability,
    IAlgorithmProcessingCapability algorithmProcessingCapability,
    IRealtimeSignalImagingService realtimeSignalImagingService,
    IRuntimeParameterProvider parameterProvider)
    : IExperimentCapabilityInvoker
{
    /// <inheritdoc />
    public async Task<OperationResult> InitializeAsync(
        CapabilityPlan plan,
        CancellationToken cancellationToken = default)
    {
        if (!plan.InitializeBeforeRun)
        {
            return OperationResult.Ok("Capability initialization skipped by plan.");
        }

        foreach (var capability in plan.Required)
        {
            var result = await InitializeOneAsync(capability, cancellationToken).ConfigureAwait(false);
            if (!result.Success)
            {
                return result;
            }
        }

        return OperationResult.Ok("Required capabilities initialized.");
    }

    /// <inheritdoc />
    public async Task<OperationResult> StopAsync(
        CapabilityPlan plan,
        CancellationToken cancellationToken = default)
    {
        if (!plan.StopAfterRun)
        {
            return OperationResult.Ok("Capability stop skipped by plan.");
        }

        foreach (var capability in plan.Required.Reverse())
        {
            var result = await StopOneAsync(capability, cancellationToken).ConfigureAwait(false);
            if (!result.Success)
            {
                return result;
            }
        }

        return OperationResult.Ok("Required capabilities stopped.");
    }

    /// <inheritdoc />
    public Task<OperationResult> MoveRelativeAsync(
        MotionAxis axis,
        double distance,
        CancellationToken cancellationToken = default)
    {
        return motionControlCapability.MoveRelativeAsync(axis, distance, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationResult> StartDaqAsync(CancellationToken cancellationToken = default)
    {
        return dataAcquisitionCapability.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OperationResult> AcquireDaqAsync(
        TimeSpan duration,
        ScanGridSettings gridSettings,
        bool publishDemoFrame,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(duration, cancellationToken).ConfigureAwait(false);
        if (publishDemoFrame)
        {
            realtimeSignalImagingService.ProcessDemoFrame(gridSettings);
        }

        return OperationResult.Ok($"DAQ sampled for {duration.TotalMilliseconds:0} ms.");
    }

    /// <inheritdoc />
    public Task<OperationResult> StopDaqAsync(CancellationToken cancellationToken = default)
    {
        return dataAcquisitionCapability.StopAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationResult> StartBrightfieldLiveAsync(CancellationToken cancellationToken = default)
    {
        return brightfieldCameraCapability.StartLiveAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationResult> StopBrightfieldLiveAsync(CancellationToken cancellationToken = default)
    {
        return brightfieldCameraCapability.StopLiveAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationResult<BrightfieldCameraFrame>> CaptureBrightfieldAsync(
        CancellationToken cancellationToken = default)
    {
        return brightfieldCameraCapability.CaptureSingleAsync(cancellationToken);
    }

    /// <inheritdoc />
    public OperationResult PublishSignalPreview(ScanGridSettings gridSettings)
    {
        realtimeSignalImagingService.ProcessDemoFrame(gridSettings);
        return OperationResult.Ok("Signal preview published.");
    }

    /// <inheritdoc />
    public OperationResult RunAlgorithmPreview()
    {
        var result = algorithmProcessingCapability.ToGrayscaleBytes([0, 0.5, 1]);
        return result.Success
            ? OperationResult.Ok("Algorithm preview completed.")
            : OperationResult.Error(result.Message);
    }

    private async Task<OperationResult> InitializeOneAsync(
        ExperimentCapability capability,
        CancellationToken cancellationToken)
    {
        return capability switch
        {
            ExperimentCapability.Motion => await motionControlCapability.InitializeAsync(cancellationToken)
                .ConfigureAwait(false),
            ExperimentCapability.DataAcquisition => await ConfigureDaqAsync(cancellationToken)
                .ConfigureAwait(false),
            ExperimentCapability.BrightfieldCamera => await InitializeBrightfieldAsync(cancellationToken)
                .ConfigureAwait(false),
            ExperimentCapability.Laser => await InitializeLaserAsync(cancellationToken)
                .ConfigureAwait(false),
            ExperimentCapability.PhotoDetection => await photoDetectionCapability.InitializeAsync(cancellationToken)
                .ConfigureAwait(false),
            ExperimentCapability.Algorithm => RunAlgorithmPreview(),
            ExperimentCapability.SignalImaging => OperationResult.Ok("Signal imaging is event-driven."),
            ExperimentCapability.Reporting => OperationResult.Ok("Reporting is initialized on demand."),
            _ => OperationResult.Error($"Unsupported capability: {capability}.")
        };
    }

    private async Task<OperationResult> StopOneAsync(
        ExperimentCapability capability,
        CancellationToken cancellationToken)
    {
        return capability switch
        {
            ExperimentCapability.DataAcquisition => await dataAcquisitionCapability.StopAsync(cancellationToken)
                .ConfigureAwait(false),
            ExperimentCapability.BrightfieldCamera => await brightfieldCameraCapability.StopLiveAsync(cancellationToken)
                .ConfigureAwait(false),
            ExperimentCapability.Laser => await laserControlCapability.SetEmissionAsync(false, cancellationToken)
                .ConfigureAwait(false),
            ExperimentCapability.Motion => await motionControlCapability.StopAsync(cancellationToken)
                .ConfigureAwait(false),
            _ => OperationResult.Ok($"{capability} does not require stop.")
        };
    }

    private Task<OperationResult> ConfigureDaqAsync(CancellationToken cancellationToken)
    {
        var configurationResult = parameterProvider.GetDaqRuntimeConfiguration();
        return !configurationResult.Success || configurationResult.Data is null
            ? Task.FromResult(OperationResult.Error(configurationResult.Message))
            : dataAcquisitionCapability.ConfigureAsync(configurationResult.Data, cancellationToken);
    }

    private Task<OperationResult> InitializeBrightfieldAsync(CancellationToken cancellationToken)
    {
        var settingsResult = parameterProvider.GetBrightfieldCameraSettings();
        return !settingsResult.Success || settingsResult.Data is null
            ? Task.FromResult(OperationResult.Error(settingsResult.Message))
            : brightfieldCameraCapability.InitializeAsync(settingsResult.Data, cancellationToken);
    }

    private Task<OperationResult> InitializeLaserAsync(CancellationToken cancellationToken)
    {
        var settingsResult = parameterProvider.GetLaserRuntimeSettings();
        return !settingsResult.Success || settingsResult.Data is null
            ? Task.FromResult(OperationResult.Error(settingsResult.Message))
            : laserControlCapability.InitializeAsync(settingsResult.Data, cancellationToken);
    }
}
