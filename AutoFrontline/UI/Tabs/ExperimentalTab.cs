namespace AutoFrontline.UI;

public static class ExperimentalTab
{
    public static void Draw()
    {
        AflImGui.DrawSettings(DrawContent);
    }

    private static void DrawContent()
    {
        ImGui.TextWrapped("Experimental features may change or be removed without notice.");
        ImGui.Spacing();

        CommanderFollowSettings.Draw();
        ImGui.Spacing();

        HostileModeSettings.Draw();
        ImGui.Spacing();

        PvpLimitBreakSettings.Draw();
    }
}
