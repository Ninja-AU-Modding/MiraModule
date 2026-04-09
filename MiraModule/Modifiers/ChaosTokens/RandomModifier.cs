using MiraAPI.Modifiers;

namespace MiraModule.Modifiers.ChaosTokens;

/// <summary>
/// Simplified Random modifier for TokenRandomModifier effect
/// </summary>
public class RandomModifier : BaseModifier
{
    public override string ModifierName => "Random Modifier";
    public override bool HideOnUi => true;
}
