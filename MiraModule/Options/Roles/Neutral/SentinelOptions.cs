using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Neutral;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Neutral;

public sealed class SentinelOptions : AbstractOptionGroup<SentinelRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleSentinel", "Sentinel");

    [ModdedNumberOption("MiraOptionSentinelKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedNumberOption("MiraOptionSentinelExplodeCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ExplodeCooldown { get; set; } = 60f;

    public ModdedNumberOption ExplosionRadius { get; set; } = new("MiraOptionSentinelExplosionRadius", 0.25f, 0.05f, 1f, 0.05f,
        MiraNumberSuffixes.Multiplier, "0.00");

    [ModdedToggleOption("MiraOptionSentinelImpostorVision")]
    public bool ImpostorVision { get; set; } = true;

    [ModdedToggleOption("MiraOptionSentinelCanVent")]
    public bool CanVent { get; set; }
}