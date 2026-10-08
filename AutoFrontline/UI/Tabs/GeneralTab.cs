using Dalamud.Interface.Colors;

namespace AutoFrontline.UI;

public static class GeneralTab
{
    public static void Draw()
    {
        MirageUi.Header("General");
        MirageUi.SubHeader(I18n.Get("general.overview"));
        MirageUi.Text(I18n.Get("general.overview.body"), wrap: true);

        MirageUi.SubHeader(I18n.Get("general.required"));
        foreach (var plugin in RequiredPlugins.Enumerate())
            DrawPluginStatus(plugin);

        MirageUi.SubHeader(I18n.Get("general.mode"));
        MirageUi.Text(I18n.Get("general.mode.loop"), wrap: true);
        MirageUi.Text(
            I18n.Get("general.mode.loop.body"),
            color: MirageUi.Color.Secondary,
            wrap: true);
        MirageUi.Text(I18n.Get("general.mode.manual"), wrap: true);
        MirageUi.Text(
            I18n.Get("general.mode.manual.body"),
            color: MirageUi.Color.Secondary,
            wrap: true);

        MirageUi.SubHeader(I18n.Get("general.job"));
        MirageUi.Text(I18n.Get("general.job.body"), wrap: true);
    }

    private static void DrawPluginStatus(RequiredPlugin plugin)
    {
        var loaded = RequiredPlugins.IsLoaded(plugin.InternalName);

        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(
            loaded ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed,
            loaded ? FontAwesomeIcon.Check.ToIconString() : FontAwesomeIcon.Times.ToIconString());
        ImGui.PopFont();

        ImGui.SameLine();
        MirageUi.Text(plugin.DisplayName);
    }
}
