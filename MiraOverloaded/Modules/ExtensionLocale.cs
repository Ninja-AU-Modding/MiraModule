using System.Reflection;
using BepInEx.Logging;
using MiraAPI.Utilities;

namespace MiraOverloaded.Modules;

public static class ExtensionLocale
{
    // If anyone sees this, keep it as a seperate logger so if there's any issues with the locale specifically I can filter thru
    internal static ManualLogSource LocaleLogger { get; } = BepInEx.Logging.Logger.CreateLogSource("MiraOverloadedLocale");

    public static void SearchInternalLocale()
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var locale in TouLocale.LangList)
        {
            using var resourceStream =
                assembly.GetManifestResourceStream("MiraOverloaded.Resources.Locale." + locale.Value);
            if (resourceStream == null)
            {
                LocaleLogger.LogError($"MiraOverloaded Language is not added: {locale.Key.ToDisplayString()}");
                continue;
            }

            LocaleLogger.LogWarning($"MiraOverloaded Language is being added: {locale.Key.ToDisplayString()}");
            using StreamReader reader = new(resourceStream);
            string xmlContent = reader.ReadToEnd();

            TouLocale.TouLocalization.TryAdd((SupportedLangs)locale.Key, []);
            TouLocale.ParseXmlFile(xmlContent, (SupportedLangs)locale.Key);
        }
    }
}
