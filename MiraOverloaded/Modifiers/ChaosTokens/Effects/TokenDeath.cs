namespace MiraOverloaded.Modifiers.ChaosTokens.Effects;

public sealed class TokenDeath : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.Death;
    public override string ModifierName => "Token Death";
    public override string Notification => "You feel unwell...";
    public override bool Negative => true;
    public override bool RemoveAfterMeeting => false;
}
