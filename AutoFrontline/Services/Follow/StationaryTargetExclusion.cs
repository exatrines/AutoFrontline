using System.Collections.Generic;
using System.Linq;

namespace AutoFrontline.Services;

internal enum FollowTargetExclusionReason
{
    Stationary,
    RepeatedPick,
}

internal readonly record struct FollowTargetExclusionDebugEntry(
    string Name,
    FollowTargetExclusionReason Reason,
    float RemainingSeconds);

/// <summary>静止している追跡対象を設定時間、集団選定から除外する。</summary>
internal static class StationaryTargetExclusion
{
    private sealed class ExclusionEntry
    {
        public required long ExcludedUntilTick { get; init; }
        public required FollowTargetExclusionReason Reason { get; init; }
        public required string Name { get; init; }
    }

    private static readonly Dictionary<ulong, ExclusionEntry> Excluded = [];

    internal static IReadOnlyCollection<ulong> ExcludedIds
    {
        get
        {
            PurgeExpired();
            return Excluded.Keys.ToList();
        }
    }

    internal static IReadOnlyList<FollowTargetExclusionDebugEntry> GetDebugEntries()
    {
        PurgeExpired();
        var now = Environment.TickCount64;
        return Excluded
            .Select(kvp => new FollowTargetExclusionDebugEntry(
                kvp.Value.Name,
                kvp.Value.Reason,
                Math.Max(0f, (kvp.Value.ExcludedUntilTick - now) / 1000f)))
            .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static void Exclude(ulong contentId, FollowTargetExclusionReason reason, string name = "")
    {
        if (contentId == 0 || C.StationaryTargetExclusionSeconds <= 0)
            return;

        Excluded[contentId] = new ExclusionEntry
        {
            ExcludedUntilTick = Environment.TickCount64 + C.StationaryTargetExclusionSeconds * 1000L,
            Reason = reason,
            Name = string.IsNullOrWhiteSpace(name) ? ResolveName(contentId) : name,
        };
    }

    internal static void NotifyPicked(ulong contentId)
    {
        if (contentId != 0)
            Excluded.Remove(contentId);
    }

    internal static void Clear() => Excluded.Clear();

    private static string ResolveName(ulong contentId)
    {
        var member = AllianceMemberCache.GetMembers()
            .FirstOrDefault(m => m.ContentId == contentId);
        return member.ContentId != 0 ? member.Name : $"ContentId {contentId}";
    }

    private static void PurgeExpired()
    {
        if (Excluded.Count == 0)
            return;

        var now = Environment.TickCount64;
        foreach (var contentId in Excluded.Keys.ToList())
        {
            if (Excluded[contentId].ExcludedUntilTick <= now)
                Excluded.Remove(contentId);
        }
    }
}
