using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>
/// 固定脱出座標が設定されたフィールドでは、一度スポーン除外圏を出た後に
/// 再び除外圏内へ入ったとき固定座標へ moveto する。未設定フィールドでは集団行動のみ。
/// 除外圏を一度出たかどうかは Return ゲートにも使用。
/// </summary>
internal static class InitialMovementMode
{
    private static bool matchEntryActive;
    private static Vector3? trackedEntryPosition;
    private static bool hasLeftSpawnExclusion;

    public static bool IsActive { get; private set; }

    /// <summary>マッチ開始後、一度でもスポーン除外圏外に出た。</summary>
    public static bool HasLeftSpawnExclusion => hasLeftSpawnExclusion;

    public static bool HasFixedExitForCurrentTerritory =>
        FrontlineSpawnExitDestinations.TryGetFixedExitDestination(
            Svc.ClientState.TerritoryType,
            out _);

    public static Vector3? FixedExitDestination =>
        FrontlineSpawnExitDestinations.TryGetFixedExitDestination(
            Svc.ClientState.TerritoryType,
            out var destination)
            ? destination
            : null;

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

        if (!FrontlineEntryZone.IsPlayerInExclusion())
            hasLeftSpawnExclusion = true;

        IsActive = FrontlineEntryZone.IsPlayerInExclusion()
            && HasFixedExitForCurrentTerritory
            && hasLeftSpawnExclusion;
    }

    public static bool TryGetMoveTarget(out Vector3 target)
    {
        if (!IsActive || FixedExitDestination is not Vector3 anchor)
        {
            target = default;
            return false;
        }

        target = anchor;
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

    private static void ResetMatchState()
    {
        hasLeftSpawnExclusion = false;
        IsActive = false;
    }

    private static void ResetAll()
    {
        matchEntryActive = false;
        trackedEntryPosition = null;
        ResetMatchState();
    }
}
