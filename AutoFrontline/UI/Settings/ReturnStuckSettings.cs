namespace AutoFrontline.UI;

internal static class ReturnStuckSettings
{
    public static void Draw()
    {
        AflImGui.SectionHeader("Return (stuck recovery)");
        ImGui.TextWrapped(
            "During group movement, use Return when your position stays within 1m "
            + $"for this duration while {FrontlineConstants.NaviStuckDejonMinDestinationDistanceMeters}m+ from the destination.");
        ImGui.Spacing();

        var seconds = C.DejonStallSeconds;
        seconds = Math.Clamp(seconds, FrontlineConstants.DejonStallSecondsMin, FrontlineConstants.DejonStallSecondsMax);
        ImGui.SetNextItemWidth(AflImGui.DefaultSliderWidth);
        if (ImGui.SliderFloat(
                "Stall duration (sec)##ReturnStall",
                ref seconds,
                FrontlineConstants.DejonStallSecondsMin,
                FrontlineConstants.DejonStallSecondsMax,
                "%.0f"))
        {
            C.DejonStallSeconds = seconds;
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
            EzConfig.Save();
    }
}
