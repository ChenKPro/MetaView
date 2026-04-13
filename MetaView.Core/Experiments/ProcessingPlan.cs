namespace MetaView.Core.Experiments;

/// <summary>
/// Describes how acquired signals should be transformed into data products.
/// </summary>
public sealed record ProcessingPlan(
    ProcessingMode Mode,
    IReadOnlyList<string>? InputChannels = null,
    IReadOnlyDictionary<string, string>? Parameters = null);
