using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Impostor;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Impostor;

public sealed class DistorterOptions : AbstractOptionGroup<DistorterRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleDistorter", "Distorter");

    [ModdedNumberOption("MiraOptionDistorterDistortCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DistortCooldown { get; set; } = 30f;

    [ModdedNumberOption("MiraOptionDistorterPullSpeed", 2f, 20f, 0.5f, MiraNumberSuffixes.Multiplier, "0.0")]
    public float PullSpeed { get; set; } = 8f;

    [ModdedNumberOption("MiraOptionDistorterInvisibilityDuration", 0.5f, 15f, 0.1f, MiraNumberSuffixes.Seconds)]
    public float PostInvisibilityDuration { get; set; } = 5f;

    [ModdedNumberOption("MiraOptionDistorterInvisibleSpeedMultiplier", 1f, 2f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float InvisibleSpeedMultiplier { get; set; } = 1.15f;

    [ModdedToggleOption("MiraOptionDistorterCanVent")]
    public bool CanVent { get; set; } = false;
}
