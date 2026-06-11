using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>敵プレイヤーとアイスドトームリスのうち最も近い対象をターゲットする。</summary>
internal static class ClosestEnemyPlayerTargeting
{
    public static IPlayerCharacter LastClosestEnemy { get; private set; }
    public static float LastClosestEnemyDistance { get; private set; }
    public static bool LastTargetIsIcedotomeIris { get; private set; }

    public static void Update()
    {
        if (!AutomationContext.CanRunInFrontlineMatch)
        {
            ClearDebugState();
            return;
        }

        if (!Player.Available || Player.Object == null)
        {
            ClearDebugState();
            return;
        }

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

            ClearDebugState();
            return;
        }

        if (!TrySelectClosestCombatTarget(out var target, out var distance, out var isIris))
        {
            ClearDebugState();
            return;
        }

        LastClosestEnemy = isIris ? null : target as IPlayerCharacter;
        LastClosestEnemyDistance = distance;
        LastTargetIsIcedotomeIris = isIris;

        if (Svc.Targets.Target?.GameObjectId == target.GameObjectId)
            return;

        Svc.Targets.Target = target;
    }

    private static bool TrySelectClosestCombatTarget(
        out IGameObject target,
        out float distanceMeters,
        out bool isIcedotomeIris)
    {
        target = null!;
        distanceMeters = float.MaxValue;
        isIcedotomeIris = false;

        var allies = AllianceMemberCache.GetMembers();

        if (NearbyEnemyDetector.TryGetClosestEnemy(allies, out var enemy, out var enemyDistance)
            && !FrontlineEntryZone.ShouldSkipEnemyTargeting(enemy.Position))
        {
            target = enemy;
            distanceMeters = enemyDistance;
        }

        if (IcedotomeIrisDetector.TryGetClosest(float.MaxValue, out var iris, out var irisDistance)
            && !FrontlineEntryZone.ShouldSkipEnemyTargeting(iris.Position)
            && irisDistance < distanceMeters)
        {
            target = iris;
            distanceMeters = irisDistance;
            isIcedotomeIris = true;
        }

        return target != null;
    }

    private static void ClearDebugState()
    {
        LastClosestEnemy = null;
        LastClosestEnemyDistance = 0;
        LastTargetIsIcedotomeIris = false;
    }
}
