namespace MetaView.Core.Experiments;

/// <summary>
/// Describes the platform capabilities required to execute a recipe or one modality in a recipe.
/// </summary>
public sealed record CapabilityPlan(
    IReadOnlyList<ExperimentCapability> Required,
    bool InitializeBeforeRun = true,
    bool StopAfterRun = true,
    IReadOnlyDictionary<string, string>? Parameters = null)
{
    /// <summary>
    /// Gets an empty capability plan.
    /// </summary>
    public static CapabilityPlan Empty { get; } = new([]);

    /// <summary>
    /// Returns true when the capability is required by this plan.
    /// </summary>
    public bool Requires(ExperimentCapability capability)
    {
        return Required.Contains(capability);
    }
}
