using MetaView.Core.Imaging.Brightfield;
using Prism.Events;
using Vibronix.Foundation.Common.Results;
using Vibronix.Foundation.Hardware.Camera;
using Vibronix.Foundation.Hardware.Camera.Common;
using FoundationPixelFormat = Vibronix.Foundation.Hardware.Camera.Common.CameraPixelFormat;
using FoundationTriggerMode = Vibronix.Foundation.Hardware.Camera.Common.TriggerMode;
using FoundationTriggerSource = Vibronix.Foundation.Hardware.Camera.Common.TriggerSource;
using FoundationCameraType = Vibronix.Foundation.Hardware.Camera.Common.CameraType;

namespace MetaView.Capability.ImageAcquisition;

/// <summary>
/// Adapts the Foundation camera SDK to MetaView brightfield acquisition contracts.
/// </summary>
public sealed class FoundationBrightfieldCameraCapability(IEventAggregator eventAggregator) : IBrightfieldCameraCapability, IDisposable
{
    private readonly object _syncRoot = new();
    private BrightfieldCameraSettings _settings = new();
    private ICamera? _camera;
    private Timer? _demoTimer;
    private BrightfieldCameraFrame? _lastFrame;
    private int _frameCount;
    private bool _isDemo = true;
    private bool _disposed;

    /// <inheritdoc />
    public OperationResult<BrightfieldCameraStatus> GetStatus()
    {
        return OperationResult<BrightfieldCameraStatus>.Ok(CreateStatus());
    }

    /// <inheritdoc />
    public OperationResult<IReadOnlyList<string>> GetAvailableCameraIds()
    {
        if (IsDemoCamera(_settings.CameraType))
        {
            return OperationResult<IReadOnlyList<string>>.Ok(["Demo-Brightfield"], "Demo camera is available.");
        }

        var camera = EnsureFoundationCamera();
        if (camera is null)
        {
            return OperationResult<IReadOnlyList<string>>.Error("Unsupported brightfield camera type.");
        }

        var result = camera.GetAvailableCameraList();
        return result.Success
            ? OperationResult<IReadOnlyList<string>>.Ok(result.Data ?? [], result.Message, result.ResultCode)
            : OperationResult<IReadOnlyList<string>>.Error(result.Message, result.ResultCode);
    }

