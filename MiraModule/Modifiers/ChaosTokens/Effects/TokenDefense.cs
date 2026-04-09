namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenDefense(float duration) : TokenEffect<ProtectionModifier>
{
    public override ChaosEffects Effect => ChaosEffects.Defense;
    public override string ModifierName => "Token Defense";
    public override string Notification => "You have temporary protection";
    public override bool Negative => false;
    public override bool RemoveAfterMeeting => false;

    public override bool LinkToAditional => true;

    public override void OnActivate()
    {
        Args = [duration];
        base.OnActivate();
    }
}
