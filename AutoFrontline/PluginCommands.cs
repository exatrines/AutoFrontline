using AutoFrontline.Services;
using ECommons.Logging;

namespace AutoFrontline;

internal static class PluginCommands
{
    public static void Handle(string command, string args)
    {
        var parts = args.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sub = parts.Length > 0 ? parts[0].ToLowerInvariant() : null;

        switch (sub)
        {
            case "on":
            case "enable":
                SetMode(PluginMode.Manual);
                return;
            case "off":
            case "disable":
                SetMode(PluginMode.Disable);
                return;
            case "toggle":
                ToggleMode();
                return;
            case null:
            case "":
                ToggleConfigWindow();
                return;
            default:
                DuoLog.Information($"/autofrontline {I18n.Get("command.help")}");
                return;
        }
    }

    public static void ToggleEnabled()
    {
        if (C.Mode == PluginMode.Manual)
            SetMode(PluginMode.Disable);
        else
            SetMode(PluginMode.Manual);
    }

    private static void ToggleMode()
    {
        if (C.Mode == PluginMode.Loop && AutoRunSession.Active)
        {
            AutoRunSession.Stop();
            DuoLog.Information(I18n.Get("command.loop_stopped"));
            return;
        }

        ToggleEnabled();
    }

    private static void ToggleConfigWindow()
    {
        if (EzConfigGui.Window == null)
        {
            EzConfigGui.Open();
            return;
        }

        EzConfigGui.Window.IsOpen = !EzConfigGui.Window.IsOpen;
    }

    internal static void SetMode(PluginMode mode)
    {
        if (mode != PluginMode.Disable && !RequiredPlugins.AreAllLoaded)
        {
            DuoLog.Information(RequiredPlugins.GetMissingPluginsMessage());
            return;
        }

        if (mode != PluginMode.Loop)
            AutoRunSession.Stop();

        if (C.Mode == mode && !AutoRunSession.Active)
        {
            DuoLog.Information(I18n.Format("command.already_mode", mode));
            return;
        }

        C.Mode = mode;
        EzConfig.Save();
        DuoLog.Information(I18n.Format("command.mode", mode));
    }
}
