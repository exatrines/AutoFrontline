using ECommons.Automation.UIInput;
using ECommons.UIHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontLine.Services;

/// <summary>試合終了画面からの自動退出。</summary>
public static unsafe class FrontlineLeaveAutomation
{
    private static readonly string[] RecordAddonNames = ["FrontlineRecord", "FrontLineRecord"];

    private static bool leaveClickedForRecord;
    private static bool pendingLeaveConfirm;
    private static long leaveClickedTick;

    public static bool PendingLeaveConfirm => pendingLeaveConfirm;

    public static void Update()
    {
        if (!RequiredPlugins.IsAutomationActive)
            return;

        ExpirePendingConfirmIfTimedOut();

        if (TryConfirmYesno())
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleLeaveRecord, FrontlineConstants.LeaveRecordThrottleMs))
            return;

        TryClickRecordLeaveButton();
    }

    private static void ExpirePendingConfirmIfTimedOut()
    {
        if (!pendingLeaveConfirm)
            return;

        if (Environment.TickCount64 - leaveClickedTick > FrontlineConstants.LeaveConfirmTimeoutMs)
            pendingLeaveConfirm = false;
    }

    private static bool TryConfirmYesno()
    {
        if (!pendingLeaveConfirm && !IsRecordAddonVisible())
            return false;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleLeaveYesno, FrontlineConstants.LeaveYesnoThrottleMs))
            return false;

        foreach (var yesno in AddonFinder.YesNo)
        {
            if (!yesno.IsVisible)
                continue;

            if (!pendingLeaveConfirm && !LeaveDialogText.IsLeaveConfirmation(ReadYesnoText(yesno)))
                continue;

            if (!TryClickYes(yesno))
                continue;

            if (!yesno.IsVisible)
                pendingLeaveConfirm = false;

            return true;
        }

        return false;
    }

    private static void TryClickRecordLeaveButton()
    {
        if (!TryGetRecordAddon(out var addon))
        {
            leaveClickedForRecord = false;
            return;
        }

        if (!addon->IsVisible)
        {
            leaveClickedForRecord = false;
            return;
        }

        if (leaveClickedForRecord)
            return;

        if (!ClickAddonButton(addon, FrontlineConstants.LeaveRecordButtonNodeId))
            return;

        leaveClickedForRecord = true;
        pendingLeaveConfirm = true;
        leaveClickedTick = Environment.TickCount64;
    }

    private static bool IsRecordAddonVisible() =>
        TryGetRecordAddon(out var addon) && addon->IsVisible;

    private static bool TryGetRecordAddon(out AtkUnitBase* addon)
    {
        foreach (var name in RecordAddonNames)
        {
            if (GenericHelpers.TryGetAddonByName(name, out addon))
                return true;
        }

        addon = null;
        return false;
    }

    private static bool TryClickYes(AddonMaster.SelectYesno yesno)
    {
        var addon = (AddonSelectYesno*)yesno.Base;
        var yesButton = addon->YesButton;

        if (yesButton == null)
            return FireYesCallback(yesno.Base);

        EnableButtonIfDisabled(yesButton);

        if (yesButton->IsEnabled && yesButton->AtkResNode->IsVisible())
        {
            yesButton->ClickAddonButton(yesno.Base);
            if (!yesno.Base->IsVisible)
                return true;
        }

        if (FireYesCallback(yesno.Base))
            return !yesno.Base->IsVisible;

        yesButton->ClickAddonButton(yesno.Base);
        return !yesno.Base->IsVisible;
    }

    private static bool FireYesCallback(AtkUnitBase* addon)
    {
        try
        {
            ECommons.Automation.Callback.Fire(addon, true, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool ClickAddonButton(AtkUnitBase* addon, uint buttonId)
    {
        var button = addon->GetComponentButtonById(buttonId);
        if (button == null || !button->AtkResNode->IsVisible())
            return false;

        EnableButtonIfDisabled(button);
        button->ClickAddonButton(addon);
        return true;
    }

    private static void EnableButtonIfDisabled(AtkComponentButton* button)
    {
        if (button == null || button->IsEnabled)
            return;

        var flagsPtr = (ushort*)&button->AtkComponentBase.OwnerNode->AtkResNode.NodeFlags;
        *flagsPtr ^= 1 << 5;
    }

    private static string ReadYesnoText(AddonMaster.SelectYesno yesno)
    {
        try
        {
            var addon = (AddonSelectYesno*)yesno.Base;
            return addon->PromptText != null ? yesno.Text : yesno.TextLegacy;
        }
        catch
        {
            return string.Empty;
        }
    }
}
