namespace MetaView.Capabilities.Algorithms.Pipelines;

/// <summary>
/// Represents the context passed between spectral pipeline nodes.
/// </summary>
/// <param name="Value">Current pipeline value.</param>
public sealed record PipelineNodeContext(string Value);

/// <summary>
/// Represents the result of a spectral pipeline node.
/// </summary>
/// <param name="Succeeded">Whether the node succeeded.</param>
/// <param name="Value">Output value.</param>
/// <param name="Message">Optional message.</param>
public sealed record PipelineNodeResult(bool Succeeded, string Value, string? Message = null)
{
    /// <summary>
    /// Creates a successful pipeline node result.
    /// </summary>
    /// <param name="value">Output value.</param>
    /// <returns>Successful node result.</returns>
    public static PipelineNodeResult Success(string value) => new(true, value);
}

/// <summary>
/// Represents a reusable spectral processing pipeline node.
/// </summary>
public interface ISpectralPipelineNode
{
    /// <summary>
    /// Gets the stable node identifier.
    /// </summary>
    string NodeId { get; }

    /// <summary>
    /// Executes the node.
    /// </summary>
    /// <param name="context">Input context.</param>
    /// <returns>Node result.</returns>
    PipelineNodeResult Execute(PipelineNodeContext context);
}

