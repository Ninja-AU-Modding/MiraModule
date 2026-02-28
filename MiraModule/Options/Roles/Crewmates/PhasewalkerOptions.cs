using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Crewmate;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Crewmates;

public sealed class PhasewalkerOptions : AbstractOptionGroup<PhasewalkerRole>
{
    public override string GroupName => TouLocale.Get("MiraRolePhasewalker", "Phasewalker");

    [ModdedNumberOption("Phase Cooldown (s)", 25f, 55f, 5f, MiraNumberSuffixes.Seconds)]
    public float PhaseCooldown { get; set; } = 35f;

    [ModdedNumberOption("Phase Duration (s)", 2f, 4f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float PhaseDuration { get; set; } = 3f;

    [ModdedNumberOption("Phase Speed Multiplier", 0.85f, 0.90f, 0.05f)]
    public float PhaseSpeedMultiplier { get; set; } = 0.875f;

    [ModdedToggleOption("Show Distortion Trail")]
    public bool ShowDistortionTrail { get; set; } = true;
}
