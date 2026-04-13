using MetaView.Core.DataAcquisition;
using MetaView.Core.Imaging.Signal;

namespace MetaView.Capability.DaqAndPreprocessing;

/// <summary>
/// Converts AI0/AI1 position samples and AI2/AI3 laser samples into a normalized grid image.
/// </summary>
public sealed class SignalImagingProcessor : ISignalImagingProcessor
{
    private const int TracePointCount = 240;

    /// <inheritdoc />
    public SignalImagingResult Process(DaqSamplePacket packet, ScanGridSettings settings)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(settings);

        if (packet.Samples.Count < 4)
        {
            throw new InvalidOperationException("Signal imaging requires AI0, AI1, AI2, and AI3 samples.");
        }

        var ai0 = packet.Samples[0];
        var ai1 = packet.Samples[1];
        var ai2 = packet.Samples[2];
        var ai3 = packet.Samples[3];
        var sampleCount = new[] { ai0.Length, ai1.Length, ai2.Length, ai3.Length }.Min();

        if (sampleCount == 0)
        {
            throw new InvalidOperationException("Signal imaging requires at least one sample.");
        }

        var imageFrame = BuildImageFrame(ai0, ai1, ai2, ai3, sampleCount, settings, packet.Timestamp);
        var traceFrame = new SignalTraceFrame(
            Downsample(ai0, sampleCount),
            Downsample(ai1, sampleCount),
            Downsample(ai2, sampleCount),
            Downsample(ai3, sampleCount),
            packet.Timestamp);

        return new SignalImagingResult(imageFrame, traceFrame);
    }

    private static SignalImageFrame BuildImageFrame(
        double[] xSamples,
        double[] ySamples,
        double[] ai2Samples,
        double[] ai3Samples,
        int sampleCount,
        ScanGridSettings settings,
        DateTimeOffset timestamp)
    {
        var width = Math.Max(1, settings.Width);
        var height = Math.Max(1, settings.Height);
        var cellCount = width * height;
        var sums = new double[cellCount];
        var counts = new int[cellCount];

        var xMin = xSamples.Take(sampleCount).Min();
        var xMax = xSamples.Take(sampleCount).Max();
        var yMin = ySamples.Take(sampleCount).Min();
        var yMax = ySamples.Take(sampleCount).Max();
        var xSpan = Math.Max(1e-9, xMax - xMin);
        var ySpan = Math.Max(1e-9, yMax - yMin);

        for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            var xIndex = ToGridIndex(xSamples[sampleIndex], xMin, xSpan, width);
            var yIndex = ToGridIndex(ySamples[sampleIndex], yMin, ySpan, height);
            var imageY = height - 1 - yIndex;
            var cellIndex = imageY * width + xIndex;

            sums[cellIndex] += (ai2Samples[sampleIndex] + ai3Samples[sampleIndex]) / 2.0;
            counts[cellIndex]++;
        }

        var values = new double[cellCount];
        var min = double.PositiveInfinity;
        var max = double.NegativeInfinity;

        for (var cellIndex = 0; cellIndex < cellCount; cellIndex++)
        {
            if (counts[cellIndex] == 0)
            {
                continue;
            }

            var value = sums[cellIndex] / counts[cellIndex];
            values[cellIndex] = value;
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        if (double.IsInfinity(min) || double.IsInfinity(max))
        {
            min = 0;
            max = 0;
        }

        var pixels = Normalize(values, counts, min, max);
        return new SignalImageFrame(width, height, pixels, min, max, timestamp);
    }

    private static int ToGridIndex(double value, double min, double span, int gridSize)
    {
        var normalized = (value - min) / span;
        var index = (int)Math.Floor(normalized * (gridSize - 1));
        return Math.Clamp(index, 0, gridSize - 1);
    }

    private static byte[] Normalize(double[] values, int[] counts, double min, double max)
    {
        var pixels = new byte[values.Length];
        var span = max - min;

        for (var index = 0; index < values.Length; index++)
        {
            if (counts[index] == 0)
            {
                pixels[index] = 0;
                continue;
            }

            pixels[index] = span <= 1e-12
                ? (byte)128
                : (byte)Math.Clamp((values[index] - min) / span * 255.0, 0, 255);
        }

        return pixels;
    }

    private static IReadOnlyList<double> Downsample(double[] samples, int sampleCount)
    {
        if (sampleCount <= TracePointCount)
        {
            return samples.Take(sampleCount).ToArray();
        }

        var result = new double[TracePointCount];
        for (var index = 0; index < TracePointCount; index++)
        {
            var sourceIndex = (int)Math.Round(index * (sampleCount - 1.0) / (TracePointCount - 1));
            result[index] = samples[sourceIndex];
        }

        return result;
    }
}
