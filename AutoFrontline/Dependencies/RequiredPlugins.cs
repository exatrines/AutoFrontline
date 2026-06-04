using System.Collections.Generic;

namespace AutoFrontline.Dependencies;

internal readonly record struct RequiredPlugin(string DisplayName, string InternalName);

/// <summary>vnavmesh / Rotation Solver Reborn の有効化チェックと Mode 連動。</summary>
internal static class RequiredPlugins
{
    public static readonly RequiredPlugin VNavmesh = new("vnavmesh", "vnavmesh");
    public static readonly RequiredPlugin RotationSolver = new("Rotation Solver Reborn", "RotationSolver");

    private static readonly RequiredPlugin[] All = [VNavmesh, RotationSolver];

    public static IEnumerable<RequiredPlugin> Enumerate() => All;

    public static bool AreAllLoaded
    {
        get
        {
            foreach (var plugin in All)
            {
                if (!IsLoaded(plugin.InternalName))
                    return false;
            }

            return true;
        }
    }

    public static bool IsAutomationActive =>
        AreAllLoaded && (C.Mode == PluginMode.Manual
            || (C.Mode == PluginMode.Loop && AutoRunSession.Active));

    public static bool ShouldAutoEnter =>
        C.Mode == PluginMode.Loop && AutoRunSession.Active || C.AutoEnterEnabled;

    public static bool ShouldAutoLeave =>
        C.Mode == PluginMode.Loop && AutoRunSession.Active || C.AutoLeaveEnabled;

    public static void SyncEnabledState()
    {
        if (!AreAllLoaded && C.Mode != PluginMode.Disable)
        {
            if (C.Mode == PluginMode.Loop)
                AutoRunSession.Stop();

            C.Mode = PluginMode.Disable;
            EzConfig.Save();
        }
    }

    public static string GetMissingPluginsMessage()
    {
        var missing = new List<string>(All.Length);
        foreach (var plugin in All)
        {
            if (!IsLoaded(plugin.InternalName))
                missing.Add(plugin.DisplayName);
        }

        return $"Missing plugins: {string.Join(", ", missing)}";
    }

    public static bool IsLoaded(string internalName)
    {
        foreach (var plugin in Svc.PluginInterface.InstalledPlugins)
        {
            if (plugin.IsLoaded && plugin.InternalName == internalName)
                return true;
        }

        return false;
    }
}
