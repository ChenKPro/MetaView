namespace MetaView.Capabilities.Algorithms.Acquisition;

/// <summary>
/// Describes a regular large-area tile grid planning request.
/// </summary>
/// <param name="TileColumns">Number of tile columns.</param>
/// <param name="TileRows">Number of tile rows.</param>
/// <param name="TileWidth">Single tile width in stage or pixel units.</param>
/// <param name="TileHeight">Single tile height in stage or pixel units.</param>
/// <param name="OverlapXFraction">Horizontal overlap fraction in the range [0, 1).</param>
/// <param name="OverlapYFraction">Vertical overlap fraction in the range [0, 1).</param>
public sealed record LargeAreaTileGridRequest(
    int TileColumns,
    int TileRows,
    double TileWidth,
    double TileHeight,
    double OverlapXFraction,
    double OverlapYFraction);

/// <summary>
/// Describes a planned large-area tile position.
/// </summary>
/// <param name="TileIndex">Zero-based row-major tile index.</param>
/// <param name="Column">Zero-based tile column.</param>
/// <param name="Row">Zero-based tile row.</param>
/// <param name="OffsetX">Tile X offset in the same unit as tile width.</param>
/// <param name="OffsetY">Tile Y offset in the same unit as tile height.</param>
public sealed record LargeAreaTilePosition(
    int TileIndex,
    int Column,
    int Row,
    double OffsetX,
    double OffsetY);

/// <summary>
/// Describes a planned large-area tile grid.
/// </summary>
/// <param name="Tiles">Row-major tile positions.</param>
/// <param name="TotalWidth">Total covered width after overlap.</param>
/// <param name="TotalHeight">Total covered height after overlap.</param>
public sealed record LargeAreaTileGridPlan(
    IReadOnlyList<LargeAreaTilePosition> Tiles,
    double TotalWidth,
    double TotalHeight);

/// <summary>
/// Plans regular large-area 2D tile positions without depending on hardware SDK types.
/// </summary>
public static class LargeAreaTileGridPlanner
{
    /// <summary>
    /// Creates row-major tile positions and coverage size for a regular overlapped grid.
    /// </summary>
    /// <param name="request">Tile grid request.</param>
    /// <returns>Tile positions and coverage size.</returns>
    public static LargeAreaTileGridPlan Plan(LargeAreaTileGridRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatePositive(request.TileColumns, nameof(request.TileColumns));
        ValidatePositive(request.TileRows, nameof(request.TileRows));
        ValidatePositive(request.TileWidth, nameof(request.TileWidth));
        ValidatePositive(request.TileHeight, nameof(request.TileHeight));
        ValidateOverlap(request.OverlapXFraction, nameof(request.OverlapXFraction));
        ValidateOverlap(request.OverlapYFraction, nameof(request.OverlapYFraction));

        var stepX = request.TileWidth * (1.0 - request.OverlapXFraction);
        var stepY = request.TileHeight * (1.0 - request.OverlapYFraction);
        var tiles = new List<LargeAreaTilePosition>(request.TileColumns * request.TileRows);
        for (var row = 0; row < request.TileRows; row++)
        {
            for (var column = 0; column < request.TileColumns; column++)
            {
                tiles.Add(new LargeAreaTilePosition(
                    tiles.Count,
                    column,
                    row,
                    column * stepX,
                    row * stepY));
            }
        }

        var totalWidth = request.TileWidth + Math.Max(0, request.TileColumns - 1) * stepX;
        var totalHeight = request.TileHeight + Math.Max(0, request.TileRows - 1) * stepY;
        return new LargeAreaTileGridPlan(tiles, totalWidth, totalHeight);
    }

    private static void ValidatePositive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be positive.");
        }
    }

    private static void ValidatePositive(double value, string parameterName)
    {
        if (value <= 0.0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be positive.");
        }
    }

    private static void ValidateOverlap(double value, string parameterName)
    {
        if (value is < 0.0 or >= 1.0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Overlap fraction must be in the range [0, 1).");
        }
    }
}

