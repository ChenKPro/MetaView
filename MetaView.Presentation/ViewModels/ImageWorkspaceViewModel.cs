using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.Imaging.Signal;
using MetaView.Presentation.Infrastructure;
using MetaView.Services.Interfaces;
using Prism.Events;
using Vibronix.Presentation.Wpf.Plot.Models;
using Vibronix.Presentation.Wpf.Plot.Services;
using DelegateCommand = MetaView.Presentation.Infrastructure.DelegateCommand;
using DrawingColor = System.Drawing.Color;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Exposes the central image workspace state.
/// </summary>
public sealed class ImageWorkspaceViewModel : MetaView.Presentation.Infrastructure.BindableBase
{
    private const string SignalChartId = "MetaView.Signal.Trace";
    private readonly IPlotService _signalPlotService;
    private string _selectedView = "Live";
    private bool _roiVisible = true;
    private double _roiLeft = 280;
    private double _roiTop = 160;
    private double _roiWidth = 260;
    private double _roiHeight = 160;
    private string _activeTool = "Rectangle ROI";
    private bool _showSignalPanel = true;
    private bool _showHistogramPanel = true;
    private bool _showContextPanel = true;
    private ImageSource? _signalImageSource;
    private PointCollection _ai0Points = [];
    private PointCollection _ai1Points = [];
    private PointCollection _ai2Points = [];
    private PointCollection _ai3Points = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageWorkspaceViewModel" /> class.
    /// </summary>
    public ImageWorkspaceViewModel(IPlotServiceFactory plotServiceFactory, IEventAggregator eventAggregator)
    {
        _signalPlotService = plotServiceFactory.Create(SignalChartId);
        _signalPlotService.SetBehavior(new PlotBehavior { AutoScale = true, ShowCrosshair = true, ShowLegend = true });
        _signalPlotService.SetStyle(CreateSignalPlotStyle());
        WorkspaceViews = ["Live", "Capture", "2D Large Area", "3D", "Multimodality"];
        ToggleRoiCommand = new DelegateCommand(() => RoiVisible = !RoiVisible);
        ShiftRoiCommand = new DelegateCommand(ShiftRoi);
        eventAggregator
            .GetEvent<SignalImageFramePublishedEvent>()
            .Subscribe(ApplySignalImageFrame, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);
        eventAggregator
            .GetEvent<SignalTraceFramePublishedEvent>()
            .Subscribe(ApplySignalTraceFrame, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);
        eventAggregator
            .GetEvent<BrightfieldCameraFramePublishedEvent>()
            .Subscribe(ApplyCameraFrame, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);
    }

    /// <summary>
    /// Gets the available workspace views.
    /// </summary>
    public ObservableCollection<string> WorkspaceViews { get; }

    /// <summary>
    /// Gets the source image path.
    /// </summary>
    public string ImagePath => ResolveImagePath();

