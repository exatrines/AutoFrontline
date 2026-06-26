using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>追跡対象の選定と移動先の算出（外部 API）。</summary>
public static class FollowTargetService
{
    public static Vector3? LastMoveTarget => FollowTargetState.LastMoveTarget;

    public static string TrackedMemberName => FollowTargetState.LastTrackedMemberName;
    public static string CurrentFollowModeLabel => FollowTargetState.SelectionMode.ToDisplayLabel();
    public static bool IsGroupMovementMode =>
        FollowTargetState.SelectionMode == FollowSelectionMode.GroupMovement;
    public static bool IsHostileMode => FollowTargetState.SelectionMode == FollowSelectionMode.Hostile;
    public static bool IsCommanderMode =>
        FollowTargetState.SelectionMode == FollowSelectionMode.FollowCommander;

    public static int MoveRefreshIntervalMs =>
        IsCommanderMode
            ? ConfigIntervals.GroupMovementRefreshMs
            : IsHostileMode
                ? ConfigIntervals.HostileModeRefreshMs
                : ConfigIntervals.GroupMovementRefreshMs;

    public static void UpdateSelection() => FollowModeSelector.Update();

    public static IGameObject TryGetTrackedGameObject()
    {
        if (FollowTargetState.TrackedContentId == 0 || Player.Object == null)
            return null;

        if (PlayerObjectResolver.FindByContentId(FollowTargetState.TrackedContentId) is { } byContentId)
            return byContentId;

        var snapshot = AllianceMemberCache.GetMembers()
            .FirstOrDefault(m => m.ContentId == FollowTargetState.TrackedContentId);
        if (snapshot.ContentId == 0)
            return null;

        return PlayerObjectResolver.ResolveFromSnapshot(snapshot);
    }

    public static bool TryGetMoveDestinationDistance(out float distanceMeters)
    {
        distanceMeters = 0;
        if (Player.Object == null)
            return false;

        Vector3? destination = FollowTargetState.LastMoveTarget;
        if (destination == null)
        {
            var tracked = TryGetTrackedGameObject();
            if (tracked == null)
                return false;

            destination = tracked.Position;
        }

        distanceMeters = Vector3.Distance(Player.Object.Position, destination.Value);
        return true;
    }

    public static Vector3? TryGetMoveTarget() => FollowMoveResolver.TryGetMoveTarget();
}
