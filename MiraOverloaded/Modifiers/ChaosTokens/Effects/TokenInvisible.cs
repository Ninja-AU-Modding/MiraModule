namespace MiraOverloaded.Modifiers.ChaosTokens.Effects;

public sealed class TokenInvisible : TokenEffect<InvisibleModifier>
{
    public override ChaosEffects Effect => ChaosEffects.Invisible;
    public override string ModifierName => "Token Invisible";
    public override string Notification => "You are invisible!";
    public override bool Negative => false;
    public override bool RemoveOnDeath => true;

    public override bool LinkToAditional => true;

    public override void OnActivate()
    {
        Args = [UnityEngine.Random.Range(5f, 15f)];
        base.OnActivate();
    }
}
