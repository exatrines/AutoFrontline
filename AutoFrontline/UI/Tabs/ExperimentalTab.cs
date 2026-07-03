namespace AutoFrontline.UI;

public static class ExperimentalTab
{
    private const float ControlWidth = 200f;

    public static void Draw()
    {
        MirageUi.SubHeader("Experimental");

        MirageUi.Text(
            "Experimental features may change or be removed without notice.",
            color: MirageUi.Color.Secondary,
            wrap: true);

        CommanderFollowSettings.Draw();
        HostileModeSettings.Draw(ControlWidth);
        PvpLimitBreakSettings.Draw();
    }
}
