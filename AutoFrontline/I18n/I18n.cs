using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AutoFrontline;

internal static class I18n
{
    public const string FollowClient = "client";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static IDalamudPluginInterface pluginInterface;
    private static Dictionary<string, string> strings = new(StringComparer.Ordinal);
    private static string lang = "en";

    public static event Action Reloaded;

    public static string CurrentLang => lang;

    public static void Init(IDalamudPluginInterface plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        pluginInterface = plugin;
        plugin.LanguageChanged += OnClientLanguageChanged;
        ApplyFromConfig();
    }

    public static void Dispose()
    {
        if (pluginInterface != null)
            pluginInterface.LanguageChanged -= OnClientLanguageChanged;
        pluginInterface = null;
        Reloaded = null;
        strings = new(StringComparer.Ordinal);
    }

    public static void ApplyFromConfig()
    {
        Load(ResolveConfiguredLang());
        Reloaded?.Invoke();
    }

    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (strings.TryGetValue(key, out var value) && value != null)
            return value;

        return key;
    }

    public static string Format(string key, params object[] args)
    {
        var template = Get(key);
        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    private static void OnClientLanguageChanged(string langCode)
    {
        if (!IsFollowClient())
            return;

        Load(NormalizeLangCode(langCode));
        Reloaded?.Invoke();
    }

    private static bool IsFollowClient()
    {
        try
        {
            var mode = C.UiLanguage;
            return string.IsNullOrWhiteSpace(mode)
                   || string.Equals(mode, FollowClient, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return true;
        }
    }

    private static string ResolveConfiguredLang()
    {
        try
        {
            var mode = C.UiLanguage?.Trim();
            if (!string.IsNullOrEmpty(mode)
                && !string.Equals(mode, FollowClient, StringComparison.OrdinalIgnoreCase))
                return NormalizeLangCode(mode);
        }
        catch
        {
            // C may be unavailable during early init.
        }

        return NormalizeLangCode(pluginInterface?.UiLanguage);
    }

    private static void Load(string langCode)
    {
        lang = NormalizeLangCode(langCode);
        var map = ReadLangFile(lang);
        if (map.Count == 0 && !string.Equals(lang, "en", StringComparison.Ordinal))
            map = ReadLangFile("en");

        strings = map;
    }

    private static string NormalizeLangCode(string langCode)
    {
        var code = string.IsNullOrWhiteSpace(langCode) ? "en" : langCode.Trim().ToLowerInvariant();
        return code.Length > 2 ? code[..2] : code;
    }

    private static Dictionary<string, string> ReadLangFile(string language)
    {
        try
        {
            var pi = pluginInterface ?? Svc.PluginInterface;
            var dir = pi.AssemblyLocation.DirectoryName ?? AppContext.BaseDirectory;
            var path = Path.Combine(dir, "Data", "I18n", $"{language}.json");
            if (!File.Exists(path))
                return new Dictionary<string, string>(StringComparer.Ordinal);

            var json = File.ReadAllText(path);
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);
            return parsed == null
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : new Dictionary<string, string>(parsed, StringComparer.Ordinal);
        }
        catch (Exception ex)
        {
            try
            {
                Svc.Log.Warning(ex, "Failed to load i18n file for {Lang}", language);
            }
            catch
            {
                // Svc may not be ready during early init.
            }

            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }
}
