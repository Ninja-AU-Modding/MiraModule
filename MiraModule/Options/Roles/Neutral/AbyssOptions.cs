using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Neutral;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Neutral;

public sealed class AbyssOptions : AbstractOptionGroup<AbyssRole>
{
    public override string GroupName => TouLocale.Get("ExampleRoleAbyss", "Abyss");

    [ModdedNumberOption("ExampleOptionAbyssGulpCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GulpCooldown { get; set; } = 25f;

    [ModdedToggleOption("ExampleOptionAbyssImpostorVision")]
    public bool ImpostorVision { get; set; } = true;

    [ModdedToggleOption("ExampleOptionAbyssCanVent")]
    public bool CanVent { get; set; }
}
