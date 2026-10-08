namespace AutoFrontline.UI;

internal static class ReturnStuckSettings
{
    public static void Draw()
    {
        MirageUi.SubHeader(I18n.Get("settings.return.header"));
        MirageUi.Text(
            I18n.Format("settings.return.help", FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters),
            wrap: true);

        var seconds = C.DejonStallSeconds;
        if (MirageUi.SliderFloat(
                I18n.Get("settings.return.stall"),
                ref seconds,
                FrontlineConstants.DejonStallSecondsMin,
                FrontlineConstants.DejonStallSecondsMax,
                format: "%.0f",
                id: "ReturnStall"))
        {
            C.DejonStallSeconds = seconds;
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
            EzConfig.Save();
    }
}
