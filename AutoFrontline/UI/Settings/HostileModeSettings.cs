using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class HostileModeSettings
{
    public static void Draw()
    {
        MirageUi.SubHeader(I18n.Get("experimental.combat"));

        using (var group = MirageUi.CheckboxGroup(I18n.Get("experimental.combat.enable"), ref C.HostileModeEnabled))
        {
            if (group.Changed)
                EzConfig.Save();

            using (MirageUi.DisabledIf(!C.HostileModeEnabled))
            {
                MirageUi.Text(
                    I18n.Get("experimental.combat.help"),
                    color: MirageUi.Color.Secondary,
                    wrap: true);

                MirageUi.SliderFloat(
                    I18n.Get("experimental.combat.refresh"),
                    ref C.HostileModeRefreshIntervalSeconds,
                    0.5f,
                    3.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                    EzConfig.Save();

                MirageUi.SliderFloat(
                    I18n.Get("experimental.combat.position"),
                    ref C.HostileModePositionRatio,
                    0f,
                    1f,
                    format: "%.2f");
                if (ImGui.IsItemDeactivatedAfterEdit())
                    EzConfig.Save();
            }
        }
    }
}
