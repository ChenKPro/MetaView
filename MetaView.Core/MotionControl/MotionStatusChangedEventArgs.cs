namespace MetaView.Core.MotionControl;

/// <summary>
/// Provides updated position and axis status published by the motion capability.
/// </summary>
public sealed class MotionStatusChangedEventArgs : EventArgs
{
    public MotionStatusChangedEventArgs(MotionPosition position, IReadOnlyList<MotionAxisStatus> axisStatuses)
    {
        Position = position;
        AxisStatuses = axisStatuses;
    }

    /// <summary>
    /// Gets the latest logical stage position.
    /// </summary>
    public MotionPosition Position { get; }

    /// <summary>
    /// Gets the latest axis statuses.
    /// </summary>
    public IReadOnlyList<MotionAxisStatus> AxisStatuses { get; }
}
