using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Neutral;

namespace MiraModule.Options.Roles.Neutral;

public sealed class FakeposterOptions : AbstractOptionGroup<FakeposterRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleFakeposter", "Fake-poster");

    [ModdedNumberOption("MiraOptionFakeposterKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("MiraOptionFakeposterCanVent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("MiraOptionFakeposterHasImposterVision")]
    public bool ImposterVision { get; set; } = true;

    [ModdedToggleOption("MiraOptionFakeposterHasAssasin")]
    public bool HasAssassin { get; set; } = true;
}