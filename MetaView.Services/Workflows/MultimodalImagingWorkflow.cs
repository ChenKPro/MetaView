using MetaView.Core.Experiments;
using MetaView.Core.Imaging.Signal;
using MetaView.Core.MotionControl;
using MetaView.Services.Interfaces;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Services.Workflows;

/// <summary>
/// Executes recipes that contain multiple imaging modalities.
/// </summary>
public sealed class MultimodalImagingWorkflow(
    IExperimentCapabilityInvoker capabilityInvoker,
    IWorkflowLogPublisher logPublisher)
    : ExperimentWorkflowBase(logPublisher)
{
    /// <inheritdoc />
    public override string WorkflowId => "Multimodal-Imaging";

    /// <inheritdoc />
    public override bool CanRun(ExperimentRecipe recipe)
    {
        return recipe.Modality == ImagingModality.Multimodal
            && recipe.EffectiveModalities.Count > 0;
    }

    /// <inheritdoc />
    protected override IReadOnlyList<WorkflowStep> BuildSteps(ExperimentRecipe recipe)
    {
        var steps = new List<WorkflowStep>
        {
            new("validate.recipe", "Validate multimodal recipe", ValidateRecipeAsync)
        };

        foreach (var modality in recipe.EffectiveModalities)
        {
            steps.Add(new WorkflowStep(
                $"modality.{modality.ModalityId}",
                $"Run {modality.Modality} modality",
                (context, token) => RunModalityAsync(context, modality, token)));
        }

        return steps;
    }

    private static Task<OperationResult> ValidateRecipeAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken)
    {
        var recipe = context.Recipe;
        if (recipe.Modality != ImagingModality.Multimodal)
        {
            return Task.FromResult(OperationResult.Error("Multimodal workflow requires a multimodal recipe."));
        }

        if (recipe.EffectiveModalities.Count == 0)
        {
            return Task.FromResult(OperationResult.Error("Multimodal recipe must contain at least one modality."));
        }

        if (recipe.EffectiveModalities.Any(modality => string.IsNullOrWhiteSpace(modality.ModalityId)))
        {
            return Task.FromResult(OperationResult.Error("Every modality must have a stable modality id."));
        }

        return Task.FromResult(OperationResult.Ok("Multimodal recipe is valid."));
    }

    private async Task<OperationResult> RunModalityAsync(
        WorkflowExecutionContext context,
        ModalityPlan modality,
        CancellationToken cancellationToken)
    {
        var initializeResult = await capabilityInvoker.InitializeAsync(modality.CapabilityPlan, cancellationToken)
            .ConfigureAwait(false);
        if (!initializeResult.Success)
        {
            return initializeResult;
        }

        var runResult = modality.Modality switch
        {
            ImagingModality.Srs => await RunSrsAsync(context, modality, cancellationToken)
                .ConfigureAwait(false),
            ImagingModality.Brightfield => await RunBrightfieldAsync(context, modality, cancellationToken)
                .ConfigureAwait(false),
            ImagingModality.Fluorescence => await RunPhotoDetectionAsync(context, modality, cancellationToken)
                .ConfigureAwait(false),
            ImagingModality.Tpef => await RunPhotoDetectionAsync(context, modality, cancellationToken)
                .ConfigureAwait(false),
            ImagingModality.Cars => await RunSrsAsync(context, modality, cancellationToken)
                .ConfigureAwait(false),
            ImagingModality.Dc => await RunSrsAsync(context, modality, cancellationToken)
                .ConfigureAwait(false),
            _ => OperationResult.Error($"Unsupported modality: {modality.Modality}.")
        };

        if (!runResult.Success)
        {
            return runResult;
        }

        return await capabilityInvoker.StopAsync(modality.CapabilityPlan, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<OperationResult> RunSrsAsync(
        WorkflowExecutionContext context,
        ModalityPlan modality,
        CancellationToken cancellationToken)
    {
        var moveXResult = await capabilityInvoker.MoveRelativeAsync(
                MotionAxis.X,
                GetDouble(modality, DemoExperimentRecipes.XRelativeDistanceKey),
                cancellationToken)
            .ConfigureAwait(false);
        if (!moveXResult.Success)
        {
            return moveXResult;
        }

        var moveYResult = await capabilityInvoker.MoveRelativeAsync(
                MotionAxis.Y,
                GetDouble(modality, DemoExperimentRecipes.YRelativeDistanceKey),
                cancellationToken)
            .ConfigureAwait(false);
        if (!moveYResult.Success)
        {
            return moveYResult;
        }

        var moveZResult = await capabilityInvoker.MoveRelativeAsync(
                MotionAxis.Z,
                GetDouble(modality, DemoExperimentRecipes.ZRelativeDistanceKey),
                cancellationToken)
            .ConfigureAwait(false);
        if (!moveZResult.Success)
        {
            return moveZResult;
        }

        var previewResult = capabilityInvoker.PublishSignalPreview(CreateGridSettings(modality));
        if (!previewResult.Success)
        {
            return previewResult;
        }

        var startResult = await capabilityInvoker.StartDaqAsync(cancellationToken).ConfigureAwait(false);
        if (!startResult.Success)
        {
            return startResult;
        }

        var acquireResult = await capabilityInvoker.AcquireDaqAsync(
                TimeSpan.FromMilliseconds(GetDouble(modality, DemoExperimentRecipes.DaqAcquireMillisecondsKey)),
                CreateGridSettings(modality),
                publishDemoFrame: true,
                cancellationToken)
            .ConfigureAwait(false);
        if (!acquireResult.Success)
        {
            return acquireResult;
        }

        context.AddDataProduct(new ExperimentDataProduct(
            DataProductKind.Image,
            $"{modality.ModalityId} signal image",
            $"{modality.Modality} signal image generated from DAQ channels."));
        context.AddDataProduct(new ExperimentDataProduct(
            DataProductKind.Curve,
            $"{modality.ModalityId} signal trace",
            $"{modality.Modality} four-channel signal trace."));
        return OperationResult.Ok($"{modality.Modality} modality completed.");
    }

    private async Task<OperationResult> RunBrightfieldAsync(
        WorkflowExecutionContext context,
        ModalityPlan modality,
        CancellationToken cancellationToken)
    {
        var liveResult = await capabilityInvoker.StartBrightfieldLiveAsync(cancellationToken).ConfigureAwait(false);
        if (!liveResult.Success)
        {
            return liveResult;
        }

        var captureResult = await capabilityInvoker.CaptureBrightfieldAsync(cancellationToken).ConfigureAwait(false);
        if (!captureResult.Success)
        {
            return OperationResult.Error(captureResult.Message);
        }

        context.AddDataProduct(new ExperimentDataProduct(
            DataProductKind.Image,
            $"{modality.ModalityId} brightfield image",
            "Brightfield camera frame captured during multimodal workflow."));
        return OperationResult.Ok("Brightfield modality completed.");
    }

    private Task<OperationResult> RunPhotoDetectionAsync(
        WorkflowExecutionContext context,
        ModalityPlan modality,
        CancellationToken cancellationToken)
    {
        context.AddDataProduct(new ExperimentDataProduct(
            DataProductKind.Curve,
            $"{modality.ModalityId} detector signal",
            $"{modality.Modality} detector capability initialized for acquisition."));
        return Task.FromResult(OperationResult.Ok($"{modality.Modality} modality completed."));
    }

    private static ScanGridSettings CreateGridSettings(ModalityPlan modality)
    {
        return new ScanGridSettings(
            Math.Max(1, modality.ScanPlan.SizeX),
            Math.Max(1, modality.ScanPlan.SizeY));
    }

    private static double GetDouble(ModalityPlan modality, string key)
    {
        if (modality.ScanPlan.Parameters is not null
            && modality.ScanPlan.Parameters.TryGetValue(key, out var value)
            && double.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return 0;
    }
}
