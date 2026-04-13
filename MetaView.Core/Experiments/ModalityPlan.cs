namespace MetaView.Core.Experiments;

/// <summary>
/// Describes one imaging or spectroscopy modality inside a single experiment recipe.
/// </summary>
public sealed record ModalityPlan(
    string ModalityId,
    ImagingModality Modality,
    AcquisitionDimension Dimension,
    ScanPlan ScanPlan,
    ProcessingPlan ProcessingPlan,
    CapabilityPlan CapabilityPlan,
    IReadOnlyDictionary<string, string>? Parameters = null);
