using MetaView.Presentation.Core;

namespace MetaView.Presentation.Services;

/// <summary>
/// Starts and controls image acquisition tasks.
/// </summary>
public interface IAcquisitionService
{
    /// <summary>
    /// Runs a simulated live preview session.
    /// </summary>
    Task StartLiveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Captures a single simulated frame.
    /// </summary>
    Task CaptureSingleAsync(AcquisitionRecipe recipe, CancellationToken cancellationToken);

    /// <summary>
    /// Runs a simulated acquisition task and reports progress.
    /// </summary>
    Task RunTaskAsync(AcquisitionRecipe recipe, IProgress<AcquisitionProgress> progress, CancellationToken cancellationToken);
}

/// <summary>
/// Controls laser state used by the UI prototype.
/// </summary>
public interface ILaserService
{
    /// <summary>
    /// Warms up the laser and reports progress.
    /// </summary>
    Task WarmupAsync(IProgress<int> progress, CancellationToken cancellationToken);
}

