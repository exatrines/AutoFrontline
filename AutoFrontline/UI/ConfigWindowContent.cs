using System.IO;
using MirageUI.Layout;

namespace AutoFrontline.UI;

public static class ConfigWindowContent
{
    private static string selectedId = "general";

    public static void Draw() =>
        MirageUi.TwoColumn.Draw(CreateState(), DrawMainContent);

    private static string ResolvePluginIconPath()
    {
        var dir = Svc.PluginInterface.AssemblyLocation.DirectoryName
                  ?? AppContext.BaseDirectory;
        return Path.Combine(dir, "Data", "plugin-icon.png");
    }

    private static MirageTwoColumnState CreateState()
    {
        var iconPath = ResolvePluginIconPath();
        return new()
        {
            ShowSidebarHeader = true,
            ShowSidebarFooter = false,
            AllowDeselect = false,
            SidebarHeader = new MirageTwoColumnSidebarHeader
            {
                ImagePath = File.Exists(iconPath) ? iconPath : null,
                ImageWidth = 48f,
                ImageHeight = 48f,
                Title = "Auto Frontline",
                Subtitle = $"v{Svc.PluginInterface.Manifest.AssemblyVersion}",
            },
            Entries =
            [
                new MirageTwoColumnEntry { Id = "general", Label = "General" },
                new MirageTwoColumnEntry { Id = "settings", Label = "Settings" },
                new MirageTwoColumnEntry { Id = "experimental", Label = "Experimental" },
                new MirageTwoColumnEntry { Id = "debug", Label = "Debug" },
            ],
            SelectedId = selectedId,
            OnSelectionChanged = id => selectedId = id,
        };
    }

    private static void DrawMainContent()
    {
        RequiredPlugins.SyncEnabledState();

        try
        {
            switch (selectedId)
            {
                case "general":
                    GeneralTab.Draw();
                    break;
                case "settings":
                    SettingsTab.Draw();
                    break;
                case "experimental":
                    ExperimentalTab.Draw();
                    break;
                case "debug":
                    DebugTab.Draw();
                    break;
            }
        }
        catch (Exception e)
        {
            e.Log();
        }
    }
}
