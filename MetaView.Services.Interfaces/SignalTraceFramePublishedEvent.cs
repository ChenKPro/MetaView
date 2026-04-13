using MetaView.Core.Imaging.Signal;
using Prism.Events;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Prism event published when a new four-channel signal trace is available.
/// </summary>
public sealed class SignalTraceFramePublishedEvent : PubSubEvent<SignalTraceFrame>
{
}
