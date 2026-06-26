using System.Numerics;
using Dalamud.Game;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.UIHelpers;

namespace AutoFrontline.Services;

/// <summary>集団行動中にプレイヤー座標が一定時間変わらないとき vnav stop → Return（JP: デジョン）→ SelectYesno Yes。</summary>
internal static class NaviStuckDejonAutomation
{
    private static string ReturnActionName =>
        Svc.ClientState.ClientLanguage == ClientLanguage.Japanese ? "デジョン" : "Return";

    private static bool movetoActive;
    private static bool awaitingReturnConfirm;
    private static Vector3 anchorPosition;
    private static bool hasAnchorPosition;
    private static long anchorPositionTick;

    public static bool IsAwaitingConfirm => awaitingReturnConfirm;
    public static bool IsMonitoringStall { get; private set; }

    public static float StallElapsedSeconds =>
        hasAnchorPosition ? (Environment.TickCount64 - anchorPositionTick) / 1000f : 0f;

    public static float StallThresholdSeconds => C.DejonStallSeconds;

    private static long StallThresholdMs =>
        (long)(Math.Clamp(
            C.DejonStallSeconds,
            FrontlineConstants.DejonStallSecondsMin,
            FrontlineConstants.DejonStallSecondsMax) * 1000f);

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

            if (!InitialMovementMode.HasLeftSpawnExclusion)
                return "before first spawn exit";

            if (!TryGetStallDestinationDistance(out var distance))
                return "no move destination";

            if (distance < FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters)
                return $"destination < {FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters}m";

            return string.Empty;
        }
    }

    public static string StallPhaseLabel
    {
        get
        {
            if (awaitingReturnConfirm)
                return "awaiting return confirm";

            if (!movetoActive)
                return "idle";

            if (!IsGroupMovementMonitoring())
                return "not group movement";

            if (TryGetStallDestinationDistance(out var distance)
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
        awaitingReturnConfirm = false;
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

        if (awaitingReturnConfirm)
        {
            TryConfirmReturn();
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

        if (Environment.TickCount64 - anchorPositionTick < StallThresholdMs)
            return;

        TriggerRecovery();
    }

    private static bool IsGroupMovementMonitoring() =>
        FollowTargetService.IsGroupMovementMode && InitialMovementMode.HasLeftSpawnExclusion;

    private static bool CanMonitorStall() =>
        IsGroupMovementMonitoring()
        && TryGetStallDestinationDistance(out var distance)
        && distance >= FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters;

    private static bool TryGetStallDestinationDistance(out float distanceMeters)
    {
        if (!Player.Available || Player.Object == null)
        {
            distanceMeters = 0;
            return false;
        }

        if (FollowTargetService.TryGetMoveDestinationDistance(out distanceMeters))
            return true;

        if (FollowTargetService.LastMoveTarget is Vector3 target)
        {
            distanceMeters = Vector3.Distance(Player.Object.Position, target);
            return true;
        }

        distanceMeters = 0;
        return false;
    }

    private static void ClearAnchor()
    {
        hasAnchorPosition = false;
        anchorPositionTick = 0;
    }

    private static void TriggerRecovery()
    {
        MovementCommands.Stop();
        awaitingReturnConfirm = true;
        Chat.ExecuteCommand($"/pvpaction {ReturnActionName}");
    }

    private static void TryConfirmReturn()
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
            awaitingReturnConfirm = false;
            return;
        }
    }
}
