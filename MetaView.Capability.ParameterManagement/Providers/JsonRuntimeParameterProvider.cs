using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MetaView.Capability.ParameterManagement.Defaults;
using MetaView.Capability.ParameterManagement.Sources;
using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging;
using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.Laser;
using MetaView.Core.MotionControl;
using MetaView.Core.Parameters;
using Vibronix.Foundation.Common.Results;

namespace MetaView.Capability.ParameterManagement.Providers;

/// <summary>
/// Provides runtime parameters from a JSON device configuration file with environment defaults as fallback.
/// </summary>
public sealed class JsonRuntimeParameterProvider : IRuntimeParameterProvider
{
    private readonly object _syncRoot = new();
    private readonly EnvironmentParameterReader _reader = new();
    private readonly string _configurationPath;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private DeviceConfigurationDocument? _document;
    private bool _loaded;
    private BrightfieldCameraSettings? _brightfieldCameraSettings;
    private MotionSystemConfiguration? _motionSystemConfiguration;
    private DaqRuntimeConfiguration? _daqRuntimeConfiguration;
    private LaserRuntimeSettings? _laserRuntimeSettings;
    private ImageStageNavigationSettings? _imageStageNavigationSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRuntimeParameterProvider" /> class.
    /// </summary>
    public JsonRuntimeParameterProvider(string configurationPath)
    {
        _configurationPath = ResolveConfigurationPath(configurationPath);
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <inheritdoc />
    public OperationResult<BrightfieldCameraSettings> GetBrightfieldCameraSettings()
    {
        lock (_syncRoot)
        {
            LoadDocumentIfNeeded();
            _brightfieldCameraSettings ??= _document?.BrightfieldCamera ?? BrightfieldCameraParameterDefaults.Create(_reader);
            return OperationResult<BrightfieldCameraSettings>.Ok(_brightfieldCameraSettings, "Brightfield camera settings loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetBrightfieldCameraSettings(BrightfieldCameraSettings settings)
    {
        lock (_syncRoot)
        {
            _brightfieldCameraSettings = settings;
        }

        return OperationResult.Ok("Brightfield camera settings updated.");
    }

    /// <inheritdoc />
    public OperationResult<MotionSystemConfiguration> GetMotionSystemConfiguration()
    {
        lock (_syncRoot)
        {
            LoadDocumentIfNeeded();
            _motionSystemConfiguration ??= _document?.MotionSystem ?? MotionParameterDefaults.CreateSystem(_reader);
            return OperationResult<MotionSystemConfiguration>.Ok(_motionSystemConfiguration, "Motion system configuration loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetMotionSystemConfiguration(MotionSystemConfiguration configuration)
    {
        lock (_syncRoot)
        {
            _motionSystemConfiguration = configuration;
        }

        return OperationResult.Ok("Motion system configuration updated.");
    }

    /// <inheritdoc />
    public OperationResult<DaqRuntimeConfiguration> GetDaqRuntimeConfiguration()
    {
        lock (_syncRoot)
        {
            LoadDocumentIfNeeded();
            _daqRuntimeConfiguration ??= _document?.Daq ?? DaqParameterDefaults.Create(_reader);
            return OperationResult<DaqRuntimeConfiguration>.Ok(_daqRuntimeConfiguration, "DAQ runtime configuration loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetDaqRuntimeConfiguration(DaqRuntimeConfiguration configuration)
    {
        lock (_syncRoot)
        {
            _daqRuntimeConfiguration = configuration;
        }

        return OperationResult.Ok("DAQ runtime configuration updated.");
    }

    /// <inheritdoc />
    public OperationResult<LaserRuntimeSettings> GetLaserRuntimeSettings()
    {
        lock (_syncRoot)
        {
            LoadDocumentIfNeeded();
            _laserRuntimeSettings ??= _document?.Laser ?? LaserParameterDefaults.Create(_reader);
            return OperationResult<LaserRuntimeSettings>.Ok(_laserRuntimeSettings, "Laser runtime settings loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetLaserRuntimeSettings(LaserRuntimeSettings settings)
    {
        lock (_syncRoot)
        {
            _laserRuntimeSettings = settings;
        }

        return OperationResult.Ok("Laser runtime settings updated.");
    }

    /// <inheritdoc />
    public OperationResult<ImageStageNavigationSettings> GetImageStageNavigationSettings()
    {
        lock (_syncRoot)
        {
            LoadDocumentIfNeeded();
            _imageStageNavigationSettings ??= _document?.ImageStageNavigation ?? ImageStageNavigationParameterDefaults.Create(_reader);
            return OperationResult<ImageStageNavigationSettings>.Ok(_imageStageNavigationSettings, "Image-stage navigation settings loaded.");
        }
    }

    /// <inheritdoc />
    public OperationResult SetImageStageNavigationSettings(ImageStageNavigationSettings settings)
    {
        lock (_syncRoot)
        {
            _imageStageNavigationSettings = settings;
        }

        return OperationResult.Ok("Image-stage navigation settings updated.");
    }

    private void LoadDocumentIfNeeded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        if (!File.Exists(_configurationPath))
        {
            _document = new DeviceConfigurationDocument();
            return;
        }

        var json = File.ReadAllText(_configurationPath);
        _document = JsonSerializer.Deserialize<DeviceConfigurationDocument>(json, _jsonOptions)
            ?? new DeviceConfigurationDocument();
    }

    private static string ResolveConfigurationPath(string configurationPath)
    {
        if (Path.IsPathRooted(configurationPath))
        {
            return configurationPath;
        }

        var outputPath = Path.Combine(AppContext.BaseDirectory, configurationPath);
        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        return Path.Combine(Environment.CurrentDirectory, configurationPath);
    }
}
