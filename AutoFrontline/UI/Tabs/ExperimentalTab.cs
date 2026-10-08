namespace AutoFrontline.UI;

public static class ExperimentalTab
{
    public static void Draw()
    {
        MirageUi.Header("Experimental");

        MirageUi.Text(
            I18n.Get("experimental.notice"),
            color: MirageUi.Color.Secondary,
            wrap: true);

        CommanderFollowSettings.Draw();
        HostileModeSettings.Draw();
        PvpLimitBreakSettings.Draw();
    }
}
