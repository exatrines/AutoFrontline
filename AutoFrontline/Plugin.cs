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

        C = EzConfig.Init<Configuration>();
        C.MigrateIfNeeded();
        I18n.Init(pluginInterface);

        MirageUi.ConfigureTheme(() => MirageColorSettings.CreateDefault());
        MirageUi.Init(pluginInterface, Svc.Texture, Svc.Log);
        MirageUi.ConfigurePluginInfo(info =>
        {
            info.Message = "Support development via the Support page.";
            info.DiscordUrl = SupportLinks.DiscordUrl;
            info.SupportUrl = SupportLinks.SupportUrl;
        });
        EzConfigGui.Init(new UI.ConfigWindow(), windowType: EzConfigGui.WindowType.Both);
        EzCmd.Add("/autofrontline", PluginCommands.Handle, I18n.Get("command.help"));
        PluginDtr.Init();
        AllianceCommanderTracker.Init();

        Svc.Framework.Update += OnFrameworkUpdate;
    }

    private static void OnFrameworkUpdate(object _) => FrontlineAutomation.Update();

    public void Dispose()
    {
        Svc.Framework.Update -= OnFrameworkUpdate;
        AllianceCommanderTracker.Dispose();
        I18n.Dispose();
        MirageUi.Dispose();
        ECommonsMain.Dispose();
        C = null!;
    }
}
