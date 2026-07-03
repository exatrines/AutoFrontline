using AutoFrontline.Services;
using AutoFrontline.UI;
using Dalamud.Plugin;
using ECommons;
using ECommons.SimpleGui;
using MirageUI;
using MirageUI.Theme;

namespace AutoFrontline;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Auto Frontline";

    internal static Configuration C = null!;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);

        MirageUi.ConfigureTheme(() => MirageColorSettings.CreateDefault());
        MirageUi.Init(pluginInterface, Svc.Texture, Svc.Log);

        C = EzConfig.Init<Configuration>();
        C.MigrateIfNeeded();
        EzConfigGui.Init(new UI.ConfigWindow(), windowType: EzConfigGui.WindowType.Both);
        const string help = "on|off|toggle — Manual/Disable. No args: toggle settings.";
        EzCmd.Add("/autofrontline", PluginCommands.Handle, help);
        PluginDtr.Init();
        AllianceCommanderTracker.Init();

        Svc.Framework.Update += OnFrameworkUpdate;
    }

    private static void OnFrameworkUpdate(object _) => FrontlineAutomation.Update();

    public void Dispose()
    {
        Svc.Framework.Update -= OnFrameworkUpdate;
        AllianceCommanderTracker.Dispose();
        MirageUi.Dispose();
        ECommonsMain.Dispose();
        C = null!;
    }
}
