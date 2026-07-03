using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class CommanderFollowSettings
{
    public static void Draw()
    {
        MirageUi.SubHeader("Commander follow");

        using (var group = MirageUi.CheckboxGroup("Enable commander follow##ExpCmdFollow", ref C.CommanderFollowEnabled))
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
                    "Follow the latest alliance chat speaker during Frontline. "
                    + "Combat mode still takes priority over commander follow.",
                    color: MirageUi.Color.Secondary,
                    wrap: true);
            }
        }
    }
}
