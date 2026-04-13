namespace MetaView.Core.Experiments;

/// <summary>
/// Combines dimensionality, modality, scan, processing, and save settings into an executable experiment.
/// </summary>
public sealed record ExperimentRecipe(
    string RecipeId,
    AcquisitionDimension Dimension,
    ImagingModality Modality,
    ScanPlan ScanPlan,
    ProcessingPlan ProcessingPlan,
    SavePlan SavePlan,
    IReadOnlyDictionary<string, string>? Metadata = null,
    IReadOnlyList<ModalityPlan>? Modalities = null,
    CapabilityPlan? CapabilityPlan = null)
{
    /// <summary>
    /// Gets the effective modality plans. A single-modality recipe is exposed as one plan.
    /// </summary>
    public IReadOnlyList<ModalityPlan> EffectiveModalities =>
        Modalities is { Count: > 0 }
            ? Modalities
            :
            [
                new ModalityPlan(
                    RecipeId,
                    Modality,
                    Dimension,
                    ScanPlan,
                    ProcessingPlan,
                    CapabilityPlan ?? global::MetaView.Core.Experiments.CapabilityPlan.Empty,
                    Metadata)
            ];

    /// <summary>
    /// Gets the effective capability plan for the recipe.
    /// </summary>
    public CapabilityPlan EffectiveCapabilityPlan =>
        CapabilityPlan ?? global::MetaView.Core.Experiments.CapabilityPlan.Empty;
}
