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
    private readonly DispatcherTimer _temporaryStageNavigationTimer;
    private ImageViewerMouseEventArgs? _lastDragArgs;
    private bool _isStageDragging;
    private bool _isTemporaryStageNavigation;
    private RoiToolMode _previousToolMode = RoiToolMode.Pan;

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
        _temporaryStageNavigationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _temporaryStageNavigationTimer.Tick += OnTemporaryStageNavigationTimeout;
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
        if (DataContext is ImageWorkspaceViewModel viewModel)
        {
            viewModel.UpdateMousePosition(e);
        }

        if (e.ChangedButton == MouseButton.Middle)
        {
            ToggleTemporaryStageNavigation();
            return;
        }

        if (!IsStageNavigationActive(e.ToolMode) || e.ChangedButton != MouseButton.Left || e.ClickCount > 1)
        {
            return;
        }

        TouchTemporaryStageNavigation();
        _isStageDragging = true;
        _lastDragArgs = e;
    }

    private async void OnImageMouseMove(object sender, ImageViewerMouseEventArgs e)
    {
        if (DataContext is ImageWorkspaceViewModel viewModel)
        {
            viewModel.UpdateMousePosition(e);
        }

        if (!IsStageNavigationActive(e.ToolMode) || !_isStageDragging || Mouse.LeftButton != MouseButtonState.Pressed || _lastDragArgs is null)
        {
            return;
        }

        TouchTemporaryStageNavigation();
        if (DataContext is ImageWorkspaceViewModel stageViewModel)
        {
            await stageViewModel.StartStageImageDragJogAsync(_lastDragArgs, e).ConfigureAwait(true);
        }

        _lastDragArgs = e;
    }

    private async void OnImageMouseUp(object sender, ImageViewerMouseButtonEventArgs e)
    {
        if (!IsStageNavigationActive(e.ToolMode) || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        TouchTemporaryStageNavigation();
        _isStageDragging = false;
        _lastDragArgs = null;
        if (DataContext is ImageWorkspaceViewModel viewModel)
        {
            await viewModel.StopStageImageJogAsync().ConfigureAwait(true);
        }
    }

    private async void OnImageMouseDoubleClick(object sender, ImageViewerMouseButtonEventArgs e)
    {
        if (DataContext is ImageWorkspaceViewModel mouseViewModel)
        {
            mouseViewModel.UpdateMousePosition(e);
        }

        if (!IsStageNavigationActive(e.ToolMode) || e.ChangedButton != MouseButton.Left || DataContext is not ImageWorkspaceViewModel viewModel)
        {
            return;
        }

        TouchTemporaryStageNavigation();
        _isStageDragging = false;
        _lastDragArgs = null;
        await viewModel.StopStageImageJogAsync().ConfigureAwait(true);
        await viewModel.MoveStageToImageCenterAsync(e).ConfigureAwait(true);
    }

    private async void OnImageMouseWheel(object sender, ImageViewerMouseWheelEventArgs e)
    {
        if (DataContext is ImageWorkspaceViewModel mouseViewModel)
        {
            mouseViewModel.UpdateMousePosition(e);
        }

        if (IsStageNavigationActive(e.ToolMode) && DataContext is ImageWorkspaceViewModel viewModel)
        {
            TouchTemporaryStageNavigation();
            await viewModel.MoveStageByMouseWheelAsync(e).ConfigureAwait(true);
        }
    }

    private bool IsStageNavigationActive(RoiToolMode toolMode)
    {
        return _isTemporaryStageNavigation || toolMode == RoiToolMode.StageNavigation;
    }

    private void ToggleTemporaryStageNavigation()
    {
        if (_isTemporaryStageNavigation)
        {
            _ = DeactivateTemporaryStageNavigationAsync();
            return;
        }

        _previousToolMode = ImageViewer.ToolMode == RoiToolMode.StageNavigation ? RoiToolMode.Pan : ImageViewer.ToolMode;
        _isTemporaryStageNavigation = true;
        ImageViewer.ToolMode = RoiToolMode.StageNavigation;
        if (DataContext is ImageWorkspaceViewModel viewModel)
        {
            viewModel.SetStageInteractionMode(isStageLinked: true);
        }

        TouchTemporaryStageNavigation();
    }

    private void TouchTemporaryStageNavigation()
    {
        if (!_isTemporaryStageNavigation)
        {
            return;
        }

        _temporaryStageNavigationTimer.Stop();
        _temporaryStageNavigationTimer.Start();
    }

    private async void OnTemporaryStageNavigationTimeout(object? sender, EventArgs e)
    {
        await DeactivateTemporaryStageNavigationAsync().ConfigureAwait(true);
    }

    private async Task DeactivateTemporaryStageNavigationAsync()
    {
        if (!_isTemporaryStageNavigation)
        {
            return;
        }

        _temporaryStageNavigationTimer.Stop();
        _isTemporaryStageNavigation = false;
        _isStageDragging = false;
        _lastDragArgs = null;
        if (DataContext is ImageWorkspaceViewModel viewModel)
        {
            await viewModel.StopStageImageJogAsync().ConfigureAwait(true);
            viewModel.SetStageInteractionMode(isStageLinked: false);
        }

        ImageViewer.ToolMode = _previousToolMode;
    }
}
