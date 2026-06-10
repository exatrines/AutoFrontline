using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>戦闘モード（敵対追従）中に、有効なジョブ設定の PvP LB を /pvpaction で実行する。</summary>
internal static class PvpLimitBreakAutomation
{
    public static string LastActionName { get; private set; } = string.Empty;
    public static bool LastAttempted { get; private set; }

    public static void Update()
    {
        LastAttempted = false;

        if (!AutomationContext.CanRunInFrontlineMatch)
        {
            LastActionName = string.Empty;
            return;
        }

        if (!FollowTargetService.IsHostileMode)
        {
            LastActionName = string.Empty;
            return;
        }

        if (!Player.Available || Player.Object == null)
            return;

        if (!PvpLimitBreakCatalog.TryGetEnabledActionForJob(Player.Job, out var actionName))
        {
            LastActionName = string.Empty;
            return;
        }

        LastActionName = actionName;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottlePvpLimitBreak, FrontlineConstants.PvpLimitBreakIntervalMs))
            return;

        Chat.ExecuteCommand($"/pvpaction {actionName}");
        LastAttempted = true;
    }
}
