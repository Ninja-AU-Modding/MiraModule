using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Neutral;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Neutral;

public sealed class PelicanOptions : AbstractOptionGroup<PelicanRole>
{
    public override string GroupName => TouLocale.Get("ExampleRolePelican", "Pelican");

    [ModdedNumberOption("ExampleOptionPelicanGulpCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GulpCooldown { get; set; } = 25f;

    [ModdedToggleOption("ExampleOptionPelicanImpostorVision")]
    public bool ImpostorVision { get; set; } = true;

    [ModdedToggleOption("ExampleOptionPelicanCanVent")]
    public bool CanVent { get; set; }
}
