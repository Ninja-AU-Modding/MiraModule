namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenRandomModifier : TokenEffect<RandomModifier>
{
    public override ChaosEffects Effect => ChaosEffects.RandomModifier;
    public override string ModifierName => "Token Random Modifier";
    public override string Notification => "You got a random modifier!";
    public override bool Negative => false;

    public override bool LinkToAditional => true;
}
