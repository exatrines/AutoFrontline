using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>Forces Rotation Solver to Manual in a match.</summary>
internal static class RotationModeAutomation
{
    public static void Update()
    {
        if (!AutomationContext.CanRunInFrontlineMatch)
            return;

        if (!RequiredPlugins.IsLoaded(RequiredPlugins.RotationSolver.InternalName))
            return;

        if (RotationSolverState.Get() is RotationSolverOperatingState.Manual)
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleRotationManual, FrontlineConstants.RotationManualIntervalMs))
            return;

        Chat.ExecuteCommand("/rotation manual");
    }
}
