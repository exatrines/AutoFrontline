using Lumina.Excel.Sheets;

namespace AutoFrontline.Services;

/// <summary>デイリーチャレンジ・フロントラインの ContentFinderCondition を解決する。</summary>
internal static class DailyFrontlineCfcResolver
{
    private static uint? cachedRowId;
    private static string cachedDutyName = string.Empty;

    public static bool TryResolve(out uint contentFinderConditionRowId, out string dutyName)
    {
        if (cachedRowId is uint rowId && cachedDutyName.Length > 0)
        {
            contentFinderConditionRowId = rowId;
            dutyName = cachedDutyName;
            return true;
        }

        var sheet = Svc.Data.GetExcelSheet<ContentFinderCondition>();
        if (sheet == null)
        {
            contentFinderConditionRowId = 0;
            dutyName = string.Empty;
            return false;
        }

        foreach (var row in sheet)
        {
            if (!row.DailyFrontlineChallenge)
                continue;

            var name = row.Name.ToString();
            if (BilingualTextMatcher.IsNullOrWhiteSpace(name))
                continue;

            cachedRowId = row.RowId;
            cachedDutyName = name;
            contentFinderConditionRowId = row.RowId;
            dutyName = name;
            return true;
        }

        contentFinderConditionRowId = 0;
        dutyName = string.Empty;
        return false;
    }
}
