using MetaView.Capabilities.Algorithms.Acquisition;
using MetaView.Capabilities.Algorithms.Bbo;
using MetaView.Capabilities.Algorithms.Calibration;
using MetaView.Capabilities.Algorithms.Corrections;
using MetaView.Capabilities.Algorithms.FrameProcessing;
using MetaView.Capabilities.Algorithms.Rendering;
using MetaView.Capabilities.Algorithms.Spectral;
using MetaView.Core.Algorithms;
using Vibronix.Foundation.Common.Results;
using CoreBboSignalAnalysis = MetaView.Core.Algorithms.BboSignalAnalysis;
using CoreSpectralRoiStatistics = MetaView.Core.Algorithms.SpectralRoiStatistics;
using CoreTileGridPlan = MetaView.Core.Algorithms.TileGridPlan;
using CoreTilePosition = MetaView.Core.Algorithms.TilePosition;

namespace MetaView.Capabilities.Algorithms;

/// <summary>
/// Provides the algorithm processing capability required by MetaView workflows.
/// </summary>
public sealed class AlgorithmProcessingCapability : IAlgorithmProcessingCapability
{
    /// <inheritdoc />
    public OperationResult<AlgorithmRawFrame> AverageFrames(IReadOnlyList<AlgorithmRawFrame> frames)
    {
        return Execute(() =>
        {
            var capabilityFrames = frames
                .Select(frame => new DaqRawFrame(frame.FrameIndex, frame.Samples))
                .ToArray();
            var averaged = FrameAverager.Average(capabilityFrames);
            return new AlgorithmRawFrame(averaged.FrameIndex, averaged.Samples);
        });
    }

    /// <inheritdoc />
    public OperationResult<IReadOnlyList<double>> CorrectShadow(
        IReadOnlyList<double> sourceSamples,
        IReadOnlyList<double> darkSamples,
        IReadOnlyList<double> referenceSamples)
    {
        return Execute(() => ShadowCorrectionProcessor.Correct(sourceSamples, darkSamples, referenceSamples));
    }

    /// <inheritdoc />
    public OperationResult<IReadOnlyList<double>> ApplyLinearCalibration(
        IReadOnlyList<double> samples,
        double scale,
        double offset)
    {
        return Execute(() => SpectralCalibrationProcessor.ApplyLinear(samples, scale, offset));
    }

    /// <inheritdoc />
    public OperationResult<CoreBboSignalAnalysis> AnalyzeBboSignal(IReadOnlyList<double> samples)
    {
        return Execute(() =>
        {
            var analysis = BboSignalAnalyzer.Analyze(samples);
            return new CoreBboSignalAnalysis(
                analysis.PeakIndex,
                analysis.PeakValue,
                analysis.Baseline,
                analysis.SignalToBaseline,
                analysis.HalfMaximumLevel,
                analysis.LeftHalfMaximumIndex,
                analysis.RightHalfMaximumIndex,
                analysis.FullWidthHalfMaximum);
        });
    }

    /// <inheritdoc />
    public OperationResult<SpectralFeatureSet> ExtractSpectralFeatures(IReadOnlyList<double> samples)
    {
        return Execute(() =>
        {
            var features = SpectralFeatureExtractor.Extract(samples);
            return new SpectralFeatureSet(features.PeakValue, features.PeakIndex, features.Integral);
        });
    }

    /// <inheritdoc />
    public OperationResult<CoreSpectralRoiStatistics> ExtractSpectralRoiStatistics(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int roiX,
        int roiY,
        int roiWidth,
        int roiHeight)
    {
        return Execute(() =>
        {
            var statistics = RoiSpectrumExtractor.ExtractStatistics(
                values,
                xLength,
                yLength,
                spectralLength,
                roiX,
                roiY,
                roiWidth,
                roiHeight);

            return new CoreSpectralRoiStatistics(
                statistics.PixelCount,
                statistics.AverageSpectrum,
                statistics.MinimumSpectrum,
                statistics.MaximumSpectrum,
                statistics.StandardDeviationSpectrum);
        });
    }

    /// <inheritdoc />
    public OperationResult<IReadOnlyList<double>> ExtractPeakIntensityMap(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int spectralStartIndex = 0,
        int? spectralEndIndex = null)
    {
        return Execute(() => SpectralMapGenerator.ExtractPeakIntensityMap(
            values,
            xLength,
            yLength,
            spectralLength,
            spectralStartIndex,
            spectralEndIndex));
    }

    /// <inheritdoc />
    public OperationResult<IReadOnlyList<byte>> ToGrayscaleBytes(
        IReadOnlyList<double> values,
        double? minimum = null,
        double? maximum = null,
        bool invert = false)
    {
        return Execute(() => SpectralImagePreviewBuilder.ToGrayscaleBytes(values, minimum, maximum, invert));
    }

    /// <inheritdoc />
    public OperationResult<CoreTileGridPlan> PlanTileGrid(
        int tileColumns,
        int tileRows,
        double tileWidth,
        double tileHeight,
        double overlapXFraction,
        double overlapYFraction)
    {
        return Execute(() =>
        {
            var plan = LargeAreaTileGridPlanner.Plan(new LargeAreaTileGridRequest(
                tileColumns,
                tileRows,
                tileWidth,
                tileHeight,
                overlapXFraction,
                overlapYFraction));

            var tiles = plan.Tiles
                .Select(tile => new CoreTilePosition(
                    tile.TileIndex,
                    tile.Column,
                    tile.Row,
                    tile.OffsetX,
                    tile.OffsetY))
                .ToArray();

            return new CoreTileGridPlan(tiles, plan.TotalWidth, plan.TotalHeight);
        });
    }

    private static OperationResult<T> Execute<T>(Func<T> operation)
    {
        try
        {
            return OperationResult<T>.Ok(operation());
        }
        catch (Exception ex)
        {
            return OperationResult<T>.Error(ex.Message);
        }
    }
}
