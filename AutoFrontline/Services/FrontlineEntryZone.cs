using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>フロントライン入室座標とスポーン除外半径（セッション専用、設定には保存しない）。</summary>
internal static class FrontlineEntryZone
{
    public static Vector3? EntryPosition { get; private set; }
    public static bool LastMoveBlocked { get; private set; }

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
        }
    }

    public static bool ShouldSkipMoveTarget(Vector3 target)
    {
        if (EntryPosition is not Vector3 entry)
        {
            LastMoveBlocked = false;
            return false;
        }

        LastMoveBlocked = GameCoords.IsWithinRadius(entry, target, FrontlineConstants.SpawnExclusionRadiusMeters);
        return LastMoveBlocked;
    }

    public static float? DistanceToEntry(Vector3 position) =>
        EntryPosition is Vector3 entry ? Vector3.Distance(entry, position) : null;
}
