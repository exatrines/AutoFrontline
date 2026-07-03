namespace AutoFrontline.UI;

internal static class ReturnStuckSettings
{
    public static void Draw(float width)
    {
        MirageUi.SubHeader("Return (stuck recovery)");
        MirageUi.Text(
            "During group movement, use Return when your position stays within 1m "
            + $"for this duration while {FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters}m+ from the destination.",
            wrap: true);

        var seconds = C.DejonStallSeconds;
        if (MirageUi.SliderFloat(
                "Stall duration (sec)##ReturnStall",
                ref seconds,
                FrontlineConstants.DejonStallSecondsMin,
                FrontlineConstants.DejonStallSecondsMax,
                width,
                format: "%.0f"))
        {
            C.DejonStallSeconds = seconds;
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
            EzConfig.Save();
    }
}
