using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraOverloaded.Roles.Impostor;
namespace MiraOverloaded.Options.Roles.Impostor;

public sealed class NinjaOptions : AbstractOptionGroup<NinjaRole>
{
    public override string GroupName => MiraOverloadedLocale.GetString("MiraRoleNinja", "Ninja");

    [ModdedNumberOption("MiraOptionNinjaMarkCooldown", 0f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MarkCooldown { get; set; } = 10f;

    [ModdedNumberOption("MiraOptionNinjaAssassinateCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AssassinateCooldown { get; set; } = 25f;

    [ModdedNumberOption("MiraOptionNinjaInvisibilityDuration", 1f, 30f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float InvisibilityDuration { get; set; } = 6f;

    [ModdedNumberOption("MiraOptionNinjaTraceDuration", 1f, 30f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float TraceDuration { get; set; } = 10f;

    [ModdedToggleOption("MiraOptionNinjaCanVent")]
    public bool CanVent { get; set; } = false;
}
