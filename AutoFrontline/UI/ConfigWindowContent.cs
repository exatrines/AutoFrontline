using MirageUI.Layout;

namespace AutoFrontline.UI;

public static class ConfigWindowContent
{
    private static string selectedId = "general";

    public static void Draw() =>
        MirageUi.TwoColumn.Draw(CreateState(), DrawMainContent);

    private static MirageTwoColumnState CreateState() => new()
    {
        ShowSidebarHeader = true,
        ShowSidebarFooter = true,
        SidebarHeader = new MirageTwoColumnSidebarHeader
        {
            ImagePath = System.IO.Path.Combine(
                Svc.PluginInterface.AssemblyLocation.DirectoryName!,
                "Assets",
                "AutoFrontlineIcon.png"),
            ImageWidth = 48f,
            ImageHeight = 48f,
            Title = "Auto Frontline",
            Subtitle = $"v{Svc.PluginInterface.Manifest.AssemblyVersion}",
        },
        SidebarFooterLinks =
        [
            new MirageTwoColumnSidebarFooterLink { Label = "GitHub", Url = SupportLinks.GitHubUrl },
            new MirageTwoColumnSidebarFooterLink { Label = "OFUSE", Url = SupportLinks.OfuseUrl },
            new MirageTwoColumnSidebarFooterLink { Label = "Ko-fi", Url = SupportLinks.KoFiUrl },
        ],
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
