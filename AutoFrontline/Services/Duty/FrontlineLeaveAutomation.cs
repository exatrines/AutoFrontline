using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontline.Services;

/// <summary>Leaves the duty while the result screen is shown.</summary>
public static unsafe class FrontlineLeaveAutomation
{
    public static bool IsRecordScreenVisible => IsRecordScreenReady();
    public static bool IsBlockingAutomation => IsRecordScreenReady();

    public static void Update()
    {
        if (!RequiredPlugins.IsAutomationActive)
            return;

        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            return;

        if (!RequiredPlugins.ShouldAutoLeave || !IsRecordScreenReady())
            return;

        if (!EventFramework.CanLeaveCurrentContent())
            return;

        EventFramework.LeaveCurrentContent();
    }

    private static bool IsRecordScreenReady()
    {
        if (!GenericHelpers.IsScreenReady())
            return false;

        if (!GenericHelpers.TryGetAddonByName("FrontlineRecord", out AtkUnitBase* addon))
            return false;

        return addon->IsVisible && GenericHelpers.IsAddonReady(addon);
    }
}
