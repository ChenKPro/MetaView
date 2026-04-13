using Prism.Events;

namespace MetaView.Core.Imaging.Brightfield;

/// <summary>
/// Publishes brightfield camera frames to presentation subscribers.
/// </summary>
public sealed class BrightfieldCameraFramePublishedEvent : PubSubEvent<BrightfieldCameraFrame>
{
}
