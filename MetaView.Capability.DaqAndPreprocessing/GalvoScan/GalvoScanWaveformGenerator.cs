using MetaView.Core.DataAcquisition;

namespace MetaView.Capability.DaqAndPreprocessing.GalvoScan;

/// <summary>
/// Generates synchronized X/Y AO waveforms for Foundation DAQ galvo scans.
/// </summary>
internal static class GalvoScanWaveformGenerator
{
    public static GalvoScanWaveform Generate(GalvoScanRuntimeConfiguration settings)
    {
        Validate(settings);

        var xSamples = new List<double>();
        var ySamples = new List<double>();
        var lines = new List<GalvoScanLineDescriptor>();
        var forwardSamples = checked(settings.CommandLinePixelCount * settings.SamplesPerPixel);
        var turnSamples = CalculateTurnSampleCount(forwardSamples, settings);
        var startRampSamples = CalculateRampSampleCount(settings);
        var endRampSamples = startRampSamples;
        var firstX = settings.CenterXVoltage - settings.CommandAmplitudeXVoltage;
        var firstY = GetRowYVoltage(settings, 0);

        AppendLinearSegment(
            xSamples,
            ySamples,
            settings.CenterXVoltage,
            firstX,
            settings.CenterYVoltage,
            firstY,
            startRampSamples);

        for (var rowIndex = 0; rowIndex < settings.ImageHeight; rowIndex++)
        {
            var reverse = ShouldReverseRow(settings.ScanMode, rowIndex);
            var rowY = GetRowYVoltage(settings, rowIndex);
            var activeStart = xSamples.Count;

            AppendActiveLine(xSamples, ySamples, settings, rowY, forwardSamples, reverse);
            lines.Add(new GalvoScanLineDescriptor(0, rowIndex, activeStart, forwardSamples, reverse));
            AppendTurnSegment(xSamples, ySamples, settings, rowIndex, reverse, turnSamples);
        }

        AppendLinearSegment(
            xSamples,
            ySamples,
            xSamples[^1],
            settings.CenterXVoltage,
            ySamples[^1],
            settings.CenterYVoltage,
            endRampSamples);

        var xArray = xSamples.ToArray();
        var yArray = ySamples.ToArray();
        if (settings.EnableSlewLimit)
        {
            ValidateSlewRate(xArray, settings, "X");
            ValidateSlewRate(yArray, settings, "Y");
        }

        ValidateVoltageBounds(xArray, settings, "X");
        ValidateVoltageBounds(yArray, settings, "Y");

        return new GalvoScanWaveform(
            xArray,
            yArray,
            startRampSamples,
            endRampSamples,
            forwardSamples,
            turnSamples,
            lines);
    }

    private static void Validate(GalvoScanRuntimeConfiguration settings)
    {
        if (settings.ImageWidth < 2 || settings.ImageHeight < 2)
        {
            throw new InvalidOperationException("Galvo scan image width and height must be at least 2.");
        }

        if (settings.XExtraPixels < 0)
        {
            throw new InvalidOperationException("Galvo scan X extra pixels cannot be negative.");
        }

        if (settings.FrameCount < 1)
        {
            throw new InvalidOperationException("Galvo scan frame count must be at least 1.");
        }

        if (settings.SampleRate <= 0)
        {
            throw new InvalidOperationException("Galvo scan sample rate must be greater than zero.");
        }

        if (settings.SamplesPerPixel < 1)
        {
            throw new InvalidOperationException("Galvo scan samples per pixel must be at least 1.");
        }

        if (settings.FillFraction <= 0 || settings.FillFraction > 1)
        {
            throw new InvalidOperationException("Galvo scan fill fraction must be in the range (0, 1].");
        }

        if (settings.RetraceRatio < 0)
        {
            throw new InvalidOperationException("Galvo scan retrace ratio cannot be negative.");
        }

        if (settings.XFeedbackScale <= 0 || settings.YFeedbackScale <= 0)
        {
            throw new InvalidOperationException("Galvo scan feedback scales must be greater than zero.");
        }

        if (settings.DetectorSampleOffsetSamples is < -1_000_000 or > 1_000_000)
        {
            throw new InvalidOperationException("Galvo scan detector offset samples must be in [-1000000, 1000000].");
        }

        if (settings.EnableSlewLimit && settings.MaxSlewRateVoltsPerSecond <= 0)
        {
            throw new InvalidOperationException("Galvo scan max slew rate must be greater than zero when enabled.");
        }

        if (settings.RampMilliseconds < 0)
        {
            throw new InvalidOperationException("Galvo scan ramp milliseconds cannot be negative.");
        }

        ValidateVoltageWindow(settings.CenterXVoltage, settings.CommandAmplitudeXVoltage, settings, "X commanded");
        ValidateVoltageWindow(settings.CenterYVoltage, settings.AmplitudeYVoltage, settings, "Y");
    }

    private static bool ShouldReverseRow(GalvoScanRuntimeMode scanMode, int rowIndex)
    {
        return scanMode is GalvoScanRuntimeMode.BidirectionalRaster
                or GalvoScanRuntimeMode.FeedbackResample
                or GalvoScanRuntimeMode.XFeedbackRaster
            && rowIndex % 2 == 1;
    }

