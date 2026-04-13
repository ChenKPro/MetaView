namespace MetaView.Presentation.Core;

/// <summary>
/// Defines Prism-style region names used by the shell layout.
/// </summary>
public static class RegionNames
{
    /// <summary>
    /// Hosts the menu, task summary, and instrument readiness status.
    /// </summary>
    public const string TopBarRegion = nameof(TopBarRegion);

    /// <summary>
    /// Hosts workflow navigation, task lists, and runtime logs.
    /// </summary>
    public const string NavigationRegion = nameof(NavigationRegion);

    /// <summary>
    /// Hosts the image, ROI, graph, histogram, and BBO workspaces.
    /// </summary>
    public const string WorkspaceRegion = nameof(WorkspaceRegion);

    /// <summary>
    /// Hosts stage, laser, light-path, and advanced hardware controls.
    /// </summary>
    public const string HardwareRegion = nameof(HardwareRegion);

    /// <summary>
    /// Hosts acquisition setup, modality, region, save, and run controls.
    /// </summary>
    public const string AcquisitionRegion = nameof(AcquisitionRegion);

    /// <summary>
    /// Hosts runtime state, warnings, and progress messages.
    /// </summary>
    public const string StatusRegion = nameof(StatusRegion);

    /// <summary>
    /// Hosts modal workflows such as calibration and multi-position acquisition.
    /// </summary>
    public const string DialogRegion = nameof(DialogRegion);
}

