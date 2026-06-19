using System.Collections.Generic;
using System.Numerics;

namespace AutoFrontline;

/// <summary>初回スポーン脱出時の固定 moveto 先（フィールド別）。未設定はプレイヤー位置を記録。</summary>
internal static class FrontlineSpawnExitDestinations
{
    private static readonly Dictionary<uint, Vector3> FixedExitByTerritory = new()
    {
        [431] = new Vector3(10, 10, 1), // シールロック（争奪戦）
        [554] = new Vector3(0, 0, 0), // フィールド・オブ・グローリー（砕氷戦）
        [888] = new Vector3(0, 0, 1), // オンサル・ハカイル（終節戦）
        [1273] = new Vector3(5, 5, 30), // 外縁遺跡群（制圧戦）
        [1313] = new Vector3(-10, -15, 0), // ウォーコー・チーテ（演習戦）
    };

    public static bool TryGetFixedExitDestination(uint territoryId, out Vector3 destination) =>
        FixedExitByTerritory.TryGetValue(territoryId, out destination);
}
