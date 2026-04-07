namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenMedium : TokenEffect<MediumModifier>
{
    public override ChaosEffects Effect => ChaosEffects.MediumIsReal;
    public override string ModifierName => "Token Medium";
    public override string Notification => "You can see ghosts!";
    public override bool Negative => false;
    public override bool RemoveOnDeath => true;

    public override bool LinkToAditional => true;
}
