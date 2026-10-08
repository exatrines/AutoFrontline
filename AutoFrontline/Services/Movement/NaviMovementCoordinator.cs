namespace AutoFrontline.Services;

/// <summary>Issues follow moveto commands.</summary>
internal static class NaviMovementCoordinator
{
    public static void IssueMoveTo(System.Numerics.Vector3 target)
    {
        MovementCommands.MoveTo(target);
        NaviStuckDejonAutomation.NotifyMoveIssued();
    }

    public static void Reset() => NaviStuckDejonAutomation.NotifyStopped();
}
