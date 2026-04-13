namespace MetaView.Core.MotionControl;

/// <summary>
/// Describes the high-level lifecycle state of a logical motion axis.
/// </summary>
public enum MotionAxisState
{
    Unknown,
    Disabled,
    Ready,
    Moving,
    Homing,
    Homed,
    Alarm,
    Stopped
}
