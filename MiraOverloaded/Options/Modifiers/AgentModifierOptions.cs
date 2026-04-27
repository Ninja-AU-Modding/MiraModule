using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraModule.Options.Modifiers;

public sealed class AgentModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Alliance Modifiers";
    public override uint GroupPriority => 1;
    public override bool ShowInModifiersMenu => true;

    [ModdedNumberOption("Agent Amount", 0f, 5f)]
    public float AgentAmount { get; set; } = 0f;

    public ModdedNumberOption AgentChance { get; } =
        new("Agent Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<AgentModifierOptions>.Instance.AgentAmount > 0
        };
}