    /// <inheritdoc />
    public async Task<OperationResult> InitializeAsync(BrightfieldCameraSettings settings, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _settings = settings;
        _isDemo = IsDemoCamera(settings.CameraType);
        _frameCount = 0;

        if (_isDemo)
        {
            return OperationResult.Ok("Demo brightfield camera initialized.");
        }

        var camera = EnsureFoundationCamera();
        if (camera is null)
        {
            return OperationResult.Error($"Unsupported brightfield camera type: {settings.CameraType}.");
        }

        var cameraId = settings.CameraId;
        if (string.IsNullOrWhiteSpace(cameraId))
        {
            var listResult = camera.GetAvailableCameraList();
            if (!listResult.Success)
            {
                return ToOperationResult(listResult);
            }

            cameraId = listResult.Data?.FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cameraId))
            {
                return OperationResult.Error("No Foundation camera was detected.");
            }
        }
         camera.GetAvailableCameraList();
        var connectResult = await camera.ConnectAsync(cameraId, cancellationToken).ConfigureAwait(false);
        if (!connectResult.Success)
        {
            return ToOperationResult(connectResult);
        }

        _settings = settings with { CameraId = cameraId };
        return await ApplySettingsAsync(_settings, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<OperationResult> ApplySettingsAsync(BrightfieldCameraSettings settings, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _settings = settings;
        _isDemo = IsDemoCamera(settings.CameraType);

        if (_isDemo)
        {
            return OperationResult.Ok("Demo brightfield camera settings applied.");
        }

        if (_camera is null || !_camera.IsConnected)
        {
            return OperationResult.Error("Brightfield camera is not connected.");
        }

        //var parameterResult = _camera.SetCameraParameters(new CameraParameters
        //{
        //    ExposureTime = settings.ExposureTime,
        //    Gain = settings.Gain,
        //    Gamma = settings.Gamma,
        //    FrameRate = settings.FrameRate
        //});
        //if (!parameterResult.Success)
        //{
        //    return ToOperationResult(parameterResult);
        //}

        var triggerResult = _camera.SetTriggerMode(settings.TriggerEnabled ? FoundationTriggerMode.ON : FoundationTriggerMode.OFF);
        if (!triggerResult.Success)
        {
            return ToOperationResult(triggerResult);
        }

        if (Enum.TryParse<FoundationTriggerSource>(settings.TriggerSource, ignoreCase: true, out var triggerSource))
        {
            var triggerSourceResult = _camera.SetTriggerSource(triggerSource);
            if (!triggerSourceResult.Success)
            {
                return ToOperationResult(triggerSourceResult);
            }
        }

        if (settings.RoiWidth > 0 && settings.RoiHeight > 0)
        {
            var roiResult = await _camera
                .SetRegionOfInterest(new ICamera.RoiConfiguration(settings.RoiOffsetX, settings.RoiOffsetY, settings.RoiWidth, settings.RoiHeight))
                .ConfigureAwait(false);
            if (!roiResult.Success)
            {
                return ToOperationResult(roiResult);
            }
        }

        return OperationResult.Ok("Brightfield camera settings applied.");
    }

    /// <inheritdoc />
    public async Task<OperationResult> StartLiveAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_isDemo)
        {
            StartDemoTimer();
            return OperationResult.Ok("Demo brightfield live acquisition started.");
        }

        if (_camera is null || !_camera.IsConnected)
        {
            var initializeResult = await InitializeAsync(_settings, cancellationToken).ConfigureAwait(false);
            if (!initializeResult.Success)
            {
                return initializeResult;
            }
        }

        return ToOperationResult(await _camera!.StartGrabbingAsync().ConfigureAwait(false));
    }

    /// <inheritdoc />
    public Task<OperationResult> StopLiveAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        if (_isDemo)
        {
            StopDemoTimer();
            return Task.FromResult(OperationResult.Ok("Demo brightfield live acquisition stopped."));
        }

        if (_camera is null || !_camera.IsGrabbing)
        {
            return Task.FromResult(OperationResult.Ok("Brightfield camera is already stopped."));
        }

        return Task.FromResult(ToOperationResult(_camera.StopGrabbing()));
    }

    /// <inheritdoc />
    public async Task<OperationResult<BrightfieldCameraFrame>> CaptureSingleAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        if (_isDemo)
        {
            var demoFrame = CreateDemoFrame();
            PublishFrame(demoFrame);
            return OperationResult<BrightfieldCameraFrame>.Ok(demoFrame, "Demo brightfield frame captured.");
        }

        if (_camera is null || !_camera.IsConnected)
        {
            var initializeResult = await InitializeAsync(_settings, cancellationToken).ConfigureAwait(false);
            if (!initializeResult.Success)
            {
                return OperationResult<BrightfieldCameraFrame>.Error(initializeResult.Message, initializeResult.ResultCode);
            }
        }

        var result = _camera!.GetCurrentFrame();
        if (!result.Success || result.Data is null)
        {
            return _lastFrame is not null
                ? OperationResult<BrightfieldCameraFrame>.Ok(_lastFrame, "Latest brightfield frame returned.")
                : OperationResult<BrightfieldCameraFrame>.Error(result.Message, result.ResultCode);
        }

        if (_lastFrame is not null && _lastFrame.Pixels.Length == result.Data.Length)
        {
            return OperationResult<BrightfieldCameraFrame>.Ok(_lastFrame with { Pixels = result.Data, Timestamp = DateTimeOffset.Now }, result.Message, result.ResultCode);
        }

        if (_settings.RoiWidth <= 0 || _settings.RoiHeight <= 0)
        {
            return OperationResult<BrightfieldCameraFrame>.Error("No frame dimensions are available. Start live acquisition first or configure ROI width and height.");
        }

        var capturedFrame = new BrightfieldCameraFrame(
            _settings.CameraId,
            Math.Max(1, _settings.RoiWidth),
            Math.Max(1, _settings.RoiHeight),
            BrightfieldCameraPixelFormat.Unknown,
            result.Data,
            DateTimeOffset.Now);
        PublishFrame(capturedFrame);
        return OperationResult<BrightfieldCameraFrame>.Ok(capturedFrame, result.Message, result.ResultCode);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopDemoTimer();
        if (_camera is not null)
        {
            _camera.CameraCallbackReceived -= OnCameraCallbackReceived;
            _camera.Dispose();
            _camera = null;
        }
    }

    private ICamera? EnsureFoundationCamera()
    {
        if (_camera is not null)
        {
            return _camera;
        }

        if (!Enum.TryParse<FoundationCameraType>(_settings.CameraType, ignoreCase: true, out var cameraType))
        {
            return null;
        }

        _camera = CameraFactory.CreateCamera(cameraType);
        if (_camera is not null)
        {
            _camera.CameraCallbackReceived += OnCameraCallbackReceived;
        }

        return _camera;
    }

    private void OnCameraCallbackReceived(object? sender, CameraCallbackEventArgs e)
    {
        if (e.EventType is not CameraEventType.FrameArrived || e.Width <= 0 || e.Height <= 0)
        {
            return;
        }

        var pixels = e.ImageBuffer.ToArray();
        var frame = new BrightfieldCameraFrame(
            _settings.CameraId,
            e.Width,
            e.Height,
            MapPixelFormat(e.PixelFormat, e.Width, e.Height, pixels.Length),
            pixels,
            DateTimeOffset.Now);
        PublishFrame(frame);
    }

    private void StartDemoTimer()
    {
        lock (_syncRoot)
        {
            _demoTimer ??= new Timer(_ => PublishFrame(CreateDemoFrame()), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000.0 / GetDemoFrameRate()));
        }
    }

    private void StopDemoTimer()
    {
        lock (_syncRoot)
        {
            _demoTimer?.Dispose();
            _demoTimer = null;
        }
    }

    private BrightfieldCameraFrame CreateDemoFrame()
    {
        var width = _settings.RoiWidth > 0 ? _settings.RoiWidth : 640;
        var height = _settings.RoiHeight > 0 ? _settings.RoiHeight : 480;
        var frameIndex = Interlocked.Increment(ref _frameCount);
        var pixels = new byte[width * height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var vignette = 1.0 - 0.55 * Math.Min(1.0, DistanceFromCenter(x, y, width, height));
                var wave = 0.5 + 0.5 * Math.Sin((x + frameIndex * 7) * 0.028) * Math.Cos((y - frameIndex * 3) * 0.021);
                var value = (byte)Math.Clamp(36 + 185 * vignette * (0.72 + wave * 0.28), 0, 255);
                pixels[y * width + x] = value;
            }
        }

        return new BrightfieldCameraFrame("Demo-Brightfield", width, height, BrightfieldCameraPixelFormat.Mono8, pixels, DateTimeOffset.Now);
    }

    private BrightfieldCameraStatus CreateStatus()
    {
        var connected = _isDemo || _camera?.IsConnected == true;
        var grabbing = _demoTimer is not null || _camera?.IsGrabbing == true;
        return new BrightfieldCameraStatus(
            _settings.CameraType,
            _settings.CameraId,
            connected,
            grabbing,
            _frameCount,
            connected ? (grabbing ? "Live acquisition active" : "Camera connected") : "Camera disconnected");
    }

    private void PublishFrame(BrightfieldCameraFrame frame)
    {
        _lastFrame = frame;
        Interlocked.Increment(ref _frameCount);
        eventAggregator.GetEvent<BrightfieldCameraFramePublishedEvent>().Publish(frame);
    }

    private int GetDemoFrameRate()
    {
        return (int)Math.Clamp(_settings.FrameRate ?? 10, 1, 30);
    }

    private static bool IsDemoCamera(string cameraType)
    {
        return string.IsNullOrWhiteSpace(cameraType) || string.Equals(cameraType, "Demo", StringComparison.OrdinalIgnoreCase);
    }

    private static double DistanceFromCenter(int x, int y, int width, int height)
    {
        var nx = (x - width / 2.0) / Math.Max(1.0, width / 2.0);
        var ny = (y - height / 2.0) / Math.Max(1.0, height / 2.0);
        return Math.Sqrt(nx * nx + ny * ny);
    }

    private static BrightfieldCameraPixelFormat MapPixelFormat(FoundationPixelFormat pixelFormat, int width, int height, int byteCount)
    {
        return pixelFormat switch
        {
            FoundationPixelFormat.Mono8 => BrightfieldCameraPixelFormat.Mono8,
            FoundationPixelFormat.Mono16 => BrightfieldCameraPixelFormat.Mono16,
            FoundationPixelFormat.Bgr24 => BrightfieldCameraPixelFormat.Bgr24,
            FoundationPixelFormat.Rgb24 => BrightfieldCameraPixelFormat.Rgb24,
            FoundationPixelFormat.Bgra32 => BrightfieldCameraPixelFormat.Bgra32,
            FoundationPixelFormat.BayerRG8 => BrightfieldCameraPixelFormat.BayerRG8,
            FoundationPixelFormat.BayerGB8 => BrightfieldCameraPixelFormat.BayerGB8,
            FoundationPixelFormat.BayerGR8 => BrightfieldCameraPixelFormat.BayerGR8,
            FoundationPixelFormat.BayerBG8 => BrightfieldCameraPixelFormat.BayerBG8,
            _ when byteCount == width * height => BrightfieldCameraPixelFormat.Mono8,
            _ when byteCount == width * height * 2 => BrightfieldCameraPixelFormat.Mono16,
            _ when byteCount == width * height * 3 => BrightfieldCameraPixelFormat.Bgr24,
            _ when byteCount == width * height * 4 => BrightfieldCameraPixelFormat.Bgra32,
            _ => BrightfieldCameraPixelFormat.Unknown
        };
    }

    private static OperationResult ToOperationResult(OperationResult result)
    {
        return result.Success ? OperationResult.Ok(result.Message, result.ResultCode) : OperationResult.Error(result.Message, result.ResultCode);
    }

    private static OperationResult ToOperationResult<T>(OperationResult<T> result)
    {
        return result.Success ? OperationResult.Ok(result.Message, result.ResultCode) : OperationResult.Error(result.Message, result.ResultCode);
    }
}
