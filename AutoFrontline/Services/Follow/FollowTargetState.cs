using System.Numerics;

namespace AutoFrontline.Services;

/// <summary>追跡対象・移動先の共有状態。</summary>
internal static class FollowTargetState
{
    internal static ulong TrackedContentId { get; set; }
    internal static string LastTrackedMemberName { get; set; } = string.Empty;
    internal static FollowSelectionMode SelectionMode { get; set; }
    internal static long LastSelectionTick { get; set; }
    internal static Vector3? CachedTrackedPosition { get; set; }
    internal static Vector3? LastMoveTarget { get; set; }
    internal static ulong GroupMovementPickStreakContentId { get; set; }
    internal static int GroupMovementPickStreakCount { get; set; }
    internal static ulong StationaryStreakContentId { get; set; }
    internal static int StationaryStreakCount { get; set; }

    internal static void ClearMoveState()
    {
        LastMoveTarget = null;
        CachedTrackedPosition = null;
    }

    internal static void ClearTarget()
    {
        TrackedContentId = 0;
        SelectionMode = FollowSelectionMode.None;
        ClearMoveState();
        LastTrackedMemberName = string.Empty;
        GroupMovementPickStreakContentId = 0;
        GroupMovementPickStreakCount = 0;
        StationaryStreakContentId = 0;
        StationaryStreakCount = 0;
        StationaryTargetExclusion.Clear();
    }
}
