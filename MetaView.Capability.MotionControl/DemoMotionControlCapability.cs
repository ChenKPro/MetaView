using MetaView.Core.MotionControl;
using Prism.Events;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Capability.MotionControl;

/// <summary>
/// Provides a deterministic demo implementation of platform motion control.
/// </summary>
public sealed class DemoMotionControlCapability(IEventAggregator eventAggregator) : IMotionControlCapability
{
    private PeriodicTimer? _statusTimer;
    private CancellationTokenSource? _monitoringCancellation;
    private Task? _monitoringTask;

    private readonly Dictionary<MotionAxis, MotionAxisStatus> _statuses = new()
    {
        [MotionAxis.X] = CreateInitialStatus(MotionAxis.X, 4892.67),
        [MotionAxis.Y] = CreateInitialStatus(MotionAxis.Y, -630.24),
        [MotionAxis.Z] = CreateInitialStatus(MotionAxis.Z, 22921.2)
    };

    /// <inheritdoc />
    public MotionPosition CurrentPosition => new(
        _statuses[MotionAxis.X].Position,
        _statuses[MotionAxis.Y].Position,
        _statuses[MotionAxis.Z].Position);

    /// <inheritdoc />
    public async Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(120, cancellationToken).ConfigureAwait(false);

        foreach (var axis in _statuses.Keys.ToArray())
        {
            _statuses[axis] = _statuses[axis] with
            {
                State = MotionAxisState.Ready,
                IsEnabled = true,
                IsMoving = false,
                Message = "Demo axis ready"
            };
        }

        PublishStatus();
        return OperationResult.Ok("Demo motion controller initialized.");
    }

    /// <inheritdoc />
    public Task<OperationResult> StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default)
    {
        StopMonitoring();

        _monitoringCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _statusTimer = new PeriodicTimer(interval);
        _monitoringTask = MonitorStatusAsync(_monitoringCancellation.Token);
        PublishStatus();

        return Task.FromResult(OperationResult.Ok("Motion status monitoring started."));
    }

    /// <inheritdoc />
    public async Task<OperationResult> StopMonitoringAsync(CancellationToken cancellationToken = default)
    {
        await StopMonitoringAsyncCore().ConfigureAwait(false);
        return OperationResult.Ok("Motion status monitoring stopped.");
    }

    /// <inheritdoc />
    public async Task<OperationResult> HomeAsync(MotionAxis axis, CancellationToken cancellationToken = default)
    {
        await SetStateDuringMotionAsync(axis, MotionAxisState.Homing, cancellationToken).ConfigureAwait(false);
        SetAxisPosition(axis, 0, MotionAxisState.Homed, "Demo axis homed.");
        PublishStatus();
        return OperationResult.Ok("Axis homed.");
    }

    /// <inheritdoc />
    public async Task<OperationResult> MoveRelativeAsync(MotionAxis axis, double distance, CancellationToken cancellationToken = default)
    {
        await SetStateDuringMotionAsync(axis, MotionAxisState.Moving, cancellationToken).ConfigureAwait(false);
        SetAxisPosition(axis, _statuses[axis].Position + distance, MotionAxisState.Ready, "Relative move complete.");
        PublishStatus();
        return OperationResult.Ok("Relative move complete.");
    }

    /// <inheritdoc />
    public async Task<OperationResult> MoveAbsoluteAsync(MotionAxis axis, double position, CancellationToken cancellationToken = default)
    {
        await SetStateDuringMotionAsync(axis, MotionAxisState.Moving, cancellationToken).ConfigureAwait(false);
        SetAxisPosition(axis, position, MotionAxisState.Ready, "Absolute move complete.");
        PublishStatus();
        return OperationResult.Ok("Absolute move complete.");
    }

    /// <inheritdoc />
    public Task<OperationResult> StartJogAsync(MotionAxis axis, double speed, MotionJogDirection direction, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _statuses[axis] = _statuses[axis] with
        {
            State = MotionAxisState.Moving,
            IsMoving = true,
            Position = _statuses[axis].Position + speed * Math.Sign((int)direction) * 0.1,
            Message = $"Jogging {axis} {direction}"
        };

        PublishStatus();
        return Task.FromResult(OperationResult.Ok("Demo jog started."));
    }

    /// <inheritdoc />
    public Task<OperationResult> StopAxisAsync(MotionAxis axis, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _statuses[axis] = _statuses[axis] with
        {
            State = MotionAxisState.Stopped,
            IsMoving = false,
            Message = "Axis stopped."
        };

        PublishStatus();
        return Task.FromResult(OperationResult.Ok("Axis stopped."));
    }

    /// <inheritdoc />
    public Task<OperationResult> StopAsync(CancellationToken cancellationToken = default)
    {
        foreach (var axis in _statuses.Keys.ToArray())
        {
            _statuses[axis] = _statuses[axis] with
            {
                State = MotionAxisState.Stopped,
                IsMoving = false,
                Message = "Motion stopped."
            };
        }

        PublishStatus();
        return Task.FromResult(OperationResult.Ok("Motion stopped."));
    }

    /// <inheritdoc />
    public Task<OperationResult<MotionAxisStatus>> GetStatusAsync(MotionAxis axis, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperationResult<MotionAxisStatus>.Ok(_statuses[axis]));
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
                PublishStatus();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static MotionAxisStatus CreateInitialStatus(MotionAxis axis, double position)
    {
        return new MotionAxisStatus(
            axis,
            position,
            MotionAxisState.Ready,
            IsEnabled: true,
            IsMoving: false,
            IsHomed: false,
            HasAlarm: false,
            Message: "Demo axis ready");
    }

    private async Task SetStateDuringMotionAsync(MotionAxis axis, MotionAxisState state, CancellationToken cancellationToken)
    {
        _statuses[axis] = _statuses[axis] with
        {
            State = state,
            IsMoving = true,
            Message = state == MotionAxisState.Homing ? "Homing..." : "Moving..."
        };

        PublishStatus();
        await Task.Delay(120, cancellationToken).ConfigureAwait(false);
    }

    private void SetAxisPosition(MotionAxis axis, double position, MotionAxisState state, string message)
    {
        _statuses[axis] = _statuses[axis] with
        {
            Position = position,
            State = state,
            IsMoving = false,
            IsHomed = state == MotionAxisState.Homed || _statuses[axis].IsHomed,
            Message = message
        };
    }

    private void PublishStatus()
    {
        eventAggregator
            .GetEvent<MotionStatusChangedEvent>()
            .Publish(new MotionStatusChangedEventArgs(CurrentPosition, _statuses.Values.ToArray()));
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
}
