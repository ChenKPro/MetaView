namespace MetaView.Services.Interfaces;

/// <summary>
/// Describes one workflow preflight validation message.
/// </summary>
public sealed record ExperimentPreflightIssue(
    bool IsBlocking,
    string Message);
