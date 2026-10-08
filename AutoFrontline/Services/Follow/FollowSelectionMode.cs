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
    public static string ToDisplayLabel(this FollowSelectionMode mode) => mode switch
    {
        FollowSelectionMode.GroupMovement => I18n.Get("follow.group"),
        FollowSelectionMode.Hostile => I18n.Get("follow.combat"),
        FollowSelectionMode.FollowCommander => I18n.Get("follow.commander"),
        _ => I18n.Get("follow.none"),
    };
}
