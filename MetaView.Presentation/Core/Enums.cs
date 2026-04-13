namespace MetaView.Presentation.Core;

/// <summary>
/// Identifies the active acquisition state.
/// </summary>
public enum AcquisitionState
{
    Idle,
    Ready,
    LivePreview,
    Capturing,
    Acquiring,
    Aborting,
    Error,
    EmergencyStopped
}

/// <summary>
/// Identifies supported imaging modalities.
/// </summary>
public enum ImagingModality
{
    Srs,
    Tpef,
    Dc,
    Multimodal
}

/// <summary>
/// Identifies the scan motion mode.
/// </summary>
public enum ScanMode
{
    OneWay,
    RoundTrip
}

/// <summary>
/// Identifies stage axes.
/// </summary>
public enum Axis
{
    X,
    Y,
    Z
}

