namespace MetaView.Core.MotionControl;

/// <summary>
/// Maps a MetaView logical axis to a physical controller axis.
/// </summary>
public sealed record MotionAxisBinding
{
    /// <summary>
    /// Gets the logical axis consumed by UI and workflows.
    /// </summary>
    public MotionAxis Axis { get; init; }

    /// <summary>
    /// Gets the physical controller id.
    /// </summary>
    public string ControllerId { get; init; } = "Stage";

    /// <summary>
    /// Gets the physical zero-based axis index on the controller.
    /// </summary>
    public int PhysicalAxisIndex { get; init; }

    /// <summary>
    /// Gets a human-readable function name from the system distribution diagram.
    /// </summary>
    public string Function { get; init; } = string.Empty;
}
