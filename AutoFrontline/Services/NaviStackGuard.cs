using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>moveto 連続リフレッシュによるスタック悪化を抑える。</summary>
internal static class NaviStackGuard
{
    private static int refreshCount;
    private static Vector3 anchorPosition;
    private static bool isLocked;
    private static Vector3? lockedTarget;

    public static bool IsRefreshLocked => isLocked;
    public static int RefreshCount => refreshCount;
    public static Vector3? LockedTarget => lockedTarget;

    public static void Update()
    {
        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType) || Player.Object == null)
        {
            Reset();
            return;
        }

        if (!RequiredPlugins.IsAutomationActive)
        {
            Reset();
            return;
        }

        var currentPos = Player.Object.Position;

        if (isLocked && lockedTarget is Vector3 locked)
        {
            if (GameCoords.IsWithinRadius(currentPos, locked, FrontlineConstants.NaviStackArrivalDistanceMeters))
                Reset();

            return;
        }

        if (refreshCount > 0
            && !GameCoords.AreNear(anchorPosition, currentPos, FrontlineConstants.NaviStackPositionThresholdMeters))
        {
            refreshCount = 0;
            anchorPosition = currentPos;
        }
    }

    public static bool ShouldSuppressMoveRefresh()
    {
        if (!isLocked || lockedTarget is not Vector3 locked || Player.Object == null)
            return false;

        return !GameCoords.IsWithinRadius(
            Player.Object.Position,
            locked,
            FrontlineConstants.NaviStackArrivalDistanceMeters);
    }

    public static void OnMoveIssued(Vector3 target)
    {
        if (Player.Object == null)
            return;

        var currentPos = Player.Object.Position;
        if (refreshCount == 0)
            anchorPosition = currentPos;

        refreshCount++;

        if (refreshCount < FrontlineConstants.NaviStackRefreshThreshold)
            return;

        if (!GameCoords.AreNear(anchorPosition, currentPos, FrontlineConstants.NaviStackPositionThresholdMeters))
            return;

        isLocked = true;
        lockedTarget = target;
    }

    public static void Reset()
    {
        refreshCount = 0;
        isLocked = false;
        lockedTarget = null;

        if (Player.Object != null)
            anchorPosition = Player.Object.Position;
    }
}
