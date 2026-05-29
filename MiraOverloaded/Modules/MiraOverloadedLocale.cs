using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using BepInEx;
using MiraAPI.LocalSettings;
using MiraOverloaded.Options;

namespace MiraOverloaded.Modules;

public static class MiraOverloadedLocale
{
    private const string DefaultLocale = "en_US";
    private const string ExternalFilePrefix = "MiraOverloaded.";

    private static readonly Dictionary<string, Dictionary<string, string>> Translations =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> EmbeddedLocaleFiles =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Dictionary<string, string>> ExternalOverrides =
        new(StringComparer.OrdinalIgnoreCase);
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        DiscoverEmbeddedLocales();
        LoadEmbeddedLocales();
        LoadExternalOverrides();
        _initialized = true;
    }

    public static string GetString(string name, string? defaultValue = null)
    {
        return GetStringForLocale(GetSelectedLocaleCode(), name, defaultValue);
    }

    public static string GetStringForLocale(string localeCode, string name, string? defaultValue = null)
    {
        var normalized = NormalizeLocaleCode(localeCode) ?? DefaultLocale;
        var text = defaultValue ?? "STRMISS_" + name;

        Translations.TryGetValue(DefaultLocale, out var englishDict);
        if (englishDict != null && englishDict.TryGetValue(name, out var englishText))
        {
            text = englishText;
        }

        if (!normalized.Equals(DefaultLocale, StringComparison.OrdinalIgnoreCase) &&
            Translations.TryGetValue(normalized, out var localeDict) &&
            localeDict.TryGetValue(name, out var localeText))
        {
            text = localeText;
        }

        if (englishDict != null &&
            englishDict.ContainsKey(name) &&
            ExternalOverrides.TryGetValue(normalized, out var overrideDict) &&
            overrideDict.TryGetValue(name, out var overrideText))
        {
            text = overrideText;
        }

        return ParseText(text);
    }

    private static string GetSelectedLocaleCode()
    {
        var raw = LocalSettingsTabSingleton<MiraOverloadedOptions>.Instance?.Locale?.Value.ToString();
        var normalized = NormalizeLocaleCode(raw);
        return normalized != null && EmbeddedLocaleFiles.ContainsKey(normalized)
            ? normalized
            : DefaultLocale;
    }

    private static void DiscoverEmbeddedLocales()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string prefix = "MiraOverloaded.Resources.Locale.";

        EmbeddedLocaleFiles.Clear();

        foreach (var resourceName in assembly.GetManifestResourceNames()
                     .Where(n => n.StartsWith(prefix, StringComparison.Ordinal) &&
                                 n.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
        {
            var fileName = resourceName[prefix.Length..];
            var localeCode = Path.GetFileNameWithoutExtension(fileName);
            if (!string.IsNullOrWhiteSpace(localeCode))
            {
                EmbeddedLocaleFiles[localeCode] = fileName;
            }
        }
    }

    private static void LoadEmbeddedLocales()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string prefix = "MiraOverloaded.Resources.Locale.";

        foreach (var (localeCode, fileName) in EmbeddedLocaleFiles)
        {
            using var stream = assembly.GetManifestResourceStream(prefix + fileName);
            if (stream == null)
            {
                Error($"Failed to load embedded locale: {fileName}");
                continue;
            }

            using var reader = new StreamReader(stream);
            Translations[localeCode] = new Dictionary<string, string>();
            ParseXml(reader.ReadToEnd(), Translations[localeCode]);
        }
    }

    private static void LoadExternalOverrides()
    {
        if (!Translations.TryGetValue(DefaultLocale, out var bakedEnglish))
        {
            return;
        }

        foreach (var localeCode in EmbeddedLocaleFiles.Keys)
        {
            var fileName = ExternalFilePrefix + localeCode + ".xml";
            var filePath = Path.Combine(Paths.PluginPath, fileName);
            if (!File.Exists(filePath))
            {
                continue;
            }

            try
            {
                var parsed = new Dictionary<string, string>();
                ParseXml(File.ReadAllText(filePath), parsed);

                ExternalOverrides[localeCode] = parsed
                    .Where(kv => bakedEnglish.ContainsKey(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                Info($"Loaded external locale override for {localeCode}: {fileName} " +
                     $"({ExternalOverrides[localeCode].Count} strings)");
            }
            catch (Exception ex)
            {
                Error($"Failed to load external locale override {fileName}: {ex.Message}");
            }
        }
    }

    private static string? NormalizeLocaleCode(string? code)
    {
        return string.IsNullOrWhiteSpace(code) ? null : code.Trim().Replace('-', '_');
    }

    private static void ParseXml(string xmlContent, Dictionary<string, string> target)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        var root = xmlDoc.DocumentElement;
        if (root == null || root.Name != "resources")
        {
            Error("Locale XML root node must be <resources>.");
            return;
        }

        var nodes = root.SelectNodes("string");
        if (nodes == null)
        {
            return;
        }

        foreach (XmlNode node in nodes)
        {
            var name = node.Attributes?["name"]?.Value;
            if (!string.IsNullOrWhiteSpace(name))
            {
                target[name] = node.InnerXml ?? string.Empty;
            }
        }
    }

    private static string ParseText(string text)
    {
        text = text.Replace("[nl]", "\n");
        text = text.Replace("[and]", "&");
        text = Regex.Replace(text, @"\[([^\]]+)\]", @"<$1>");
        return text;
    }
}
