using MetaView.Capability.DaqAndPreprocessing.GalvoScan;
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
        return Process(packet, settings, new GalvoScanRuntimeConfiguration { Enabled = false });
    }

    /// <inheritdoc />
    public SignalImagingResult Process(
        DaqSamplePacket packet,
        ScanGridSettings settings,
        GalvoScanRuntimeConfiguration galvoSettings)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(galvoSettings);

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

        var detector = CombineDetector(ai2, ai3, sampleCount);
        var imageFrame = galvoSettings.Enabled
            ? BuildGalvoImageFrame(ai0, ai1, detector, sampleCount, galvoSettings, packet.Timestamp)
            : BuildPositionGridImageFrame(ai0, ai1, detector, sampleCount, settings, packet.Timestamp);
        var traceFrame = new SignalTraceFrame(
            Downsample(ai0, sampleCount),
            Downsample(ai1, sampleCount),
            Downsample(ai2, sampleCount),
            Downsample(ai3, sampleCount),
            packet.Timestamp);

        return new SignalImagingResult(imageFrame, traceFrame);
    }

    private static SignalImageFrame BuildGalvoImageFrame(
        double[] xSamples,
        double[] ySamples,
        double[] detectorSamples,
        int sampleCount,
        GalvoScanRuntimeConfiguration settings,
        DateTimeOffset timestamp)
    {
        var waveformSettings = CreateSingleFrameWaveformSettings(settings);
        var waveform = GalvoScanWaveformGenerator.Generate(waveformSettings);
        var sums = new double[settings.ImageHeight, settings.ImageWidth];
        var counts = new double[settings.ImageHeight, settings.ImageWidth];

        switch (settings.ScanMode)
        {
            case GalvoScanRuntimeMode.FeedbackResample:
                AccumulateByPositionFeedback(settings, waveform, detectorSamples, xSamples, ySamples, sampleCount, sums, counts);
                break;
            case GalvoScanRuntimeMode.XFeedbackRaster:
                AccumulateByXFeedbackRaster(settings, waveform, detectorSamples, xSamples, sampleCount, sums, counts);
                break;
            default:
                AccumulateByPlannedRaster(settings, waveform, detectorSamples, sampleCount, sums, counts);
                break;
        }

        return CreateImageFrame(sums, counts, timestamp);
    }

    private static SignalImageFrame BuildPositionGridImageFrame(
        double[] xSamples,
        double[] ySamples,
        double[] detectorSamples,
        int sampleCount,
        ScanGridSettings settings,
        DateTimeOffset timestamp)
    {
        var width = Math.Max(1, settings.Width);
        var height = Math.Max(1, settings.Height);
        var sums = new double[height, width];
        var counts = new double[height, width];
        var xMin = xSamples.Take(sampleCount).Min();
        var xMax = xSamples.Take(sampleCount).Max();
        var yMin = ySamples.Take(sampleCount).Min();
        var yMax = ySamples.Take(sampleCount).Max();

        for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            AccumulateNearest(
                sums,
                counts,
                detectorSamples[sampleIndex],
                VoltageToContinuousIndex(xSamples[sampleIndex], xMin, xMax, width),
                VoltageToContinuousIndex(yMax - ySamples[sampleIndex] + yMin, yMin, yMax, height));
        }

        return CreateImageFrame(sums, counts, timestamp);
    }

    private static void AccumulateByPlannedRaster(
        GalvoScanRuntimeConfiguration settings,
        GalvoScanWaveform waveform,
        IReadOnlyList<double> detector,
        int availableSampleCount,
        double[,] sums,
        double[,] counts)
    {
        foreach (var line in waveform.Lines)
        {
            var lineStart = line.ActiveStartSampleIndex;
            if (line.IsReverse)
            {
                lineStart += settings.BidirectionalPhaseSamples;
            }

            var visibleStartOffset = checked(settings.XExtraPixels * settings.SamplesPerPixel);
            for (var pixelIndex = 0; pixelIndex < settings.ImageWidth; pixelIndex++)
            {
                var imageColumn = line.IsReverse ? settings.ImageWidth - 1 - pixelIndex : pixelIndex;
                var sampleStart = lineStart + visibleStartOffset + pixelIndex * settings.SamplesPerPixel;

                for (var sampleOffset = 0; sampleOffset < settings.SamplesPerPixel; sampleOffset++)
                {
                    var sampleIndex = sampleStart + sampleOffset + settings.DetectorSampleOffsetSamples;
                    if (sampleIndex < 0 || sampleIndex >= availableSampleCount)
                    {
                        continue;
                    }

                    sums[line.RowIndex, imageColumn] += detector[sampleIndex];
                    counts[line.RowIndex, imageColumn]++;
                }
            }
        }
    }

    private static void AccumulateByPositionFeedback(
        GalvoScanRuntimeConfiguration settings,
        GalvoScanWaveform waveform,
        IReadOnlyList<double> detector,
        IReadOnlyList<double> positionX,
        IReadOnlyList<double> positionY,
        int availableSampleCount,
        double[,] sums,
        double[,] counts)
    {
        var xMin = settings.CenterXVoltage - settings.AmplitudeXVoltage;
        var xMax = settings.CenterXVoltage + settings.AmplitudeXVoltage;
        var yMin = settings.CenterYVoltage - settings.AmplitudeYVoltage;
        var yMax = settings.CenterYVoltage + settings.AmplitudeYVoltage;

        foreach (var line in waveform.Lines)
        {
            var startSample = Math.Max(line.ActiveStartSampleIndex, 0);
            var endSample = Math.Min(line.ActiveStartSampleIndex + line.ActiveSampleCount, availableSampleCount);
            for (var sampleIndex = startSample; sampleIndex < endSample; sampleIndex++)
            {
                var x = positionX[sampleIndex] * settings.XFeedbackScale;
                var y = positionY[sampleIndex] * settings.YFeedbackScale;
                if (x < xMin || x > xMax || y < yMin || y > yMax)
                {
                    continue;
                }

                var detectorIndex = sampleIndex + settings.DetectorSampleOffsetSamples;
                if (detectorIndex < 0 || detectorIndex >= detector.Count)
                {
                    continue;
                }

                AccumulateBilinear(
                    sums,
                    counts,
                    detector[detectorIndex],
                    VoltageToContinuousIndex(x, xMin, xMax, settings.ImageWidth),
                    VoltageToContinuousIndex(yMax - y + yMin, yMin, yMax, settings.ImageHeight));
            }
        }
    }

    private static void AccumulateByXFeedbackRaster(
        GalvoScanRuntimeConfiguration settings,
        GalvoScanWaveform waveform,
        IReadOnlyList<double> detector,
        IReadOnlyList<double> positionX,
        int availableSampleCount,
        double[,] sums,
        double[,] counts)
    {
        var xMin = settings.CenterXVoltage - settings.AmplitudeXVoltage;
        var xMax = settings.CenterXVoltage + settings.AmplitudeXVoltage;

        foreach (var line in waveform.Lines)
        {
            var startSample = FindXFeedbackLineStart(settings, waveform, line, positionX, availableSampleCount);
            var endSample = Math.Min(startSample + line.ActiveSampleCount, availableSampleCount);
            for (var sampleIndex = startSample; sampleIndex < endSample; sampleIndex++)
            {
                var x = positionX[sampleIndex] * settings.XFeedbackScale;
                if (x < xMin || x > xMax)
                {
                    continue;
                }

                var detectorIndex = sampleIndex + settings.DetectorSampleOffsetSamples;
                if (detectorIndex < 0 || detectorIndex >= detector.Count)
                {
                    continue;
                }

                AccumulateBilinear(
                    sums,
                    counts,
                    detector[detectorIndex],
                    VoltageToContinuousIndex(x, xMin, xMax, settings.ImageWidth),
                    line.RowIndex);
            }
        }
    }

    private static int FindXFeedbackLineStart(
        GalvoScanRuntimeConfiguration settings,
        GalvoScanWaveform waveform,
        GalvoScanLineDescriptor line,
        IReadOnlyList<double> positionX,
        int availableSampleCount)
    {
        var searchRadius = Math.Max(waveform.TurnSampleCount, settings.SamplesPerPixel);
        var searchStart = Math.Max(0, line.ActiveStartSampleIndex - searchRadius);
        var searchEnd = Math.Min(availableSampleCount - 1, line.ActiveStartSampleIndex + searchRadius);

        if (searchStart > searchEnd)
        {
            return Math.Clamp(line.ActiveStartSampleIndex, 0, Math.Max(availableSampleCount - 1, 0));
        }

        var plannedStart = Math.Clamp(line.ActiveStartSampleIndex, searchStart, searchEnd);
        var bestSample = plannedStart;
        var bestValue = positionX[bestSample] * settings.XFeedbackScale;

        for (var sampleIndex = searchStart; sampleIndex <= searchEnd; sampleIndex++)
        {
            var value = positionX[sampleIndex] * settings.XFeedbackScale;
            var isBetter = line.IsReverse
                ? value > bestValue || (value.Equals(bestValue) && Math.Abs(sampleIndex - plannedStart) < Math.Abs(bestSample - plannedStart))
                : value < bestValue || (value.Equals(bestValue) && Math.Abs(sampleIndex - plannedStart) < Math.Abs(bestSample - plannedStart));

            if (!isBetter)
            {
                continue;
            }

            bestSample = sampleIndex;
            bestValue = value;
        }

        return bestSample;
    }

    private static void AccumulateNearest(double[,] sums, double[,] counts, double value, double column, double row)
    {
        var targetRow = (int)Math.Round(row);
        var targetColumn = (int)Math.Round(column);
        if (targetRow < 0 || targetRow >= sums.GetLength(0) || targetColumn < 0 || targetColumn >= sums.GetLength(1))
        {
            return;
        }

        sums[targetRow, targetColumn] += value;
        counts[targetRow, targetColumn]++;
    }

    private static void AccumulateBilinear(double[,] sums, double[,] counts, double value, double column, double row)
    {
        var height = sums.GetLength(0);
        var width = sums.GetLength(1);
        var left = (int)Math.Floor(column);
        var top = (int)Math.Floor(row);
        var xWeight = column - left;
        var yWeight = row - top;

        AccumulateWeighted(sums, counts, top, left, value, (1d - xWeight) * (1d - yWeight), height, width);
        AccumulateWeighted(sums, counts, top, left + 1, value, xWeight * (1d - yWeight), height, width);
        AccumulateWeighted(sums, counts, top + 1, left, value, (1d - xWeight) * yWeight, height, width);
        AccumulateWeighted(sums, counts, top + 1, left + 1, value, xWeight * yWeight, height, width);
    }

    private static void AccumulateWeighted(
        double[,] sums,
        double[,] counts,
        int row,
        int column,
        double value,
        double weight,
        int height,
        int width)
    {
        if (weight <= 0 || row < 0 || row >= height || column < 0 || column >= width)
        {
            return;
        }

        sums[row, column] += value * weight;
        counts[row, column] += weight;
    }

    private static SignalImageFrame CreateImageFrame(double[,] sums, double[,] counts, DateTimeOffset timestamp)
    {
        var height = sums.GetLength(0);
        var width = sums.GetLength(1);
        var values = new double[width * height];
        var hitCounts = new int[values.Length];
        var min = double.PositiveInfinity;
        var max = double.NegativeInfinity;

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var index = row * width + column;
                if (counts[row, column] <= 0)
                {
                    continue;
                }

                var value = sums[row, column] / counts[row, column];
                values[index] = value;
                hitCounts[index] = 1;
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        if (double.IsInfinity(min) || double.IsInfinity(max))
        {
            min = 0;
            max = 0;
        }

        return new SignalImageFrame(width, height, Normalize(values, hitCounts, min, max), min, max, timestamp);
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

    private static double VoltageToContinuousIndex(double voltage, double minimum, double maximum, int count)
    {
        if (count <= 1 || Math.Abs(maximum - minimum) <= double.Epsilon)
        {
            return 0;
        }

        var ratio = (voltage - minimum) / (maximum - minimum);
        return Math.Clamp(ratio * (count - 1), 0d, count - 1d);
    }

    private static double[] CombineDetector(double[] ai2, double[] ai3, int sampleCount)
    {
        var detector = new double[sampleCount];
        for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            detector[sampleIndex] = (ai2[sampleIndex] + ai3[sampleIndex]) / 2d;
        }

        return detector;
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

    private static GalvoScanRuntimeConfiguration CreateSingleFrameWaveformSettings(GalvoScanRuntimeConfiguration settings)
    {
        return new GalvoScanRuntimeConfiguration
        {
            Enabled = settings.Enabled,
            DaqType = settings.DaqType,
            DeviceName = settings.DeviceName,
            AnalogOutputXChannel = settings.AnalogOutputXChannel,
            AnalogOutputYChannel = settings.AnalogOutputYChannel,
            PositionXInputChannel = settings.PositionXInputChannel,
            PositionYInputChannel = settings.PositionYInputChannel,
            SignalAInputChannel = settings.SignalAInputChannel,
            SignalBInputChannel = settings.SignalBInputChannel,
            InputTerminalConfiguration = settings.InputTerminalConfiguration,
            AnalogOutputClockSource = settings.AnalogOutputClockSource,
            AnalogOutputStartTriggerSource = settings.AnalogOutputStartTriggerSource,
            ScanMode = settings.ScanMode,
            ImageWidth = settings.ImageWidth,
            ImageHeight = settings.ImageHeight,
            XExtraPixels = settings.XExtraPixels,
            FrameCount = 1,
            SampleRate = settings.SampleRate,
            SamplesPerPixel = settings.SamplesPerPixel,
            CenterXVoltage = settings.CenterXVoltage,
            CenterYVoltage = settings.CenterYVoltage,
            AmplitudeXVoltage = settings.AmplitudeXVoltage,
            AmplitudeYVoltage = settings.AmplitudeYVoltage,
            XFeedbackScale = settings.XFeedbackScale,
            YFeedbackScale = settings.YFeedbackScale,
            VoltageMinimum = settings.VoltageMinimum,
            VoltageMaximum = settings.VoltageMaximum,
            FillFraction = settings.FillFraction,
            RetraceRatio = settings.RetraceRatio,
            BidirectionalPhaseSamples = settings.BidirectionalPhaseSamples,
            DetectorSampleOffsetSamples = settings.DetectorSampleOffsetSamples,
            EnableSlewLimit = settings.EnableSlewLimit,
            MaxSlewRateVoltsPerSecond = settings.MaxSlewRateVoltsPerSecond,
            RampMilliseconds = settings.RampMilliseconds,
            Continuous = settings.Continuous
        };
    }
}
