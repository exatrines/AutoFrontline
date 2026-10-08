using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>Blocks moveto while casting.</summary>
internal static class PlayerMovementGate
{
    public static bool CanIssueVnavMoveTo => !Player.IsCasting;
}
