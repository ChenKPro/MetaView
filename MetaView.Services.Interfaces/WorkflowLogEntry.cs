namespace MetaView.Services.Interfaces;

/// <summary>
/// Represents one workflow log line published by an application service.
/// </summary>
public sealed record WorkflowLogEntry(DateTimeOffset Timestamp, string Message, bool IsError = false)
{
    /// <summary>
    /// Gets the compact timestamp and message used by the shell log panel.
    /// </summary>
    public string DisplayText => $"{Timestamp:HH:mm:ss}  {Message}";
}
