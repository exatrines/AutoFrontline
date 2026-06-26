using Lumina.Excel.Sheets;

namespace AutoFrontline.Services;

/// <summary>デイリーチャレンジ・フロントラインの ContentRoulette（コンテンツルーレット）を解決する。</summary>
internal static class DailyFrontlineRouletteResolver
{
    private static byte? cachedRowId;
    private static string cachedName = string.Empty;

    public static bool TryResolve(out byte contentRouletteRowId, out string rouletteName)
    {
        if (cachedRowId is byte rowId && cachedName.Length > 0)
        {
            contentRouletteRowId = rowId;
            rouletteName = cachedName;
            return true;
        }

        var sheet = Svc.Data.GetExcelSheet<ContentRoulette>();
        if (sheet == null)
        {
            contentRouletteRowId = 0;
            rouletteName = string.Empty;
            return false;
        }

        foreach (var row in sheet)
        {
            if (!row.IsInDutyFinder || !row.IsPvP)
                continue;

            var name = row.Name.ToString();
            if (!MatchesDailyFrontlineRouletteName(name))
                continue;

            cachedRowId = (byte)row.RowId;
            cachedName = name;
            contentRouletteRowId = (byte)row.RowId;
            rouletteName = name;
            return true;
        }

        contentRouletteRowId = 0;
        rouletteName = string.Empty;
        return false;
    }

    public static bool MatchesDailyFrontlineRouletteName(string text)
    {
        if (BilingualTextMatcher.IsNullOrWhiteSpace(text))
            return false;

        if (BilingualTextMatcher.ContainsAll(text, StringComparison.Ordinal, "デイリー", "フロントライン"))
            return true;

        return BilingualTextMatcher.ContainsAll(
            text,
            StringComparison.OrdinalIgnoreCase,
            "Daily Challenge",
            "Frontline");
    }

    /// <summary>CF リスト行 Text #6 など、短い表示名向けの厳密マッチ。</summary>
    public static bool MatchesFrontlineListLabel(string text)
    {
        if (BilingualTextMatcher.IsNullOrWhiteSpace(text))
            return false;

        var normalized = BilingualTextMatcher.Normalize(text);
        if (normalized.Equals("フロントライン", StringComparison.Ordinal))
            return true;
        if (normalized.Equals("Frontline", StringComparison.OrdinalIgnoreCase))
            return true;

        return MatchesFrontlineListRowName(normalized);
    }

    /// <summary>CF 詳細パネル名など、部分一致でよい場合。</summary>
    public static bool MatchesFrontlineDetailName(string text)
    {
        if (BilingualTextMatcher.IsNullOrWhiteSpace(text))
            return false;

        return MatchesDailyFrontlineRouletteName(text)
            || MatchesFrontlineListRowName(text)
            || text.Contains("Frontline", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>CF リスト行の部分一致（ツリー StringValues 用）。</summary>
    public static bool MatchesFrontlineListRowName(string text) =>
        !BilingualTextMatcher.IsNullOrWhiteSpace(text)
        && text.Contains("フロントライン", StringComparison.Ordinal);

    public static bool IsDailyChallengeSectionHeader(string text)
    {
        if (BilingualTextMatcher.IsNullOrWhiteSpace(text))
            return false;

        if (text.Contains("デイリーチャレンジ", StringComparison.Ordinal))
            return true;

        return text.Contains("Daily Challenge", StringComparison.OrdinalIgnoreCase);
    }
}
