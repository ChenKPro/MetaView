using MetaView.Capabilities.Algorithms;
using MetaView.Capabilities.LaserControl;
using MetaView.Capabilities.PhotoDetection;
using MetaView.Capabilities.Reporting;
using MetaView.Capability.DaqAndPreprocessing;
using MetaView.Capability.ImageAcquisition;
using MetaView.Capability.MotionControl;
using MetaView.Capability.ParameterManagement.Providers;
using MetaView.Core.Algorithms;
using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.Imaging.Signal;
using MetaView.Core.Laser;
using MetaView.Core.MotionControl;
using MetaView.Core.Parameters;
using MetaView.Core.PhotoDetection;
using MetaView.Core.Reporting;
using MetaView.Presentation;
using MetaView.Presentation.Services;
using MetaView.Presentation.ViewModels;
using MetaView.Services;
using MetaView.Services.Interfaces;
using MetaView.Services.Workflows;
using Prism.Ioc;
using System.IO;
using Vibronix.Presentation.Wpf.Plot.Services;

namespace MetaView.Composition;

/// <summary>
/// Registers MetaView modules in the Prism container.
/// </summary>
internal static class MetaViewContainerRegistration
{
    /// <summary>
    /// Registers runtime parameters, services, capabilities, and presentation models.
    /// </summary>
    public static void RegisterMetaViewModules(this IContainerRegistry containerRegistry)
    {
        var parameterProvider = new JsonRuntimeParameterProvider(Path.Combine("config", "metaview.devices.json"));
        containerRegistry.RegisterInstance<IRuntimeParameterProvider>(parameterProvider);
        containerRegistry.RegisterCapabilities(parameterProvider);
        containerRegistry.RegisterApplicationServices();
        containerRegistry.RegisterPresentationServices();
    }

    private static void RegisterCapabilities(
        this IContainerRegistry containerRegistry,
        IRuntimeParameterProvider parameterProvider)
    {
        containerRegistry.RegisterSingleton<IBrightfieldCameraCapability, FoundationBrightfieldCameraCapability>();
        containerRegistry.RegisterSingleton<IDataAcquisitionCapability, FoundationDataAcquisitionCapability>();
        containerRegistry.RegisterSingleton<IAlgorithmProcessingCapability, AlgorithmProcessingCapability>();
        containerRegistry.RegisterSingleton<ISignalImagingProcessor, SignalImagingProcessor>();
        containerRegistry.RegisterSingleton<ILaserControlCapability, LaserControlCapability>();
        containerRegistry.RegisterSingleton<IPhotoDetectionCapability, PhotoDetectionCapability>();
        containerRegistry.RegisterSingleton<IReportGenerationCapability, ReportGenerationCapability>();
        containerRegistry.RegisterMotionControl(parameterProvider);
    }

    private static void RegisterMotionControl(
        this IContainerRegistry containerRegistry,
        IRuntimeParameterProvider parameterProvider)
    {
        var configuration = parameterProvider.GetMotionSystemConfiguration().Data ?? new MotionSystemConfiguration();
        if (configuration.UseDemo)
        {
            containerRegistry.RegisterSingleton<IMotionControlCapability, DemoMotionControlCapability>();
            return;
        }

        containerRegistry.RegisterInstance(configuration);
        containerRegistry.RegisterSingleton<IMotionControlCapability, CompositeMotionControlCapability>();
    }

    private static void RegisterApplicationServices(this IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IRealtimeSignalImagingService, RealtimeSignalImagingService>();
        containerRegistry.RegisterSingleton<IWorkflowLogPublisher, WorkflowLogPublisher>();
        containerRegistry.RegisterSingleton<IExperimentPreflightValidator, ExperimentPreflightValidator>();
        containerRegistry.RegisterSingleton<IExperimentCapabilityInvoker, ExperimentCapabilityInvoker>();
        containerRegistry.Register<IExperimentWorkflow, SrsTwoDDemoWorkflow>();
        containerRegistry.Register<IExperimentWorkflow, MultimodalImagingWorkflow>();
        containerRegistry.Register<IExperimentWorkflowRunner, ExperimentWorkflowRunner>();
    }

    private static void RegisterPresentationServices(this IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IAcquisitionService, MockAcquisitionService>();
        containerRegistry.RegisterSingleton<ILaserService, MockLaserService>();
        containerRegistry.RegisterSingleton<IPlotHostRegistry, PlotHostRegistry>();
        containerRegistry.RegisterSingleton<IPlotServiceFactory, PlotServiceFactory>();
        containerRegistry.RegisterSingleton<IPresentationThemeManager, PresentationThemeManager>();
        containerRegistry.RegisterSingleton<ScanSetupViewModel>();
        containerRegistry.RegisterSingleton<AcquisitionWorkflowViewModel>();
        containerRegistry.RegisterSingleton<HardwarePanelViewModel>();
        containerRegistry.RegisterSingleton<ImageWorkspaceViewModel>();
        containerRegistry.RegisterSingleton<StatusBarViewModel>();
        containerRegistry.RegisterSingleton<WorkspaceSidePanelViewModel>();
        containerRegistry.RegisterSingleton<ShellViewModel>();
        containerRegistry.Register<MetaView.Presentation.MainWindow>();
    }
}
