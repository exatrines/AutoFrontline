using Dalamud.Interface.Colors;

namespace AutoFrontline.UI;

public static class GeneralTab
{
    public static void Draw()
    {
        MirageUi.SubHeader("Overview");
        MirageUi.Text(
            "Auto Frontline is a plugin that automatically joins and leaves Frontline duty.",
            wrap: true);

        MirageUi.SubHeader("Required plugins");
        foreach (var plugin in RequiredPlugins.Enumerate())
            DrawPluginStatus(plugin);

        MirageUi.SubHeader("Mode");
        MirageUi.Text("Loop Mode:", wrap: true);
        MirageUi.Text(
            "Automatically queue, enter, and leave Frontline up to Max count.",
            color: MirageUi.Color.Secondary,
            wrap: true);
        MirageUi.Text("Manual Mode:", wrap: true);
        MirageUi.Text(
            "Manually join Frontline on Contents Finder.",
            color: MirageUi.Color.Secondary,
            wrap: true);

        MirageUi.SubHeader("Recommended Job");
        MirageUi.Text("BLM or other ranged DPS jobs.", wrap: true);
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
