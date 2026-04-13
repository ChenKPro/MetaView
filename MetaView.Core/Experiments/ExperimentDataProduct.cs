namespace MetaView.Core.Experiments;

/// <summary>
/// Describes one data product produced by an experiment workflow.
/// </summary>
public sealed record ExperimentDataProduct(
    DataProductKind Kind,
    string Name,
    string Description,
    IReadOnlyDictionary<string, string>? Metadata = null);
