using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AutoFrontline.Services;

/// <summary>追跡アンカーから moveto 先を算出する。</summary>
internal static class FollowMoveResolver
{
    internal static Vector3? TryGetMoveTarget()
    {
        if (FollowTargetState.TrackedContentId == 0)
            return null;

        if (!TryResolveAnchorPosition(out var anchor))
            return null;

        if (FollowTargetState.CachedTrackedPosition is Vector3 cached
            && FollowTargetState.LastMoveTarget is Vector3 existingTarget
            && GameCoords.AreNear(
                cached,
                anchor,
                FrontlineConstants.PositionUnchangedThresholdMeters))
            return existingTarget;

        FollowTargetState.CachedTrackedPosition = anchor;
        FollowTargetState.LastMoveTarget = FollowMovePlanner.CreateDestination(
            anchor,
            FollowTargetState.SelectionMode);
        return FollowTargetState.LastMoveTarget;
    }

    private static bool TryResolveAnchorPosition(out Vector3 anchor)
    {
        anchor = default;

        var members = AllianceMemberCache.GetMembers();
        var tracked = FindTracked(members);
        if (tracked == null || tracked.Value.IsDead)
        {
            if (FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander && C.CommanderFollowEnabled)
            {
                AllianceCommanderTracker.ClearDueToDeath();
                FollowModeSelector.SelectFallbackTarget(members);
            }
            else
            {
                FollowModeSelector.SelectNewTarget(members);
            }

            tracked = FindTracked(members);
        }

        if (tracked == null)
        {
            FollowTargetState.ClearMoveState();
            return false;
        }

        if (FollowTargetState.SelectionMode == FollowSelectionMode.Hostile
            && HostileModeFollow.TryCreateEligibleSnapshot(members, out var hostileSnapshot))
        {
            anchor = HostileModeFollow.GetNavPosition(hostileSnapshot);
            return true;
        }

        anchor = tracked.Value.Position;
        return true;
    }

    private static AllianceMemberSnapshot? FindTracked(IReadOnlyList<AllianceMemberSnapshot> members) =>
        members.FirstOrDefault(m => m.ContentId == FollowTargetState.TrackedContentId);
}
