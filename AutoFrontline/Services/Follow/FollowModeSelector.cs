using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>追跡モードの優先順位に従い対象を選定する。</summary>
internal static class FollowModeSelector
{
    internal static void Update()
    {
        if (Player.Object == null)
        {
            FollowTargetState.ClearTarget();
            return;
        }

        var members = AllianceMemberCache.GetMembers();

        if (!C.CommanderFollowEnabled)
        {
            if (FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander
                || AllianceCommanderTracker.IsFollowPending)
            {
                AllianceCommanderTracker.DismissFollowRequest();
                if (FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander)
                {
                    SelectFallbackTarget(members);
                    return;
                }
            }
        }
        else
        {
            if (TryCompleteCommanderFollow(members))
                return;

            if (FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander
                && ShouldPreferCombatMode(members))
            {
                AllianceCommanderTracker.DismissFollowRequest();
                SelectFallbackTarget(members);
                return;
            }
        }

        if (AllianceCommanderTracker.NeedsReselect)
        {
            if (C.CommanderFollowEnabled && AllianceCommanderTracker.IsFollowPending)
                SelectNewTarget(members);
            else
                SelectFallbackTarget(members);

            if (!AllianceCommanderTracker.IsFollowPending
                || FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander)
                AllianceCommanderTracker.ConsumeNeedsReselect();

            return;
        }

        if (FollowTargetState.TrackedContentId != 0 && !IsTrackedAlive(members))
        {
            if (FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander && C.CommanderFollowEnabled)
                AllianceCommanderTracker.ClearDueToDeath();

            if (FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander && C.CommanderFollowEnabled)
                SelectFallbackTarget(members);
            else
                SelectNewTarget(members);

            return;
        }

        if (FollowTargetState.TrackedContentId == 0)
        {
            SelectNewTarget(members);
            return;
        }

        if (ShouldReselectOnTimer())
        {
            if (IsTrackedAnchorStationary(members))
            {
                UpdateStationaryStreak(FollowTargetState.TrackedContentId);
                if (FollowTargetState.StationaryStreakCount
                    >= FrontlineConstants.StationaryTargetExcludePickCount)
                {
                    StationaryTargetExclusion.Exclude(
                        FollowTargetState.TrackedContentId,
                        FollowTargetExclusionReason.Stationary,
                        FollowTargetState.LastTrackedMemberName);
                }
            }
            else
            {
                ResetStationaryStreak();
            }

            SelectNewTarget(members);
        }
    }

    internal static void SelectNewTarget(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        FollowTargetState.LastSelectionTick = Environment.TickCount64;

        if (Player.Object == null)
        {
            FollowTargetState.ClearTarget();
            return;
        }

        if (ShouldPreferCombatMode(members))
        {
            AllianceCommanderTracker.DismissFollowRequest();
            SelectFallbackTarget(members);
            return;
        }

        if (C.CommanderFollowEnabled && AllianceCommanderTracker.TryGetCommander(members, out var commander))
        {
            if (IsWithinCommanderFollowArrival(commander))
            {
                CompleteCommanderFollowAndFallback(members);
                return;
            }

            ApplyTarget(commander, FollowSelectionMode.FollowCommander);
            return;
        }

        SelectFallbackTarget(members);
    }

    internal static void SelectFallbackTarget(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        FollowTargetState.LastSelectionTick = Environment.TickCount64;

        if (Player.Object == null)
        {
            FollowTargetState.ClearTarget();
            return;
        }

        if (HostileModeFollow.TryCreateEligibleSnapshot(members, out var hostileSnapshot))
        {
            var ally = HostileModeFollow.GetTrackTarget(hostileSnapshot);
            ApplyTarget(ally, FollowSelectionMode.Hostile);
            return;
        }

        var pick = SelectGroupMovementTarget(members);
        if (pick == null)
        {
            FollowTargetState.ClearTarget();
            return;
        }

        ApplyTarget(pick.Value.Member, FollowSelectionMode.GroupMovement);
    }

    private static (AllianceMemberSnapshot Member, int NeighborCount)? SelectGroupMovementTarget(
        IReadOnlyList<AllianceMemberSnapshot> members)
    {
        var excluded = StationaryTargetExclusion.ExcludedIds;
        var pick = GroupMoveSelector.Find(members, excludeSelf: true, excludeContentIds: excluded);
        if (pick != null)
            return pick;

        if (excluded.Count == 0)
            return null;

        return GroupMoveSelector.Find(members, excludeSelf: true);
    }

