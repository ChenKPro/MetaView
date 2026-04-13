namespace MetaView.Core.Experiments;

/// <summary>
/// Identifies one platform capability required by an experiment recipe or modality.
/// </summary>
public enum ExperimentCapability
{
    /// <summary>
    /// Logical stage or scanner motion.
    /// </summary>
    Motion,

    /// <summary>
    /// Data acquisition hardware.
    /// </summary>
    DataAcquisition,

    /// <summary>
    /// Realtime signal-to-image processing.
    /// </summary>
    SignalImaging,

    /// <summary>
    /// Brightfield camera acquisition.
    /// </summary>
    BrightfieldCamera,

    /// <summary>
    /// Laser source control.
    /// </summary>
    Laser,

    /// <summary>
    /// Photo-detector acquisition.
    /// </summary>
    PhotoDetection,

    /// <summary>
    /// Algorithm processing.
    /// </summary>
    Algorithm,

    /// <summary>
    /// Report generation.
    /// </summary>
    Reporting
}
