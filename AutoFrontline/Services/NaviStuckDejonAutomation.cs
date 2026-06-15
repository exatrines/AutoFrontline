using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.UIHelpers;

namespace AutoFrontline.Services;

/// <summary>集団行動中にプレイヤー座標が一定時間変わらないとき vnav stop → デジョン → SelectYesno Yes。</summary>
internal static class NaviStuckDejonAutomation
{
    private const string DejonActionName = "デジョン";

    private static bool movetoActive;
    private static bool awaitingDejonConfirm;
    private static Vector3 anchorPosition;
    private static bool hasAnchorPosition;
    private static long anchorPositionTick;

    public static bool IsAwaitingConfirm => awaitingDejonConfirm;
    public static bool IsMovetoTracked => movetoActive;
    public static bool IsMonitoringStall { get; private set; }

    public static float StallElapsedSeconds =>
        hasAnchorPosition ? (Environment.TickCount64 - anchorPositionTick) / 1000f : 0f;

    public static float StallThresholdSeconds =>
        FrontlineConstants.NaviStuckDejonStallMs / 1000f;

    public static Vector3? StallPreviousPlayerPosition =>
        hasAnchorPosition ? anchorPosition : null;

    public static Vector3? StallCurrentPlayerPosition =>
        Player.Available && Player.Object != null ? Player.Object.Position : null;

    public static float? StallDestinationDistanceMeters
    {
        get
        {
            if (!Player.Available || Player.Object == null)
                return null;

            if (FollowTargetService.TryGetMoveDestinationDistance(out var distance))
                return distance;

            if (FollowTargetService.LastMoveTarget is Vector3 target)
                return Vector3.Distance(Player.Object.Position, target);

            return null;
        }
    }

    public static bool IsGroupMovementEligible =>
        FollowTargetService.IsGroupMovementMode && !InitialMovementMode.IsActive;

    public static bool IsStallTimerActive => hasAnchorPosition;

    public static string StallMonitorBlockReason
    {
        get
        {
            if (!AutomationContext.IsInFrontline)
                return "not in frontline";

            if (!movetoActive)
                return "moveto inactive";

            if (!FollowTargetService.IsGroupMovementMode)
                return "not group movement";

            if (InitialMovementMode.IsActive)
                return "initial movement";

            if (StallDestinationDistanceMeters is not float distance)
                return "no move destination";

            if (distance < FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters)
                return $"destination < {FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters}m";

            return string.Empty;
        }
    }

    public static float? StallPositionDeltaMeters
    {
        get
        {
            if (StallCurrentPlayerPosition is not Vector3 current
                || StallPreviousPlayerPosition is not Vector3 anchor)
                return null;

            return Vector3.Distance(current, anchor);
        }
    }

    public static string StallPhaseLabel
    {
        get
        {
            if (awaitingDejonConfirm)
                return "awaiting dejon confirm";

            if (!movetoActive)
                return "idle";

            if (!IsGroupMovementMonitoring())
                return "not group movement";

            if (StallDestinationDistanceMeters is float distance
                && distance < FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters)
                return "near destination";

            if (StallElapsedSeconds >= StallThresholdSeconds)
                return "triggering";

            return "monitoring";
        }
    }

    public static void Reset()
    {
        movetoActive = false;
        awaitingDejonConfirm = false;
        hasAnchorPosition = false;
        anchorPositionTick = 0;
    }

    public static void NotifyMoveIssued() => movetoActive = true;

    public static void NotifyStopped()
    {
        movetoActive = false;
        hasAnchorPosition = false;
        anchorPositionTick = 0;
    }

    public static void Update(bool hasNavigationIntent)
    {
        IsMonitoringStall = false;

        if (!AutomationContext.CanRunInFrontlineMatch || Player.IsDead)
        {
            Reset();
            return;
        }

        if (awaitingDejonConfirm)
        {
            TryConfirmDejon();
            return;
        }

        if (!hasNavigationIntent && !movetoActive)
        {
            ClearAnchor();
            return;
        }

        if (!movetoActive || !Player.Available || Player.Object == null)
            return;

        if (!CanMonitorStall())
        {
            ClearAnchor();
            return;
        }

        var currentPosition = Player.Object.Position;
        if (!hasAnchorPosition
            || !GameCoords.AreNear(
                anchorPosition,
                currentPosition,
                FrontlineConstants.NaviStuckDejonPositionThresholdMeters))
        {
            anchorPosition = currentPosition;
            hasAnchorPosition = true;
            anchorPositionTick = Environment.TickCount64;
            return;
        }

        IsMonitoringStall = true;

        if (Environment.TickCount64 - anchorPositionTick < FrontlineConstants.NaviStuckDejonStallMs)
            return;

        TriggerRecovery();
    }

    private static bool IsGroupMovementMonitoring() =>
        FollowTargetService.IsGroupMovementMode && !InitialMovementMode.IsActive;

    private static bool CanMonitorStall() =>
        IsGroupMovementMonitoring()
        && TryGetMoveDestinationDistance(out var distance)
        && distance >= FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters;

    private static bool TryGetMoveDestinationDistance(out float distanceMeters) =>
        FollowTargetService.TryGetMoveDestinationDistance(out distanceMeters);

    private static void ClearAnchor()
    {
        hasAnchorPosition = false;
        anchorPositionTick = 0;
    }

    private static void TriggerRecovery()
    {
        MovementCommands.Stop();
        movetoActive = false;
        ClearAnchor();
        awaitingDejonConfirm = true;
        Chat.ExecuteCommand($"/pvpaction {DejonActionName}");
    }

    private static void TryConfirmDejon()
    {
        foreach (var yesno in AddonFinder.YesNo)
        {
            if (!yesno.IsVisible || !yesno.IsAddonReady)
                continue;

            if (!EzThrottler.Throttle(
                    FrontlineConstants.ThrottleNaviStuckDejonConfirm,
                    FrontlineConstants.NaviStuckDejonConfirmThrottleMs))
                return;

            yesno.Yes();
            awaitingDejonConfirm = false;
            return;
        }
    }
}
