using MetaView.Core.Experiments;
using MetaView.Core.Parameters;
using MetaView.Services.Interfaces;

namespace MetaView.Services;

/// <summary>
/// Performs lightweight recipe and runtime parameter validation before workflow execution.
/// </summary>
public sealed class ExperimentPreflightValidator(IRuntimeParameterProvider parameterProvider) : IExperimentPreflightValidator
{
    /// <inheritdoc />
    public ExperimentPreflightReport Validate(ExperimentRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var issues = new List<ExperimentPreflightIssue>();
        ValidateRecipe(recipe, issues);
        foreach (var capability in recipe.EffectiveModalities
                     .SelectMany(modality => modality.CapabilityPlan.Required)
                     .Concat(recipe.EffectiveCapabilityPlan.Required)
                     .Distinct())
        {
            ValidateCapability(capability, issues);
        }

        if (issues.Count == 0)
        {
            issues.Add(new ExperimentPreflightIssue(false, "Preflight passed."));
        }

        return new ExperimentPreflightReport(issues);
    }

    private static void ValidateRecipe(
        ExperimentRecipe recipe,
        ICollection<ExperimentPreflightIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(recipe.RecipeId))
        {
            issues.Add(new ExperimentPreflightIssue(true, "Recipe id is required."));
        }

        if (recipe.ScanPlan.SizeX <= 0 || recipe.ScanPlan.SizeY <= 0 || recipe.ScanPlan.SizeZ <= 0 || recipe.ScanPlan.SizeT <= 0)
        {
            issues.Add(new ExperimentPreflightIssue(true, "Scan sizes must be positive."));
        }

        foreach (var modality in recipe.EffectiveModalities)
        {
            if (string.IsNullOrWhiteSpace(modality.ModalityId))
            {
                issues.Add(new ExperimentPreflightIssue(true, "Every modality requires a modality id."));
            }

            if (modality.ScanPlan.SizeX <= 0 || modality.ScanPlan.SizeY <= 0 || modality.ScanPlan.SizeZ <= 0 || modality.ScanPlan.SizeT <= 0)
            {
                issues.Add(new ExperimentPreflightIssue(true, $"{modality.ModalityId} scan sizes must be positive."));
            }
        }
    }

    private void ValidateCapability(
        ExperimentCapability capability,
        ICollection<ExperimentPreflightIssue> issues)
    {
        switch (capability)
        {
            case ExperimentCapability.Motion:
                ValidateMotion(issues);
                break;
            case ExperimentCapability.DataAcquisition:
                ValidateDaq(issues);
                break;
            case ExperimentCapability.BrightfieldCamera:
                ValidateBrightfield(issues);
                break;
            case ExperimentCapability.Laser:
                ValidateLaser(issues);
                break;
            case ExperimentCapability.SignalImaging:
            case ExperimentCapability.Algorithm:
            case ExperimentCapability.PhotoDetection:
            case ExperimentCapability.Reporting:
                issues.Add(new ExperimentPreflightIssue(false, $"{capability} capability is registered for workflow use."));
                break;
            default:
                issues.Add(new ExperimentPreflightIssue(true, $"Unsupported capability: {capability}."));
                break;
        }
    }

    private void ValidateMotion(ICollection<ExperimentPreflightIssue> issues)
    {
        var result = parameterProvider.GetMotionSystemConfiguration();
        if (!result.Success || result.Data is null)
        {
            issues.Add(new ExperimentPreflightIssue(true, $"Motion configuration failed: {result.Message}"));
            return;
        }

        var configuration = result.Data;
        if (!configuration.UseDemo && configuration.Controllers.Count == 0)
        {
            issues.Add(new ExperimentPreflightIssue(true, "Motion requires at least one controller when demo mode is disabled."));
        }

        if (!configuration.UseDemo && configuration.AxisBindings.Count == 0)
        {
            issues.Add(new ExperimentPreflightIssue(true, "Motion requires axis bindings when demo mode is disabled."));
        }

        issues.Add(new ExperimentPreflightIssue(false, configuration.UseDemo
            ? "Motion uses demo controller."
            : $"Motion controllers configured: {configuration.Controllers.Count}."));
    }

    private void ValidateDaq(ICollection<ExperimentPreflightIssue> issues)
    {
        var result = parameterProvider.GetDaqRuntimeConfiguration();
        if (!result.Success || result.Data is null)
        {
            issues.Add(new ExperimentPreflightIssue(true, $"DAQ configuration failed: {result.Message}"));
            return;
        }

        if (!result.Data.UseDemo && string.IsNullOrWhiteSpace(result.Data.ConfigurationPath))
        {
            issues.Add(new ExperimentPreflightIssue(true, "DAQ configuration path is required when demo mode is disabled."));
        }

        issues.Add(new ExperimentPreflightIssue(false, result.Data.UseDemo
            ? "DAQ uses demo acquisition."
            : $"DAQ configuration path: {result.Data.ConfigurationPath}."));
    }

    private void ValidateBrightfield(ICollection<ExperimentPreflightIssue> issues)
    {
        var result = parameterProvider.GetBrightfieldCameraSettings();
        if (!result.Success || result.Data is null)
        {
            issues.Add(new ExperimentPreflightIssue(true, $"Brightfield configuration failed: {result.Message}"));
            return;
        }

        if (!string.Equals(result.Data.CameraType, "Demo", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(result.Data.CameraId))
        {
            issues.Add(new ExperimentPreflightIssue(false, "Brightfield camera id is empty; first available camera will be used."));
        }

        issues.Add(new ExperimentPreflightIssue(false, $"Brightfield camera type: {result.Data.CameraType}."));
    }

    private void ValidateLaser(ICollection<ExperimentPreflightIssue> issues)
    {
        var result = parameterProvider.GetLaserRuntimeSettings();
        if (!result.Success || result.Data is null)
        {
            issues.Add(new ExperimentPreflightIssue(true, $"Laser configuration failed: {result.Message}"));
            return;
        }

        issues.Add(new ExperimentPreflightIssue(false, $"Laser type: {result.Data.LaserType}."));
    }
}
