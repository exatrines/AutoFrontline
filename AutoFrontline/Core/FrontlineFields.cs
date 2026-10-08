using System.Collections.Generic;

namespace AutoFrontline;

public static class FrontlineFields
{
    public const uint ConquestTerritoryId = 1273;
    public const uint SealRockTerritoryId = 431;
    public const uint ShatterTerritoryId = 554;
    public const uint OnsalHakairTerritoryId = 888;
    public const uint TrainingTerritoryId = 1313;

    private static readonly HashSet<uint> TerritoryIds =
    [
        ConquestTerritoryId,
        SealRockTerritoryId,
        ShatterTerritoryId,
        OnsalHakairTerritoryId,
        TrainingTerritoryId,
    ];

    public static bool IsFrontline(uint territoryId) => TerritoryIds.Contains(territoryId);
}
