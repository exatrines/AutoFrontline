using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class HostileModeSettings
{
    public static void Draw(float controlWidth)
    {
        MirageUi.SubHeader("Combat mode");

        using (var group = MirageUi.CheckboxGroup("Enable combat mode##ExpHostileMode", ref C.HostileModeEnabled))
        {
            if (group.Changed)
                EzConfig.Save();

            using (MirageUi.DisabledIf(!C.HostileModeEnabled))
            {
                MirageUi.Text(
                    "When enabled, move toward allies near the closest enemy within 30m. "
                    + "Takes priority over commander follow and group movement.",
                    color: MirageUi.Color.Secondary,
                    wrap: true);

                MirageUi.SliderFloat(
                    "Combat mode refresh (sec)",
                    ref C.HostileModeRefreshIntervalSeconds,
                    0.5f,
                    3.0f,
                    controlWidth);
                if (ImGui.IsItemDeactivatedAfterEdit())
                    EzConfig.Save();

                MirageUi.SliderFloat(
                    "Combat mode position",
                    ref C.HostileModePositionRatio,
                    0f,
                    1f,
                    controlWidth,
                    format: "%.2f");
                if (ImGui.IsItemDeactivatedAfterEdit())
                    EzConfig.Save();
            }
        }
    }
}
