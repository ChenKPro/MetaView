using System.Windows.Controls;
using MetaView.Presentation.ViewModels;

namespace MetaView.Presentation.Views;

/// <summary>
/// Displays sample stage and hardware controls.
/// </summary>
public partial class HardwarePanelView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HardwarePanelView" /> class.
    /// </summary>
    public HardwarePanelView(HardwarePanelViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
