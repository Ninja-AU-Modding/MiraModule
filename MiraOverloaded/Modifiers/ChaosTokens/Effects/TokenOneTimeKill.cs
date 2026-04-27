namespace MiraOverloaded.Modifiers.ChaosTokens.Effects;

public sealed class TokenOneTimeKill : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.KillButton;
    public override string ModifierName => "Token Kill";
    public override string Notification => "You have a one-time kill!";
    public override bool Negative => false;
    public override bool RemoveAfterMeeting => false;
}
