using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraModule.Modifiers.Alliance;
using UnityEngine;

namespace MiraModule.Options.Modifiers.Alliance;
public sealed class AgentOptions : AbstractOptionGroup<AgentModifier>
{
    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<AgentModifierOptions>.Instance.AgentAmount > 0;
    public override string GroupName => "Agent";
    public override uint GroupPriority => 10;
    public override Color GroupColor => new Color32(114, 179, 202, 255);

    [ModdedNumberOption("Number of Associates", 1f, 4f, 1f)]
    public float NumAware { get; set; } = 2f;
}