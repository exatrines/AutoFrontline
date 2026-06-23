namespace AutoFrontline.UI;

public static class ExperimentalTab
{
    public static void Draw()
    {
        AflImGui.DrawSettingsWhenReady(DrawContent);
    }

    private static void DrawContent()
    {
        ImGui.TextWrapped("Experimental features may change or be removed without notice.");
        ImGui.Spacing();

        CommanderFollowSettings.Draw();
        ImGui.Spacing();

        ExperimentalGroupMoveSettings.Draw();
        ImGui.Spacing();

        DejonStuckSettings.Draw();
        ImGui.Spacing();

        PvpLimitBreakSettings.Draw();
    }
}
