using ECommons.DalamudServices;

namespace AutoFrontline.Services;

/// <summary>Shared Frontline automation gates.</summary>
internal static class AutomationContext
{
    public static bool IsAutomationActive => RequiredPlugins.IsAutomationActive;

    public static bool IsLeaveFlowBlocking => FrontlineLeaveAutomation.IsBlockingAutomation;

    public static bool IsInFrontline => FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType);

    public static bool CanRunInFrontlineMatch =>
        IsAutomationActive && !IsLeaveFlowBlocking && IsInFrontline;
}
