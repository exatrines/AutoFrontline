using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class CommanderFollowSettings
{
    public static void Draw()
    {
        AflImGui.SectionHeader("Commander follow");
        ImGui.TextWrapped(
            "Follow the latest alliance chat speaker during Frontline. "
            + "Combat mode still takes priority over commander follow.");
        ImGui.Spacing();

        if (ImGui.Checkbox("Enable commander follow##ExpCmdFollow", ref C.CommanderFollowEnabled))
        {
            if (!C.CommanderFollowEnabled)
                AllianceCommanderTracker.DismissFollowRequest();

            EzConfig.Save();
        }
    }
}
