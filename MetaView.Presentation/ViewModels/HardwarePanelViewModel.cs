using System.Windows.Input;
using MetaView.Core.MotionControl;
using MetaView.Core.Parameters;
using MetaView.Presentation.Core;
using MetaView.Presentation.Infrastructure;
using MetaView.Presentation.Services;
using Prism.Events;
using AsyncDelegateCommand = MetaView.Presentation.Infrastructure.AsyncDelegateCommand;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Exposes simulated hardware controls for the left instrument panel.
/// </summary>
public sealed class HardwarePanelViewModel : MetaView.Presentation.Infrastructure.BindableBase
{
    private readonly IMotionControlCapability _motionControlCapability;
    private readonly ILaserService _laserService;
    private readonly IRuntimeParameterProvider _parameterProvider;
    private double _x;
    private double _y;
    private double _z;
    private double _xyStep = 10;
    private double _zStep = 0.5;
    private MotionPosition? _savedPosition;
    private bool _stageConnected = true;
    private bool _laserConnected = true;
    private bool _emission = true;
    private int _warmupProgress = 100;
    private string _status = "Standby";

    /// <summary>
    /// Initializes a new instance of the <see cref="HardwarePanelViewModel" /> class.
    /// </summary>
    public HardwarePanelViewModel(
        IMotionControlCapability motionControlCapability,
        ILaserService laserService,
        IRuntimeParameterProvider parameterProvider,
        IEventAggregator eventAggregator,
        ScanSetupViewModel scan)
    {
        _motionControlCapability = motionControlCapability;
        _laserService = laserService;
        _parameterProvider = parameterProvider;
        Scan = scan;
        eventAggregator
            .GetEvent<MotionStatusChangedEvent>()
            .Subscribe(ApplyMotionStatus, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);

        var position = _motionControlCapability.CurrentPosition;
        _x = position.X;
        _y = position.Y;
        _z = position.Z;
        var laserSettings = _parameterProvider.GetLaserRuntimeSettings().Data;
        if (laserSettings is not null)
        {
            _emission = laserSettings.EmissionEnabled;
            _laserConnected = string.Equals(laserSettings.LaserType, "Demo", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(laserSettings.DeviceId);
        }

        MoveXNegativeCommand = new AsyncDelegateCommand(() => MoveAsync(Axis.X, -XyStep));
        MoveXPositiveCommand = new AsyncDelegateCommand(() => MoveAsync(Axis.X, XyStep));
        MoveYNegativeCommand = new AsyncDelegateCommand(() => MoveAsync(Axis.Y, -XyStep));
        MoveYPositiveCommand = new AsyncDelegateCommand(() => MoveAsync(Axis.Y, XyStep));
        MoveZNegativeCommand = new AsyncDelegateCommand(() => MoveAsync(Axis.Z, -ZStep));
        MoveZPositiveCommand = new AsyncDelegateCommand(() => MoveAsync(Axis.Z, ZStep));
        StopCommand = new AsyncDelegateCommand(StopAsync);
        SavePositionCommand = new AsyncDelegateCommand(SavePositionAsync);
        MoveToSavedPositionCommand = new AsyncDelegateCommand(MoveToSavedPositionAsync);
        UnloadCommand = new AsyncDelegateCommand(UnloadAsync);
        WarmupCommand = new AsyncDelegateCommand(WarmupAsync);

        _ = InitializeMotionAsync();
    }

    public ScanSetupViewModel Scan { get; }
    public HardwarePanelViewModel Hardware => this;
    public double X { get => _x; set => SetProperty(ref _x, value); }
    public double Y { get => _y; set => SetProperty(ref _y, value); }
    public double Z { get => _z; set => SetProperty(ref _z, value); }
    public double XyStep { get => _xyStep; set => SetProperty(ref _xyStep, Math.Round(value, 2)); }
    public double ZStep { get => _zStep; set => SetProperty(ref _zStep, value); }
    public bool StageConnected { get => _stageConnected; set => SetProperty(ref _stageConnected, value); }
    public bool LaserConnected { get => _laserConnected; set => SetProperty(ref _laserConnected, value); }
    public bool Emission { get => _emission; set => SetProperty(ref _emission, value); }
    public int WarmupProgress { get => _warmupProgress; set => SetProperty(ref _warmupProgress, value); }
    public string Status { get => _status; set => SetProperty(ref _status, value); }

    public ICommand MoveXNegativeCommand { get; }
    public ICommand MoveXPositiveCommand { get; }
    public ICommand MoveYNegativeCommand { get; }
    public ICommand MoveYPositiveCommand { get; }
    public ICommand MoveZNegativeCommand { get; }
    public ICommand MoveZPositiveCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand SavePositionCommand { get; }
    public ICommand MoveToSavedPositionCommand { get; }
    public ICommand UnloadCommand { get; }
    public ICommand WarmupCommand { get; }

    private async Task InitializeMotionAsync()
    {
        Status = "Initializing";
        var initializeResult = await _motionControlCapability.InitializeAsync(CancellationToken.None).ConfigureAwait(true);
        if (!initializeResult.Success)
        {
            StageConnected = false;
            Status = initializeResult.Message;
            return;
        }

        var monitorResult = await _motionControlCapability
            .StartMonitoringAsync(TimeSpan.FromMilliseconds(200), CancellationToken.None)
            .ConfigureAwait(true);

        StageConnected = monitorResult.Success;
        Status = monitorResult.Success ? "Standby" : monitorResult.Message;
        RefreshPosition();
    }

    private async Task MoveAsync(Axis axis, double step)
    {
        Status = $"Moving {axis}";
        var result = await _motionControlCapability
            .MoveRelativeAsync(ToMotionAxis(axis), step, CancellationToken.None)
            .ConfigureAwait(true);

        if (!result.Success)
        {
            Status = result.Message;
            return;
        }

        RefreshPosition();
        Status = "Standby";
    }

    private async Task StopAsync()
    {
        Status = "Stage stopped";
        await _motionControlCapability.StopAsync(CancellationToken.None).ConfigureAwait(true);
        await Task.Delay(800).ConfigureAwait(true);
        Status = "Standby";
    }

    private Task SavePositionAsync()
    {
        _savedPosition = _motionControlCapability.CurrentPosition;
        RefreshPosition();
        Status = "Position saved";
        return Task.CompletedTask;
    }

    private async Task MoveToSavedPositionAsync()
    {
        if (_savedPosition is null)
        {
            Status = "No saved position";
            return;
        }

        Status = "Moving saved pos";
        if (!await MoveAbsoluteAxisAsync(MotionAxis.X, _savedPosition.X).ConfigureAwait(true))
        {
            return;
        }

        if (!await MoveAbsoluteAxisAsync(MotionAxis.Y, _savedPosition.Y).ConfigureAwait(true))
        {
            return;
        }

        if (!await MoveAbsoluteAxisAsync(MotionAxis.Z, _savedPosition.Z).ConfigureAwait(true))
        {
            return;
        }

        RefreshPosition();
        Status = "Standby";
    }

    private async Task UnloadAsync()
    {
        Status = "Unloading";
        if (!await MoveAbsoluteAxisAsync(MotionAxis.Z, 0).ConfigureAwait(true))
        {
            return;
        }

        if (!await MoveAbsoluteAxisAsync(MotionAxis.X, 0).ConfigureAwait(true))
        {
            return;
        }

        if (!await MoveAbsoluteAxisAsync(MotionAxis.Y, 0).ConfigureAwait(true))
        {
            return;
        }

        RefreshPosition();
        Status = "Unload ready";
    }

    private async Task WarmupAsync()
    {
        var settingsResult = _parameterProvider.GetLaserRuntimeSettings();
        if (!settingsResult.Success || settingsResult.Data is null)
        {
            Status = settingsResult.Message;
            return;
        }

        Status = "Laser warming";
        var progress = new Progress<int>(value => WarmupProgress = value);
        await _laserService.WarmupAsync(progress, CancellationToken.None).ConfigureAwait(true);
        Emission = settingsResult.Data.EmissionEnabled;
        Status = $"Laser ready {settingsResult.Data.PowerMilliWatts:0.##} mW";
    }

    private static MotionAxis ToMotionAxis(Axis axis)
    {
        return axis switch
        {
            Axis.X => MotionAxis.X,
            Axis.Y => MotionAxis.Y,
            Axis.Z => MotionAxis.Z,
            _ => MotionAxis.X
        };
    }

    private async Task<bool> MoveAbsoluteAxisAsync(MotionAxis axis, double position)
    {
        var result = await _motionControlCapability
            .MoveAbsoluteAsync(axis, position, CancellationToken.None)
            .ConfigureAwait(true);

        if (result.Success)
        {
            return true;
        }

        Status = result.Message;
        return false;
    }

    private void RefreshPosition()
    {
        var position = _motionControlCapability.CurrentPosition;
        X = position.X;
        Y = position.Y;
        Z = position.Z;
    }

    private void ApplyMotionStatus(MotionStatusChangedEventArgs e)
    {
        X = e.Position.X;
        Y = e.Position.Y;
        Z = e.Position.Z;
        StageConnected = e.AxisStatuses.All(status => status.IsEnabled);

        var movingAxis = e.AxisStatuses.FirstOrDefault(status => status.IsMoving);
        var alarmAxis = e.AxisStatuses.FirstOrDefault(status => status.HasAlarm);
        Status = alarmAxis is not null
            ? $"{alarmAxis.Axis} alarm"
            : movingAxis is not null
                ? $"Moving {movingAxis.Axis}"
                : "Standby";
    }
}
