namespace AutoFrontline.UI;

public static class GeneralTab
{
    public static void Draw()
    {
        AflImGui.SectionHeader("Auto Frontline");
        ImGui.TextWrapped("Auto Frontline is a plugin that automatically joins and leaves Frontline duty.");
        ImGui.Spacing();

        AflImGui.SectionHeader("Required plugins");
        foreach (var plugin in RequiredPlugins.Enumerate())
            AflImGui.DrawPluginStatus(plugin);

        AflImGui.SectionHeader("Mode");
        ImGui.TextWrapped("Loop Mode:");
        ImGui.Indent();
        ImGui.TextWrapped("Automatically queue, enter, and leave Frontline up to Max count.");
        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.TextWrapped("Manual Mode:");
        ImGui.Indent();
        ImGui.TextWrapped("Manually join Frontline on Contents Finder.");
        ImGui.Unindent();
        ImGui.Spacing();

        AflImGui.SectionHeader("Recommended Job");
        ImGui.TextWrapped("BLM or other ranged DPS jobs.");
    }
}
