using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>Loop モードのループ（キュー・入室カウント・MaxCount 停止）を制御する。</summary>
internal static class FrontlineAutoRunOrchestrator
{
    public static void Update()
    {
        if (C.Mode != PluginMode.Loop || !AutoRunSession.Active)
            return;

        UpdateTerritoryCount();
        TryFinishAfterLeave();

        if (!AutoRunSession.Active)
            return;

        if (CanQueue())
            ContentsFinderQueueAutomation.Update();
    }

    private static void UpdateTerritoryCount()
    {
        var territoryId = Svc.ClientState.TerritoryType;
        var inFrontline = FrontlineFields.IsFrontline(territoryId);

        if (inFrontline && !AutoRunSession.WasInFrontline && !AutoRunSession.CountedEnterThisRound)
        {
            AutoRunSession.CurrentCount++;
            AutoRunSession.CountedEnterThisRound = true;

            if (AutoRunSession.CurrentCount >= C.AutoMaxCount)
                AutoRunSession.PendingStopAfterLeave = true;
        }

        if (!inFrontline)
            AutoRunSession.CountedEnterThisRound = false;

        AutoRunSession.WasInFrontline = inFrontline;
    }

    private static void TryFinishAfterLeave()
    {
        if (!AutoRunSession.PendingStopAfterLeave)
            return;

        if (FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            return;

        if (FrontlineLeaveAutomation.IsRecordScreenVisible)
            return;

        AutoRunSession.Stop();
    }

    private static bool CanQueue()
    {
        if (AutoRunSession.CurrentCount >= C.AutoMaxCount)
            return false;

        if (FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            return false;

        if (!Player.Available || Player.IsDead)
            return false;

        return true;
    }
}