    public ImageSource LiveImageSource
    {
        get
        {
            if (_signalImageSource is not null)
            {
                return _signalImageSource;
            }

            var imagePath = ResolveImagePath();
            if (!File.Exists(imagePath))
            {
                return CreateFallbackImage();
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(imagePath, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
    }

    /// <summary>
    /// Gets or sets the selected workspace view.
    /// </summary>
    public string SelectedView
    {
        get => _selectedView;
        set => SetProperty(ref _selectedView, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the ROI is visible.
    /// </summary>
    public bool RoiVisible
    {
        get => _roiVisible;
        set => SetProperty(ref _roiVisible, value);
    }

    public double RoiLeft { get => _roiLeft; set => SetProperty(ref _roiLeft, value); }
    public double RoiTop { get => _roiTop; set => SetProperty(ref _roiTop, value); }
    public double RoiWidth { get => _roiWidth; set => SetProperty(ref _roiWidth, value); }
    public double RoiHeight { get => _roiHeight; set => SetProperty(ref _roiHeight, value); }
    public string ActiveTool { get => _activeTool; set => SetProperty(ref _activeTool, value); }

    public bool ShowSignalPanel
    {
        get => _showSignalPanel;
        set => SetAnalysisPanelVisibility(ref _showSignalPanel, value, nameof(ShowSignalPanel));
    }

    public bool ShowHistogramPanel
    {
        get => _showHistogramPanel;
        set => SetAnalysisPanelVisibility(ref _showHistogramPanel, value, nameof(ShowHistogramPanel));
    }

    public bool ShowContextPanel
    {
        get => _showContextPanel;
        set => SetAnalysisPanelVisibility(ref _showContextPanel, value, nameof(ShowContextPanel));
    }

    public bool SignalPanelVisible => ShowSignalPanel;
    public bool HistogramPanelVisible => ShowHistogramPanel;
    public bool ContextPanelVisible => ShowContextPanel;

    public GridLength SignalPanelWidth => GetAnalysisPanelWidth("Signal");
    public GridLength HistogramPanelWidth => GetAnalysisPanelWidth("Histogram");
    public GridLength ContextPanelWidth => GetAnalysisPanelWidth("Context");
    public string SignalPlotChartId => SignalChartId;
    public PointCollection Ai0Points { get => _ai0Points; private set => SetProperty(ref _ai0Points, value); }
    public PointCollection Ai1Points { get => _ai1Points; private set => SetProperty(ref _ai1Points, value); }
    public PointCollection Ai2Points { get => _ai2Points; private set => SetProperty(ref _ai2Points, value); }
    public PointCollection Ai3Points { get => _ai3Points; private set => SetProperty(ref _ai3Points, value); }

    public ICommand ToggleRoiCommand { get; }
    public ICommand ShiftRoiCommand { get; }

    private void ShiftRoi()
    {
        RoiLeft = RoiLeft > 520 ? 180 : RoiLeft + 80;
        RoiTop = RoiTop > 300 ? 120 : RoiTop + 35;
        ActiveTool = "ROI adjusted";
    }

    private GridLength GetAnalysisPanelWidth(string panel)
    {
        var visible = panel switch
        {
            "Signal" => ShowSignalPanel,
            "Histogram" => ShowHistogramPanel,
            "Context" => ShowContextPanel,
            _ => false
        };

        return visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    }

    private void SetAnalysisPanelVisibility(ref bool field, bool value, string propertyName)
    {
        if (!value && VisibleAnalysisPanelCount == 1 && field)
        {
            RaisePropertyChanged(propertyName);
            return;
        }

        if (SetProperty(ref field, value, propertyName))
        {
            RaiseAnalysisLayoutProperties();
        }
    }

    private int VisibleAnalysisPanelCount =>
        (ShowSignalPanel ? 1 : 0) +
        (ShowHistogramPanel ? 1 : 0) +
        (ShowContextPanel ? 1 : 0);

    private void RaiseAnalysisLayoutProperties()
    {
        RaisePropertyChanged(nameof(SignalPanelVisible));
        RaisePropertyChanged(nameof(HistogramPanelVisible));
        RaisePropertyChanged(nameof(ContextPanelVisible));
        RaisePropertyChanged(nameof(SignalPanelWidth));
        RaisePropertyChanged(nameof(HistogramPanelWidth));
        RaisePropertyChanged(nameof(ContextPanelWidth));
    }

    public void ApplySignalImageFrame(SignalImageFrame frame)
    {
        _signalImageSource = CreateSignalBitmapSource(frame);
        RaisePropertyChanged(nameof(LiveImageSource));
    }

    public void ApplyCameraFrame(BrightfieldCameraFrame frame)
    {
        var bitmap = CreateCameraBitmapSource(frame);
        if (bitmap is null)
        {
            return;
        }

        _signalImageSource = bitmap;
        RaisePropertyChanged(nameof(LiveImageSource));
    }

    public void ApplySignalTraceFrame(SignalTraceFrame frame)
    {
        Ai0Points = CreatePolyline(frame.Ai0, 440, 142, 10);
        Ai1Points = CreatePolyline(frame.Ai1, 440, 142, 10);
        Ai2Points = CreatePolyline(frame.Ai2, 440, 142, 10);
        Ai3Points = CreatePolyline(frame.Ai3, 440, 142, 10);
        _signalPlotService.SetDataSeries(CreateSignalSeries(frame));
    }

    private static IReadOnlyList<DataSeries> CreateSignalSeries(SignalTraceFrame frame)
    {
        return
        [
            CreateSeries("AI0 X", frame.Ai0, DrawingColor.FromArgb(110, 143, 168), AxisType.Left),
            CreateSeries("AI1 Y", frame.Ai1, DrawingColor.FromArgb(178, 141, 255), AxisType.Left),
            CreateSeries("AI2 Laser", frame.Ai2, DrawingColor.FromArgb(90, 174, 255), AxisType.Right),
            CreateSeries("AI3 Laser", frame.Ai3, DrawingColor.FromArgb(255, 176, 0), AxisType.Right)
        ];
    }

    private static DataSeries CreateSeries(
        string name,
        IReadOnlyList<double> values,
        DrawingColor color,
        AxisType axisType)
    {
        return new DataSeries
        {
            Name = name,
            X = Enumerable.Range(0, values.Count).Select(index => (double)index).ToArray(),
            Y = values.ToArray(),
            XName = "Sample",
            YName = name,
            Type = PlotType.ScatterLine,
            AxisType = axisType,
            Width = 2,
            Color = color,
            YAxisColor = color
        };
    }

    private static PlotStyle CreateSignalPlotStyle()
    {
        var style = PlotStyle.GetDark();
        style.Background = DrawingColor.FromArgb(21, 31, 40);
        style.AxisLabelFontSize = 11;
        style.AxisTickFontSize = 10;
        style.LegendFontSize = 11;
        style.CrosshairLabelFontSize = 11;
        return style;
    }

    private static PointCollection CreatePolyline(IReadOnlyList<double> values, double width, double height, double padding)
    {
        var points = new PointCollection();
        if (values.Count == 0)
        {
            return points;
        }

        var min = values.Min();
        var max = values.Max();
        var span = Math.Max(1e-9, max - min);
        var drawableWidth = width - padding * 2;
        var drawableHeight = height - padding * 2;

        for (var index = 0; index < values.Count; index++)
        {
            var x = padding + index * drawableWidth / Math.Max(1, values.Count - 1);
            var y = padding + (1 - (values[index] - min) / span) * drawableHeight;
            points.Add(new Point(x, y));
        }

        points.Freeze();
        return points;
    }

    private static BitmapSource? CreateCameraBitmapSource(BrightfieldCameraFrame frame)
    {
        if (frame.Width <= 0 || frame.Height <= 0 || frame.Pixels.Length == 0)
        {
            return null;
        }

        var pixelFormat = ResolvePixelFormat(frame.PixelFormat, frame.Width, frame.Height, frame.Pixels.Length);
        if (pixelFormat == PixelFormats.Default)
        {
            return null;
        }

        var stride = CalculateStride(pixelFormat, frame.Width);
        if (frame.Pixels.Length < stride * frame.Height)
        {
            return null;
        }

        var bitmap = BitmapSource.Create(frame.Width, frame.Height, 96, 96, pixelFormat, null, frame.Pixels, stride);
        bitmap.Freeze();
        return bitmap;
    }

    private static BitmapSource CreateSignalBitmapSource(SignalImageFrame frame)
    {
        var scale = Math.Max(1, (int)Math.Ceiling(512.0 / Math.Max(frame.Width, frame.Height)));
        var displayWidth = frame.Width * scale;
        var displayHeight = frame.Height * scale;
        var stride = displayWidth * 4;
        var pixels = new byte[displayHeight * stride];

        for (var y = 0; y < displayHeight; y++)
        {
            var sourceY = Math.Min(frame.Height - 1, y / scale);
            for (var x = 0; x < displayWidth; x++)
            {
                var sourceX = Math.Min(frame.Width - 1, x / scale);
                var value = frame.GrayPixels[sourceY * frame.Width + sourceX];
                var offset = y * stride + x * 4;
                pixels[offset] = value;
                pixels[offset + 1] = value;
                pixels[offset + 2] = value;
                pixels[offset + 3] = 255;
            }
        }

        var bitmap = BitmapSource.Create(displayWidth, displayHeight, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
        bitmap.Freeze();
        return bitmap;
    }

    private static PixelFormat ResolvePixelFormat(BrightfieldCameraPixelFormat pixelFormat, int width, int height, int byteCount)
    {
        return pixelFormat switch
        {
            BrightfieldCameraPixelFormat.Mono8 or
            BrightfieldCameraPixelFormat.BayerRG8 or
            BrightfieldCameraPixelFormat.BayerGB8 or
            BrightfieldCameraPixelFormat.BayerGR8 or
            BrightfieldCameraPixelFormat.BayerBG8 => PixelFormats.Gray8,
            BrightfieldCameraPixelFormat.Mono16 => PixelFormats.Gray16,
            BrightfieldCameraPixelFormat.Bgr24 => PixelFormats.Bgr24,
            BrightfieldCameraPixelFormat.Rgb24 => PixelFormats.Rgb24,
            BrightfieldCameraPixelFormat.Bgra32 => PixelFormats.Bgra32,
            _ when byteCount == width * height => PixelFormats.Gray8,
            _ when byteCount == width * height * 2 => PixelFormats.Gray16,
            _ when byteCount == width * height * 3 => PixelFormats.Bgr24,
            _ when byteCount == width * height * 4 => PixelFormats.Bgra32,
            _ => PixelFormats.Default
        };
    }

    private static int CalculateStride(PixelFormat pixelFormat, int width)
    {
        return (width * pixelFormat.BitsPerPixel + 7) / 8;
    }

    private static string ResolveImagePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var assetCandidate = Path.Combine(directory.FullName, "Assets", "pic.png");
            if (File.Exists(assetCandidate))
            {
                return assetCandidate;
            }

            var candidate = Path.Combine(directory.FullName, "pic.png");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(Environment.CurrentDirectory, "pic.png");
    }

    private static ImageSource CreateFallbackImage()
    {
        const int width = 512;
        const int height = 384;
        const int stride = width * 4;

        var pixels = new byte[height * stride];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = y * stride + x * 4;
                var value = (byte)Math.Clamp(32 + x * 0.16 + y * 0.08, 0, 255);
                pixels[offset] = value;
                pixels[offset + 1] = (byte)Math.Clamp(value + 18, 0, 255);
                pixels[offset + 2] = (byte)Math.Clamp(value + 34, 0, 255);
                pixels[offset + 3] = 255;
            }
        }

        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
        bitmap.Freeze();
        return bitmap;
    }
}

