namespace AutoFrontline.Services;

internal enum FollowSelectionMode
{
    None,
    GroupMovement,
    Hostile,
    FollowCommander,
}

internal static class FollowSelectionModeExtensions
{
    public static string ToDebugLabel(this FollowSelectionMode mode) => mode switch
    {
        FollowSelectionMode.GroupMovement when C.ExperimentalGroupMoveEnabled => "ExpGroup",
        FollowSelectionMode.GroupMovement => "Densest",
        FollowSelectionMode.Hostile => "EnemyProximate",
        FollowSelectionMode.FollowCommander => "Commander",
        _ => string.Empty,
    };

    public static string ToDisplayLabel(this FollowSelectionMode mode) => mode switch
    {
        FollowSelectionMode.GroupMovement => "集団行動",
        FollowSelectionMode.Hostile => "戦闘",
        FollowSelectionMode.FollowCommander => "軍師追従",
        _ => "—",
    };
}
