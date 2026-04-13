using MetaView.Services.Interfaces;
using Prism.Events;

namespace MetaView.Services;

/// <summary>
/// Publishes workflow logs through Prism events.
/// </summary>
public sealed class WorkflowLogPublisher(IEventAggregator eventAggregator) : IWorkflowLogPublisher
{
    /// <inheritdoc />
    public void Information(string message)
    {
        Publish(message, isError: false);
    }

    /// <inheritdoc />
    public void Error(string message)
    {
        Publish(message, isError: true);
    }

    private void Publish(string message, bool isError)
    {
        eventAggregator
            .GetEvent<WorkflowLogPublishedEvent>()
            .Publish(new WorkflowLogEntry(DateTimeOffset.Now, message, isError));
    }
}
