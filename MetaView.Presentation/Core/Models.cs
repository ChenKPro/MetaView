namespace MetaView.Presentation.Core;

/// <summary>
/// Describes a stage position in micrometers.
/// </summary>
public sealed record StagePosition(double X, double Y, double Z);

/// <summary>
/// Describes scan settings used to generate an acquisition recipe.
/// </summary>
public sealed record ScanSettings(
    string Zoom,
    int Width,
    int Height,
    double DwellTimeUs,
    int Captures,
    int Average,
    ScanMode ScanMode,
    int LineSpeedHz,
    double OffsetX,
    double OffsetY,
    string Shutter);

/// <summary>
/// Describes a spectral acquisition window.
/// </summary>
public sealed class SpectralWindow
{
    /// <summary>
    /// Gets or sets the spectral window range.
    /// </summary>
    public string Window { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the central wavenumber.
    /// </summary>
    public int Central { get; set; }

    /// <summary>
    /// Gets or sets the pump wavelength.
    /// </summary>
    public int Pump { get; set; }

    /// <summary>
    /// Gets or sets the spectral region label.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the effective acquisition window.
    /// </summary>
    public int EffectiveWindow { get; set; }

    /// <summary>
    /// Gets or sets the wavenumber step.
    /// </summary>
    public int Step { get; set; }

    /// <summary>
    /// Gets or sets the BBO offset.
    /// </summary>
    public int BboOffset { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the window is selected.
    /// </summary>
    public bool IsSelected { get; set; }
}

/// <summary>
/// Describes the full acquisition recipe composed by the UI.
/// </summary>
public sealed record AcquisitionRecipe(
    ScanSettings Scan,
    ImagingModality Modality,
    string SavePath,
    string SaveName,
    bool AutoSave,
    bool UseModal,
    bool UseLargeArea,
    bool Use3D,
    bool UseTimeLapse);

/// <summary>
/// Reports progress for an acquisition task.
/// </summary>
public sealed record AcquisitionProgress(int CurrentFrame, int TotalFrames, string Message, TimeSpan Elapsed);

