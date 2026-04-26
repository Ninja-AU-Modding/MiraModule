using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraOverloaded.Roles.Impostor;

namespace MiraOverloaded.Options.Roles.Impostor;

public sealed class RevenantOptions : AbstractOptionGroup<RevenantRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleRevenant", "Revenant");

    [ModdedNumberOption("MiraOptionRevenantFakeDeathCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float FakeDeathCooldown { get; set; } = 30f;

    [ModdedNumberOption("MiraOptionRevenantGhostKills", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public float GhostKills { get; set; } = 2f;

    [ModdedNumberOption("MiraOptionRevenantGhostKillCooldown", 10f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GhostKillCooldown { get; set; } = 40f;

    [ModdedToggleOption("MiraOptionRevenantCanVent")]
    public bool CanVent { get; set; } = true;
}
