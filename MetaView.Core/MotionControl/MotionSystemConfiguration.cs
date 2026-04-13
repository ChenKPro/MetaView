namespace MetaView.Core.MotionControl;

/// <summary>
/// Describes the complete multi-controller motion system.
/// </summary>
public sealed record MotionSystemConfiguration
{
    /// <summary>
    /// Gets a value indicating whether the platform should use demo motion.
    /// </summary>
    public bool UseDemo { get; init; } = true;

    /// <summary>
    /// Gets the configured physical motion controller endpoints.
    /// </summary>
    public IReadOnlyList<MotionControllerEndpoint> Controllers { get; init; } = [];

    /// <summary>
    /// Gets logical-to-physical axis bindings.
    /// </summary>
    public IReadOnlyList<MotionAxisBinding> AxisBindings { get; init; } = [];
}
