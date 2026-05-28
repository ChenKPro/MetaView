using System.Windows;
using MetaView.Presentation.ViewModels;

namespace MetaView.Presentation.Views;

/// <summary>
/// Displays the menu-driven galvo DAQ scan setup panel.
/// </summary>
public partial class GalvoDaqScanSetupWindow : Window
{
    public GalvoDaqScanSetupWindow(GalvoDaqScanSetupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
