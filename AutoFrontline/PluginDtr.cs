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
            return I18n.Format("dtr.loop", AutoRunSession.CurrentCount, C.AutoMaxCount);

        return I18n.Format("dtr.mode", C.Mode);
    }
}
