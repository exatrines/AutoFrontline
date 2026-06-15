using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>フレームごとの自動化オーケストレーション。</summary>
public static class FrontlineAutomation
{
    private static bool wasAlive = true;

    public static void Update()
    {
        AllianceMemberCache.BeginFrame();
        AllianceCommanderTracker.Update();
        FrontlineEntryZone.Update();
        RequiredPlugins.SyncEnabledState();
        FrontlineAutoRunOrchestrator.Update();
        RotationModeAutomation.Update();

        if (Player.IsDead)
        {
            if (wasAlive)
            {
                MovementCommands.Stop();
                NaviStuckDejonAutomation.Reset();
                wasAlive = false;
            }

            return;
        }

        wasAlive = true;

        FrontlineDutyConfirmAutomation.Update();
        FrontlineLeaveAutomation.Update();

        if (FrontlineLeaveAutomation.IsBlockingAutomation)
            return;

        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
        {
            NaviMovementCoordinator.Reset();
            NaviStuckDejonAutomation.Reset();
            return;
        }

        if (!RequiredPlugins.IsAutomationActive)
        {
            NaviMovementCoordinator.Reset();
            NaviStuckDejonAutomation.Reset();
            return;
        }

        FollowTargetService.UpdateSelection();
        InitialMovementMode.Update();
        NaviMovementCoordinator.Update();
        ClosestEnemyPlayerTargeting.Update();
        PvpLimitBreakAutomation.Update();
        TrackedPlayerSync.Update();

        var hasInitialMove = InitialMovementMode.TryGetMoveTarget(out var initialTarget);
        var followMoveTarget = FollowTargetService.TryGetMoveTarget();
        var hasNavigationIntent = hasInitialMove
            || followMoveTarget != null
            || FollowTargetService.LastMoveTarget != null
            || InitialMovementMode.IsActive;
        NaviStuckDejonAutomation.Update(hasNavigationIntent);

        if (NaviStuckDejonAutomation.IsAwaitingConfirm)
            return;

        if (TrackedPlayerSync.ShouldDeferMovement)
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleMove, FollowTargetService.MoveRefreshIntervalMs))
            return;

        if (!PlayerMovementGate.CanIssueVnavMoveTo)
            return;

        var target = hasInitialMove
            ? initialTarget
            : followMoveTarget;

        if (target is not Vector3 moveTarget)
            return;

        if (FrontlineEntryZone.ShouldSkipMoveTarget(moveTarget))
            return;

        NaviMovementCoordinator.IssueMoveTo(moveTarget);
    }
}
