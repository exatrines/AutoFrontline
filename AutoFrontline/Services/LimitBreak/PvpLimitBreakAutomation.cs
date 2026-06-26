using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>戦闘モード（敵対追従）中に、有効なジョブ設定の PvP LB を /pvpaction で実行する。</summary>
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
