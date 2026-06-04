using Dalamud.Game.Text.SeStringHandling;
using ECommons.EzDTR;

namespace AutoFrontline;

internal static class PluginDtr
{
    public static void Init() =>
        _ = new EzDtr(GetText, PluginCommands.ToggleEnabled, title: "AutoFrontline");

    private static SeString GetText()
    {
        if (C.Mode == PluginMode.Loop && AutoRunSession.Active)
            return $"AutoFrontline: Loop {AutoRunSession.CurrentCount}/{C.AutoMaxCount}";

        return $"AutoFrontline: {C.Mode}";
    }
}
