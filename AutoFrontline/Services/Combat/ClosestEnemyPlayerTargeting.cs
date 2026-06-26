using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>敵プレイヤーと特殊戦闘オブジェクト（リス・ドローン等）のうち最も近い対象をターゲットする。</summary>
internal static class ClosestEnemyPlayerTargeting
{
    public static void Update()
    {
        if (!AutomationContext.CanRunInFrontlineMatch)
            return;

        if (!Player.Available || Player.Object == null)
            return;

        if (!EzThrottler.Throttle(
                FrontlineConstants.ThrottleEnemyTarget,
                FollowTargetService.IsHostileMode
                    ? ConfigIntervals.HostileModeRefreshMs
                    : ConfigIntervals.GroupMovementRefreshMs))
            return;

        if (FrontlineEntryZone.ShouldSkipEnemyTargeting())
        {
            if (Svc.Targets.Target != null)
                Svc.Targets.Target = null;

            return;
        }

        if (!TrySelectClosestCombatTarget(
                out var target,
                out _,
                out var specialCombatTargetName))
            return;

        if (Svc.Targets.Target?.GameObjectId == target.GameObjectId)
            return;

        Svc.Targets.Target = target;
    }

    private static bool TrySelectClosestCombatTarget(
        out IGameObject target,
        out float distanceMeters,
        out string specialCombatTargetName)
    {
        target = null!;
        distanceMeters = float.MaxValue;
        specialCombatTargetName = string.Empty;

        var allies = AllianceMemberCache.GetMembers();

        if (NearbyEnemyDetector.TryGetClosestEnemy(allies, out var enemy, out var enemyDistance)
            && !FrontlineEntryZone.ShouldSkipEnemyTargeting(enemy.Position))
        {
            target = enemy;
            distanceMeters = enemyDistance;
        }

        if (FrontlineSpecialCombatTargetDetector.TryGetClosest(
                float.MaxValue,
                out var specialTarget,
                out var specialDistance,
                out var specialName)
            && !FrontlineEntryZone.ShouldSkipEnemyTargeting(specialTarget.Position)
            && specialDistance < distanceMeters)
        {
            target = specialTarget;
            distanceMeters = specialDistance;
            specialCombatTargetName = specialName;
        }

        return target != null;
    }
}
