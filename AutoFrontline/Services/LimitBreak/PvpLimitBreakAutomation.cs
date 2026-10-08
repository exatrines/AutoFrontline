using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>Uses /pvpaction for the current job while in hostile follow.</summary>
internal static class PvpLimitBreakAutomation
{
    public static void Update()
    {
        if (!AutomationContext.CanRunInFrontlineMatch)
            return;

        if (!FollowTargetService.IsHostileMode)
            return;

        if (!Player.Available || Player.Object == null)
            return;

        if (!PvpLimitBreakCatalog.TryGetEnabledActionForJob(Player.Job, out var actionName))
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottlePvpLimitBreak, FrontlineConstants.PvpLimitBreakIntervalMs))
            return;

        Chat.ExecuteCommand($"/pvpaction {actionName}");
    }
}
