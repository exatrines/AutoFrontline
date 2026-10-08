using AutoFrontline.Services;
using ECommons.ExcelServices;

namespace AutoFrontline.UI;

internal static class PvpLimitBreakSettings
{
    public static void Draw()
    {
        MirageUi.SubHeader(I18n.Get("experimental.lb"));
        MirageUi.Text(
            I18n.Get("experimental.lb.help"),
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

            var enabled = PvpLimitBreakCatalog.IsEnabled(entry.Id);
            if (MirageUi.Checkbox($"{entry.ActionName}##Lb{entry.Id}", ref enabled))
                PvpLimitBreakCatalog.SetEnabled(entry.Id, enabled);
        }

        if (PvpLimitBreakCatalog.IsEnabled("SMN_Bahamut") && PvpLimitBreakCatalog.IsEnabled("SMN_Phoenix"))
        {
            MirageUi.Text(
                I18n.Get("experimental.lb.smn_note"),
                color: MirageUi.Color.Secondary,
                wrap: true);
        }
    }
}
