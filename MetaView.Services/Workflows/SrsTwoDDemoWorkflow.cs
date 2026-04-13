using MetaView.Core.Experiments;
using MetaView.Core.Imaging.Signal;
using MetaView.Core.MotionControl;
using MetaView.Services.Interfaces;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Services.Workflows;

/// <summary>
/// Demonstrates a two-dimensional SRS workflow using motion, DAQ, and signal imaging capabilities.
/// </summary>
public sealed class SrsTwoDDemoWorkflow(
    IExperimentCapabilityInvoker capabilityInvoker,
    IWorkflowLogPublisher logPublisher)
    : ExperimentWorkflowBase(logPublisher)
{
    /// <inheritdoc />
    public override string WorkflowId => "SRS-2D-Demo";

    /// <inheritdoc />
    public override bool CanRun(ExperimentRecipe recipe)
    {
        return recipe.Dimension == AcquisitionDimension.TwoD
            && recipe.Modality == ImagingModality.Srs
            && recipe.ProcessingPlan.Mode == ProcessingMode.SignalImage;
    }

    /// <inheritdoc />
    protected override IReadOnlyList<WorkflowStep> BuildSteps(ExperimentRecipe recipe)
    {
        return
        [
            Step("validate.recipe", "Validate recipe", ValidateRecipeAsync),
            Step("capabilities.initialize", "Initialize capabilities", (_, token) => capabilityInvoker.InitializeAsync(recipe.EffectiveCapabilityPlan, token)),
            Step("publish.preview", "Publish signal preview", PublishSignalPreviewAsync),
            Step("motion.move.x", "Move X", (_, token) => capabilityInvoker.MoveRelativeAsync(MotionAxis.X, GetDouble(recipe, DemoExperimentRecipes.XRelativeDistanceKey), token)),
            Step("motion.move.y", "Move Y", (_, token) => capabilityInvoker.MoveRelativeAsync(MotionAxis.Y, GetDouble(recipe, DemoExperimentRecipes.YRelativeDistanceKey), token)),
            Step("motion.move.z", "Move Z", (_, token) => capabilityInvoker.MoveRelativeAsync(MotionAxis.Z, GetDouble(recipe, DemoExperimentRecipes.ZRelativeDistanceKey), token)),
            Step("daq.start", "Start DAQ", (_, token) => capabilityInvoker.StartDaqAsync(token)),
            Step("daq.acquire", "Acquire DAQ", AcquireDaqAsync),
            Step("capabilities.stop", "Stop capabilities", (_, token) => capabilityInvoker.StopAsync(recipe.EffectiveCapabilityPlan, token))
        ];

        static WorkflowStep Step(
            string stepId,
            string displayName,
            Func<WorkflowExecutionContext, CancellationToken, Task<OperationResult>> executeAsync)
        {
            return new WorkflowStep(stepId, displayName, executeAsync);
        }
    }

    private Task<OperationResult> ValidateRecipeAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken)
    {
        var recipe = context.Recipe;
        if (recipe.Dimension != AcquisitionDimension.TwoD)
        {
            return Task.FromResult(OperationResult.Error($"Recipe dimension must be {AcquisitionDimension.TwoD}."));
        }

        if (recipe.ScanPlan.SizeX <= 0 || recipe.ScanPlan.SizeY <= 0)
        {
            return Task.FromResult(OperationResult.Error("2D acquisition requires positive X and Y sizes."));
        }

        if (recipe.Modality != ImagingModality.Srs)
        {
            return Task.FromResult(OperationResult.Error($"Recipe modality must be {ImagingModality.Srs}."));
        }

        if (recipe.ProcessingPlan.Mode is not ProcessingMode.SignalImage and not ProcessingMode.Spectrum)
        {
            return Task.FromResult(OperationResult.Error("SRS requires spectrum or signal-image processing."));
        }

        return Task.FromResult(OperationResult.Ok("SRS 2D recipe is valid."));
    }

    private Task<OperationResult> PublishSignalPreviewAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken)
    {
        var result = capabilityInvoker.PublishSignalPreview(CreateGridSettings(context.Recipe));
        if (!result.Success)
        {
            return Task.FromResult(result);
        }

        context.AddDataProduct(new ExperimentDataProduct(
            DataProductKind.Image,
            "Demo SRS signal image",
            "Preview image generated from AI0/AI1 positions and AI2/AI3 signals."));
        context.AddDataProduct(new ExperimentDataProduct(
            DataProductKind.Curve,
            "Demo four-channel signal trace",
            "Signal trace generated from AI0, AI1, AI2, and AI3."));
        return Task.FromResult(result);
    }

    private async Task<OperationResult> AcquireDaqAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken)
    {
        var duration = TimeSpan.FromMilliseconds(GetDouble(context.Recipe, DemoExperimentRecipes.DaqAcquireMillisecondsKey));
        return await capabilityInvoker.AcquireDaqAsync(
                duration,
                CreateGridSettings(context.Recipe),
                publishDemoFrame: true,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static ScanGridSettings CreateGridSettings(ExperimentRecipe recipe)
    {
        return new ScanGridSettings(
            Math.Max(1, recipe.ScanPlan.SizeX),
            Math.Max(1, recipe.ScanPlan.SizeY));
    }

    private static double GetDouble(ExperimentRecipe recipe, string key)
    {
        if (recipe.ScanPlan.Parameters is not null
            && recipe.ScanPlan.Parameters.TryGetValue(key, out var value)
            && double.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return 0;
    }
}
