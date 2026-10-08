using AutoFrontline.Services;

namespace AutoFrontline.UI;

public static class SettingsTab
{
    public static void Draw()
    {
        MirageUi.Header("Settings");
        DrawLanguageRow();

        MirageUi.SubHeader(I18n.Get("settings.mode"));
        DrawModeRow();

        MirageUi.SubHeader(I18n.Get("settings.movement"));
        MountPicker.Draw();
        MirageUi.SliderInt(
            I18n.Get("settings.dismount"),
            ref C.DismountEnemyDistanceMeters,
            0,
            100);
        MirageUi.SliderFloat(
            I18n.Get("settings.move_refresh"),
            ref C.GroupMovementRefreshIntervalSeconds,
            0.5f,
            3.0f);
        MirageUi.SliderInt(
            I18n.Get("settings.group_radius"),
            ref C.GroupMoveSelfSearchRadiusMeters,
            FrontlineConstants.GroupMoveSelfSearchRadiusMinMeters,
            FrontlineConstants.GroupMoveSelfSearchRadiusMaxMeters);
        MirageUi.SliderInt(
            I18n.Get("settings.stationary"),
            ref C.StationaryTargetExclusionSeconds,
            FrontlineConstants.StationaryTargetExclusionSecondsMin,
            FrontlineConstants.StationaryTargetExclusionSecondsMax);
        MirageUi.SliderInt(
            I18n.Get("settings.repeated"),
            ref C.RepeatedFollowTargetExcludePickCount,
            FrontlineConstants.RepeatedFollowTargetExcludePickCountMin,
            FrontlineConstants.RepeatedFollowTargetExcludePickCountMax);
        ReturnStuckSettings.Draw();

        MirageUi.SubHeader(I18n.Get("settings.spawn"));
        MirageUi.SliderInt(
            I18n.Get("settings.spawn.radius"),
            ref C.SpawnExclusionRadiusMeters,
            0,
            100);

        MirageUi.SubHeader(I18n.Get("settings.duty"));
        using (var group = MirageUi.CheckboxGroup(I18n.Get("settings.auto_enter"), ref C.AutoEnterEnabled))
        {
            if (group.Changed)
                EzConfig.Save();

            using (MirageUi.DisabledIf(!C.AutoEnterEnabled))
            {
                MirageUi.Text(
                    I18n.Get("settings.auto_enter.help"),
                    color: MirageUi.Color.Secondary,
                    wrap: true);
            }
        }

        using (var group = MirageUi.CheckboxGroup(I18n.Get("settings.auto_leave"), ref C.AutoLeaveEnabled))
        {
            if (group.Changed)
                EzConfig.Save();

            using (MirageUi.DisabledIf(!C.AutoLeaveEnabled))
            {
                MirageUi.Text(
                    I18n.Get("settings.auto_leave.help"),
                    color: MirageUi.Color.Secondary,
                    wrap: true);
            }
        }
    }

    private static string[] ModeLabels() =>
    [
        I18n.Get("settings.mode.disable"),
        I18n.Get("settings.mode.manual"),
        I18n.Get("settings.mode.loop"),
    ];

    private static void DrawLanguageRow()
    {
        var selected = NormalizeUiLanguageSetting(C.UiLanguage);
        var labels = new[]
        {
            I18n.Get("settings.language.client"),
            I18n.Get("settings.language.en"),
            I18n.Get("settings.language.ja"),
        };
        var values = new[] { I18n.FollowClient, "en", "ja" };
        var selectedLabel = labels[Array.IndexOf(values, selected)];

        if (!MirageUi.Dropdown(
                I18n.Get("settings.language"),
                ref selectedLabel,
                labels,
                id: "uiLanguage",
                allowClear: false))
            return;

        var index = Array.IndexOf(labels, selectedLabel);
        if (index < 0)
            return;

        var next = values[index];
        if (string.Equals(C.UiLanguage, next, StringComparison.OrdinalIgnoreCase))
            return;

        C.UiLanguage = next;
        EzConfig.Save();
        I18n.ApplyFromConfig();
    }

    private static string NormalizeUiLanguageSetting(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || string.Equals(value, I18n.FollowClient, StringComparison.OrdinalIgnoreCase))
            return I18n.FollowClient;

        var lang = value.Trim().ToLowerInvariant();
        if (lang.Length > 2)
            lang = lang[..2];
        return lang is "en" or "ja" ? lang : I18n.FollowClient;
    }

    private static void DrawModeRow()
    {
        var pluginsReady = RequiredPlugins.AreAllLoaded;
        var labels = ModeLabels();
        var modeIndex = pluginsReady ? (int)C.Mode : (int)PluginMode.Disable;
        if (modeIndex < 0 || modeIndex >= labels.Length)
            modeIndex = 0;

        var selected = labels[modeIndex];
        using (MirageUi.DisabledIf(AutoRunSession.Active || !pluginsReady))
        {
            if (MirageUi.Dropdown(I18n.Get("settings.mode"), ref selected, labels, allowClear: false, id: "mode"))
            {
                var newIndex = Array.IndexOf(labels, selected);
                if (newIndex >= 0)
                {
                    var newMode = (PluginMode)newIndex;
                    if (C.Mode != newMode)
                    {
                        if (newMode != PluginMode.Loop)
                            AutoRunSession.Stop();

                        C.Mode = newMode;
                        EzConfig.Save();
                    }
                }
            }
        }

        if (!pluginsReady)
            MirageUi.Text(RequiredPlugins.GetMissingPluginsMessage(), color: MirageUi.Color.Secondary, wrap: true);

        if (C.Mode != PluginMode.Loop)
            return;

        MirageUi.Text($"{AutoRunSession.CurrentCount} / {C.AutoMaxCount}", color: MirageUi.Color.Secondary);

        var maxCount = C.AutoMaxCount;
        if (MirageUi.InputInt(I18n.Get("settings.loop.count"), ref maxCount, id: "AutoMaxCount"))
            C.AutoMaxCount = Math.Clamp(maxCount, FrontlineConstants.AutoMaxCountMin, FrontlineConstants.AutoMaxCountMax);

        var canStart = RequiredPlugins.AreAllLoaded
            && !AutoRunSession.Active
            && C.AutoMaxCount >= FrontlineConstants.AutoMaxCountMin;

        if (MirageUi.PrimaryButton(I18n.Get("settings.start"), enabled: canStart, id: "loop-start"))
            AutoRunSession.Start();

        ImGui.SameLine();
        if (MirageUi.SecondaryButton(I18n.Get("settings.stop"), enabled: AutoRunSession.Active, id: "loop-stop"))
            AutoRunSession.Stop();
    }
}
