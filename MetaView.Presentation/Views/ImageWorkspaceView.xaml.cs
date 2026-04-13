using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using MetaView.Presentation.ViewModels;

namespace MetaView.Presentation.Views;

/// <summary>
/// Displays the image viewport, ROI overlay, and analysis panels.
/// </summary>
public partial class ImageWorkspaceView : UserControl
{
    /// <summary>
    /// Identifies the <see cref="IsImageToolBarOpen" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsImageToolBarOpenProperty =
        DependencyProperty.Register(
            nameof(IsImageToolBarOpen),
            typeof(bool),
            typeof(ImageWorkspaceView),
            new PropertyMetadata(false));

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageWorkspaceView" /> class.
    /// </summary>
    public ImageWorkspaceView(ImageWorkspaceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the ImageViewer2D left toolbar is open.
    /// </summary>
    public bool IsImageToolBarOpen
    {
        get => (bool)GetValue(IsImageToolBarOpenProperty);
        set => SetValue(IsImageToolBarOpenProperty, value);
    }

    private void ToggleImageToolBar(object sender, MouseButtonEventArgs e)
    {
        IsImageToolBarOpen = !IsImageToolBarOpen;
        e.Handled = true;
    }
}
