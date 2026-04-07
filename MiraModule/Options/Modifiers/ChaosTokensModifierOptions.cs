using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraModule.Options.Modifiers;

/// <summary>
/// Spawn-count and chance options for Chaos Tokens in Universal Modifiers section.
/// </summary>
public sealed class ChaosTokensModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Universal Modifiers";
    public override uint GroupPriority => 1;
    public override bool ShowInModifiersMenu => true;

    [ModdedNumberOption("Chaos Token Amount", 0, 15)]
    public float ChaosTokenAmount { get; set; } = 0;

    public ModdedNumberOption ChaosTokenChance { get; } =
        new("Chaos Token Chance", 100f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensModifierOptions>.Instance.ChaosTokenAmount > 0
        };
}
