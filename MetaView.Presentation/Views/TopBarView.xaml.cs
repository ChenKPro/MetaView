using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MetaView.Presentation.Views;

/// <summary>
/// Displays the main menu, task summary, and instrument readiness state.
/// </summary>
public partial class TopBarView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TopBarView" /> class.
    /// </summary>
    public TopBarView()
    {
        InitializeComponent();
    }

    private void DragSurface_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsInteractiveElement(e.OriginalSource as DependencyObject))
        {
            return;
        }

        var window = Window.GetWindow(this);
        if (window is null)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleWindowState(window);
            return;
        }

        window.DragMove();
    }

    private void Minimize_OnClick(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window is not null)
        {
            window.WindowState = WindowState.Minimized;
        }
    }

    private void MaximizeRestore_OnClick(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window is not null)
        {
            ToggleWindowState(window);
        }
    }

    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }

    private static void ToggleWindowState(Window window)
    {
        window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private static bool IsInteractiveElement(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ButtonBase or MenuItem or TextBox or ComboBox or Popup)
            {
                return true;
            }

            if (source is FrameworkContentElement contentElement)
            {
                source = contentElement.Parent;
                continue;
            }

            if (source is Visual or System.Windows.Media.Media3D.Visual3D)
            {
                source = VisualTreeHelper.GetParent(source);
                continue;
            }

            source = null;
        }

        return false;
    }
}

