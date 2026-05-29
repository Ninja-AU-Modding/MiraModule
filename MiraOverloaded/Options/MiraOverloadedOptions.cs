using BepInEx.Configuration;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;

namespace MiraOverloaded.Options;

public enum GameLocale
{
    en_US,
    es_ES,
}

public sealed class MiraOverloadedOptions(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "Mira Overloaded";

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabColor = MiraOverloadedColors.MiraOverloaded,
    };

    [LocalToggleSetting]
    public ConfigEntry<bool> DisableRoles { get; } = config.Bind("General", "Disable Roles", true);

    [LocalEnumSetting]
    public ConfigEntry<GameLocale> Locale { get; } = config.Bind("General", "Game Locale", GameLocale.en_US);
}
