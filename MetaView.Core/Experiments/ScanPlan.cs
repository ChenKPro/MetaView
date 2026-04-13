namespace MetaView.Core.Experiments;

/// <summary>
/// Describes how an experiment samples space, time, or spectrum.
/// </summary>
public sealed record ScanPlan(
    ScanPattern Pattern,
    int SizeX = 1,
    int SizeY = 1,
    int SizeZ = 1,
    int SizeT = 1,
    double StepX = 0,
    double StepY = 0,
    double StepZ = 0,
    TimeSpan? TimeStep = null,
    IReadOnlyDictionary<string, string>? Parameters = null);
