using ImageViewer2D.Controls.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageViewer2D.Controls;

/// <summary>
/// Displays a 2D image with zoom, pan, navigation, status, and simple ROI tools.
/// </summary>
[TemplatePart(Name = SurfacePartName, Type = typeof(Canvas))]
[TemplatePart(Name = ImagePartName, Type = typeof(Image))]
[TemplatePart(Name = RoiCanvasPartName, Type = typeof(Canvas))]
[TemplatePart(Name = CrosshairCanvasPartName, Type = typeof(Canvas))]
[TemplatePart(Name = NavigatorPartName, Type = typeof(Border))]
[TemplatePart(Name = NavigatorCanvasPartName, Type = typeof(Canvas))]
[TemplatePart(Name = NavigatorImagePartName, Type = typeof(Image))]
[TemplatePart(Name = NavigatorViewportPartName, Type = typeof(Rectangle))]
[TemplatePart(Name = StatusTextPartName, Type = typeof(TextBlock))]
[TemplatePart(Name = ToolBarPartName, Type = typeof(Border))]
[TemplatePart(Name = PanToolPartName, Type = typeof(ToggleButton))]
[TemplatePart(Name = SelectToolPartName, Type = typeof(ToggleButton))]
[TemplatePart(Name = RectangleToolPartName, Type = typeof(ToggleButton))]
[TemplatePart(Name = EllipseToolPartName, Type = typeof(ToggleButton))]
[TemplatePart(Name = CrosshairToolPartName, Type = typeof(ToggleButton))]
[TemplatePart(Name = ClearRoisToolPartName, Type = typeof(ButtonBase))]
public sealed class ImageViewer2D : Control
{
    private const string SurfacePartName = "PART_Surface";
    private const string ImagePartName = "PART_Image";
    private const string RoiCanvasPartName = "PART_RoiCanvas";
    private const string CrosshairCanvasPartName = "PART_CrosshairCanvas";
    private const string NavigatorPartName = "PART_Navigator";
    private const string NavigatorCanvasPartName = "PART_NavigatorCanvas";
    private const string NavigatorImagePartName = "PART_NavigatorImage";
    private const string NavigatorViewportPartName = "PART_NavigatorViewport";
    private const string StatusTextPartName = "PART_StatusText";
    private const string ToolBarPartName = "PART_ToolBar";
    private const string PanToolPartName = "PART_PanTool";
    private const string SelectToolPartName = "PART_SelectTool";
    private const string RectangleToolPartName = "PART_RectangleTool";
    private const string EllipseToolPartName = "PART_EllipseTool";
    private const string CrosshairToolPartName = "PART_CrosshairTool";
    private const string ClearRoisToolPartName = "PART_ClearRoisTool";
    private const double RoiEdgeHitTolerance = 6.0;
    private const double CrosshairHitTolerance = 6.0;

    private readonly ImageViewport _viewport = new();
    private Canvas? _surface;
    private Image? _image;
    private Canvas? _roiCanvas;
    private Canvas? _crosshairCanvas;
    private Border? _navigator;
    private Canvas? _navigatorCanvas;
    private Image? _navigatorImage;
    private Rectangle? _navigatorViewport;
    private Border? _toolBar;
    private ToggleButton? _panTool;
    private ToggleButton? _selectTool;
    private ToggleButton? _rectangleTool;
    private ToggleButton? _ellipseTool;
    private ToggleButton? _crosshairTool;
    private ButtonBase? _clearRoisTool;
    //private TextBlock? _statusText;
    private Point _lastMousePoint;
    private bool _isPanning;
    private bool _isNavigatorDragging;
    private RoiShape? _activeRoi;
    private RoiShape? _editingRoi;
    private RoiEditOperation _roiEditOperation = RoiEditOperation.None;
    private ResizeHandle _resizeHandle = ResizeHandle.None;
    private Rect _editStartBounds;
    private Point _editStartImagePoint;
    private CrosshairEditOperation _crosshairEditOperation = CrosshairEditOperation.None;
    private double _navigatorScale;
    private double _navigatorImageLeft;
    private double _navigatorImageTop;

