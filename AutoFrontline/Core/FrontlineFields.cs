using System.Collections.Generic;

namespace AutoFrontline;

public static class FrontlineFields
{
    public const uint ConquestTerritoryId = 1273;
    public const uint SealRockTerritoryId = 431;
    public const uint ShatterTerritoryId = 554;
    public const uint OnsalHakairTerritoryId = 888;
    public const uint TrainingTerritoryId = 1313;

    private static readonly Dictionary<uint, string> Fields = new()
    {
        [1273] = "外縁遺跡群（制圧戦）",
        [431] = "シールロック（争奪戦）",
        [554] = "フィールド・オブ・グローリー（砕氷戦）",
        [888] = "オンサル・ハカイル（終節戦）",
        [1313] = "ウォーコー・チーテ（演習戦）",
    };

    public static bool IsFrontline(uint territoryId) => Fields.ContainsKey(territoryId);

    public static string GetDisplayName(uint territoryId) =>
        Fields.TryGetValue(territoryId, out var name) ? name : string.Empty;
}
