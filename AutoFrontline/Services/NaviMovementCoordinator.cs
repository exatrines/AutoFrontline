namespace AutoFrontline.Services;

/// <summary>追従 moveto の経路種別（初期移動は追従モードより優先）。</summary>
internal enum NaviMovementRoute
{
    None,
    Initial,
    GroupMovement,
    Hostile,
    FollowCommander,
}

/// <summary>移動モード切替時に vnav stop → moveto の順を保証する。</summary>
internal static class NaviMovementCoordinator
{
    private static NaviMovementRoute lastRoute;
    private static bool stopBeforeNextMove;

    public static void Update()
    {
        var route = ResolveRoute();
        if (route == lastRoute)
            return;

        stopBeforeNextMove = true;
        lastRoute = route;
    }

    public static void IssueMoveTo(System.Numerics.Vector3 target)
    {
        if (stopBeforeNextMove)
        {
            MovementCommands.Stop();
            stopBeforeNextMove = false;
        }

        MovementCommands.MoveTo(target);
        NaviStuckDejonAutomation.NotifyMoveIssued();
    }

    public static void Reset()
    {
        lastRoute = NaviMovementRoute.None;
        stopBeforeNextMove = false;
        NaviStuckDejonAutomation.NotifyStopped();
    }

    private static NaviMovementRoute ResolveRoute()
    {
        if (InitialMovementMode.IsActive)
            return NaviMovementRoute.Initial;

        return FollowTargetService.SelectionMode switch
        {
            FollowSelectionMode.GroupMovement => NaviMovementRoute.GroupMovement,
            FollowSelectionMode.Hostile => NaviMovementRoute.Hostile,
            FollowSelectionMode.FollowCommander => NaviMovementRoute.FollowCommander,
            _ => NaviMovementRoute.None,
        };
    }
}
