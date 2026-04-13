using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MetaView.Presentation.ViewModels;

namespace MetaView.Presentation.Views;

/// <summary>
/// Displays recipe, task, and log context beside the image workspace.
/// </summary>
public partial class WorkspaceSidePanelView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceSidePanelView" /> class.
    /// </summary>
    public WorkspaceSidePanelView(WorkspaceSidePanelViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void RecipeRow_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.Tag is not string recipeName)
        {
            return;
        }

        RecipeDetailPopup.PlacementTarget = element;
        RecipeDetailPopup.Placement = PlacementMode.Right;
        RecipeDetailPopup.HorizontalOffset = 8;
        RecipeDetailPopup.Tag = recipeName;
        RecipeDetailPopup.IsOpen = true;
        e.Handled = true;
    }

    private void TaskRow_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount < 2)
        {
            return;
        }

        ShowTaskDetails(sender);
        e.Handled = true;
    }

    private void LogAlert_OnClick(object sender, RoutedEventArgs e)
    {
        LogAlertDetails.Visibility = LogAlertDetails.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
        e.Handled = true;
    }

    private void ShowTaskDetails(object sender)
    {
        if (sender is not FrameworkElement element || element.Tag is not string taskName)
        {
            return;
        }

        RecipeDetailPopup.IsOpen = false;

        var owner = Window.GetWindow(this);
        var window = new Window
        {
            Title = taskName,
            Width = 640,
            Height = 420,
            Owner = owner,
            WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
            Background = TryFindResource("BrushWindow") as Brush ?? TryFindResource("BrushBackground") as Brush ?? Brushes.Black,
            Content = new Border
            {
                Background = TryFindResource("BrushPanel") as Brush ?? Brushes.Transparent,
                BorderBrush = TryFindResource("BrushLine") as Brush ?? Brushes.Transparent,
                BorderThickness = new Thickness(1),
            },
        };

        window.Show();
    }
}

