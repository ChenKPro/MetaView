using MetaView.Core.Experiments;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Provides built-in experiment recipe templates used by the MetaView shell.
/// </summary>
public static class ExperimentRecipeCatalog
{
    /// <summary>
    /// Gets the built-in SRS 2D template id.
    /// </summary>
    public const string SrsTwoDTemplateId = "template.srs.2d";

    /// <summary>
    /// Gets the built-in brightfield 2D template id.
    /// </summary>
    public const string BrightfieldTwoDTemplateId = "template.brightfield.2d";

    /// <summary>
    /// Gets the built-in SRS plus brightfield 2D template id.
    /// </summary>
    public const string SrsBrightfieldTwoDTemplateId = "template.srs-brightfield.2d";

    private static readonly IReadOnlyList<ExperimentRecipeTemplate> BuiltInTemplates =
    [
        new(
            SrsTwoDTemplateId,
            "SRS 2D",
            AcquisitionDimension.TwoD,
            ImagingModality.Srs,
            [
                ExperimentCapability.Motion,
                ExperimentCapability.DataAcquisition,
                ExperimentCapability.SignalImaging,
                ExperimentCapability.Algorithm
            ],
            () => DemoExperimentRecipes.CreateSrsTwoD(10, 10, 0.5, TimeSpan.FromMilliseconds(300))),
        new(
            BrightfieldTwoDTemplateId,
            "Brightfield 2D",
            AcquisitionDimension.TwoD,
            ImagingModality.Brightfield,
            [
                ExperimentCapability.BrightfieldCamera
            ],
            DemoExperimentRecipes.CreateBrightfieldTwoD),
        new(
            SrsBrightfieldTwoDTemplateId,
            "SRS + Brightfield 2D",
            AcquisitionDimension.TwoD,
            ImagingModality.Multimodal,
            [
                ExperimentCapability.Motion,
                ExperimentCapability.DataAcquisition,
                ExperimentCapability.SignalImaging,
                ExperimentCapability.Algorithm,
                ExperimentCapability.BrightfieldCamera
            ],
            () => DemoExperimentRecipes.CreateSrsBrightfieldTwoD(10, 10, 0.5, TimeSpan.FromMilliseconds(300)))
    ];

    /// <summary>
    /// Gets all built-in templates.
    /// </summary>
    public static IReadOnlyList<ExperimentRecipeTemplate> Templates => BuiltInTemplates;

    /// <summary>
    /// Creates a recipe from the selected built-in template.
    /// </summary>
    public static ExperimentRecipe Create(string templateId)
    {
        var template = Templates.FirstOrDefault(candidate => candidate.TemplateId == templateId);
        if (template is null)
        {
            throw new ArgumentException($"Unknown recipe template: {templateId}.", nameof(templateId));
        }

        return template.CreateRecipe();
    }
}
