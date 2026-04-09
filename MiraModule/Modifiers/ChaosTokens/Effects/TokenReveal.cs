using AmongUs.GameOptions;

namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenReveal(RoleTypes role, byte revealerId) : TokenEffect<RevealModifier>
{
    public override ChaosEffects Effect => ChaosEffects.RevealSelf;
    public override string ModifierName => "Token Reveal";
    public override string Notification => "Your role is revealed to someone!";
    public override bool Negative => true;
    public override bool RemoveAfterMeeting => true;

    public override bool LinkToAditional => true;

    public RoleTypes RevealedRole { get; } = role;
    public byte RevealerId { get; } = revealerId;

    public override void OnActivate()
    {
        Args = [RevealedRole, RevealerId];
        base.OnActivate();
    }
}
