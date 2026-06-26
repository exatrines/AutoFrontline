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
        DrawDistanceSlider("Dismount distance for enemy (m)", ref C.DismountEnemyDistanceMeters);

        AflImGui.SectionHeader("Movement");
        AflImGui.SliderSeconds("Move refresh (sec)", ref C.GroupMovementRefreshIntervalSeconds, 0.5f, 3.0f);
        DrawDistanceSlider(
            "Group search radius (m)",
            ref C.GroupMoveSelfSearchRadiusMeters,
            FrontlineConstants.GroupMoveSelfSearchRadiusMinMeters,
            FrontlineConstants.GroupMoveSelfSearchRadiusMaxMeters);
        DrawDistanceSlider(
            "Stationary target exclusion (sec)",
            ref C.StationaryTargetExclusionSeconds,
            FrontlineConstants.StationaryTargetExclusionSecondsMin,
            FrontlineConstants.StationaryTargetExclusionSecondsMax);
        DrawDistanceSlider(
            "Repeated follow exclude (picks)",
            ref C.RepeatedFollowTargetExcludePickCount,
            FrontlineConstants.RepeatedFollowTargetExcludePickCountMin,
            FrontlineConstants.RepeatedFollowTargetExcludePickCountMax);
        ReturnStuckSettings.Draw();

        AflImGui.SectionHeader("Spawn");
        DrawDistanceSlider("Spawn exclusion radius (m)", ref C.SpawnExclusionRadiusMeters);

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

    private static void DrawDistanceSlider(string label, ref int meters, int min, int max)
    {
        ImGui.SetNextItemWidth(AflImGui.DefaultSliderWidth);
        ImGui.SliderInt(label, ref meters, min, max);
    }

    private static void DrawDistanceSlider(string label, ref int meters)
    {
        DrawDistanceSlider(label, ref meters, 0, 100);
    }
}
