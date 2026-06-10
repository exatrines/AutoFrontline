using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>フロントライン入室座標とスポーン除外半径（セッション専用、設定には保存しない）。</summary>
internal static class FrontlineEntryZone
{
    public static Vector3? EntryPosition { get; private set; }
    public static bool LastMoveBlocked { get; private set; }
    public static bool LastTargetBlocked { get; private set; }

    public static void Update()
    {
        var inFrontline = FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType);

        // 入室直後は Player が未 Available のフレームがあり、遷移 edge だけでは取り逃す
        if (inFrontline && EntryPosition == null && Player.Available && Player.Object != null)
            EntryPosition = Player.Object.Position;

        if (!inFrontline)
        {
            EntryPosition = null;
            LastMoveBlocked = false;
            LastTargetBlocked = false;
        }
    }

    public static bool IsWithinExclusion(Vector3 position)
    {
        if (EntryPosition is not Vector3 entry)
            return false;

        return GameCoords.IsWithinRadius(entry, position, FrontlineConstants.SpawnExclusionRadiusMeters);
    }

    public static bool IsPlayerInExclusion()
    {
        if (!Player.Available || Player.Object == null)
            return false;

        return IsWithinExclusion(Player.Object.Position);
    }

    public static bool ShouldSkipMoveTarget(Vector3 target)
    {
        if (EntryPosition is not Vector3 _)
        {
            LastMoveBlocked = false;
            return false;
        }

        LastMoveBlocked = IsWithinExclusion(target);
        return LastMoveBlocked;
    }

    /// <summary>スポーン除外圏内では敵ターゲットを取らない（自位置または敵位置が圏内）。</summary>
    public static bool ShouldSkipEnemyTargeting()
    {
        LastTargetBlocked = IsPlayerInExclusion();
        return LastTargetBlocked;
    }

    /// <summary>スポーン除外圏内では敵ターゲットを取らない（自位置または敵位置が圏内）。</summary>
    public static bool ShouldSkipEnemyTargeting(Vector3 enemyPosition)
    {
        LastTargetBlocked = IsPlayerInExclusion() || IsWithinExclusion(enemyPosition);
        return LastTargetBlocked;
    }

    public static float? DistanceToEntry(Vector3 position) =>
        EntryPosition is Vector3 entry ? Vector3.Distance(entry, position) : null;
}
