using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>
/// 集団行動: search radius 内に 2 名以上いればその中で最も密集した味方を選ぶ。
/// radius 内が 1 名のときは radius 内の単独より、30m 以内 2 名以上の集団のうち
/// 中心が最も近いものを選ぶ（集団がなければ radius 内の単独を追う）。
/// radius 内が 0 名のときは最寄り 2 名を候補に密集度判定する。
/// 同規模の集団が複数あるときは、自分に最も近い候補を選ぶ。
/// </summary>
internal static class GroupMoveSelector
{
    private const int MinGroupMemberCount = 2;

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

        var selfPosition = Player.Object.Position;
        var selfRadiusSq = (float)(C.GroupMoveSelfSearchRadiusMeters * C.GroupMoveSelfSearchRadiusMeters);
        var withinSelfRange = alive
            .Where(m => Vector3.DistanceSquared(selfPosition, m.Position) <= selfRadiusSq)
            .ToList();

        if (withinSelfRange.Count >= 2)
            return FindDensestAmong(withinSelfRange, alive, selfPosition);

        if (withinSelfRange.Count == 1)
        {
            var groupPick = FindNearestGroupTarget(alive, selfPosition);
            if (groupPick != null)
                return groupPick;

            var solo = withinSelfRange[0];
            return (solo, CountNeighbors(solo, alive));
        }

        var candidates = alive
            .OrderBy(m => Vector3.DistanceSquared(selfPosition, m.Position))
            .Take(2)
            .ToList();

        return FindDensestAmong(candidates, alive, selfPosition);
    }

    private static (AllianceMemberSnapshot Member, int NeighborCount)? FindNearestGroupTarget(
        IReadOnlyList<AllianceMemberSnapshot> alive,
        Vector3 selfPosition)
    {
        var groups = FindGroups(alive);
        if (groups.Count == 0)
            return null;

        var nearestGroup = groups
            .OrderBy(g => Vector3.DistanceSquared(selfPosition, GetGroupCenter(g)))
            .First();

        return FindDensestAmong(nearestGroup, alive, selfPosition);
    }

    /// <summary>30m 以内で連結した 2 名以上の味方クラスタを列挙する。</summary>
    private static List<List<AllianceMemberSnapshot>> FindGroups(IReadOnlyList<AllianceMemberSnapshot> alive)
    {
        var radiusSq = FrontlineConstants.GroupMoveDensityRadiusMeters
            * FrontlineConstants.GroupMoveDensityRadiusMeters;
        var visited = new bool[alive.Count];
        var groups = new List<List<AllianceMemberSnapshot>>();

        for (var i = 0; i < alive.Count; i++)
        {
            if (visited[i])
                continue;

            var component = new List<AllianceMemberSnapshot>();
            var queue = new Queue<int>();
            queue.Enqueue(i);
            visited[i] = true;

            while (queue.Count > 0)
            {
                var index = queue.Dequeue();
                component.Add(alive[index]);

                for (var j = 0; j < alive.Count; j++)
                {
                    if (visited[j])
                        continue;

                    if (Vector3.DistanceSquared(alive[index].Position, alive[j].Position) <= radiusSq)
                    {
                        visited[j] = true;
                        queue.Enqueue(j);
                    }
                }
            }

            if (component.Count >= MinGroupMemberCount)
                groups.Add(component);
        }

        return groups;
    }

    private static Vector3 GetGroupCenter(IReadOnlyList<AllianceMemberSnapshot> group)
    {
        var sum = Vector3.Zero;
        foreach (var member in group)
            sum += member.Position;

        return sum / group.Count;
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
