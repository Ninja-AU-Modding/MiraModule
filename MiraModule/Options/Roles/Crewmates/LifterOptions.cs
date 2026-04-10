using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Crewmate;

namespace MiraModule.Options.Roles.Crewmates;

public sealed class LifterOptions : AbstractOptionGroup<LifterRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleLifter", "Lifter");

    [ModdedNumberOption("MiraOptionLifterLiftCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float LiftCooldown { get; set; } = 20f;
}
