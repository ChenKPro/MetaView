using System.Windows;
using MetaView.Presentation.ViewModels;

namespace MetaView.Presentation;

/// <summary>
/// Provides the main instrument workstation window.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow" /> class.
    /// </summary>
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

