using System.Windows;
using MetaView.Composition;
using MetaView.Presentation;
using MetaView.Presentation.Core;
using MetaView.Presentation.Services;
using MetaView.Presentation.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Navigation.Regions;

namespace MetaView
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            Container.Resolve<IPresentationThemeManager>().ApplyTheme(PresentationThemeNames.Default);
            return Container.Resolve<MainWindow>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            RegisterShellRegions(Container.Resolve<IRegionManager>());
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterMetaViewModules();
        }

        private static void RegisterShellRegions(IRegionManager regionManager)
        {
            regionManager.RegisterViewWithRegion(RegionNames.TopBarRegion, typeof(TopBarView));
            regionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(WorkspaceSidePanelView));
            regionManager.RegisterViewWithRegion(RegionNames.WorkspaceRegion, typeof(ImageWorkspaceView));
            regionManager.RegisterViewWithRegion(RegionNames.HardwareRegion, typeof(HardwarePanelView));
            regionManager.RegisterViewWithRegion(RegionNames.AcquisitionRegion, typeof(AcquisitionPanelView));
            regionManager.RegisterViewWithRegion(RegionNames.StatusRegion, typeof(StatusBarView));
        }
    }
}