    private static void ValidateVoltageWindow(
        double center,
        double amplitude,
        GalvoScanRuntimeConfiguration settings,
        string axisName)
    {
        var low = center - amplitude;
        var high = center + amplitude;
        if (low < settings.VoltageMinimum || high > settings.VoltageMaximum)
        {
            throw new InvalidOperationException(
                $"{axisName} scan window [{low:F3}, {high:F3}] V exceeds [{settings.VoltageMinimum:F3}, {settings.VoltageMaximum:F3}] V.");
        }
    }

    private static int CalculateTurnSampleCount(int forwardSamples, GalvoScanRuntimeConfiguration settings)
    {
        var fillSamples = (int)Math.Ceiling(forwardSamples / settings.FillFraction) - forwardSamples;
        var retraceSamples = (int)Math.Ceiling(forwardSamples * settings.RetraceRatio);
        return Math.Max(1, Math.Max(fillSamples, retraceSamples));
    }

    private static int CalculateRampSampleCount(GalvoScanRuntimeConfiguration settings)
    {
        return settings.RampMilliseconds <= 0
            ? 1
            : Math.Max(1, (int)Math.Round(settings.SampleRate * settings.RampMilliseconds / 1000d));
    }

    private static double GetRowYVoltage(GalvoScanRuntimeConfiguration settings, int rowIndex)
    {
        if (settings.ImageHeight <= 1)
        {
            return settings.CenterYVoltage;
        }

        var top = settings.CenterYVoltage + settings.AmplitudeYVoltage;
        var span = settings.AmplitudeYVoltage * 2d;
        return top - span * rowIndex / (settings.ImageHeight - 1);
    }

    private static void AppendActiveLine(
        ICollection<double> xSamples,
        ICollection<double> ySamples,
        GalvoScanRuntimeConfiguration settings,
        double y,
        int sampleCount,
        bool reverse)
    {
        var low = settings.CenterXVoltage - settings.CommandAmplitudeXVoltage;
        var high = settings.CenterXVoltage + settings.CommandAmplitudeXVoltage;
        AppendLinearSegment(xSamples, ySamples, reverse ? high : low, reverse ? low : high, y, y, sampleCount);
    }

    private static void AppendTurnSegment(
        ICollection<double> xSamples,
        ICollection<double> ySamples,
        GalvoScanRuntimeConfiguration settings,
        int rowIndex,
        bool currentReverse,
        int sampleCount)
    {
        var hasNextRow = rowIndex < settings.ImageHeight - 1;
        var currentEndX = currentReverse
            ? settings.CenterXVoltage - settings.CommandAmplitudeXVoltage
            : settings.CenterXVoltage + settings.CommandAmplitudeXVoltage;
        var currentY = GetRowYVoltage(settings, rowIndex);

        if (!hasNextRow)
        {
            AppendLinearSegment(xSamples, ySamples, currentEndX, currentEndX, currentY, currentY, sampleCount);
            return;
        }

        var nextRow = rowIndex + 1;
        var nextReverse = ShouldReverseRow(settings.ScanMode, nextRow);
        var nextStartX = nextReverse
            ? settings.CenterXVoltage + settings.CommandAmplitudeXVoltage
            : settings.CenterXVoltage - settings.CommandAmplitudeXVoltage;
        var nextY = GetRowYVoltage(settings, nextRow);
        AppendLinearSegment(xSamples, ySamples, currentEndX, nextStartX, currentY, nextY, sampleCount);
    }

    private static void AppendLinearSegment(
        ICollection<double> xSamples,
        ICollection<double> ySamples,
        double startX,
        double endX,
        double startY,
        double endY,
        int sampleCount)
    {
        var count = Math.Max(1, sampleCount);
        for (var sampleIndex = 0; sampleIndex < count; sampleIndex++)
        {
            var ratio = count == 1 ? 1d : (double)sampleIndex / (count - 1);
            xSamples.Add(Lerp(startX, endX, ratio));
            ySamples.Add(Lerp(startY, endY, ratio));
        }
    }

    private static void ValidateSlewRate(IReadOnlyList<double> samples, GalvoScanRuntimeConfiguration settings, string axisName)
    {
        var maxDeltaPerSample = settings.MaxSlewRateVoltsPerSecond / settings.SampleRate;
        for (var sampleIndex = 1; sampleIndex < samples.Count; sampleIndex++)
        {
            var delta = samples[sampleIndex] - samples[sampleIndex - 1];
            if (Math.Abs(delta) <= maxDeltaPerSample)
            {
                continue;
            }

            var requiredSlewRate = Math.Abs(delta) * settings.SampleRate;
            throw new InvalidOperationException(
                $"{axisName} waveform slew rate {requiredSlewRate:F3} V/s exceeds MaxSlewRate {settings.MaxSlewRateVoltsPerSecond:F3} V/s at sample {sampleIndex}.");
        }
    }

    private static void ValidateVoltageBounds(IReadOnlyList<double> samples, GalvoScanRuntimeConfiguration settings, string axisName)
    {
        for (var sampleIndex = 0; sampleIndex < samples.Count; sampleIndex++)
        {
            var sample = samples[sampleIndex];
            if (sample < settings.VoltageMinimum || sample > settings.VoltageMaximum)
            {
                throw new InvalidOperationException(
                    $"{axisName} waveform sample {sampleIndex}={sample:F3} V exceeds [{settings.VoltageMinimum:F3}, {settings.VoltageMaximum:F3}] V.");
            }
        }
    }

    private static double Lerp(double start, double end, double ratio)
    {
        return start + (end - start) * ratio;
    }
}
