using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>
/// スポーン中心から水平 25m 地点を初回脱出座標として記録し、再び除外圏内に入ったときそこへ moveto する。
/// </summary>
internal static class InitialMovementMode
{
    private static bool matchEntryActive;
    private static Vector3? trackedEntryPosition;
    private static Vector3? firstExitPosition;

    public static bool IsActive { get; private set; }
    public static Vector3? FirstExitPosition => firstExitPosition;

    public static void Update()
    {
        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
        {
            ResetAll();
            return;
        }

        if (!SyncMatchEntry())
            return;

        if (!RequiredPlugins.IsAutomationActive)
        {
            IsActive = false;
            return;
        }

        if (!Player.Available || Player.Object == null)
            return;

        var playerPosition = Player.Object.Position;

        if (firstExitPosition == null)
            TryRecordFirstExitPosition(playerPosition);

        IsActive = FrontlineEntryZone.IsPlayerInExclusion() && firstExitPosition != null;
    }

    public static bool TryGetMoveTarget(out Vector3 target)
    {
        if (!IsActive || firstExitPosition is not Vector3 anchor)
        {
            target = default;
            return false;
        }

        target = anchor;
        return true;
    }

    public static bool TryGetMoveDestinationDistance(out float distanceMeters)
    {
        if (!TryGetMoveTarget(out var target) || Player.Object == null)
        {
            distanceMeters = 0;
            return false;
        }

        distanceMeters = HorizontalDistanceTo(Player.Object.Position, target);
        return true;
    }

    public static float? DistanceToAnchor()
    {
        if (firstExitPosition is not Vector3 anchor || Player.Object == null)
            return null;

        return HorizontalDistanceTo(Player.Object.Position, anchor);
    }

    /// <summary>Debug: スポーン中心を現在地で再記録し、初回脱出先をクリアする。</summary>
    public static bool DebugResetInitialState()
    {
        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            return false;

        if (!FrontlineEntryZone.DebugRecaptureEntryPosition())
            return false;

        ResetMatchState();
        matchEntryActive = true;
        trackedEntryPosition = FrontlineEntryZone.EntryPosition;
        return true;
    }

    private static bool SyncMatchEntry()
    {
        if (FrontlineEntryZone.EntryPosition is not Vector3 entry)
        {
            if (matchEntryActive)
                ResetMatchState();

            matchEntryActive = false;
            trackedEntryPosition = null;
            IsActive = false;
            return false;
        }

        if (trackedEntryPosition is Vector3 previous
            && !GameCoords.AreNearHorizontal(previous, entry, FrontlineConstants.PositionUnchangedThresholdMeters))
        {
            ResetMatchState();
        }

        trackedEntryPosition = entry;

        if (!matchEntryActive)
        {
            ResetMatchState();
            matchEntryActive = true;
        }

        return true;
    }

    private static void TryRecordFirstExitPosition(Vector3 playerPosition)
    {
        if (FrontlineEntryZone.EntryPosition is not Vector3 entry)
            return;

        // 中心から水平 25m 以上離れたときだけ記録（25m 以内＝スポーン直後は記録しない）
        if (GameCoords.HorizontalDistance(entry, playerPosition)
            < FrontlineConstants.InitialMovementRecordRadiusMeters)
            return;

        firstExitPosition = playerPosition;
    }

    private static float HorizontalDistanceTo(Vector3 from, Vector3 to) =>
        GameCoords.HorizontalDistance(from, to);

    private static void ResetMatchState()
    {
        firstExitPosition = null;
        IsActive = false;
    }

    private static void ResetAll()
    {
        matchEntryActive = false;
        trackedEntryPosition = null;
        ResetMatchState();
    }
}
