namespace AutoFrontline.Services;

/// <summary>追従 moveto の発行。</summary>
internal static class NaviMovementCoordinator
{
    public static void IssueMoveTo(System.Numerics.Vector3 target)
    {
        MovementCommands.MoveTo(target);
        NaviStuckDejonAutomation.NotifyMoveIssued();
    }

    public static void Reset() => NaviStuckDejonAutomation.NotifyStopped();
}
