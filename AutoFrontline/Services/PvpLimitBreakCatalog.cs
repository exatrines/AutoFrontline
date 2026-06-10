using System.Collections.Generic;
using ECommons.ExcelServices;

namespace AutoFrontline.Services;

internal readonly record struct PvpLimitBreakEntry(string Id, Job Job, string ActionName);

/// <summary>PvP リミットブレイク（/pvpaction）とジョブの対応表。</summary>
internal static class PvpLimitBreakCatalog
{
    public static readonly IReadOnlyList<PvpLimitBreakEntry> All =
    [
        new("WHM", Job.WHM, "ハート・オブ・パーゲーション"),
        new("SCH", Job.SCH, "サモン・セラフィム"),
        new("AST", Job.AST, "星河一天"),
        new("SGE", Job.SGE, "メソテース"),
        new("BLM", Job.BLM, "ソウルレゾナンス"),
        new("RDM", Job.RDM, "サザンクロス"),
        new("SMN_Bahamut", Job.SMN, "サモン・バハムート"),
        new("SMN_Phoenix", Job.SMN, "サモン・フェニックス"),
        new("PCT", Job.PCT, "ウォール・オブ・ファット"),
        new("MCH", Job.MCH, "魔弾の射手"),
        new("BRD", Job.BRD, "英雄のファンタジア"),
        new("DNC", Job.DNC, "コントラダンス"),
    ];

    public static bool IsEnabled(string entryId) =>
        C.AutoLimitBreakByEntryId.TryGetValue(entryId, out var enabled) && enabled;

    public static void SetEnabled(string entryId, bool enabled)
    {
        C.AutoLimitBreakByEntryId[entryId] = enabled;
        EzConfig.Save();
    }

    public static bool TryGetEnabledActionForJob(Job job, out string actionName)
    {
        foreach (var entry in All)
        {
            if (entry.Job != job || !IsEnabled(entry.Id))
                continue;

            actionName = entry.ActionName;
            return true;
        }

        actionName = string.Empty;
        return false;
    }

    public static string GetJobLabel(Job job) => job.GetData().Name.ToString();
}
