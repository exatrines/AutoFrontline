using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class CommanderFollowSettings
{
    public static void Draw()
    {
        MirageUi.SubHeader(I18n.Get("experimental.commander"));

        using (var group = MirageUi.CheckboxGroup(I18n.Get("experimental.commander.enable"), ref C.CommanderFollowEnabled))
        {
            if (group.Changed)
            {
                if (!C.CommanderFollowEnabled)
                    AllianceCommanderTracker.DismissFollowRequest();

                EzConfig.Save();
            }

            using (MirageUi.DisabledIf(!C.CommanderFollowEnabled))
            {
                MirageUi.Text(
                    I18n.Get("experimental.commander.help"),
                    color: MirageUi.Color.Secondary,
                    wrap: true);
            }
        }
    }
}
