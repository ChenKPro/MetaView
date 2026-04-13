using MetaView.Core.Experiments;
using MetaView.Core.Imaging.Signal;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Creates built-in demo recipes used by the shell.
/// </summary>
public static class DemoExperimentRecipes
{
    public const string XRelativeDistanceKey = "XRelativeDistance";
    public const string YRelativeDistanceKey = "YRelativeDistance";
    public const string ZRelativeDistanceKey = "ZRelativeDistance";
    public const string DaqAcquireMillisecondsKey = "DaqAcquireMilliseconds";

    /// <summary>
    /// Creates a smoke-test SRS 2D recipe.
    /// </summary>
    public static ExperimentRecipe CreateSrsTwoD(
        double xRelativeDistance,
        double yRelativeDistance,
        double zRelativeDistance,
        TimeSpan daqAcquireDuration)
    {
        return new ExperimentRecipe(
            "demo.srs.2d",
            AcquisitionDimension.TwoD,
            ImagingModality.Srs,
            new ScanPlan(
                ScanPattern.Raster,
                SizeX: ScanGridSettings.Default.Width,
                SizeY: ScanGridSettings.Default.Height,
                Parameters: new Dictionary<string, string>
                {
                    [XRelativeDistanceKey] = xRelativeDistance.ToString("R"),
                    [YRelativeDistanceKey] = yRelativeDistance.ToString("R"),
                    [ZRelativeDistanceKey] = zRelativeDistance.ToString("R"),
                    [DaqAcquireMillisecondsKey] = daqAcquireDuration.TotalMilliseconds.ToString("R")
                }),
            new ProcessingPlan(
                ProcessingMode.SignalImage,
                ["AI0", "AI1", "AI2", "AI3"],
                new Dictionary<string, string>
                {
                    ["PositionXChannel"] = "AI0",
                    ["PositionYChannel"] = "AI1",
                    ["SignalChannels"] = "AI2,AI3"
                }),
            new SavePlan(
                AutoSave: false,
                Directory: string.Empty,
                Name: "Demo SRS 2D",
                [DataProductKind.Image, DataProductKind.Curve]),
            new Dictionary<string, string>
            {
                ["Purpose"] = "Five-layer platform workflow smoke test"
            },
            CapabilityPlan: SignalImagingCapabilities());
    }

    /// <summary>
    /// Creates a smoke-test brightfield 2D recipe.
    /// </summary>
    public static ExperimentRecipe CreateBrightfieldTwoD()
    {
        return new ExperimentRecipe(
            "demo.brightfield.2d",
            AcquisitionDimension.TwoD,
            ImagingModality.Brightfield,
            new ScanPlan(ScanPattern.Raster, SizeX: 1, SizeY: 1),
            new ProcessingPlan(ProcessingMode.Raw, ["Camera"]),
            new SavePlan(
                AutoSave: false,
                Directory: string.Empty,
                Name: "Demo Brightfield 2D",
                [DataProductKind.Image]),
            new Dictionary<string, string>
            {
                ["Purpose"] = "Brightfield capability workflow smoke test"
            },
            CapabilityPlan: BrightfieldCapabilities());
    }

    /// <summary>
    /// Creates a multimodal SRS plus brightfield 2D recipe.
    /// </summary>
    public static ExperimentRecipe CreateSrsBrightfieldTwoD(
        double xRelativeDistance,
        double yRelativeDistance,
        double zRelativeDistance,
        TimeSpan daqAcquireDuration)
    {
        var srsScanPlan = new ScanPlan(
            ScanPattern.Raster,
            SizeX: ScanGridSettings.Default.Width,
            SizeY: ScanGridSettings.Default.Height,
            Parameters: new Dictionary<string, string>
            {
                [XRelativeDistanceKey] = xRelativeDistance.ToString("R"),
                [YRelativeDistanceKey] = yRelativeDistance.ToString("R"),
                [ZRelativeDistanceKey] = zRelativeDistance.ToString("R"),
                [DaqAcquireMillisecondsKey] = daqAcquireDuration.TotalMilliseconds.ToString("R")
            });

        var srsProcessingPlan = new ProcessingPlan(
            ProcessingMode.SignalImage,
            ["AI0", "AI1", "AI2", "AI3"],
            new Dictionary<string, string>
            {
                ["PositionXChannel"] = "AI0",
                ["PositionYChannel"] = "AI1",
                ["SignalChannels"] = "AI2,AI3"
            });

        var brightfieldScanPlan = new ScanPlan(ScanPattern.Raster, SizeX: 1, SizeY: 1);
        var brightfieldProcessingPlan = new ProcessingPlan(ProcessingMode.Raw, ["Camera"]);

        return new ExperimentRecipe(
            "demo.srs-brightfield.2d",
            AcquisitionDimension.TwoD,
            ImagingModality.Multimodal,
            srsScanPlan,
            srsProcessingPlan,
            new SavePlan(
                AutoSave: false,
                Directory: string.Empty,
                Name: "Demo SRS + Brightfield 2D",
                [DataProductKind.Image, DataProductKind.Curve]),
            new Dictionary<string, string>
            {
                ["Purpose"] = "Multimodal platform workflow smoke test"
            },
            [
                new ModalityPlan(
                    "srs",
                    ImagingModality.Srs,
                    AcquisitionDimension.TwoD,
                    srsScanPlan,
                    srsProcessingPlan,
                    SignalImagingCapabilities()),
                new ModalityPlan(
                    "brightfield",
                    ImagingModality.Brightfield,
                    AcquisitionDimension.TwoD,
                    brightfieldScanPlan,
                    brightfieldProcessingPlan,
                    BrightfieldCapabilities())
            ],
            MultimodalCapabilities());
    }

    private static CapabilityPlan SignalImagingCapabilities()
    {
        return new CapabilityPlan(
            [
                ExperimentCapability.Motion,
                ExperimentCapability.DataAcquisition,
                ExperimentCapability.SignalImaging,
                ExperimentCapability.Algorithm
            ]);
    }

    private static CapabilityPlan BrightfieldCapabilities()
    {
        return new CapabilityPlan(
            [
                ExperimentCapability.BrightfieldCamera
            ]);
    }

    private static CapabilityPlan MultimodalCapabilities()
    {
        return new CapabilityPlan(
            [
                ExperimentCapability.Motion,
                ExperimentCapability.DataAcquisition,
                ExperimentCapability.SignalImaging,
                ExperimentCapability.Algorithm,
                ExperimentCapability.BrightfieldCamera
            ]);
    }
}
