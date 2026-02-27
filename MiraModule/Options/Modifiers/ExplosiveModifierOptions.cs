using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraModule.Options.Modifiers;

/// <summary>
/// Spawn-count and chance options for MiraModule's universal modifiers.
/// Shown in the same "Universal Modifiers" section as TOU's entries.
/// </summary>
public sealed class ExplosiveModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Universal Modifiers";
    public override uint GroupPriority => 1;
    public override bool ShowInModifiersMenu => true;

    [ModdedNumberOption("Explosive Amount", 0, 5)]
    public float ExplosiveAmount { get; set; } = 0;

    public ModdedNumberOption ExplosiveChance { get; } =
        new("Explosive Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ExplosiveModifierOptions>.Instance.ExplosiveAmount > 0
        };
}
