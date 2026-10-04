using AutoFrontline.Services;
using ECommons.ExcelServices;

namespace AutoFrontline.UI;

internal static class PvpLimitBreakSettings
{
    public static void Draw()
    {
        MirageUi.SubHeader("Auto Limit Break");
        MirageUi.Text(
            "Hostile mode only. Uses /pvpaction when enabled for your current job.",
            wrap: true);

        Job? previousJob = null;
        foreach (var entry in PvpLimitBreakCatalog.All)
        {
            if (previousJob != entry.Job)
            {
                previousJob = entry.Job;
                MirageUi.Text(
                    PvpLimitBreakCatalog.GetJobLabel(entry.Job),
                    color: MirageUi.Color.Secondary,
                    wrap: true,
                    spaced: true);
            }

            var name = PvpLimitBreakCatalog.GetActionName(entry);
            if (name.Length == 0)
                name = $"Action #{entry.ActionId}";

            var enabled = PvpLimitBreakCatalog.IsEnabled(entry.Id);
            if (MirageUi.Checkbox($"{name}##Lb{entry.Id}", ref enabled))
                PvpLimitBreakCatalog.SetEnabled(entry.Id, enabled);
        }

        if (PvpLimitBreakCatalog.IsEnabled("SMN_Bahamut") && PvpLimitBreakCatalog.IsEnabled("SMN_Phoenix"))
        {
            MirageUi.Text(
                "Note: only the first enabled Summoner option is used (Bahamut).",
                color: MirageUi.Color.Secondary,
                wrap: true);
        }
    }
}
