using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraOverloaded.Roles.Neutral;

namespace MiraOverloaded.Options.Roles.Neutral;

public sealed class FakePostorOptions : AbstractOptionGroup<FakePostorRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleFakePostor", "Fake-poster");

    [ModdedNumberOption("MiraOptionFakePostorKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedToggleOption("MiraOptionFakePostorCanVent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("MiraOptionFakePostorHasImposterVision")]
    public bool ImposterVision { get; set; } = true;

    [ModdedToggleOption("MiraOptionFakePostorHasAssassin")]
    public bool HasAssassin { get; set; } = true;
}