using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Neutral;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Neutral;

public sealed class AbyssOptions : AbstractOptionGroup<AbyssRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleAbyss", "Abyss");

    [ModdedNumberOption("MiraOptionAbyssGulpCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GulpCooldown { get; set; } = 25f;

    [ModdedToggleOption("MiraOptionAbyssImpostorVision")]
    public bool ImpostorVision { get; set; } = true;

    [ModdedToggleOption("MiraOptionAbyssCanVent")]
    public bool CanVent { get; set; }
}
