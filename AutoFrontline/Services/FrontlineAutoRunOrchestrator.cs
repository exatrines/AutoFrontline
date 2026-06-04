using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontline.Services;

/// <summary>Loop モードのループ（キュー・入室カウント・MaxCount 停止）を制御する。</summary>
internal static unsafe class FrontlineAutoRunOrchestrator
{
    public static string LastPhase { get; private set; } = "Idle";

    public static void Update()
    {
        if (C.Mode != PluginMode.Loop || !AutoRunSession.Active)
        {
            LastPhase = "Idle";
            return;
        }

        UpdateTerritoryCount();
        TryFinishAfterLeave();

        if (!AutoRunSession.Active)
        {
            LastPhase = "Stopped";
            ContentsFinderQueueAutomation.ResetState();
            return;
        }

        if (CanQueue())
        {
            LastPhase = $"Queue ({AutoRunSession.CurrentCount}/{C.AutoMaxCount})";
            ContentsFinderQueueAutomation.Update();
            return;
        }

        if (FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            LastPhase = $"In match ({AutoRunSession.CurrentCount}/{C.AutoMaxCount})";
        else if (ConditionsVisible())
            LastPhase = "Duty confirm";
        else
            LastPhase = $"Waiting ({AutoRunSession.CurrentCount}/{C.AutoMaxCount})";
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
        ContentsFinderQueueAutomation.ResetState();
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

    private static bool ConditionsVisible()
    {
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("ContentsFinderConfirm", out var addon))
            return false;

        return addon->IsVisible && GenericHelpers.IsAddonReady(addon);
    }
}
