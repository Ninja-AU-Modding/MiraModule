namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenNoSkip : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.NoSkip;
    public override string ModifierName => "Token No Skip";
    public override string Notification => "You cannot skip!";
    public override bool Negative => true;
    public override bool RemoveAfterMeeting => true;
}
