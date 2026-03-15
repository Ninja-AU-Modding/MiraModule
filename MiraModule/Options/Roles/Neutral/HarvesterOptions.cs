using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Neutral;

namespace MiraModule.Options.Roles.Neutral;

public sealed class HarvesterOptions : AbstractOptionGroup<HarvesterRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleHarvester", "Harvester");

    [ModdedNumberOption("MiraOptionHarvesterKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("MiraOptionHarvesterImpostorVision")]
    public bool ImpostorVision { get; set; } = true;

    [ModdedToggleOption("MiraOptionHarvesterCanVent")]
    public bool CanVent { get; set; }
}
