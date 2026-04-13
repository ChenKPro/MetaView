using MetaView.Presentation.Core;
using MetaView.Presentation.Infrastructure;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Exposes scan settings for the acquisition setup panel.
/// </summary>
public sealed class ScanSetupViewModel : MetaView.Presentation.Infrastructure.BindableBase
{
    private string _zoom = "4x";
    private int _width = 800;
    private int _height = 800;
    private double _dwellTimeUs = 10;
    private int _captures = 3;
    private int _average = 1;
    private ScanMode _scanMode = ScanMode.RoundTrip;
    private int _lineSpeedHz = 200;
    private double _offsetX;
    private double _offsetY;
    private string _shutter = "Standard";

    /// <summary>
    /// Occurs when a setting changes and the recipe should be recomputed.
    /// </summary>
    public event EventHandler? SettingsChanged;

    /// <summary>
    /// Gets available zoom values.
    /// </summary>
    public IReadOnlyList<string> ZoomOptions { get; } = ["1x", "2x", "4x", "8x"];

    /// <summary>
    /// Gets available scan modes.
    /// </summary>
    public IReadOnlyList<ScanMode> ScanModes { get; } = [ScanMode.OneWay, ScanMode.RoundTrip];

    /// <summary>
    /// Gets or sets the zoom factor.
    /// </summary>
    public string Zoom
    {
        get => _zoom;
        set => SetAndNotify(ref _zoom, value);
    }

    /// <summary>
    /// Gets or sets image width in pixels.
    /// </summary>
    public int Width
    {
        get => _width;
        set => SetAndNotify(ref _width, value);
    }

    /// <summary>
    /// Gets or sets image height in pixels.
    /// </summary>
    public int Height
    {
        get => _height;
        set => SetAndNotify(ref _height, value);
    }

    /// <summary>
    /// Gets or sets pixel dwell time.
    /// </summary>
    public double DwellTimeUs
    {
        get => _dwellTimeUs;
        set => SetAndNotify(ref _dwellTimeUs, value);
    }

    /// <summary>
    /// Gets or sets the number of frames to capture.
    /// </summary>
    public int Captures
    {
        get => _captures;
        set => SetAndNotify(ref _captures, value);
    }

    /// <summary>
    /// Gets or sets the averaging count.
    /// </summary>
    public int Average
    {
        get => _average;
        set => SetAndNotify(ref _average, value);
    }

    /// <summary>
    /// Gets or sets the scan mode.
    /// </summary>
    public ScanMode ScanMode
    {
        get => _scanMode;
        set => SetAndNotify(ref _scanMode, value);
    }

    /// <summary>
    /// Gets or sets line speed.
    /// </summary>
    public int LineSpeedHz
    {
        get => _lineSpeedHz;
        set => SetAndNotify(ref _lineSpeedHz, value);
    }

    /// <summary>
    /// Gets or sets the X offset.
    /// </summary>
    public double OffsetX
    {
        get => _offsetX;
        set => SetAndNotify(ref _offsetX, value);
    }

    /// <summary>
    /// Gets or sets the Y offset.
    /// </summary>
    public double OffsetY
    {
        get => _offsetY;
        set => SetAndNotify(ref _offsetY, value);
    }

    /// <summary>
    /// Gets or sets the shutter mode.
    /// </summary>
    public string Shutter
    {
        get => _shutter;
        set => SetAndNotify(ref _shutter, value);
    }

    /// <summary>
    /// Creates a scan settings snapshot.
    /// </summary>
    public ScanSettings ToSettings()
    {
        return new ScanSettings(Zoom, Width, Height, DwellTimeUs, Captures, Average, ScanMode, LineSpeedHz, OffsetX, OffsetY, Shutter);
    }

    private void SetAndNotify<T>(ref T field, T value)
    {
        if (SetProperty(ref field, value))
        {
            RaisePropertyChanged(nameof(PixelSizeText));
            RaisePropertyChanged(nameof(ImageSizeText));
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets the simulated pixel size.
    /// </summary>
    public string PixelSizeText => $"{(86.42 / Math.Max(1, Width)):0.00} um";

    /// <summary>
    /// Gets the simulated image size.
    /// </summary>
    public string ImageSizeText => $"{86.42:0.00} um";
}

