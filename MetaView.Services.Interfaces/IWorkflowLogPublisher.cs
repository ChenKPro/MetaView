namespace MetaView.Services.Interfaces;

/// <summary>
/// Publishes workflow log entries to the presentation layer.
/// </summary>
public interface IWorkflowLogPublisher
{
    /// <summary>
    /// Publishes an informational workflow message.
    /// </summary>
    void Information(string message);

    /// <summary>
    /// Publishes an error workflow message.
    /// </summary>
    void Error(string message);
}
