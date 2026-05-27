using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraOverloaded.Roles.Neutral;

namespace MiraOverloaded.Options.Roles.Neutral;

public sealed class HiveMindOptions : AbstractOptionGroup<HiveMindRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleHiveMind", "Hive Mind");

    [ModdedNumberOption("MiraOptionHiveMindKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("MiraOptionHiveMindCanVent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("MiraOptionHiveMindHasImposterVision")]
    public bool ImposterVision { get; set; } = true;

    [ModdedToggleOption("MiraOptionHiveMindHasAssassin")]
    public bool HasAssassin { get; set; } = true;

    [ModdedNumberOption("MiraOptionOtherConnectedMinds", 1f, 4f, 1f)]
    public float OtherConnectedMinds { get; set; } = 2f;

    [ModdedNumberOption("MiraOptionHiveMindDeathCooldownAddition", 0f, 100f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DeathCooldownAddition { get; set; } = 25f;

    [ModdedToggleOption("MiraOptionHiveMindKnowsEachother")]
    public bool HiveMindKnows { get; set; } = true;

    [ModdedToggleOption("MiraOptionHiveMindCooldownsLinked")]
    public bool CooldownsLinked { get; set; } = true;

    [ModdedToggleOption("MiraOptionHiveMindKnowsDeath")]
    public bool KnowsDeath { get; set; } = true;

    [ModdedNumberOption("MiraOptionHiveMindSlowdownAfterKill", 0f, 1f, 0.15f, MiraNumberSuffixes.Multiplier)]
    public float SlowdownAfterKill { get; set; } = 0.15f;

    [ModdedToggleOption("MiraOptionHiveMindScreenDarkenAfterKill")]
    public bool ScreenDarkenAfterKill { get; set; } = true;

    [ModdedNumberOption("MiraOptionHiveMindEffectDuration", 0f, 30f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float EffectDuration { get; set; } = 5f;

    [ModdedNumberOption("MiraOptionHiveMindAwakeningWindow", 5f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float AwakeningWindow { get; set; } = 30f;
}
