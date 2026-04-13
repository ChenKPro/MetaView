using MetaView.Core.Imaging.Signal;
using Prism.Events;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Prism event published when a new normalized signal image frame is available.
/// </summary>
public sealed class SignalImageFramePublishedEvent : PubSubEvent<SignalImageFrame>
{
}
