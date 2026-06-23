using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>アライアンスチャットの直近発言者を軍師として追跡する。</summary>
public static class AllianceCommanderTracker
{
    public static ulong LatestCommanderContentId { get; private set; }
    public static string LatestCommanderName { get; private set; } = string.Empty;
    public static bool IsFollowPending { get; private set; }
    public static bool NeedsReselect { get; private set; }

    public static void Init() => Svc.Chat.ChatMessage += OnChatMessage;

    public static void Dispose()
    {
        Svc.Chat.ChatMessage -= OnChatMessage;
        Clear();
    }

    public static void Update()
    {
        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
        {
            if (LatestCommanderContentId != 0 || IsFollowPending)
                Clear();
            return;
        }

        if (!IsFollowPending || LatestCommanderContentId == 0)
            return;

        var members = AllianceMemberCache.GetMembers();
        var commander = members.FirstOrDefault(m => m.ContentId == LatestCommanderContentId);
        if (commander.ContentId == 0 || commander.IsDead)
            ClearDueToDeath();
    }

    public static bool TryGetCommander(
        IReadOnlyList<AllianceMemberSnapshot> members,
        out AllianceMemberSnapshot commander)
    {
        commander = default;
        if (!IsFollowPending || LatestCommanderContentId == 0)
            return false;

        var found = members.FirstOrDefault(m => m.ContentId == LatestCommanderContentId);
        if (found.ContentId == 0 || found.IsDead)
            return false;

        commander = found;
        return true;
    }

    public static void CompleteFollow()
    {
        IsFollowPending = false;
        NeedsReselect = true;
    }

    public static void ClearDueToDeath()
    {
        if (!IsFollowPending)
            return;

        IsFollowPending = false;
        NeedsReselect = true;
    }

    public static void ConsumeNeedsReselect() => NeedsReselect = false;

    /// <summary>戦闘優先などで軍師追従要求を取り消す（LatestCommander は Debug 用に残す）。</summary>
    public static void DismissFollowRequest() => IsFollowPending = false;

    public static void Clear()
    {
        LatestCommanderContentId = 0;
        LatestCommanderName = string.Empty;
        IsFollowPending = false;
        NeedsReselect = false;
    }

    private static void OnChatMessage(IHandleableChatMessage message)
    {
        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            return;

        if (message.LogKind != XivChatType.Alliance)
            return;

        var members = AllianceMemberCache.GetMembers();
        if (!TryResolveSpeaker(message, members, out var member))
            return;

        if (AllyMemberFilters.IsSelf(member))
            return;

        LatestCommanderContentId = member.ContentId;
        LatestCommanderName = member.Name;

        if (!C.CommanderFollowEnabled)
            return;

        if (HostileModeFollow.IsEligible(members))
            return;

        IsFollowPending = true;
        NeedsReselect = true;
    }

    private static bool TryResolveSpeaker(
        IHandleableChatMessage message,
        IReadOnlyList<AllianceMemberSnapshot> members,
        out AllianceMemberSnapshot member)
    {
        member = default;

        if (GenericHelpers.TryDecodeSender(message.Sender, out var decoded) && !string.IsNullOrEmpty(decoded.Name))
        {
            if (TryMatchMember(decoded.Name, members, out member))
                return true;

            if (decoded.TryFind(out var pc))
                return TrySnapshotFromPlayer(pc, members, out member);
        }

        var textSender = NormalizeSenderName(message.Sender.GetText());
        if (!string.IsNullOrEmpty(textSender) && TryMatchMember(textSender, members, out member))
            return true;

        return false;
    }

    private static unsafe bool TrySnapshotFromPlayer(
        Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter pc,
        IReadOnlyList<AllianceMemberSnapshot> members,
        out AllianceMemberSnapshot member)
    {
        member = default;
        var contentId = pc.Struct()->ContentId;
        if (contentId == 0)
            return false;

        member = members.FirstOrDefault(m => m.ContentId == contentId);
        if (member.ContentId != 0)
            return true;

        member = new AllianceMemberSnapshot
        {
            Name = pc.Name.ToString(),
            ContentId = contentId,
            EntityId = pc.EntityId,
            Position = pc.Position,
            IsDead = pc.CurrentHp == 0,
        };
        return true;
    }

    internal static bool TryMatchMember(
        string senderName,
        IReadOnlyList<AllianceMemberSnapshot> members,
        out AllianceMemberSnapshot member)
    {
        member = default;
        if (string.IsNullOrEmpty(senderName))
            return false;

        var normalized = NormalizeSenderName(senderName);
        member = members.FirstOrDefault(m => NamesMatch(m.Name, normalized));
        return member.ContentId != 0;
    }

    private static bool NamesMatch(string memberName, string chatName)
    {
        var member = NormalizeSenderName(memberName);
        if (member == chatName)
            return true;

        return member.StartsWith(chatName, System.StringComparison.Ordinal)
               || chatName.StartsWith(member, System.StringComparison.Ordinal);
    }

    private static string NormalizeSenderName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        var cut = name.IndexOf('«');
        if (cut > 0)
            return name[..cut];

        cut = name.IndexOf('(');
        if (cut > 0)
            return name[..cut].TrimEnd();

        cut = name.IndexOf('@');
        if (cut > 0)
            return name[..cut];

        return name.Trim();
    }
}
