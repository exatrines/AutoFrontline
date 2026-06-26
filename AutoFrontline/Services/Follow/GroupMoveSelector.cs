using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>
/// 集団行動: 自分 50m 内で最も密集した味方を選ぶ。
/// 50m 内が 2 名未満のときは最寄り 2 名を候補に密集度判定する。
/// 同規模の集団が複数あるときは、自分に最も近い候補を選ぶ。
/// </summary>
internal static class GroupMoveSelector
{
    public static (AllianceMemberSnapshot Member, int NeighborCount)? Find(
        IReadOnlyList<AllianceMemberSnapshot> members,
        bool excludeSelf = false,
        IReadOnlyCollection<ulong> excludeContentIds = null)
    {
        if (Player.Object == null)
            return null;

        var excluded = excludeContentIds ?? [];
        var alive = members
            .Where(m => !m.IsDead
                        && (!excludeSelf || !AllyMemberFilters.IsSelf(m))
                        && !excluded.Contains(m.ContentId))
            .ToList();

        if (alive.Count == 0)
            return null;

        if (alive.Count == 1)
            return (alive[0], CountNeighbors(alive[0], alive));

        var selfPosition = Player.Object.Position;
        var selfRadiusSq = (float)(C.GroupMoveSelfSearchRadiusMeters * C.GroupMoveSelfSearchRadiusMeters);
        var withinSelfRange = alive
            .Where(m => Vector3.DistanceSquared(selfPosition, m.Position) <= selfRadiusSq)
            .ToList();

        var candidates = withinSelfRange.Count >= 2
            ? withinSelfRange
            : alive
                .OrderBy(m => Vector3.DistanceSquared(selfPosition, m.Position))
                .Take(2)
                .ToList();

        return FindDensestAmong(candidates, alive, selfPosition);
    }

    private static (AllianceMemberSnapshot Member, int NeighborCount)? FindDensestAmong(
        IReadOnlyList<AllianceMemberSnapshot> candidates,
        IReadOnlyList<AllianceMemberSnapshot> alive,
        Vector3 selfPosition)
    {
        var radiusSq = FrontlineConstants.GroupMoveDensityRadiusMeters
            * FrontlineConstants.GroupMoveDensityRadiusMeters;
        var counts = candidates
            .Select(candidate => (Member: candidate, Count: alive.Count(m =>
                Vector3.DistanceSquared(candidate.Position, m.Position) <= radiusSq)))
            .ToList();

        var maxCount = counts.Max(c => c.Count);
        var best = counts
            .Where(c => c.Count == maxCount)
            .OrderBy(c => Vector3.DistanceSquared(selfPosition, c.Member.Position))
            .First();

        return (best.Member, maxCount);
    }

    private static int CountNeighbors(
        AllianceMemberSnapshot member,
        IReadOnlyList<AllianceMemberSnapshot> alive)
    {
        var radiusSq = FrontlineConstants.GroupMoveDensityRadiusMeters
            * FrontlineConstants.GroupMoveDensityRadiusMeters;
        return alive.Count(m => Vector3.DistanceSquared(member.Position, m.Position) <= radiusSq);
    }
}
