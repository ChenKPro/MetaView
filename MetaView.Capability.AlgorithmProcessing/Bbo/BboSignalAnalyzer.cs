namespace MetaView.Capabilities.Algorithms.Bbo;

/// <summary>
/// Extracts tuning features from a BBO signal trace.
/// </summary>
public static class BboSignalAnalyzer
{
    /// <summary>
    /// Finds the peak and edge baseline in a BBO signal trace.
    /// </summary>
    /// <param name="samples">Signal samples ordered by scan position.</param>
    /// <returns>BBO peak analysis result.</returns>
    public static BboAnalysisResult Analyze(IReadOnlyList<double> samples)
    {
        if (samples.Count == 0)
        {
            throw new ArgumentException("BBO signal must contain at least one sample.", nameof(samples));
        }

        var peakIndex = 0;
        var peakValue = samples[0];
        for (var index = 1; index < samples.Count; index++)
        {
            if (samples[index] > peakValue)
            {
                peakIndex = index;
                peakValue = samples[index];
            }
        }

        var baseline = samples.Count == 1
            ? samples[0]
            : (samples[0] + samples[^1]) / 2.0;

        var signalToBaseline = peakValue - baseline;
        var halfMaximumLevel = baseline + (signalToBaseline / 2.0);
        var leftHalfMaximumIndex = signalToBaseline > 0.0
            ? FindLeftHalfMaximumIndex(samples, peakIndex, halfMaximumLevel)
            : null;
        var rightHalfMaximumIndex = signalToBaseline > 0.0
            ? FindRightHalfMaximumIndex(samples, peakIndex, halfMaximumLevel)
            : null;
        double? fullWidthHalfMaximum = leftHalfMaximumIndex.HasValue && rightHalfMaximumIndex.HasValue
            ? rightHalfMaximumIndex.Value - leftHalfMaximumIndex.Value
            : null;

        return new BboAnalysisResult(
            peakIndex,
            peakValue,
            baseline,
            signalToBaseline,
            halfMaximumLevel,
            leftHalfMaximumIndex,
            rightHalfMaximumIndex,
            fullWidthHalfMaximum);
    }

    private static double? FindLeftHalfMaximumIndex(
        IReadOnlyList<double> samples,
        int peakIndex,
        double halfMaximumLevel)
    {
        for (var index = peakIndex; index > 0; index--)
        {
            var leftValue = samples[index - 1];
            var rightValue = samples[index];
            if (leftValue <= halfMaximumLevel && rightValue >= halfMaximumLevel)
            {
                return InterpolateIndex(index - 1, leftValue, index, rightValue, halfMaximumLevel);
            }
        }

        return null;
    }

    private static double? FindRightHalfMaximumIndex(
        IReadOnlyList<double> samples,
        int peakIndex,
        double halfMaximumLevel)
    {
        for (var index = peakIndex; index < samples.Count - 1; index++)
        {
            var leftValue = samples[index];
            var rightValue = samples[index + 1];
            if (leftValue >= halfMaximumLevel && rightValue <= halfMaximumLevel)
            {
                return InterpolateIndex(index, leftValue, index + 1, rightValue, halfMaximumLevel);
            }
        }

        return null;
    }

    private static double InterpolateIndex(
        int leftIndex,
        double leftValue,
        int rightIndex,
        double rightValue,
        double targetValue)
    {
        var delta = rightValue - leftValue;
        if (delta == 0.0)
        {
            return leftIndex;
        }

        var fraction = (targetValue - leftValue) / delta;
        return leftIndex + ((rightIndex - leftIndex) * fraction);
    }
}

/// <summary>
/// BBO signal analysis result.
/// </summary>
/// <param name="PeakIndex">Index of the strongest BBO signal sample.</param>
/// <param name="PeakValue">Strongest BBO signal value.</param>
/// <param name="Baseline">Baseline estimated from trace edges.</param>
/// <param name="SignalToBaseline">Peak value minus baseline.</param>
/// <param name="HalfMaximumLevel">Baseline-referenced half-maximum signal level.</param>
/// <param name="LeftHalfMaximumIndex">Interpolated left half-maximum crossing index, if available.</param>
/// <param name="RightHalfMaximumIndex">Interpolated right half-maximum crossing index, if available.</param>
/// <param name="FullWidthHalfMaximum">Distance between left and right half-maximum crossings, if available.</param>
public sealed record BboAnalysisResult(
    int PeakIndex,
    double PeakValue,
    double Baseline,
    double SignalToBaseline,
    double HalfMaximumLevel = 0.0,
    double? LeftHalfMaximumIndex = null,
    double? RightHalfMaximumIndex = null,
    double? FullWidthHalfMaximum = null);

