namespace MiraOverloaded.Modifiers.ChaosTokens.Effects;

public sealed class TokenDrunk : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.Drunk;
    public override string ModifierName => "Token Drunk";
    public override string Notification => "You're drunk!";
    public override bool Negative => true;
    public override bool RemoveOnDeath => true;
}
