using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>移動コマンド発行可否（詠唱中は不可）。</summary>
internal static class PlayerMovementGate
{
    public static bool CanIssueVnavMoveTo => !Player.IsCasting;
}
