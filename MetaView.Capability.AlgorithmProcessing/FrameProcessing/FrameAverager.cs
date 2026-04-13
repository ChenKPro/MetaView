
namespace MetaView.Capabilities.Algorithms.FrameProcessing;

/// <summary>
/// Provides sample-wise averaging for raw DAQ frames.
/// </summary>
public static class FrameAverager
{
    /// <summary>
    /// Averages equal-length raw DAQ frames sample by sample.
    /// </summary>
    /// <param name="frames">Frames to average.</param>
    /// <returns>A raw frame containing the averaged samples.</returns>
    /// <exception cref="ArgumentException">Thrown when no frames are provided or frame lengths differ.</exception>
    public static DaqRawFrame Average(IReadOnlyList<DaqRawFrame> frames)
    {
        if (frames.Count == 0)
        {
            throw new ArgumentException("At least one frame is required.", nameof(frames));
        }

        var sampleLength = frames[0].Samples.Count;
        if (frames.Any(frame => frame.Samples.Count != sampleLength))
        {
            throw new ArgumentException("All frames must have the same sample length.", nameof(frames));
        }

        var averagedSamples = new double[sampleLength];
        foreach (var frame in frames)
        {
            for (var index = 0; index < sampleLength; index++)
            {
                averagedSamples[index] += frame.Samples[index];
            }
        }

        for (var index = 0; index < sampleLength; index++)
        {
            averagedSamples[index] /= frames.Count;
        }

        return new DaqRawFrame(frames[0].FrameIndex, averagedSamples);
    }
}

