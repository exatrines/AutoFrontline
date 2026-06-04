using AutoFrontline.Services;

namespace AutoFrontline.UI;

public static class SettingsTab
{
    private static readonly string[] ModeLabels = ["Disable", "Manual", "Loop"];

    public static void Draw()
    {
        AflImGui.DrawSettingsWhenReady(DrawSettings);
    }

    private static void DrawSettings()
    {
        DrawModeRow();

        AflImGui.SectionHeader("General");
        MountPicker.Draw();
        DrawDistanceSlider("Mount distance for target (m)", ref C.MountDistanceMeters);
        DrawDistanceSlider("Dismount distance for enemy (m)", ref C.DismountEnemyDistanceMeters);
        AflImGui.SliderSeconds("Group movement refresh (sec)", ref C.GroupMovementRefreshIntervalSeconds, 0.5f, 3.0f);
        AflImGui.SliderSeconds("Hostile mode refresh (sec)", ref C.HostileModeRefreshIntervalSeconds, 0.5f, 3.0f);
        DrawRatioSlider(
            "Hostile mode position",
            ref C.HostileModePositionRatio,
            "0 = front ally, 1 = rearmost ally");

        AflImGui.SectionHeader("Duty");
        ImGui.Checkbox("Auto enter", ref C.AutoEnterEnabled);
        ImGui.TextDisabled("Enter Frontline when Contents Finder matched Daily Frontline.");
        ImGui.Checkbox("Auto leave", ref C.AutoLeaveEnabled);
        ImGui.TextDisabled("Leave Frontline when Frontline result screen is opened.");
    }

    private static void DrawModeRow()
    {
        var modeIndex = (int)C.Mode;
        if (modeIndex < 0 || modeIndex >= ModeLabels.Length)
            modeIndex = 0;

        if (AutoRunSession.Active)
            ImGui.BeginDisabled();

        ImGui.SetNextItemWidth(120f);
        if (ImGui.Combo("Mode", ref modeIndex, ModeLabels, ModeLabels.Length))
        {
            var newMode = (PluginMode)modeIndex;
            if (C.Mode != newMode)
            {
                if (newMode != PluginMode.Loop)
                {
                    AutoRunSession.Stop();
                    ContentsFinderQueueAutomation.ResetState();
                }

                C.Mode = newMode;
                EzConfig.Save();
            }
        }

        if (AutoRunSession.Active)
            ImGui.EndDisabled();

        if (C.Mode == PluginMode.Loop)
        {
            ImGui.SameLine();
            ImGui.TextDisabled($"{AutoRunSession.CurrentCount} /");

            ImGui.SameLine();
            ImGui.SetNextItemWidth(80f);
            var maxCount = C.AutoMaxCount;
            if (ImGui.InputInt("##AutoMaxCount", ref maxCount, 1, 5))
            {
                C.AutoMaxCount = Math.Clamp(maxCount, FrontlineConstants.AutoMaxCountMin, FrontlineConstants.AutoMaxCountMax);
            }

            ImGui.SameLine();
            var canStart = RequiredPlugins.AreAllLoaded
                && !AutoRunSession.Active
                && C.AutoMaxCount >= FrontlineConstants.AutoMaxCountMin;

            if (!canStart)
                ImGui.BeginDisabled();

            if (ImGui.Button("Start"))
            {
                AutoRunSession.Start();
                ContentsFinderQueueAutomation.ResetState();
            }

            if (!canStart)
                ImGui.EndDisabled();

            ImGui.SameLine();
            if (!AutoRunSession.Active)
                ImGui.BeginDisabled();

            if (ImGui.Button("Stop"))
                AutoRunSession.Stop();

            if (!AutoRunSession.Active)
                ImGui.EndDisabled();
        }
    }

    private static void DrawDistanceSlider(string label, ref int meters)
    {
        ImGui.SetNextItemWidth(AflImGui.DefaultSliderWidth);
        ImGui.SliderInt(label, ref meters, 0, 100);
    }

    private static void DrawRatioSlider(string label, ref float ratio, string hint)
    {
        ratio = Math.Clamp(ratio, 0f, 1f);
        ImGui.SetNextItemWidth(AflImGui.DefaultSliderWidth);
        ImGui.SliderFloat(label, ref ratio, 0f, 1f, "%.2f");
        ImGui.TextDisabled(hint);
    }
}
