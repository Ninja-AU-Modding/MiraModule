using MiraAPI.Modifiers;

namespace MiraOverloaded.Modifiers.ChaosTokens;

/// <summary>
/// Simplified Blackmailed modifier for TokenBlackmail effect
/// </summary>
public class BlackmailedModifier(byte blackmailerId) : BaseModifier
{
    public override string ModifierName => "Blackmailed";
    public override bool HideOnUi => true;

    public byte BlackmailerId { get; } = blackmailerId;
}
