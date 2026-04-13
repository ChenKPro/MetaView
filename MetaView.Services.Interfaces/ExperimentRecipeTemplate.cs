using MetaView.Core.Experiments;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Describes one reusable experiment recipe template.
/// </summary>
public sealed record ExperimentRecipeTemplate(
    string TemplateId,
    string DisplayName,
    AcquisitionDimension Dimension,
    ImagingModality Modality,
    IReadOnlyList<ExperimentCapability> Capabilities,
    Func<ExperimentRecipe> CreateRecipe);
