namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenHyperactive : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.Hyperactive;
    public override string ModifierName => "Token Hyperactive";
    public override string Notification => "You're hyperactive!";
    public override bool Negative => true;
    public override bool RemoveOnDeath => true;
}
