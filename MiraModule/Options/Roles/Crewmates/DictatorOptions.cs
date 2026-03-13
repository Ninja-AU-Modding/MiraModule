using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Roles.Crewmate;
using TownOfUs.Modules.Localization;

namespace MiraModule.Options.Roles.Crewmates;

public sealed class DictatorOptions : AbstractOptionGroup<DictatorRole>
{
    public override string GroupName => TouLocale.Get("MiraRoleDictator", "Dictator");

    [ModdedNumberOption("MiraOptionDictatorMaxUses", 1f, 10f, 1f)]
    public float MaxUses { get; set; } = 3f;

    [ModdedToggleOption("MiraOptionDictatorSacrificeOnUse")]
    public bool SacrificeOnUse { get; set; } = false;

    [ModdedToggleOption("MiraOptionDictatorSacrificeOnEmpty")]
    public bool SacrificeWhenEmpty { get; set; } = true;

    [ModdedToggleOption("MiraOptionDictatorCanCondemnCrew")]
    public bool CanCondemnCrew { get; set; } = true;
}
