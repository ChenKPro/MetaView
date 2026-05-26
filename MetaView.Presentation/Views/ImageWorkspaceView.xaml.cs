using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ImageViewer2D.Controls.Models;
using MetaView.Presentation.ViewModels;

namespace MetaView.Presentation.Views;

/// <summary>
/// Displays the image viewport, ROI overlay, and analysis panels.
/// </summary>
public partial class ImageWorkspaceView : UserControl
{
    private static readonly TimeSpan StageNavigationTimeout = TimeSpan.FromSeconds(3);
    private ImageViewerMouseEventArgs? _lastDragArgs;
    private bool _isStageDragging;
    private readonly DispatcherTimer _stageNavigationTimer;

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
        _stageNavigationTimer = new DispatcherTimer { Interval = StageNavigationTimeout };
        _stageNavigationTimer.Tick += OnStageNavigationTimerTick;
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

    private void OnImageMouseDown(object sender, ImageViewerMouseButtonEventArgs e)
    {
        UpdateMousePosition(e);
        if (e.ChangedButton == MouseButton.Middle)
        {
            ToggleStageNavigationMode();
            return;
        }

        if (e.ToolMode != RoiToolMode.StageNavigation || e.ChangedButton != MouseButton.Left || e.ClickCount > 1)
        {
            return;
        }

        KeepStageNavigationAlive();
        _isStageDragging = true;
        _lastDragArgs = e;
    }

    private async void OnImageMouseMove(object sender, ImageViewerMouseEventArgs e)
    {
        UpdateMousePosition(e);
        if (e.ToolMode != RoiToolMode.StageNavigation || !_isStageDragging || Mouse.LeftButton != MouseButtonState.Pressed || _lastDragArgs is null)
        {
            return;
        }

        KeepStageNavigationAlive();
        if (DataContext is ImageWorkspaceViewModel viewModel)
        {
            await viewModel.MoveStageByImageDragAsync(_lastDragArgs, e).ConfigureAwait(true);
        }

        _lastDragArgs = e;
    }

    private void OnImageMouseUp(object sender, ImageViewerMouseButtonEventArgs e)
    {
        UpdateMousePosition(e);
        if (e.ToolMode != RoiToolMode.StageNavigation || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        KeepStageNavigationAlive();
        _isStageDragging = false;
        _lastDragArgs = null;
    }

    private async void OnImageMouseDoubleClick(object sender, ImageViewerMouseButtonEventArgs e)
    {
        UpdateMousePosition(e);
        if (e.ToolMode != RoiToolMode.StageNavigation || e.ChangedButton != MouseButton.Left || DataContext is not ImageWorkspaceViewModel viewModel)
        {
            return;
        }

        KeepStageNavigationAlive();
        _isStageDragging = false;
        _lastDragArgs = null;
        await viewModel.MoveStageToImageCenterAsync(e).ConfigureAwait(true);
    }

    private async void OnImageMouseWheel(object sender, ImageViewerMouseWheelEventArgs e)
    {
        UpdateMousePosition(e);
        if (e.ToolMode == RoiToolMode.StageNavigation && DataContext is ImageWorkspaceViewModel viewModel)
        {
            KeepStageNavigationAlive();
            await viewModel.MoveStageByMouseWheelAsync(e).ConfigureAwait(true);
        }
    }

    private void ToggleStageNavigationMode()
    {
        if (ImageViewer.ToolMode == RoiToolMode.StageNavigation)
        {
            ExitStageNavigationMode();
            return;
        }

        EnterStageNavigationMode();
    }

    private void EnterStageNavigationMode()
    {
        ImageViewer.ToolMode = RoiToolMode.StageNavigation;
        KeepStageNavigationAlive();
    }

    private void ExitStageNavigationMode()
    {
        _stageNavigationTimer.Stop();
        _isStageDragging = false;
        _lastDragArgs = null;
        ImageViewer.ToolMode = RoiToolMode.Pan;
    }

    private void KeepStageNavigationAlive()
    {
        _stageNavigationTimer.Stop();
        _stageNavigationTimer.Start();
    }

    private void OnStageNavigationTimerTick(object? sender, EventArgs e)
    {
        _stageNavigationTimer.Stop();
        if (ImageViewer.ToolMode == RoiToolMode.StageNavigation)
        {
            ExitStageNavigationMode();
        }
    }

    private void UpdateMousePosition(ImageViewerMouseEventArgs e)
    {
        if (DataContext is ImageWorkspaceViewModel viewModel)
        {
            viewModel.UpdateMousePosition(e);
        }
    }
}
