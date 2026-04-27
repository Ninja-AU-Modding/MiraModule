namespace MiraOverloaded.Modifiers.ChaosTokens.Effects;

public sealed class TokenVision(float multiplier) : TokenEffect<VisionModifier>
{
    public override ChaosEffects Effect => ChaosEffects.Vision;
    public override string ModifierName => "Token Vision";
    public override string Notification => "You see further!";
    public override bool Negative => false;
    public override bool RemoveOnDeath => true;

    public override bool LinkToAditional => true;

    public override void OnActivate()
    {
        Args = [multiplier];
        base.OnActivate();
    }
}
