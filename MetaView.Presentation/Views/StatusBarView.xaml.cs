using System.Windows.Controls;
using MetaView.Presentation.ViewModels;

namespace MetaView.Presentation.Views;

/// <summary>
/// Displays runtime state and progress messages.
/// </summary>
public partial class StatusBarView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusBarView" /> class.
    /// </summary>
    public StatusBarView(StatusBarViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
