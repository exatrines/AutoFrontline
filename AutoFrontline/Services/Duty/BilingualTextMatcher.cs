namespace AutoFrontline.Services;

internal static class BilingualTextMatcher
{
    public static bool IsNullOrWhiteSpace(string text) => string.IsNullOrWhiteSpace(text);

    public static bool ContainsAll(string text, StringComparison comparison, params string[] required)
    {
        foreach (var pattern in required)
        {
            if (!text.Contains(pattern, comparison))
                return false;
        }

        return true;
    }

    public static bool ContainsAny(string text, StringComparison comparison, params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            if (text.Contains(pattern, comparison))
                return true;
        }

        return false;
    }

    public static bool EqualsNormalized(string left, string right)
    {
        if (IsNullOrWhiteSpace(left) || IsNullOrWhiteSpace(right))
            return false;

        return string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);
    }

    public static string Normalize(string text) =>
        string.Join(' ', text.Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries)).Trim();
}
