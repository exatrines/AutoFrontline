using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class HostileModeSettings
{
    public static void Draw()
    {
        AflImGui.SectionHeader("Combat mode");
        ImGui.TextWrapped(
            "When enabled, move toward allies near the closest enemy within 30m. "
            + "Takes priority over commander follow and group movement.");
        ImGui.Spacing();

        if (ImGui.Checkbox("Enable combat mode##ExpHostileMode", ref C.HostileModeEnabled))
            EzConfig.Save();

        if (!C.HostileModeEnabled)
            ImGui.BeginDisabled();

        AflImGui.SliderSeconds("Combat mode refresh (sec)", ref C.HostileModeRefreshIntervalSeconds, 0.5f, 3.0f);
        DrawRatioSlider(
            "Combat mode position",
            ref C.HostileModePositionRatio,
            "0 = front ally, 1 = rearmost ally");

        if (!C.HostileModeEnabled)
            ImGui.EndDisabled();
    }

    private static void DrawRatioSlider(string label, ref float ratio, string hint)
    {
        ratio = Math.Clamp(ratio, 0f, 1f);
        ImGui.SetNextItemWidth(AflImGui.DefaultSliderWidth);
        ImGui.SliderFloat(label, ref ratio, 0f, 1f, "%.2f");
        ImGui.TextDisabled(hint);
    }
}
