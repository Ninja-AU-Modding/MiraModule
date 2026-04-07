namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenBlackmail(byte blackmailerId) : TokenEffect<BlackmailedModifier>
{
    public override ChaosEffects Effect => ChaosEffects.Blackmailed;
    public override string ModifierName => "Token Blackmail";
    public override bool Negative => true;
    public override bool RemoveAfterMeeting => true;

    public override bool LinkToAditional => true;

    public override void OnActivate()
    {
        Args = [blackmailerId];
        base.OnActivate();
    }
}
