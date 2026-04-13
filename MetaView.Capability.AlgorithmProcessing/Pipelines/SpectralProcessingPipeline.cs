namespace MetaView.Capabilities.Algorithms.Pipelines;

/// <summary>
/// Executes spectral pipeline nodes in order.
/// </summary>
public sealed class SpectralProcessingPipeline
{
    private readonly IReadOnlyList<ISpectralPipelineNode> _nodes;

    /// <summary>
    /// Initializes a spectral processing pipeline.
    /// </summary>
    /// <param name="nodes">Nodes to execute in order.</param>
    public SpectralProcessingPipeline(IReadOnlyList<ISpectralPipelineNode> nodes)
    {
        _nodes = nodes;
    }

    /// <summary>
    /// Executes all nodes and passes each node output to the next node.
    /// </summary>
    /// <param name="context">Initial context.</param>
    /// <returns>Final pipeline result.</returns>
    public PipelineNodeResult Execute(PipelineNodeContext context)
    {
        var current = context;
        PipelineNodeResult? lastResult = null;

        foreach (var node in _nodes)
        {
            lastResult = node.Execute(current);
            if (!lastResult.Succeeded)
            {
                return lastResult;
            }

            current = new PipelineNodeContext(lastResult.Value);
        }

        return lastResult ?? PipelineNodeResult.Success(context.Value);
    }
}

