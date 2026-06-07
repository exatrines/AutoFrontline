using System.Numerics;
using AutoFrontline.Services;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons.SimpleGui;

namespace AutoFrontline;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Auto Frontline";

    internal static Configuration C = null!;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this);

        C = EzConfig.Init<Configuration>();
        C.MigrateIfNeeded();
        EzConfigGui.Init(UI.ConfigWindow.Draw, windowType: EzConfigGui.WindowType.Both);
        ConfigureConfigWindow();
        const string help = "on|off|toggle — Manual/Disable. No args: toggle settings.";
        EzCmd.Add("/autofrontline", PluginCommands.Handle, help);
        PluginDtr.Init();
        AllianceCommanderTracker.Init();

        Svc.Framework.Update += OnFrameworkUpdate;
    }

    private static void ConfigureConfigWindow()
    {
        if (EzConfigGui.Window == null)
            return;

        EzConfigGui.Window.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    private static void OnFrameworkUpdate(object _) => FrontlineAutomation.Update();

    public void Dispose()
    {
        Svc.Framework.Update -= OnFrameworkUpdate;
        AllianceCommanderTracker.Dispose();
        ECommonsMain.Dispose();
        C = null!;
    }
}
