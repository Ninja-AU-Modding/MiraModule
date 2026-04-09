using MiraAPI.Modifiers;

namespace MiraModule.Modifiers.ChaosTokens;

/// <summary>
/// Simplified Medium modifier for TokenMedium effect
/// </summary>
public class MediumModifier : BaseModifier
{
    public override string ModifierName => "Medium";
    public override bool HideOnUi => true;
}
