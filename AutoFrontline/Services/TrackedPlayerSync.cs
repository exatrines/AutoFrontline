using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace AutoFrontline.Services;

/// <summary>マウントの乗り降り。Experimental Group Move 時は敵がいないときのみ乗車、それ以外は移動先距離で乗車・敵付近で降車。</summary>
public static unsafe class TrackedPlayerSync
{
    public static float LastDistanceToTracked { get; private set; }
    public static float LastDistanceToMoveDestination { get; private set; }
    public static bool LastSelfInCombat { get; private set; }
    public static int LastNearbyEnemyCount { get; private set; }
    public static bool LastIcedotomeIrisNearby { get; private set; }

    public static bool ShouldDeferMovement =>
        C.ExperimentalGroupMoveEnabled
            ? !HasNearbyEnemy() && !Player.Mounted && !Player.Mounting
            : ShouldTryMountForMoveDistance() && !Player.Mounted && !Player.Mounting;

    private static bool InCombat => Svc.Condition[ConditionFlag.InCombat];

    public static void Update()
    {
        LastSelfInCombat = InCombat;
        SyncMount();

        var tracked = FollowTargetService.TryGetTrackedGameObject();
        if (tracked == null)
            return;

        LastDistanceToTracked = Vector3.Distance(Player.Object!.Position, tracked.Position);
        if (FollowTargetService.TryGetMoveDestinationDistance(out var moveDistance))
            LastDistanceToMoveDestination = moveDistance;
    }

    private static bool HasNearbyEnemy()
    {
        var allies = AllianceMemberCache.GetMembers();
        return NearbyEnemyDetector.HasNearbyEnemy(allies, out _);
    }

    private static bool ShouldTryMountForMoveDistance() =>
        InitialMovementMode.TryGetMoveDestinationDistance(out var initialDistance)
            ? initialDistance >= C.MountDistanceMeters
            : FollowTargetService.TryGetMoveDestinationDistance(out var distance)
              && distance >= C.MountDistanceMeters;

    private static void SyncMount()
    {
        var allies = AllianceMemberCache.GetMembers();
        var hasNearbyEnemy = NearbyEnemyDetector.HasNearbyEnemy(allies, out var enemyCount);
        LastNearbyEnemyCount = enemyCount;
        LastIcedotomeIrisNearby = FrontlineSpecialCombatTargetDetector.HasNearby(C.DismountEnemyDistanceMeters);

        if (C.ExperimentalGroupMoveEnabled)
            SyncMountExperimental(hasNearbyEnemy);
        else
            SyncMountDefault(hasNearbyEnemy);
    }

    private static void SyncMountExperimental(bool hasNearbyEnemy)
    {
        if (hasNearbyEnemy)
        {
            if (Player.Mounted
                && EzThrottler.Throttle(FrontlineConstants.ThrottleDismount, FrontlineConstants.DismountThrottleMs))
                Chat.ExecuteCommand("/mount");

            return;
        }

        TryMount();
    }

    private static void SyncMountDefault(bool hasNearbyEnemy)
    {
        if (Player.Mounted && (hasNearbyEnemy || LastIcedotomeIrisNearby))
        {
            if (EzThrottler.Throttle(FrontlineConstants.ThrottleDismount, FrontlineConstants.DismountThrottleMs))
                Chat.ExecuteCommand("/mount");

            return;
        }

        if (!ShouldTryMountForMoveDistance())
            return;

        TryMount();
    }

    private static void TryMount()
    {
        if (Player.Mounted || Player.Mounting)
            return;

        if (EzThrottler.Throttle(FrontlineConstants.ThrottleMount, FrontlineConstants.MountThrottleMs))
            UseConfiguredMount();
    }

    private static void UseConfiguredMount()
    {
        if (C.MountSelectionId != MountCatalog.RouletteSelectionId
            && MountCatalog.IsMountOwned(C.MountSelectionId))
        {
            UseSpecificMount(C.MountSelectionId);
            return;
        }

        UseMountRoulette();
    }

    private static void UseMountRoulette()
    {
        var actionId = MountCatalog.ResolveRouletteActionId();
        var actionManager = ActionManager.Instance();
        if (actionManager != null)
            actionManager->UseAction(ActionType.GeneralAction, actionId, 0xE0000000);

        var name = Svc.Data.GetExcelSheet<GeneralAction>().GetRowOrDefault(actionId)?.Name.ToString();
        if (!string.IsNullOrEmpty(name))
            Chat.ExecuteCommand($"/gaction \"{name}\"");
    }

    private static void UseSpecificMount(uint mountRowId)
    {
        if (Svc.Data.GetExcelSheet<Mount>().GetRowOrDefault(mountRowId) is not { } mount)
            return;

        var actionManager = ActionManager.Instance();
        if (actionManager != null)
            actionManager->UseAction(ActionType.Mount, mountRowId, 0xE0000000);
    }
}
