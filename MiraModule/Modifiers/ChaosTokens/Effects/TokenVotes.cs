namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenVotes : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.Votes;
    public override string ModifierName => "Token Votes";
    public override string Notification => "You have MULTIPLE votes";
    public override bool Negative => false;
    public override bool RemoveAfterMeeting => true;
}
