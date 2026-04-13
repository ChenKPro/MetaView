namespace MetaView.Core.MotionControl;

/// <summary>
/// Represents the current status of one logical motion axis.
/// </summary>
public sealed record MotionAxisStatus(
    MotionAxis Axis,
    double Position,
    MotionAxisState State,
    bool IsEnabled,
    bool IsMoving,
    bool IsHomed,
    bool HasAlarm,
    string Message);
