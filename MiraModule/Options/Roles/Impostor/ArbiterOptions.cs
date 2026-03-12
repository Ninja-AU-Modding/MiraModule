using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Impostor;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Impostor;

public sealed class ArbiterOptions : AbstractOptionGroup<ArbiterRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleArbiter", "Arbiter");

    [ModdedNumberOption("MiraOptionArbiterTargetCooldown", 0f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TargetCooldown { get; set; } = 10f;

    [ModdedToggleOption("MiraOptionArbiterCanChangeTarget")]
    public bool CanChangeTarget { get; set; } = false;

    [ModdedNumberOption("MiraOptionArbiterKillCooldownMultiplier", 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float KillCooldownMultiplier { get; set; } = 0.5f;

    [ModdedNumberOption("MiraOptionArbiterInvisUses", 0f, 5f, 1f, MiraNumberSuffixes.None)]
    public float InvisUses { get; set; } = 1f;

    [ModdedNumberOption("MiraOptionArbiterInvisDuration", 1f, 30f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float InvisDuration { get; set; } = 8f;

    [ModdedNumberOption("MiraOptionArbiterSpeedUses", 0f, 5f, 1f, MiraNumberSuffixes.None)]
    public float SpeedUses { get; set; } = 1f;

    [ModdedNumberOption("MiraOptionArbiterSpeedDuration", 1f, 30f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float SpeedDuration { get; set; } = 10f;

    [ModdedNumberOption("MiraOptionArbiterSpeedMultiplier", 1f, 2f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float SpeedMultiplier { get; set; } = 1.25f;

    [ModdedToggleOption("MiraOptionArbiterCanVent")]
    public bool CanVent { get; set; } = false;
}