    /// <summary>
    /// Identifies the <see cref="Source" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(ImageViewer2D),
            new PropertyMetadata(null, OnSourceChanged));

    /// <summary>
    /// Identifies the <see cref="ToolMode" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty ToolModeProperty =
        DependencyProperty.Register(
            nameof(ToolMode),
            typeof(RoiToolMode),
            typeof(ImageViewer2D),
            new PropertyMetadata(RoiToolMode.Pan, OnToolModeChanged));

    /// <summary>
    /// Identifies the <see cref="ShowToolBar" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowToolBarProperty =
        DependencyProperty.Register(
            nameof(ShowToolBar),
            typeof(bool),
            typeof(ImageViewer2D),
            new PropertyMetadata(false, OnShowToolBarChanged));

    /// <summary>
    /// Identifies the <see cref="ShowNavigator" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowNavigatorProperty =
        DependencyProperty.Register(
            nameof(ShowNavigator),
            typeof(bool),
            typeof(ImageViewer2D),
            new PropertyMetadata(true, OnShowNavigatorChanged));

    /// <summary>
    /// Identifies the <see cref="RoiStroke" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty RoiStrokeProperty =
        DependencyProperty.Register(
            nameof(RoiStroke),
            typeof(Brush),
            typeof(ImageViewer2D),
            new PropertyMetadata(Brushes.LimeGreen));

    /// <summary>
    /// Identifies the <see cref="RoiStrokeThickness" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty RoiStrokeThicknessProperty =
        DependencyProperty.Register(
            nameof(RoiStrokeThickness),
            typeof(double),
            typeof(ImageViewer2D),
            new PropertyMetadata(2.0));

    /// <summary>
    /// Identifies the <see cref="Status" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(ImageViewerStatus),
            typeof(ImageViewer2D),
            new PropertyMetadata(new ImageViewerStatus()));

    /// <summary>
    /// Identifies the <see cref="CrosshairPosition" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty CrosshairPositionProperty =
        DependencyProperty.Register(
            nameof(CrosshairPosition),
            typeof(Point?),
            typeof(ImageViewer2D),
            new PropertyMetadata(null, OnCrosshairPositionChanged));

    /// <summary>
    /// Identifies the <see cref="HasCrosshair" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty HasCrosshairProperty =
        DependencyProperty.Register(
            nameof(HasCrosshair),
            typeof(bool),
            typeof(ImageViewer2D),
            new PropertyMetadata(false, OnCrosshairPositionChanged));

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewer2D" /> class.
    /// </summary>
    public ImageViewer2D()
    {
        Rois.CollectionChanged += OnRoisChanged;
    }

    static ImageViewer2D()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewer2D), new FrameworkPropertyMetadata(typeof(ImageViewer2D)));
    }

    /// <summary>
    /// Occurs when the viewport transform changes.
    /// </summary>
    public event EventHandler? ViewportChanged;

    /// <summary>
    /// Occurs when the ROI collection changes.
    /// </summary>
    public event EventHandler? RoiChanged;

    /// <summary>
    /// Occurs after a local image is loaded.
    /// </summary>
    public event EventHandler? ImageLoaded;

    /// <summary>
    /// Occurs when the mouse moves over the image display area.
    /// </summary>
    public event EventHandler<ImageViewerMouseEventArgs>? ImageMouseMove;

    /// <summary>
    /// Occurs when a mouse button is pressed over the image display area.
    /// </summary>
    public event EventHandler<ImageViewerMouseButtonEventArgs>? ImageMouseDown;

    /// <summary>
    /// Occurs when a mouse button is released over the image display area.
    /// </summary>
    public event EventHandler<ImageViewerMouseButtonEventArgs>? ImageMouseUp;

    /// <summary>
    /// Occurs when a mouse button is double-clicked over the image display area.
    /// </summary>
    public event EventHandler<ImageViewerMouseButtonEventArgs>? ImageMouseDoubleClick;

    /// <summary>
    /// Occurs when the mouse wheel changes over the image display area.
    /// </summary>
    public event EventHandler<ImageViewerMouseWheelEventArgs>? ImageMouseWheel;

    /// <summary>
    /// Gets or sets the image source displayed by the viewer.
    /// </summary>
    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the active interaction mode.
    /// </summary>
    public RoiToolMode ToolMode
    {
        get => (RoiToolMode)GetValue(ToolModeProperty);
        set => SetValue(ToolModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the built-in image tool bar is visible.
    /// </summary>
    public bool ShowToolBar
    {
        get => (bool)GetValue(ShowToolBarProperty);
        set => SetValue(ShowToolBarProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the navigation view is visible.
    /// </summary>
    public bool ShowNavigator
    {
        get => (bool)GetValue(ShowNavigatorProperty);
        set => SetValue(ShowNavigatorProperty, value);
    }

    /// <summary>
    /// Gets or sets the default stroke used for new ROI shapes.
    /// </summary>
    public Brush RoiStroke
    {
        get => (Brush)GetValue(RoiStrokeProperty);
        set => SetValue(RoiStrokeProperty, value);
    }

    /// <summary>
    /// Gets or sets the default stroke thickness used for new ROI shapes.
    /// </summary>
    public double RoiStrokeThickness
    {
        get => (double)GetValue(RoiStrokeThicknessProperty);
        set => SetValue(RoiStrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets the current status for display or event output.
    /// </summary>
    public ImageViewerStatus Status
    {
        get => (ImageViewerStatus)GetValue(StatusProperty);
        private set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Gets or sets the red vertical and blue horizontal crosshair intersection in image pixel coordinates.
    /// </summary>
    public Point? CrosshairPosition
    {
        get => (Point?)GetValue(CrosshairPositionProperty);
        set => SetValue(CrosshairPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the crosshair is visible.
    /// </summary>
    public bool HasCrosshair
    {
        get => (bool)GetValue(HasCrosshairProperty);
        set => SetValue(HasCrosshairProperty, value);
    }

    /// <summary>
    /// Gets the ROI collection stored in image pixel coordinates.
    /// </summary>
    public ObservableCollection<RoiShape> Rois { get; } = [];

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _surface = GetTemplateChild(SurfacePartName) as Canvas;
        _image = GetTemplateChild(ImagePartName) as Image;
        _roiCanvas = GetTemplateChild(RoiCanvasPartName) as Canvas;
        _crosshairCanvas = GetTemplateChild(CrosshairCanvasPartName) as Canvas;
        _navigator = GetTemplateChild(NavigatorPartName) as Border;
        _navigatorCanvas = GetTemplateChild(NavigatorCanvasPartName) as Canvas;
        _navigatorImage = GetTemplateChild(NavigatorImagePartName) as Image;
        _navigatorViewport = GetTemplateChild(NavigatorViewportPartName) as Rectangle;
        _toolBar = GetTemplateChild(ToolBarPartName) as Border;
        _panTool = GetTemplateChild(PanToolPartName) as ToggleButton;
        _selectTool = GetTemplateChild(SelectToolPartName) as ToggleButton;
        _rectangleTool = GetTemplateChild(RectangleToolPartName) as ToggleButton;
        _ellipseTool = GetTemplateChild(EllipseToolPartName) as ToggleButton;
        _crosshairTool = GetTemplateChild(CrosshairToolPartName) as ToggleButton;
        _clearRoisTool = GetTemplateChild(ClearRoisToolPartName) as ButtonBase;
        //_statusText = GetTemplateChild(StatusTextPartName) as TextBlock;

        if (_surface is not null)
        {
            _surface.MouseWheel += OnSurfaceMouseWheel;
            _surface.MouseLeftButtonDown += OnSurfaceMouseLeftButtonDown;
            _surface.MouseLeftButtonUp += OnSurfaceMouseLeftButtonUp;
            _surface.MouseRightButtonDown += OnSurfaceMouseRightButtonDown;
            _surface.MouseRightButtonUp += OnSurfaceMouseRightButtonUp;
            _surface.MouseDown += OnSurfaceMouseDown;
            _surface.MouseUp += OnSurfaceMouseUp;
            _surface.MouseMove += OnSurfaceMouseMove;
            _surface.SizeChanged += OnSurfaceSizeChanged;
        }

        if (_navigatorCanvas is not null)
        {
            _navigatorCanvas.MouseLeftButtonDown += OnNavigatorMouseLeftButtonDown;
            _navigatorCanvas.MouseMove += OnNavigatorMouseMove;
            _navigatorCanvas.MouseLeftButtonUp += OnNavigatorMouseLeftButtonUp;
        }

        AttachToolButtonHandlers();
        UpdateToolBarVisibility();
        UpdateToolButtonStates();
        ApplySource();
        RenderAll();
    }

    /// <summary>
    /// Loads a local image file into memory and displays it.
    /// </summary>
    /// <param name="filePath">The local image file path.</param>
    public void LoadImage(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();

        Source = bitmap;
        ImageLoaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears the displayed image and ROI data.
    /// </summary>
    public void UnloadImage()
    {
        Source = null;
        Rois.Clear();
        _viewport.SetImageSize(new Size());
        RenderAll();
    }

    /// <summary>
    /// Resets the view so the full image fits inside the viewport.
    /// </summary>
    public void ResetView()
    {
        _viewport.FitToView();
        RenderAll();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Deletes the selected ROI shape.
    /// </summary>
    public void DeleteSelectedRoi()
    {
        var selected = Rois.FirstOrDefault(roi => roi.IsSelected);
        if (selected is not null)
        {
            Rois.Remove(selected);
        }
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ImageViewer2D)d).ApplySource();
    }

    private static void OnShowNavigatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ImageViewer2D)d).RenderNavigator();
    }

    private static void OnShowToolBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ImageViewer2D)d).UpdateToolBarVisibility();
    }

    private static void OnToolModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ImageViewer2D)d).UpdateToolButtonStates();
    }

    private static void OnCrosshairPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ImageViewer2D)d).RenderCrosshairs();
    }

    private void AttachToolButtonHandlers()
    {
        AttachToolButtonHandler(_panTool);
        AttachToolButtonHandler(_selectTool);
        AttachToolButtonHandler(_rectangleTool);
        AttachToolButtonHandler(_ellipseTool);
        AttachToolButtonHandler(_crosshairTool);
        AttachClearRoisToolHandler();
    }

    private void AttachToolButtonHandler(ToggleButton? button)
    {
        if (button is null)
        {
            return;
        }

        button.Click -= OnToolButtonClick;
        button.Click += OnToolButtonClick;
    }

    private void AttachClearRoisToolHandler()
    {
        if (_clearRoisTool is null)
        {
            return;
        }

        _clearRoisTool.Click -= OnClearRoisToolClick;
        _clearRoisTool.Click += OnClearRoisToolClick;
    }

    private void OnClearRoisToolClick(object sender, RoutedEventArgs e)
    {
        Rois.Clear();
        CrosshairPosition = null;
        HasCrosshair = false;
        if (sender is ToggleButton button)
        {
            button.IsChecked = false;
        }

        RenderAll();
        RoiChanged?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void OnToolButtonClick(object sender, RoutedEventArgs e)
    {
        ToolMode = sender switch
        {
            _ when ReferenceEquals(sender, _panTool) => RoiToolMode.Pan,
            _ when ReferenceEquals(sender, _selectTool) => RoiToolMode.Select,
            _ when ReferenceEquals(sender, _rectangleTool) => RoiToolMode.Rectangle,
            _ when ReferenceEquals(sender, _ellipseTool) => RoiToolMode.Ellipse,
            _ when ReferenceEquals(sender, _crosshairTool) => RoiToolMode.Crosshair,
            _ => ToolMode
        };
        UpdateToolButtonStates();
    }

    private void UpdateToolButtonStates()
    {
        SetToolChecked(_panTool, ToolMode == RoiToolMode.Pan);
        SetToolChecked(_selectTool, ToolMode == RoiToolMode.Select);
        SetToolChecked(_rectangleTool, ToolMode == RoiToolMode.Rectangle);
        SetToolChecked(_ellipseTool, ToolMode == RoiToolMode.Ellipse);
        SetToolChecked(_crosshairTool, ToolMode == RoiToolMode.Crosshair);
    }

    private static void SetToolChecked(ToggleButton? button, bool isChecked)
    {
        if (button is not null)
        {
            button.IsChecked = isChecked;
        }
    }

    private void UpdateToolBarVisibility()
    {
        if (_toolBar is not null)
        {
            _toolBar.Visibility = ShowToolBar ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ApplySource()
    {
        if (_image is not null)
        {
            _image.Source = Source;
        }

        if (_navigatorImage is not null)
        {
            _navigatorImage.Source = Source;
        }

        if (Source is BitmapSource bitmap)
        {
            _viewport.SetImageSize(new Size(bitmap.PixelWidth, bitmap.PixelHeight));
            if (_surface is not null)
            {
                _viewport.SetViewportSize(new Size(_surface.ActualWidth, _surface.ActualHeight));
            }

            _viewport.FitToView();
        }

        RenderAll();
    }

    private void OnSurfaceSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _viewport.SetViewportSize(e.NewSize);
        _viewport.FitToView();
        RenderAll();
    }

    private void OnSurfaceMouseWheel(object sender, MouseWheelEventArgs e)
    {
        RaiseImageMouseWheel(e);
        var factor = e.Delta > 0 ? 1.15 : 1.0 / 1.15;
        _viewport.ZoomAt(e.GetPosition(_surface), factor, maximumScale: 20.0);
        RenderAll();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void OnSurfaceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_surface is null)
        {
            return;
        }

        Focus();
        _surface.CaptureMouse();
        _lastMousePoint = e.GetPosition(_surface);
        RaiseImageMouseDown(e);
        var imagePoint = _viewport.ScreenToImage(_lastMousePoint);

        if (TryBeginAutoRoiSelection(_lastMousePoint, imagePoint))
        {
            return;
        }

        if (ToolMode == RoiToolMode.Crosshair && TryBeginCrosshairEdit(_lastMousePoint, imagePoint))
        {
            return;
        }

        if (ToolMode == RoiToolMode.Pan)
        {
            _isPanning = true;
            return;
        }

        if (ToolMode == RoiToolMode.Select)
        {
            BeginRoiSelectionOrEdit(_lastMousePoint, imagePoint);
            return;
        }

        if (ToolMode == RoiToolMode.Crosshair)
        {
            CrosshairPosition = imagePoint;
            HasCrosshair = true;
            RenderCrosshairs();
            return;
        }

        _activeRoi = ToolMode == RoiToolMode.Rectangle
            ? new RectangleRoi(imagePoint, imagePoint)
            : new EllipseRoi(imagePoint, imagePoint);
        _activeRoi.Stroke = RoiStroke;
        _activeRoi.StrokeThickness = RoiStrokeThickness;
        SelectOnly(_activeRoi);
        Rois.Add(_activeRoi);
    }

    private void OnSurfaceMouseMove(object sender, MouseEventArgs e)
    {
        if (_surface is null)
        {
            return;
        }

        var currentPoint = e.GetPosition(_surface);
        UpdateStatus(_viewport.ScreenToImage(currentPoint));
        RaiseImageMouseMove(e);

        if (e.LeftButton != MouseButtonState.Pressed && _activeRoi is null && _editingRoi is null)
        {
            UpdateHoverCursor(currentPoint);
        }

        if (_crosshairEditOperation != CrosshairEditOperation.None && e.LeftButton == MouseButtonState.Pressed)
        {
            UpdateCrosshairEdit(_viewport.ScreenToImage(currentPoint));
            RenderCrosshairs();
            return;
        }

        if (_isPanning && e.LeftButton == MouseButtonState.Pressed)
        {
            _viewport.Pan(currentPoint - _lastMousePoint);
            _lastMousePoint = currentPoint;
            RenderAll();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (_editingRoi is not null && e.LeftButton == MouseButtonState.Pressed)
        {
            UpdateRoiEdit(_viewport.ScreenToImage(currentPoint));
            RenderRois();
            return;
        }

        if (_activeRoi is not null && e.LeftButton == MouseButtonState.Pressed)
        {
            _activeRoi.EndPoint = _viewport.ScreenToImage(currentPoint);
            RenderRois();
            RenderNavigator();
        }
    }

    private void OnSurfaceMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        RaiseImageMouseUp(e);
        _isPanning = false;
        _activeRoi = null;
        _editingRoi = null;
        _roiEditOperation = RoiEditOperation.None;
        _resizeHandle = ResizeHandle.None;
        _crosshairEditOperation = CrosshairEditOperation.None;
        _surface?.ReleaseMouseCapture();
        RenderAll();
    }

    private void OnSurfaceMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        RaiseImageMouseDown(e);
    }

    private void OnSurfaceMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        RaiseImageMouseUp(e);
    }

    private void OnSurfaceMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
        {
            return;
        }

        RaiseImageMouseDown(e);
    }

    private void OnSurfaceMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
        {
            return;
        }

        RaiseImageMouseUp(e);
    }

    private void OnRoisChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RenderAll();
        RoiChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BeginRoiSelectionOrEdit(Point screenPoint, Point imagePoint)
    {
        if (TryBeginEditSelectedRoi(screenPoint, imagePoint))
        {
            return;
        }

        SelectRoiAt(imagePoint);

        if (TryBeginEditSelectedRoi(screenPoint, imagePoint))
        {
            return;
        }
    }

    private bool TryBeginAutoRoiSelection(Point screenPoint, Point imagePoint)
    {
        var roi = HitTestRoiBoundary(screenPoint, imagePoint);
        if (roi is null)
        {
            return false;
        }

        SelectOnly(roi);
        TryBeginEditSelectedRoi(screenPoint, imagePoint);
        RenderRois();
        return true;
    }

    private bool TryBeginEditSelectedRoi(Point screenPoint, Point imagePoint)
    {
        var selected = Rois.FirstOrDefault(roi => roi.IsSelected);
        if (selected is null)
        {
            return false;
        }

        var handle = HitTestResizeHandle(selected, screenPoint);
        if (handle != ResizeHandle.None)
        {
            _editingRoi = selected;
            _roiEditOperation = RoiEditOperation.Resize;
            _resizeHandle = handle;
            _editStartBounds = selected.Bounds;
            _editStartImagePoint = imagePoint;
            return true;
        }

        if (selected.Contains(imagePoint))
        {
            _editingRoi = selected;
            _roiEditOperation = RoiEditOperation.Move;
            _resizeHandle = ResizeHandle.None;
            _editStartBounds = selected.Bounds;
            _editStartImagePoint = imagePoint;
            return true;
        }

        return false;
    }

    private ResizeHandle HitTestResizeHandle(RoiShape roi, Point screenPoint)
    {
        var bounds = roi.Bounds;
        var corners = new (ResizeHandle Handle, Point Point)[]
        {
            (ResizeHandle.TopLeft, bounds.TopLeft),
            (ResizeHandle.TopRight, bounds.TopRight),
            (ResizeHandle.BottomLeft, bounds.BottomLeft),
            (ResizeHandle.BottomRight, bounds.BottomRight),
        };

        foreach (var corner in corners)
        {
            var screenCorner = _viewport.ImageToScreen(corner.Point);
            var hitArea = new Rect(screenCorner.X - 6, screenCorner.Y - 6, 12, 12);
            if (hitArea.Contains(screenPoint))
            {
                return corner.Handle;
            }
        }

        return ResizeHandle.None;
    }

    private RoiShape? HitTestRoiBoundary(Point screenPoint, Point imagePoint)
    {
        foreach (var roi in Rois.Reverse())
        {
            if (HitTestResizeHandle(roi, screenPoint) != ResizeHandle.None)
            {
                return roi;
            }

            var imageTolerance = _viewport.Scale <= 0 ? RoiEdgeHitTolerance : RoiEdgeHitTolerance / _viewport.Scale;
            if (roi.IsNearBoundary(imagePoint, imageTolerance))
            {
                return roi;
            }
        }

        return null;
    }

    private void UpdateHoverCursor(Point screenPoint)
    {
        if (ToolMode == RoiToolMode.Crosshair)
        {
            var operation = HitTestCrosshair(screenPoint);
            Cursor = operation switch
            {
                CrosshairEditOperation.MoveRedVertical => Cursors.SizeWE,
                CrosshairEditOperation.MoveBlueHorizontal => Cursors.SizeNS,
                _ => null,
            };

            return;
        }

        var imagePoint = _viewport.ScreenToImage(screenPoint);
        Cursor = HitTestRoiBoundary(screenPoint, imagePoint) is not null
            ? Cursors.Hand
            : null;
    }

    private bool TryBeginCrosshairEdit(Point screenPoint, Point imagePoint)
    {
        var operation = HitTestCrosshair(screenPoint);
        if (operation == CrosshairEditOperation.None)
        {
            return false;
        }

        _crosshairEditOperation = operation;
        if (CrosshairPosition is null)
        {
            CrosshairPosition = imagePoint;
            HasCrosshair = true;
        }

        return true;
    }

    private CrosshairEditOperation HitTestCrosshair(Point screenPoint)
    {
        if (!HasCrosshair || CrosshairPosition is not { } position)
        {
            return CrosshairEditOperation.None;
        }

        var crosshairScreenPoint = _viewport.ImageToScreen(position);
        var nearRedVertical = Math.Abs(screenPoint.X - crosshairScreenPoint.X) <= CrosshairHitTolerance;
        var nearBlueHorizontal = Math.Abs(screenPoint.Y - crosshairScreenPoint.Y) <= CrosshairHitTolerance;

        if (nearRedVertical && nearBlueHorizontal)
        {
            return Math.Abs(screenPoint.X - crosshairScreenPoint.X) <= Math.Abs(screenPoint.Y - crosshairScreenPoint.Y)
                ? CrosshairEditOperation.MoveRedVertical
                : CrosshairEditOperation.MoveBlueHorizontal;
        }

        if (nearRedVertical)
        {
            return CrosshairEditOperation.MoveRedVertical;
        }

        return nearBlueHorizontal
            ? CrosshairEditOperation.MoveBlueHorizontal
            : CrosshairEditOperation.None;
    }

    private void UpdateCrosshairEdit(Point imagePoint)
    {
        var current = CrosshairPosition ?? imagePoint;
        CrosshairPosition = _crosshairEditOperation switch
        {
            CrosshairEditOperation.MoveRedVertical => new Point(imagePoint.X, current.Y),
            CrosshairEditOperation.MoveBlueHorizontal => new Point(current.X, imagePoint.Y),
            _ => current,
        };
        HasCrosshair = true;
    }

    private void UpdateRoiEdit(Point currentImagePoint)
    {
        if (_editingRoi is null)
        {
            return;
        }

        var delta = currentImagePoint - _editStartImagePoint;
        if (_roiEditOperation == RoiEditOperation.Move)
        {
            _editingRoi.SetBounds(new Rect(
                _editStartBounds.X + delta.X,
                _editStartBounds.Y + delta.Y,
                _editStartBounds.Width,
                _editStartBounds.Height));
            return;
        }

        var left = _editStartBounds.Left;
        var top = _editStartBounds.Top;
        var right = _editStartBounds.Right;
        var bottom = _editStartBounds.Bottom;

        switch (_resizeHandle)
        {
            case ResizeHandle.TopLeft:
                left += delta.X;
                top += delta.Y;
                break;
            case ResizeHandle.TopRight:
                right += delta.X;
                top += delta.Y;
                break;
            case ResizeHandle.BottomLeft:
                left += delta.X;
                bottom += delta.Y;
                break;
            case ResizeHandle.BottomRight:
                right += delta.X;
                bottom += delta.Y;
                break;
        }

        _editingRoi.SetBounds(CreateNormalizedRect(left, top, right, bottom));
    }

    private static Rect CreateNormalizedRect(double left, double top, double right, double bottom)
    {
        var x = Math.Min(left, right);
        var y = Math.Min(top, bottom);
        var width = Math.Max(1, Math.Abs(right - left));
        var height = Math.Max(1, Math.Abs(bottom - top));
        return new Rect(x, y, width, height);
    }

    private void SelectRoiAt(Point imagePoint)
    {
        var selected = Rois.LastOrDefault(roi => roi.Contains(imagePoint));
        SelectOnly(selected);
        RenderRois();
    }

    private RoiShape? HitTestRoi(Point imagePoint)
    {
        return Rois.LastOrDefault(roi => roi.Contains(imagePoint) || roi.IsNearBoundary(imagePoint, RoiEdgeHitTolerance / Math.Max(_viewport.Scale, 0.0001)));
    }

    private ImageViewerMouseEventArgs CreateMouseEventArgs(MouseEventArgs e)
    {
        var controlPoint = e.GetPosition(_surface);
        var imagePoint = _viewport.ScreenToImage(controlPoint);

        return new ImageViewerMouseEventArgs(
            controlPoint,
            imagePoint,
            Keyboard.Modifiers,
            _viewport.Scale,
            ToolMode,
            HitTestRoi(imagePoint));
    }

    private ImageViewerMouseButtonEventArgs CreateMouseButtonEventArgs(MouseButtonEventArgs e)
    {
        var controlPoint = e.GetPosition(_surface);
        var imagePoint = _viewport.ScreenToImage(controlPoint);

        return new ImageViewerMouseButtonEventArgs(
            controlPoint,
            imagePoint,
            e.ChangedButton,
            e.ClickCount,
            Keyboard.Modifiers,
            _viewport.Scale,
            ToolMode,
            HitTestRoi(imagePoint));
    }

    private ImageViewerMouseWheelEventArgs CreateMouseWheelEventArgs(MouseWheelEventArgs e)
    {
        var controlPoint = e.GetPosition(_surface);
        var imagePoint = _viewport.ScreenToImage(controlPoint);

        return new ImageViewerMouseWheelEventArgs(
            controlPoint,
            imagePoint,
            e.Delta,
            Keyboard.Modifiers,
            _viewport.Scale,
            ToolMode,
            HitTestRoi(imagePoint));
    }

    private void RaiseImageMouseMove(MouseEventArgs e)
    {
        ImageMouseMove?.Invoke(this, CreateMouseEventArgs(e));
    }

    private void RaiseImageMouseDown(MouseButtonEventArgs e)
    {
        var args = CreateMouseButtonEventArgs(e);
        ImageMouseDown?.Invoke(this, args);

        if (args.ClickCount == 2)
        {
            ImageMouseDoubleClick?.Invoke(this, args);
        }
    }

    private void RaiseImageMouseUp(MouseButtonEventArgs e)
    {
        ImageMouseUp?.Invoke(this, CreateMouseButtonEventArgs(e));
    }

    private void RaiseImageMouseWheel(MouseWheelEventArgs e)
    {
        ImageMouseWheel?.Invoke(this, CreateMouseWheelEventArgs(e));
    }

    private void SelectOnly(RoiShape? selected)
    {
        foreach (var roi in Rois)
        {
            roi.IsSelected = ReferenceEquals(roi, selected);
        }

        if (selected is not null)
        {
            selected.IsSelected = true;
        }
    }

    private void RenderAll()
    {
        RenderImage();
        RenderRois();
        RenderCrosshairs();
        RenderNavigator();
        UpdateStatus(null);
    }

    private void RenderImage()
    {
        if (_image is null)
        {
            return;
        }

        _image.Width = _viewport.ImageSize.Width;
        _image.Height = _viewport.ImageSize.Height;
        _image.RenderTransform = new MatrixTransform(_viewport.Scale, 0, 0, _viewport.Scale, _viewport.Offset.X, _viewport.Offset.Y);
    }

    private void RenderRois()
    {
        if (_roiCanvas is null)
        {
            return;
        }

        _roiCanvas.Children.Clear();

        foreach (var roi in Rois)
        {
            var bounds = roi.Bounds;
            var topLeft = _viewport.ImageToScreen(bounds.TopLeft);
            var bottomRight = _viewport.ImageToScreen(bounds.BottomRight);
            var width = Math.Abs(bottomRight.X - topLeft.X);
            var height = Math.Abs(bottomRight.Y - topLeft.Y);
            var shape = CreateRoiElement(roi);

            shape.Width = width;
            shape.Height = height;
            shape.Stroke = roi.IsSelected ? Brushes.Gold : roi.Stroke;
            shape.StrokeThickness = roi.IsSelected ? roi.StrokeThickness + 1.0 : roi.StrokeThickness;
            shape.Fill = Brushes.Transparent;

            Canvas.SetLeft(shape, Math.Min(topLeft.X, bottomRight.X));
            Canvas.SetTop(shape, Math.Min(topLeft.Y, bottomRight.Y));
            _roiCanvas.Children.Add(shape);

            if (roi.IsSelected)
            {
                RenderRoiHandles(bounds);
            }
        }
    }

    private void RenderRoiHandles(Rect imageBounds)
    {
        if (_roiCanvas is null)
        {
            return;
        }

        var points = new[]
        {
            imageBounds.TopLeft,
            imageBounds.TopRight,
            imageBounds.BottomLeft,
            imageBounds.BottomRight,
        };

        foreach (var point in points)
        {
            var screenPoint = _viewport.ImageToScreen(point);
            var handle = new Rectangle
            {
                Width = 9,
                Height = 9,
                Fill = Brushes.White,
                Stroke = Brushes.Gold,
                StrokeThickness = 1.5,
            };

            Canvas.SetLeft(handle, screenPoint.X - 4.5);
            Canvas.SetTop(handle, screenPoint.Y - 4.5);
            _roiCanvas.Children.Add(handle);
        }
    }

    private static Shape CreateRoiElement(RoiShape roi)
    {
        return roi.Kind == RoiShapeKind.Ellipse ? new Ellipse() : new Rectangle();
    }

    private void RenderCrosshairs()
    {
        if (_crosshairCanvas is null)
        {
            return;
        }

        _crosshairCanvas.Children.Clear();
        RenderCrosshair();
    }

    private void RenderCrosshair()
    {
        if (_crosshairCanvas is null || !HasCrosshair || CrosshairPosition is not { } point)
        {
            return;
        }

        var screenPoint = _viewport.ImageToScreen(point);
        var imageBounds = _viewport.GetImageScreenBounds();
        var blueHorizontal = new Line
        {
            X1 = imageBounds.Left,
            X2 = imageBounds.Right,
            Y1 = screenPoint.Y,
            Y2 = screenPoint.Y,
            Stroke = Brushes.DeepSkyBlue,
            StrokeThickness = 1.5,
            IsHitTestVisible = false,
        };
        var redVertical = new Line
        {
            X1 = screenPoint.X,
            X2 = screenPoint.X,
            Y1 = imageBounds.Top,
            Y2 = imageBounds.Bottom,
            Stroke = Brushes.Red,
            StrokeThickness = 1.5,
            IsHitTestVisible = false,
        };

        _crosshairCanvas.Children.Add(blueHorizontal);
        _crosshairCanvas.Children.Add(redVertical);
    }

    private void RenderNavigator()
    {
        if (_navigator is null || _navigatorImage is null || _navigatorViewport is null)
        {
            return;
        }

        _navigator.Visibility = ShowNavigator && Source is not null ? Visibility.Visible : Visibility.Collapsed;
        if (_navigator.Visibility != Visibility.Visible || _viewport.ImageSize.Width <= 0 || _viewport.ImageSize.Height <= 0)
        {
            return;
        }

        const double navigatorWidth = 180.0;
        const double navigatorHeight = 120.0;
        var scale = Math.Min(navigatorWidth / _viewport.ImageSize.Width, navigatorHeight / _viewport.ImageSize.Height);
        var imageWidth = _viewport.ImageSize.Width * scale;
        var imageHeight = _viewport.ImageSize.Height * scale;
        var imageLeft = (navigatorWidth - imageWidth) / 2.0;
        var imageTop = (navigatorHeight - imageHeight) / 2.0;

        _navigatorImage.Width = imageWidth;
        _navigatorImage.Height = imageHeight;
        Canvas.SetLeft(_navigatorImage, imageLeft);
        Canvas.SetTop(_navigatorImage, imageTop);

        var visible = _viewport.GetVisibleImageRect();
        _navigatorViewport.Width = visible.Width * scale;
        _navigatorViewport.Height = visible.Height * scale;
        Canvas.SetLeft(_navigatorViewport, imageLeft + visible.X * scale);
        Canvas.SetTop(_navigatorViewport, imageTop + visible.Y * scale);

        _navigatorScale = scale;
        _navigatorImageLeft = imageLeft;
        _navigatorImageTop = imageTop;
    }

    private void OnNavigatorMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_navigatorCanvas is null)
        {
            return;
        }

        _isNavigatorDragging = true;
        _navigatorCanvas.CaptureMouse();
        CenterViewportFromNavigatorPoint(e.GetPosition(_navigatorCanvas));
        e.Handled = true;
    }

    private void OnNavigatorMouseMove(object sender, MouseEventArgs e)
    {
        if (_isNavigatorDragging && _navigatorCanvas is not null && e.LeftButton == MouseButtonState.Pressed)
        {
            CenterViewportFromNavigatorPoint(e.GetPosition(_navigatorCanvas));
            e.Handled = true;
        }
    }

    private void OnNavigatorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isNavigatorDragging = false;
        _navigatorCanvas?.ReleaseMouseCapture();
        e.Handled = true;
    }

    private void CenterViewportFromNavigatorPoint(Point navigatorPoint)
    {
        if (_navigatorScale <= 0 || _viewport.ImageSize.Width <= 0 || _viewport.ImageSize.Height <= 0)
        {
            return;
        }

        var imageX = (navigatorPoint.X - _navigatorImageLeft) / _navigatorScale;
        var imageY = (navigatorPoint.Y - _navigatorImageTop) / _navigatorScale;
        imageX = Math.Clamp(imageX, 0, _viewport.ImageSize.Width);
        imageY = Math.Clamp(imageY, 0, _viewport.ImageSize.Height);

        _viewport.CenterOnImagePoint(new Point(imageX, imageY));
        RenderAll();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateStatus(Point? mouseImagePoint)
    {
        Status = new ImageViewerStatus
        {
            ImageSize = _viewport.ImageSize,
            ZoomRatio = _viewport.Scale,
            MouseImagePosition = mouseImagePoint,
            RoiCount = Rois.Count,
        };

        //if (_statusText is not null)
        //{
        //    _statusText.Text = Status.ToString();
        //}
    }
}

internal enum RoiEditOperation
{
    None,
    Move,
    Resize,
}

internal enum ResizeHandle
{
    None,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

internal enum CrosshairEditOperation
{
    None,
    MoveRedVertical,
    MoveBlueHorizontal,
}
