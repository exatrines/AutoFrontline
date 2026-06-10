using AutoFrontline.Services;
using ECommons.ExcelServices;

namespace AutoFrontline.UI;

internal static class PvpLimitBreakSettings
{
    public static void Draw()
    {
        AflImGui.SectionHeader("Auto Limit Break");
        ImGui.TextWrapped("Hostile mode only. Uses /pvpaction when enabled for your current job.");
        ImGui.Spacing();

        Job? previousJob = null;
        foreach (var entry in PvpLimitBreakCatalog.All)
        {
            if (previousJob != entry.Job)
            {
                previousJob = entry.Job;
                ImGui.Spacing();
                ImGui.TextDisabled(PvpLimitBreakCatalog.GetJobLabel(entry.Job));
            }

            var enabled = PvpLimitBreakCatalog.IsEnabled(entry.Id);
            if (ImGui.Checkbox($"{entry.ActionName}##Lb{entry.Id}", ref enabled))
                PvpLimitBreakCatalog.SetEnabled(entry.Id, enabled);
        }

        if (PvpLimitBreakCatalog.IsEnabled("SMN_Bahamut") && PvpLimitBreakCatalog.IsEnabled("SMN_Phoenix"))
            ImGui.TextDisabled("Note: only the first enabled Summoner option is used (Bahamut).");
    }
}
