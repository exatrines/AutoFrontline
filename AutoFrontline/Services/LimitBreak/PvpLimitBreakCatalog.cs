using System.Collections.Generic;
using ECommons.ExcelServices;
using ActionSheet = Lumina.Excel.Sheets.Action;

namespace AutoFrontline.Services;

/// <summary>ActionId は Action シートの PvP 用 LB 行（言語に依存しない）。</summary>
internal readonly record struct PvpLimitBreakEntry(string Id, Job Job, uint ActionId);

/// <summary>PvP リミットブレイク（/pvpaction）とジョブの対応表。アクション名はクライアント言語の Action シートから取得する。</summary>
internal static class PvpLimitBreakCatalog
{
    public static readonly IReadOnlyList<PvpLimitBreakEntry> All =
    [
        new("WHM", Job.WHM, 29230),         // ハート・オブ・パーゲーション
        new("SCH", Job.SCH, 29237),         // サモン・セラフィム
        new("AST", Job.AST, 29255),         // 星河一天
        new("SGE", Job.SGE, 29266),         // メソテース
        new("BLM", Job.BLM, 29662),         // ソウルレゾナンス
        new("RDM", Job.RDM, 41498),         // サザンクロス
        new("SMN_Bahamut", Job.SMN, 29673), // サモン・バハムート
        new("SMN_Phoenix", Job.SMN, 29678), // サモン・フェニックス
        new("PCT", Job.PCT, 39215),         // ウォール・オブ・ファット
        new("MCH", Job.MCH, 29415),         // 魔弾の射手
        new("BRD", Job.BRD, 29401),         // 英雄のファンタジア
        new("DNC", Job.DNC, 29432),         // コントラダンス
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

            actionName = GetActionName(entry);
            return actionName.Length > 0;
        }

        actionName = string.Empty;
        return false;
    }

    /// <summary>クライアント言語のアクション名。行が見つからなければ空文字。</summary>
    public static string GetActionName(PvpLimitBreakEntry entry) =>
        Svc.Data.GetExcelSheet<ActionSheet>().GetRowOrDefault(entry.ActionId)?.Name.ToString() ?? string.Empty;

    public static string GetJobLabel(Job job) => job.GetData().Name.ToString();
}
