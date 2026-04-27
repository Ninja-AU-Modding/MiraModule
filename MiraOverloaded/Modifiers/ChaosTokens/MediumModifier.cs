using MiraAPI.Modifiers;

namespace MiraOverloaded.Modifiers.ChaosTokens;

/// <summary>
/// Simplified Medium modifier for TokenMedium effect
/// </summary>
public class MediumModifier : BaseModifier
{
    public override string ModifierName => "Medium";
    public override bool HideOnUi => true;
}
