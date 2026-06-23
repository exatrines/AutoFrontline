namespace AutoFrontline.UI;

internal static class ExperimentalGroupMoveSettings
{
    public static void Draw()
    {
        AflImGui.SectionHeader("Experimental Group Move");
        ImGui.TextWrapped(
            "Within 50m of you, follow the ally with the most nearby players (30m density radius). "
            + "If fewer than two allies are in range, pick the densest among the two nearest allies. "
            + "When multiple groups tie on density, prefer the closest group. "
            + "Combat follow mode is disabled while this is enabled. "
            + "Mount when no enemy is within Dismount distance (default 20m); dismount when enemies are nearby.");
        ImGui.Spacing();

        if (ImGui.Checkbox("Enable Experimental Group Move##ExpGroupMove", ref C.ExperimentalGroupMoveEnabled))
            EzConfig.Save();
    }
}
