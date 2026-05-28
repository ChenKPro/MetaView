using MetaView.Core.MotionControl;
using Prism.Events;
using Vibronix.Foundation.Common.Results;
using Vibronix.Foundation.Communication.ModbusRtu;
using Vibronix.Foundation.Communication.SerialPortHelper;
using Vibronix.Foundation.Hardware.MotionController;
using Vibronix.Foundation.Hardware.MotionController.Common;
using Vibronix.Foundation.Hardware.MotionController.HDX;
using Vibronix.Foundation.Hardware.MotionController.Prior;
using Vibronix.Foundation.Hardware.MotionController.ZMotion;
using FoundationAxisStatus = Vibronix.Foundation.Hardware.MotionController.Common.AxisStatus;
using FoundationMotionState = Vibronix.Foundation.Hardware.MotionController.Common.AxisMotionState;

namespace MetaView.Capability.MotionControl;

/// <summary>
/// Routes logical MetaView motion axes to multiple physical Foundation motion controllers.
/// </summary>
public sealed class CompositeMotionControlCapability(MotionSystemConfiguration configuration, IEventAggregator eventAggregator)
    : IMotionControlCapability, IDisposable
{
    private readonly Dictionary<string, dynamic> _controllers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<MotionAxis, MotionAxisBinding> _axisBindings = configuration.AxisBindings.ToDictionary(binding => binding.Axis);
    private readonly Dictionary<MotionAxis, MotionAxisStatus> _lastStatuses = CreateInitialStatuses(configuration);
    private readonly SemaphoreSlim _controllerGate = new(1, 1);
    private PeriodicTimer? _statusTimer;
    private CancellationTokenSource? _monitoringCancellation;
    private Task? _monitoringTask;

    /// <inheritdoc />
    public MotionPosition CurrentPosition => new(
        _lastStatuses[MotionAxis.X].Position,
        _lastStatuses[MotionAxis.Y].Position,
        _lastStatuses[MotionAxis.Z].Position);

    /// <inheritdoc />
    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        foreach (var endpoint in configuration.Controllers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = CreateController(endpoint);
            if (!result.Success)
            {
                return Task.FromResult(result);
            }
        }

        PublishStatus();
        return Task.FromResult(OperationResult.Ok("Composite motion controllers initialized."));
    }

    /// <inheritdoc />
    public Task<OperationResult> StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default)
    {
        StopMonitoring();
        _monitoringCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _statusTimer = new PeriodicTimer(interval);
        _monitoringTask = MonitorStatusAsync(_monitoringCancellation.Token);
        PublishStatus();
        return Task.FromResult(OperationResult.Ok("Composite motion monitoring started."));
    }

    /// <inheritdoc />
    public async Task<OperationResult> StopMonitoringAsync(CancellationToken cancellationToken = default)
    {
        await StopMonitoringAsyncCore().ConfigureAwait(false);
        return OperationResult.Ok("Composite motion monitoring stopped.");
    }

    /// <inheritdoc />
    public async Task<OperationResult> HomeAsync(MotionAxis axis, CancellationToken cancellationToken = default)
    {
        var route = ResolveRoute(axis);
        if (!route.Success || route.Data is null)
        {
            return OperationResult.Error(route.Message, route.ResultCode);
        }

        OperationResult result;
        await _controllerGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            result = await route.Data.Controller.Home(route.Data.PhysicalAxisIndex, CreateHomeOptions(), cancellationToken);
            RefreshAxisStatusCore(axis);
        }
        finally
        {
            _controllerGate.Release();
        }

        PublishStatus();
        return ToOperationResult(result);
    }

    /// <inheritdoc />
    public async Task<OperationResult> MoveRelativeAsync(MotionAxis axis, double distance, CancellationToken cancellationToken = default)
    {
        var route = ResolveRoute(axis);
        if (!route.Success || route.Data is null)
        {
            return OperationResult.Error(route.Message, route.ResultCode);
        }

        OperationResult result;
        await _controllerGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            result = await route.Data.Controller.MoveRelative(route.Data.PhysicalAxisIndex, distance, cancellationToken);
            RefreshAxisStatusCore(axis);
        }
        finally
        {
            _controllerGate.Release();
        }

        PublishStatus();
        return ToOperationResult(result);
    }

    /// <inheritdoc />
    public async Task<OperationResult> MoveAbsoluteAsync(MotionAxis axis, double position, CancellationToken cancellationToken = default)
    {
        var route = ResolveRoute(axis);
        if (!route.Success || route.Data is null)
        {
            return OperationResult.Error(route.Message, route.ResultCode);
        }

        OperationResult result;
        await _controllerGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            result = await route.Data.Controller.MoveAbsolute(route.Data.PhysicalAxisIndex, position, cancellationToken);
            RefreshAxisStatusCore(axis);
        }
        finally
        {
            _controllerGate.Release();
        }

        PublishStatus();
        return ToOperationResult(result);
    }

    /// <inheritdoc />
    public async Task<OperationResult> StartJogAsync(MotionAxis axis, double speed, MotionJogDirection direction, CancellationToken cancellationToken = default)
    {
        var route = ResolveRoute(axis);
        if (!route.Success || route.Data is null)
        {
            return OperationResult.Error(route.Message, route.ResultCode);
        }

        OperationResult result;
        await _controllerGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            result = route.Data.Controller.Jog(route.Data.PhysicalAxisIndex, Math.Abs(speed), ToFoundationJogDirection(direction));
            RefreshAxisStatusCore(axis);
        }
        finally
        {
            _controllerGate.Release();
        }

        PublishStatus();
        return ToOperationResult(result);
    }

    /// <inheritdoc />
    public async Task<OperationResult> StopAxisAsync(MotionAxis axis, CancellationToken cancellationToken = default)
    {
        var route = ResolveRoute(axis);
        if (!route.Success || route.Data is null)
        {
            return OperationResult.Error(route.Message, route.ResultCode);
        }

        OperationResult result;
        await _controllerGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            result = route.Data.Controller.StopAxis(route.Data.PhysicalAxisIndex, StopMode.Halt);
            RefreshAxisStatusCore(axis);
        }
        finally
        {
            _controllerGate.Release();
        }

        PublishStatus();
        return ToOperationResult(result);
    }

    /// <inheritdoc />
    public async Task<OperationResult> StopAsync(CancellationToken cancellationToken = default)
    {
        var failed = new List<OperationResult>();
        await _controllerGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var endpoint in configuration.Controllers)
            {
                if (!_controllers.TryGetValue(endpoint.Id, out var controller))
                {
                    continue;
                }

                for (var axisIndex = 0; axisIndex < endpoint.AxisCount; axisIndex++)
                {
                    OperationResult result = controller.StopAxis(axisIndex, StopMode.Halt);
                    if (!result.Success)
                    {
                        failed.Add(result);
                    }
                }
            }

            RefreshAllStatusesCore();
        }
        finally
        {
            _controllerGate.Release();
        }

        PublishStatus();
        return failed.Count == 0 ? OperationResult.Ok("Composite motion stopped.") : ToOperationResult(failed[0]);
    }

    /// <inheritdoc />
    public async Task<OperationResult<MotionAxisStatus>> GetStatusAsync(MotionAxis axis, CancellationToken cancellationToken = default)
    {
        await _controllerGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            RefreshAxisStatusCore(axis);
            return OperationResult<MotionAxisStatus>.Ok(_lastStatuses[axis]);
        }
        finally
        {
            _controllerGate.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        StopMonitoring();
        foreach (var controller in _controllers.Values)
        {
            (controller as IDisposable)?.Dispose();
        }

        _controllerGate.Dispose();
    }

    private OperationResult CreateController(MotionControllerEndpoint endpoint)
    {
        var type = ParseControllerType(endpoint.ControllerType);
        OperationResult result;
        dynamic? controller;

        switch (type)
        {
            case MotionControllerType.Pusi:
            case MotionControllerType.Kaifull:
            {
                var options = new ModbusRtuOptions(endpoint.SlaveId, new SerialPortHelperOptions(endpoint.PortName, endpoint.BaudRate));
                var createResult = MotionControllerFactory.Create(type, options);
                controller = createResult.Data;
                result = ToOperationResult(createResult);
                break;
            }
            case MotionControllerType.Prior:
            {
                var options = new PriorConnectionOptions { PortName = endpoint.PortName, AxisCount = endpoint.AxisCount };
                var createResult = MotionControllerFactory.Create(type, options);
                controller = createResult.Data;
                result = ToOperationResult(createResult);
                break;
            }
            case MotionControllerType.ZMotionEthernet:
            {
                var options = new ZMotionEthernetOptions { IpAddress = endpoint.IpAddress, TimeoutMs = endpoint.TimeoutMs, AxisCount = endpoint.AxisCount };
                var createResult = MotionControllerFactory.Create(type, options);
                controller = createResult.Data;
                result = ToOperationResult(createResult);
                break;
            }
            case MotionControllerType.HeidStarGclib:
            {
                var options = new HeidStarConnectionOptions { IpAddress = endpoint.IpAddress, TimeoutMs = endpoint.TimeoutMs, AxisCount = endpoint.AxisCount };
                var createResult = MotionControllerFactory.Create(type, options);
                controller = createResult.Data;
                result = ToOperationResult(createResult);
                break;
            }
            case MotionControllerType.E53XMT:
            {
                var options = new SerialPortHelperOptions(endpoint.PortName, endpoint.BaudRate);
                var createResult = MotionControllerFactory.Create(type, options);
                controller = createResult.Data;
                result = ToOperationResult(createResult);
                break;
            }
            default:
                return OperationResult.Error($"Unsupported foundation motion controller: {endpoint.ControllerType}.");
        }

        if (result.Success && controller is not null)
        {
            _controllers[endpoint.Id] = controller;
        }

        return result.Success ? OperationResult.Ok($"{endpoint.Id} initialized.") : result;
    }

    private OperationResult<MotionRoute> ResolveRoute(MotionAxis axis)
    {
        if (!_axisBindings.TryGetValue(axis, out var binding))
        {
            return OperationResult<MotionRoute>.Error($"Motion axis {axis} is not bound to a controller.");
        }

        if (!_controllers.TryGetValue(binding.ControllerId, out var controller))
        {
            return OperationResult<MotionRoute>.Error($"Motion controller '{binding.ControllerId}' is not initialized.");
        }

        return OperationResult<MotionRoute>.Ok(new MotionRoute(controller, binding.PhysicalAxisIndex));
    }

    private async Task MonitorStatusAsync(CancellationToken cancellationToken)
    {
        if (_statusTimer is null)
        {
            return;
        }

        try
        {
            while (await _statusTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                if (await _controllerGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        RefreshAllStatusesCore();
                    }
                    finally
                    {
                        _controllerGate.Release();
                    }

                    PublishStatus();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void RefreshAllStatusesCore()
    {
        foreach (var axis in _axisBindings.Keys)
        {
            RefreshAxisStatusCore(axis);
        }
    }

    private void RefreshAxisStatusCore(MotionAxis axis)
    {
        var route = ResolveRoute(axis);
        if (!route.Success || route.Data is null)
        {
            return;
        }

        OperationResult<FoundationAxisStatus> result = route.Data.Controller.GetAxisStatus(route.Data.PhysicalAxisIndex);
        if (result.Success && result.Data is not null)
        {
            _lastStatuses[axis] = ToCoreStatus(axis, result.Data);
            return;
        }

        OperationResult<double> positionResult = route.Data.Controller.GetActualPosition(route.Data.PhysicalAxisIndex);
        if (positionResult.Success)
        {
            _lastStatuses[axis] = _lastStatuses[axis] with { Position = positionResult.Data, Message = positionResult.Message };
        }
    }

    private void PublishStatus()
    {
        eventAggregator
            .GetEvent<MotionStatusChangedEvent>()
            .Publish(new MotionStatusChangedEventArgs(CurrentPosition, _lastStatuses.Values.ToArray()));
    }

    private void StopMonitoring()
    {
        _monitoringCancellation?.Cancel();
        _statusTimer?.Dispose();
        _statusTimer = null;
        _monitoringCancellation?.Dispose();
        _monitoringCancellation = null;
        _monitoringTask = null;
    }

    private async Task StopMonitoringAsyncCore()
    {
        var monitoringTask = _monitoringTask;
        _monitoringCancellation?.Cancel();
        _statusTimer?.Dispose();

        if (monitoringTask is not null)
        {
            try
            {
                await monitoringTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        _statusTimer = null;
        _monitoringCancellation?.Dispose();
        _monitoringCancellation = null;
        _monitoringTask = null;
    }

    private static Dictionary<MotionAxis, MotionAxisStatus> CreateInitialStatuses(MotionSystemConfiguration config)
    {
        var axes = config.AxisBindings.Count == 0 ? [MotionAxis.X, MotionAxis.Y, MotionAxis.Z] : config.AxisBindings.Select(binding => binding.Axis);
        return axes.Distinct().ToDictionary(axis => axis, CreateUnknownStatus);
    }

    private static MotionAxisStatus ToCoreStatus(MotionAxis axis, FoundationAxisStatus status)
    {
        return new MotionAxisStatus(
            axis,
            status.ActualPosition,
            ToCoreState(status),
            status.IsEnabled,
            status.IsMoving,
            status.IsHomed,
            status.HasAlarm,
            status.AlarmMessage ?? status.MotionState.ToString());
    }

    private static MotionAxisStatus CreateUnknownStatus(MotionAxis axis)
    {
        return new MotionAxisStatus(axis, 0, MotionAxisState.Unknown, false, false, false, false, "Not initialized");
    }

    private static MotionAxisState ToCoreState(FoundationAxisStatus status)
    {
        if (status.HasAlarm)
        {
            return MotionAxisState.Alarm;
        }

        return status.MotionState switch
        {
            FoundationMotionState.Disabled => MotionAxisState.Disabled,
            FoundationMotionState.Standstill => status.IsHomed ? MotionAxisState.Homed : MotionAxisState.Ready,
            FoundationMotionState.Moving => MotionAxisState.Moving,
            FoundationMotionState.Homing => MotionAxisState.Homing,
            FoundationMotionState.Stopping => MotionAxisState.Stopped,
            FoundationMotionState.Error => MotionAxisState.Alarm,
            _ => status.IsMoving ? MotionAxisState.Moving : MotionAxisState.Unknown
        };
    }

    private static OperationResult ToOperationResult(OperationResult result)
    {
        return result.Success ? OperationResult.Ok(result.Message, result.ResultCode) : OperationResult.Error(result.Message, result.ResultCode);
    }

    private static OperationResult ToOperationResult<T>(OperationResult<T> result)
    {
        return result.Success ? OperationResult.Ok(result.Message, result.ResultCode) : OperationResult.Error(result.Message, result.ResultCode);
    }

    private static JogDirection ToFoundationJogDirection(MotionJogDirection direction)
    {
        return direction == MotionJogDirection.Negative ? JogDirection.Negative : JogDirection.Positive;
    }

    private static MotionControllerType ParseControllerType(string controllerType)
    {
        if (string.Equals(controllerType, "HDX", StringComparison.OrdinalIgnoreCase)
            || string.Equals(controllerType, "HeidStar", StringComparison.OrdinalIgnoreCase))
        {
            return MotionControllerType.HeidStarGclib;
        }

        return Enum.TryParse<MotionControllerType>(controllerType, ignoreCase: true, out var type)
            ? type
            : throw new InvalidOperationException($"Unsupported motion controller type '{controllerType}'.");
    }

    private static HomeOptions CreateHomeOptions()
    {
        return new HomeOptions
        {
            Direction = HomeDirection.Negative,
            Velocity = 5,
            Acceleration = 20,
            SearchVelocity = 5,
            CreepVelocity = 1,
            SetZeroAfterHome = true,
            TimeoutMs = 60000
        };
    }

    private sealed record MotionRoute(dynamic Controller, int PhysicalAxisIndex);
}
