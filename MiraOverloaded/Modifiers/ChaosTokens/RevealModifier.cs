using AmongUs.GameOptions;
using MiraAPI.Modifiers;

namespace MiraOverloaded.Modifiers.ChaosTokens;

/// <summary>
/// Simplified Reveal modifier for TokenReveal effect
/// </summary>
public class RevealModifier(RoleTypes revealedRole, byte revealerId) : BaseModifier
{
    public override string ModifierName => "Revealed";
    public override bool HideOnUi => true;

    public RoleTypes RevealedRole { get; } = revealedRole;
    public byte RevealerId { get; } = revealerId;
    public bool Visible { get; set; } = true;
    public bool RevealRole { get; set; } = true;
}
