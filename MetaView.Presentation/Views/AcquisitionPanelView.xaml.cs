using System.Windows.Controls;
using MetaView.Presentation.ViewModels;

namespace MetaView.Presentation.Views;

/// <summary>
/// Displays acquisition setup, workflow, save settings, and run controls.
/// </summary>
public partial class AcquisitionPanelView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AcquisitionPanelView" /> class.
    /// </summary>
    public AcquisitionPanelView(AcquisitionWorkflowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
