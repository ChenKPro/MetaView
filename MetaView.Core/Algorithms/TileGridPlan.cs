namespace MetaView.Core.Algorithms;

/// <summary>
/// Describes one planned tile position.
/// </summary>
public sealed record TilePosition(
    int TileIndex,
    int Column,
    int Row,
    double OffsetX,
    double OffsetY);

/// <summary>
/// Describes a planned large-area tile grid.
/// </summary>
public sealed record TileGridPlan(
    IReadOnlyList<TilePosition> Tiles,
    double TotalWidth,
    double TotalHeight);
