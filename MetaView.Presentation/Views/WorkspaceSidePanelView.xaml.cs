using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MetaView.Presentation.ViewModels;
using MetaView.Services.Interfaces;

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

    private void LogEntry_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: WorkflowLogEntry entry }
            && DataContext is WorkspaceSidePanelViewModel viewModel)
        {
            viewModel.SelectLogEntry(entry);
            if (entry.IsError && sender is DependencyObject source)
            {
                ToggleInlineDiagnostic(source);
            }
        }

        e.Handled = true;
    }

    private static void ToggleInlineDiagnostic(DependencyObject source)
    {
        var parent = FindAncestor<StackPanel>(source);
        var diagnostic = parent is null ? null : FindDescendant<Border>(parent, "InlineDiagnostic");
        if (diagnostic is null)
        {
            return;
        }

        diagnostic.Visibility = diagnostic.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private static T? FindAncestor<T>(DependencyObject source)
        where T : DependencyObject
    {
        var current = VisualTreeHelper.GetParent(source);
        while (current is not null)
        {
            if (current is T target)
            {
                return target;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static T? FindDescendant<T>(DependencyObject source, string name)
        where T : FrameworkElement
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(source); index++)
        {
            var child = VisualTreeHelper.GetChild(source, index);
            if (child is T element && element.Name == name)
            {
                return element;
            }

            var match = FindDescendant<T>(child, name);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
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
                Child = new TextBlock
                {
                    Text = "任务流程可配置（下一阶段）",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 24,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = TryFindResource("BrushText") as Brush ?? Brushes.White,
                },
            },
        };

        window.Show();
    }
}

