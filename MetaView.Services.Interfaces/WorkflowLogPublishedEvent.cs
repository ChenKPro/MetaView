using Prism.Events;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Prism event published whenever a service workflow emits a log line.
/// </summary>
public sealed class WorkflowLogPublishedEvent : PubSubEvent<WorkflowLogEntry>
{
}
