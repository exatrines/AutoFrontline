using AutoFrontline.Services;

namespace AutoFrontline.UI;

public static class SettingsTab
{
    private const float ControlWidth = 200f;
    private static readonly string[] ModeLabels = ["Disable", "Manual", "Loop"];

    public static void Draw()
    {
        MirageUi.SubHeader("Mode");
        DrawModeRow();

        MirageUi.SubHeader("Movement");
        MountPicker.Draw(ControlWidth);
        MirageUi.SliderInt(
            "Dismount distance for enemy (m)",
            ref C.DismountEnemyDistanceMeters,
            0,
            100,
            ControlWidth);
        MirageUi.SliderFloat(
            "Move refresh (sec)",
            ref C.GroupMovementRefreshIntervalSeconds,
            0.5f,
            3.0f,
            ControlWidth);
        MirageUi.SliderInt(
            "Group search radius (m)",
            ref C.GroupMoveSelfSearchRadiusMeters,
            FrontlineConstants.GroupMoveSelfSearchRadiusMinMeters,
            FrontlineConstants.GroupMoveSelfSearchRadiusMaxMeters,
            ControlWidth);
        MirageUi.SliderInt(
            "Stationary target exclusion (sec)",
            ref C.StationaryTargetExclusionSeconds,
            FrontlineConstants.StationaryTargetExclusionSecondsMin,
            FrontlineConstants.StationaryTargetExclusionSecondsMax,
            ControlWidth);
        MirageUi.SliderInt(
            "Repeated follow exclude (picks)",
            ref C.RepeatedFollowTargetExcludePickCount,
            FrontlineConstants.RepeatedFollowTargetExcludePickCountMin,
            FrontlineConstants.RepeatedFollowTargetExcludePickCountMax,
            ControlWidth);
        ReturnStuckSettings.Draw(ControlWidth);

        MirageUi.SubHeader("Spawn");
        MirageUi.SliderInt("Spawn exclusion radius (m)", ref C.SpawnExclusionRadiusMeters, 0, 100, ControlWidth);

        MirageUi.SubHeader("Duty");
        using (var group = MirageUi.CheckboxGroup("Auto enter", ref C.AutoEnterEnabled))
        {
            if (group.Changed)
                EzConfig.Save();

            using (MirageUi.DisabledIf(!C.AutoEnterEnabled))
            {
                MirageUi.Text(
                    "Enter Frontline when Contents Finder matched Daily Frontline.",
                    color: MirageUi.Color.Secondary,
                    wrap: true);
            }
        }

        using (var group = MirageUi.CheckboxGroup("Auto leave", ref C.AutoLeaveEnabled))
        {
            if (group.Changed)
                EzConfig.Save();

            using (MirageUi.DisabledIf(!C.AutoLeaveEnabled))
            {
                MirageUi.Text(
                    "Leave Frontline when Frontline result screen is opened.",
                    color: MirageUi.Color.Secondary,
                    wrap: true);
            }
        }
    }

    private static void DrawModeRow()
    {
        var pluginsReady = RequiredPlugins.AreAllLoaded;
        var modeIndex = pluginsReady ? (int)C.Mode : (int)PluginMode.Disable;
        if (modeIndex < 0 || modeIndex >= ModeLabels.Length)
            modeIndex = 0;

        if (AutoRunSession.Active || !pluginsReady)
            ImGui.BeginDisabled();

        if (MirageUi.Combo("Mode", ref modeIndex, ModeLabels, ControlWidth))
        {
            var newMode = (PluginMode)modeIndex;
            if (C.Mode != newMode)
            {
                if (newMode != PluginMode.Loop)
                    AutoRunSession.Stop();

                C.Mode = newMode;
                EzConfig.Save();
            }
        }

        if (AutoRunSession.Active || !pluginsReady)
            ImGui.EndDisabled();

        if (!pluginsReady)
            MirageUi.Text(RequiredPlugins.GetMissingPluginsMessage(), color: MirageUi.Color.Secondary, wrap: true);

        if (C.Mode == PluginMode.Loop)
        {
            ImGui.SameLine();
            MirageUi.Text($"{AutoRunSession.CurrentCount} /", color: MirageUi.Color.Secondary, wrap: false);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(80f);
            var maxCount = C.AutoMaxCount;
            if (ImGui.InputInt("##AutoMaxCount", ref maxCount, 1, 5))
                C.AutoMaxCount = Math.Clamp(maxCount, FrontlineConstants.AutoMaxCountMin, FrontlineConstants.AutoMaxCountMax);

            ImGui.SameLine();
            var canStart = RequiredPlugins.AreAllLoaded
                && !AutoRunSession.Active
                && C.AutoMaxCount >= FrontlineConstants.AutoMaxCountMin;

            if (!canStart)
                ImGui.BeginDisabled();

            if (ImGui.Button("Start"))
                AutoRunSession.Start();

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
}
