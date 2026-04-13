namespace MetaView.Core.Experiments;

/// <summary>
/// Defines the imaging or spectroscopy technique used by an experiment.
/// </summary>
public enum ImagingModality
{
    /// <summary>
    /// Stimulated Raman scattering.
    /// </summary>
    Srs,

    /// <summary>
    /// Two-photon excited fluorescence.
    /// </summary>
    Tpef,

    /// <summary>
    /// Coherent anti-Stokes Raman scattering.
    /// </summary>
    Cars,

    /// <summary>
    /// Differential contrast or direct-current style signal acquisition.
    /// </summary>
    Dc,

    /// <summary>
    /// Wide-field or point-scanned brightfield imaging.
    /// </summary>
    Brightfield,

    /// <summary>
    /// Fluorescence imaging.
    /// </summary>
    Fluorescence,

    /// <summary>
    /// Multiple modalities executed in one recipe.
    /// </summary>
    Multimodal
}
