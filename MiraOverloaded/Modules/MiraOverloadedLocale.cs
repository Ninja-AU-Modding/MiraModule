using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using BepInEx;
using MiraAPI.GameOptions;
using MiraOverloaded.Options;

namespace MiraOverloaded.Modules;

public static class MiraOverloadedLocale
{
    private const string DefaultLocale = "en_US";
    private const string ExternalFileName = "MiraOverloaded.locale.xml";

    private static readonly Dictionary<string, Dictionary<string, string>> Translations =
        new(StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, string>? _externalOverride;
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        LoadEmbeddedLocales();
        LoadExternalOverride();
        _initialized = true;
    }

    public static string GetString(string name, string? defaultValue = null)
    {
        var locale = OptionGroupSingleton<MiraOverloadedOptions>.Instance.Locale.ToString();
        return GetStringForLocale(locale, name, defaultValue);
    }

    public static string GetStringForLocale(string localeCode, string name, string? defaultValue = null)
    {
        if (_externalOverride != null && _externalOverride.TryGetValue(name, out var overrideText))
        {
            return ParseText(overrideText);
        }

        var text = defaultValue ?? "STRMISS_" + name;

        if (Translations.TryGetValue(DefaultLocale, out var englishDict) &&
            englishDict.TryGetValue(name, out var englishText))
        {
            text = englishText;
        }

        if (!localeCode.Equals(DefaultLocale, StringComparison.OrdinalIgnoreCase) &&
            Translations.TryGetValue(localeCode, out var localeDict) &&
            localeDict.TryGetValue(name, out var localeText))
        {
            text = localeText;
        }

        return ParseText(text);
    }

    private static void LoadEmbeddedLocales()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string prefix = "MiraOverloaded.Resources.Locale.";

        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix, StringComparison.Ordinal) &&
                        n.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

        foreach (var resourceName in resourceNames)
        {
            var localeCode = Path.GetFileNameWithoutExtension(resourceName[prefix.Length..]);
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Error($"Failed to load embedded locale: {resourceName}");
                continue;
            }

            using var reader = new StreamReader(stream);
            Translations[localeCode] = new Dictionary<string, string>();
            ParseXml(reader.ReadToEnd(), Translations[localeCode]);
        }
    }

    private static void LoadExternalOverride()
    {
        var overridePath = Path.Combine(Paths.PluginPath, ExternalFileName);
        if (!File.Exists(overridePath))
        {
            return;
        }

        try
        {
            _externalOverride = new Dictionary<string, string>();
            ParseXml(File.ReadAllText(overridePath), _externalOverride);
            Info($"Loaded external locale override: {overridePath}");
        }
        catch (Exception ex)
        {
            Error($"Failed to load external locale override: {ex.Message}");
        }
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
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            target[name] = node.InnerXml ?? string.Empty;
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
