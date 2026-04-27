namespace MiraOverloaded.Modifiers.ChaosTokens.Effects;

public sealed class TokenTransparent(float transparency) : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.Transparent;
    public override string ModifierName => "Token Transparent";
    public override string Notification => "You are now transparent!";
    public override bool Negative => false;
    public override bool RemoveOnDeath => true;

    public float Transparency { get; private set; } = transparency;
}
