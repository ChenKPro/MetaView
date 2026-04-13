using System.Diagnostics;
using MetaView.Presentation.Core;

namespace MetaView.Presentation.Services;

/// <summary>
/// Provides simulated acquisition behavior for the UI prototype.
/// </summary>
public sealed class MockAcquisitionService : IAcquisitionService
{
    /// <inheritdoc />
    public async Task StartLiveAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(600, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CaptureSingleAsync(AcquisitionRecipe recipe, CancellationToken cancellationToken)
    {
        await Task.Delay(1400, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RunTaskAsync(AcquisitionRecipe recipe, IProgress<AcquisitionProgress> progress, CancellationToken cancellationToken)
    {
        var total = Math.Max(1, recipe.Scan.Captures);
        var watch = Stopwatch.StartNew();

        for (var frame = 1; frame <= total; frame++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(new AcquisitionProgress(frame, total, $"Acquiring frame {frame} of {total}", watch.Elapsed));
            await Task.Delay(recipe.Scan.Width >= 800 ? 1200 : 800, cancellationToken).ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Provides simulated laser warmup behavior for the UI prototype.
/// </summary>
public sealed class MockLaserService : ILaserService
{
    /// <inheritdoc />
    public async Task WarmupAsync(IProgress<int> progress, CancellationToken cancellationToken)
    {
        for (var value = 0; value <= 100; value += 10)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(value);
            await Task.Delay(120, cancellationToken).ConfigureAwait(false);
        }
    }
}

