using Prism.Events;

namespace MetaView.Core.MotionControl;

/// <summary>
/// Prism event published when the motion capability refreshes position or axis state.
/// </summary>
public sealed class MotionStatusChangedEvent : PubSubEvent<MotionStatusChangedEventArgs>
{
}