    private static void ApplyTarget(AllianceMemberSnapshot picked, FollowSelectionMode mode)
    {
        FollowTargetState.TrackedContentId = picked.ContentId;
        FollowTargetState.LastTrackedMemberName = picked.Name;
        FollowTargetState.SelectionMode = mode;
        FollowTargetState.CachedTrackedPosition = picked.Position;

        if (mode == FollowSelectionMode.GroupMovement)
            UpdateGroupMovementPickStreak(picked.ContentId);
        else
            ResetGroupMovementPickStreak();

        ResetStationaryStreak();

        StationaryTargetExclusion.NotifyPicked(picked.ContentId);

        if (mode == FollowSelectionMode.GroupMovement
            && FollowTargetState.GroupMovementPickStreakCount
                >= C.RepeatedFollowTargetExcludePickCount)
        {
            StationaryTargetExclusion.Exclude(
                picked.ContentId,
                FollowTargetExclusionReason.RepeatedPick,
                picked.Name);
        }
    }

    private static void UpdateGroupMovementPickStreak(ulong contentId)
    {
        if (contentId == FollowTargetState.GroupMovementPickStreakContentId)
            FollowTargetState.GroupMovementPickStreakCount++;
        else
        {
            FollowTargetState.GroupMovementPickStreakContentId = contentId;
            FollowTargetState.GroupMovementPickStreakCount = 1;
        }
    }

    private static void ResetGroupMovementPickStreak()
    {
        FollowTargetState.GroupMovementPickStreakContentId = 0;
        FollowTargetState.GroupMovementPickStreakCount = 0;
    }

    private static void UpdateStationaryStreak(ulong contentId)
    {
        if (contentId == FollowTargetState.StationaryStreakContentId)
            FollowTargetState.StationaryStreakCount++;
        else
        {
            FollowTargetState.StationaryStreakContentId = contentId;
            FollowTargetState.StationaryStreakCount = 1;
        }
    }

    private static void ResetStationaryStreak()
    {
        FollowTargetState.StationaryStreakContentId = 0;
        FollowTargetState.StationaryStreakCount = 0;
    }

    private static bool TryCompleteCommanderFollow(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        if (!C.CommanderFollowEnabled
            || FollowTargetState.SelectionMode != FollowSelectionMode.FollowCommander)
            return false;

        var tracked = FindTracked(members);
        if (tracked == null)
            return false;

        if (!IsWithinCommanderFollowArrival(tracked.Value))
            return false;

        CompleteCommanderFollowAndFallback(members);
        return true;
    }

    private static bool ShouldPreferCombatMode(IReadOnlyList<AllianceMemberSnapshot> members) =>
        HostileModeFollow.IsEligible(members);

    private static bool IsWithinCommanderFollowArrival(AllianceMemberSnapshot commander)
    {
        if (Player.Object == null)
            return false;

        return GameCoords.IsWithinRadius(
            Player.Object.Position,
            commander.Position,
            FrontlineConstants.CommanderFollowArrivalDistanceMeters);
    }

    private static void CompleteCommanderFollowAndFallback(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        AllianceCommanderTracker.CompleteFollow();
        SelectFallbackTarget(members);
        AllianceCommanderTracker.ConsumeNeedsReselect();
    }

    private static bool ShouldReselectOnTimer()
    {
        if (FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander)
            return false;

        return Environment.TickCount64 - FollowTargetState.LastSelectionTick
            >= FollowTargetService.MoveRefreshIntervalMs;
    }

    private static bool IsTrackedAnchorStationary(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        if (FollowTargetState.TrackedContentId == 0)
            return false;

        var tracked = FindTracked(members);
        if (tracked == null)
            return false;

        if (FollowTargetState.CachedTrackedPosition is not Vector3 cached)
            return false;

        return GameCoords.AreNear(
            cached,
            tracked.Value.Position,
            FrontlineConstants.PositionUnchangedThresholdMeters);
    }

    private static bool IsTrackedAlive(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        var tracked = FindTracked(members);
        return tracked != null && !tracked.Value.IsDead;
    }

    private static AllianceMemberSnapshot? FindTracked(IReadOnlyList<AllianceMemberSnapshot> members) =>
        members.FirstOrDefault(m => m.ContentId == FollowTargetState.TrackedContentId);
}
