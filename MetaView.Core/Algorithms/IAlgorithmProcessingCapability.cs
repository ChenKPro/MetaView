using Vibronix.Foundation.Common.Results;

namespace MetaView.Core.Algorithms;

/// <summary>
/// Defines algorithm processing functions exposed to application workflows.
/// </summary>
public interface IAlgorithmProcessingCapability
{
    /// <summary>
    /// Averages equal-length raw frames sample by sample.
    /// </summary>
    OperationResult<AlgorithmRawFrame> AverageFrames(IReadOnlyList<AlgorithmRawFrame> frames);

    /// <summary>
    /// Applies dark subtraction and reference normalization.
    /// </summary>
    OperationResult<IReadOnlyList<double>> CorrectShadow(
        IReadOnlyList<double> sourceSamples,
        IReadOnlyList<double> darkSamples,
        IReadOnlyList<double> referenceSamples);

    /// <summary>
    /// Applies linear calibration to sample values.
    /// </summary>
    OperationResult<IReadOnlyList<double>> ApplyLinearCalibration(
        IReadOnlyList<double> samples,
        double scale,
        double offset);

    /// <summary>
    /// Analyzes a BBO signal trace.
    /// </summary>
    OperationResult<BboSignalAnalysis> AnalyzeBboSignal(IReadOnlyList<double> samples);

    /// <summary>
    /// Extracts peak and integral features from a spectrum.
    /// </summary>
    OperationResult<SpectralFeatureSet> ExtractSpectralFeatures(IReadOnlyList<double> samples);

    /// <summary>
    /// Extracts ROI statistics from a spectral cube.
    /// </summary>
    OperationResult<SpectralRoiStatistics> ExtractSpectralRoiStatistics(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int roiX,
        int roiY,
        int roiWidth,
        int roiHeight);

    /// <summary>
    /// Builds a peak-intensity map from a spectral cube.
    /// </summary>
    OperationResult<IReadOnlyList<double>> ExtractPeakIntensityMap(
        IReadOnlyList<double> values,
        int xLength,
        int yLength,
        int spectralLength,
        int spectralStartIndex = 0,
        int? spectralEndIndex = null);

    /// <summary>
    /// Converts scalar preview values to normalized grayscale bytes.
    /// </summary>
    OperationResult<IReadOnlyList<byte>> ToGrayscaleBytes(
        IReadOnlyList<double> values,
        double? minimum = null,
        double? maximum = null,
        bool invert = false);

    /// <summary>
    /// Plans a regular large-area tile grid.
    /// </summary>
    OperationResult<TileGridPlan> PlanTileGrid(
        int tileColumns,
        int tileRows,
        double tileWidth,
        double tileHeight,
        double overlapXFraction,
        double overlapYFraction);
}
