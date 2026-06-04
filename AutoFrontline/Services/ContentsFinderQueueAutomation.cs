using Dalamud.Game.ClientState.Conditions;
using ECommons;
using ECommons.Automation.UIInput;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontline.Services;

/// <summary>
/// コンテンツルーレットでデイリーチャレンジ・フロントラインへ参加申請する。
/// リスト行は Text #6、選択確定は callback 3（AutoDuty SelectDuty と同じ Leaf インデックス）。
/// </summary>
internal static unsafe class ContentsFinderQueueAutomation
{
    private const uint DutyRouletteTabIndex = 0;
    private const uint ListItemLabelNodeId = 6;

    private static string lastStatus = "Idle";

    public static string LastStatus => lastStatus;

    public static void Update()
    {
        if (!AutoRunSession.Active || C.Mode != PluginMode.Loop)
        {
            ResetState();
            return;
        }

        if (!RequiredPlugins.IsAutomationActive)
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleContentsFinderQueue, FrontlineConstants.ContentsFinderQueueThrottleMs))
            return;

        if (!Player.Available || Player.IsDead)
        {
            lastStatus = "Waiting for player";
            return;
        }

        if (!DailyFrontlineRouletteResolver.TryResolve(out var rouletteRowId, out _))
        {
            lastStatus = "Daily Frontline roulette not found";
            return;
        }

        if (Svc.Condition[ConditionFlag.InDutyQueue])
        {
            lastStatus = "In duty queue";
            return;
        }

        if (GenericHelpers.TryGetAddonByName("ContentsFinderConfirm", out AtkUnitBase* confirm)
            && confirm->IsVisible
            && GenericHelpers.IsAddonReady(confirm))
        {
            lastStatus = "Waiting for duty confirm";
            return;
        }

        if (FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
        {
            lastStatus = "In Frontline";
            return;
        }

        if (!AgentHUD.Instance()->IsMainCommandEnabled(33))
        {
            lastStatus = "Contents Finder unavailable";
            return;
        }

        if (!GenericHelpers.TryGetAddonByName("ContentsFinder", out AddonContentsFinder* addon)
            || !GenericHelpers.IsAddonReady((AtkUnitBase*)addon))
        {
            lastStatus = "Opening Contents Finder (roulette)";
            AgentContentsFinder.Instance()->OpenRouletteDuty(rouletteRowId);
            return;
        }

        if (!IsDutyRouletteTabSelected(addon))
        {
            lastStatus = "Switching to Contents Roulette tab";
            EnsureDutyRouletteTab(addon);
            return;
        }

        if (addon->DutyList->Items.Count == 0)
        {
            lastStatus = "Duty list empty";
            return;
        }

        var agent = AgentContentsFinder.Instance();

        if (HasWrongQueuedContent(agent, rouletteRowId))
        {
            lastStatus = "Clearing wrong selection";
            ECommons.Automation.Callback.Fire((AtkUnitBase*)addon, true, 12, 1);
            return;
        }

        if (!IsDailyFrontlineQueued(agent, rouletteRowId))
        {
            QueueDailyFrontlineSelection(addon, agent, rouletteRowId);
            return;
        }

        lastStatus = "Joining queue";
        TryClickJoin(addon);
    }

    public static void ResetState() => lastStatus = "Idle";

    private static bool IsDailyFrontlineQueued(AgentContentsFinder* agent, byte rouletteRowId)
    {
        var selected = agent->SelectedContent;
        for (var i = 0; i < selected.Count; i++)
        {
            var entry = selected[i];
            if (entry.ContentType == ContentsType.Roulette && entry.Id == rouletteRowId)
                return true;
        }

        return false;
    }

    private static bool HasWrongQueuedContent(AgentContentsFinder* agent, byte rouletteRowId)
    {
        var selected = agent->SelectedContent;
        for (var i = 0; i < selected.Count; i++)
        {
            var entry = selected[i];
            if (entry.ContentType == ContentsType.Roulette && entry.Id != rouletteRowId)
                return true;

            if (entry.ContentType == ContentsType.Regular)
                return true;
        }

        return false;
    }

    private static void QueueDailyFrontlineSelection(
        AddonContentsFinder* addon,
        AgentContentsFinder* agent,
        byte rouletteRowId)
    {
        TryExpandDailyChallengeGroup(addon);

        agent->InterfaceSub.LoadContentRoulette(rouletteRowId);
        agent->UpdateAddon();

        if (TryToggleFrontlineSelection(addon, out var leafIndex, out var vectorIndex))
        {
            lastStatus = $"Selecting Frontline (leaf {leafIndex}, vec {vectorIndex})";
            return;
        }

        if (DetailShowsFrontline(addon))
        {
            lastStatus = "Retrying Frontline select";
            return;
        }

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleContentsFinderOpen, 500, true))
            return;

        lastStatus = "Opening Daily Frontline roulette";
        agent->OpenRouletteDuty(rouletteRowId);
    }

    /// <summary>callback 3 でチェックをトグル（表示スロットではなくツリー vector インデックス）。</summary>
    private static bool TryToggleFrontlineSelection(
        AddonContentsFinder* addon,
        out uint leafIndex,
        out int vectorIndex)
    {
        leafIndex = 0;
        vectorIndex = -1;

        if (!TryGetFrontlineVectorIndex(addon, out vectorIndex))
            return false;

        leafIndex = GetLeafCallbackIndex(addon, vectorIndex);
        ECommons.Automation.Callback.Fire((AtkUnitBase*)addon, true, 3, leafIndex);
        return true;
    }

    private static bool TryGetFrontlineVectorIndex(AddonContentsFinder* addon, out int vectorIndex)
    {
        var items = addon->DutyList->Items;
        for (var i = 0; i < items.Count; i++)
        {
            var treeItem = items[i].Value;
            if (treeItem == null || IsGroupHeader(*treeItem))
                continue;

            if (treeItem->Renderer != null
                && DailyFrontlineRouletteResolver.MatchesFrontlineListLabel(
                    GetRendererLabelText(treeItem->Renderer)))
            {
                vectorIndex = i;
                return true;
            }

            if (DailyFrontlineRouletteResolver.MatchesFrontlineListLabel(GetTreeListItemText(*treeItem)))
            {
                vectorIndex = i;
                return true;
            }
        }

        if (!DetailShowsFrontline(addon))
        {
            vectorIndex = -1;
            return false;
        }

        vectorIndex = addon->DutyList->SelectedItemIndex;
        if (vectorIndex < 0)
            vectorIndex = (int)addon->SelectedRow;

        return vectorIndex >= 0 && vectorIndex < items.Count;
    }

    private static uint GetLeafCallbackIndex(AddonContentsFinder* addon, int beforeVectorIndex)
    {
        if (beforeVectorIndex <= 0)
            return 1;

        var items = addon->DutyList->Items;
        var limit = beforeVectorIndex < items.Count ? beforeVectorIndex : items.Count;
        uint count = 0;

        for (var i = 0; i < limit; i++)
        {
            var treeItem = items[i].Value;
            if (treeItem == null)
                continue;

            if (treeItem->UIntValues[0] is 0 or 1)
                count++;
        }

        return count + 1;
    }

    private static bool DetailShowsFrontline(AddonContentsFinder* addon) =>
        DailyFrontlineRouletteResolver.MatchesFrontlineDetailName(GetSelectedDutyName(addon));

    private static string GetSelectedDutyName(AddonContentsFinder* addon)
    {
        if (addon->AtkValuesCount <= 18)
            return string.Empty;

        return addon->AtkValues[18].GetValueAsString()
            .Replace("\u0002\u001a\u0002\u0002\u0003", string.Empty)
            .Replace("\u0002\u001a\u0002\u0001\u0003", string.Empty)
            .Replace("\u0002\u001f\u0001\u0003", "\u2013");
    }

    private static string GetRendererLabelText(AtkComponentListItemRenderer* renderer)
    {
        var node = ((AtkComponentBase*)renderer)->GetTextNodeById(ListItemLabelNodeId);
        if (node == null)
            return string.Empty;

        var textNode = node->GetAsAtkTextNode();
        if (textNode == null)
            return string.Empty;

        var text = textNode->NodeText.GetText();
        if (!BilingualTextMatcher.IsNullOrWhiteSpace(text))
            return text;

        return GenericHelpers.ReadSeString(&textNode->NodeText).TextValue.ToString();
    }

    private static void TryExpandDailyChallengeGroup(AddonContentsFinder* addon)
    {
        var items = addon->DutyList->Items;
        for (var i = 0; i < items.Count; i++)
        {
            var treeItem = items[i].Value;
            if (treeItem == null || !IsGroupHeader(*treeItem))
                continue;

            if (!DailyFrontlineRouletteResolver.IsDailyChallengeSectionHeader(GetTreeListItemText(*treeItem)))
                continue;

            addon->DutyList->ExpandGroupExclusively(treeItem);
            addon->DutyList->LayoutRefreshPending = true;
            return;
        }
    }

    private static void TryClickJoin(AddonContentsFinder* addon)
    {
        var joinButton = addon->JoinButton;
        if (joinButton != null)
        {
            ((AtkComponentButton*)joinButton)->ClickAddonButton((AtkUnitBase*)addon);
            return;
        }

        ECommons.Automation.Callback.Fire((AtkUnitBase*)addon, true, 12, 0);
    }

    private static bool IsDutyRouletteTabSelected(AddonContentsFinder* addon) =>
        addon->SelectedRadioButton == DutyRouletteTabIndex;

    private static void EnsureDutyRouletteTab(AddonContentsFinder* addon)
    {
        var radio = addon->DutyRouletteRadioButton;
        if (radio == null)
            return;

        ((AtkComponentButton*)radio)->ClickAddonButton((AtkUnitBase*)addon);
    }

    private static bool IsGroupHeader(AtkComponentTreeListItem item) =>
        item.UIntValues[0] is 2 or 4;

    private static string GetTreeListItemText(AtkComponentTreeListItem item)
    {
        for (var s = 0; s < item.StringValues.Count; s++)
        {
            if (item.StringValues[s].Value == null)
                continue;

            var text = item.StringValues[s].Value->ToString();
            if (!BilingualTextMatcher.IsNullOrWhiteSpace(text))
                return text;
        }

        if (item.Renderer == null)
            return string.Empty;

        return GetRendererLabelText(item.Renderer);
    }
}
