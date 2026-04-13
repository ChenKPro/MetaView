using MetaView.Core.Experiments;

namespace MetaView.Services.Workflows;

/// <summary>
/// Carries recipe state and generated products between workflow steps.
/// </summary>
public sealed class WorkflowExecutionContext(ExperimentRecipe recipe)
{
    private readonly List<ExperimentDataProduct> _dataProducts = [];

    /// <summary>
    /// Gets the experiment recipe being executed.
    /// </summary>
    public ExperimentRecipe Recipe { get; } = recipe;

    /// <summary>
    /// Gets produced data products.
    /// </summary>
    public IReadOnlyList<ExperimentDataProduct> DataProducts => _dataProducts;

    /// <summary>
    /// Adds a data product produced by the workflow.
    /// </summary>
    public void AddDataProduct(ExperimentDataProduct dataProduct)
    {
        _dataProducts.Add(dataProduct);
    }
}
