namespace MetaView.Core.Experiments;

/// <summary>
/// Describes persistence preferences for an experiment.
/// </summary>
public sealed record SavePlan(
    bool AutoSave,
    string Directory,
    string Name,
    IReadOnlyList<DataProductKind>? ProductKinds = null);
