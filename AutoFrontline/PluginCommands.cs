using ECommons.Logging;

namespace AutoFrontline;

internal static class PluginCommands
{
    private const string Usage = "/autofrontline on|off|toggle - Enable or disable (no args: toggle settings)";

    public static void Handle(string command, string args)
    {
        var parts = args.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sub = parts.Length > 0 ? parts[0].ToLowerInvariant() : null;

        switch (sub)
        {
            case "on":
            case "enable":
                SetEnabled(true);
                return;
            case "off":
            case "disable":
                SetEnabled(false);
                return;
            case "toggle":
                ToggleEnabled();
                return;
            case null:
            case "":
                ToggleConfigWindow();
                return;
            default:
                DuoLog.Information(Usage);
                return;
        }
    }

    public static void ToggleEnabled() => SetEnabled(!C.Enabled);

    private static void ToggleConfigWindow()
    {
        if (EzConfigGui.Window == null)
        {
            EzConfigGui.Open();
            return;
        }

        EzConfigGui.Window.IsOpen = !EzConfigGui.Window.IsOpen;
    }

    internal static void SetEnabled(bool enabled)
    {
        if (enabled && !RequiredPlugins.AreAllLoaded)
        {
            DuoLog.Information(RequiredPlugins.GetMissingPluginsMessage());
            return;
        }

        if (C.Enabled == enabled)
        {
            DuoLog.Information($"Auto Frontline is already {(enabled ? "enabled" : "disabled")}.");
            return;
        }

        C.Enabled = enabled;
        EzConfig.Save();
        DuoLog.Information($"Auto Frontline {(enabled ? "enabled" : "disabled")}.");
    }
}
