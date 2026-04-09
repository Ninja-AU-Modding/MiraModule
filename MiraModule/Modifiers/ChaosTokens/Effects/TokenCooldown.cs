namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenCooldown(float multiplier) : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.LowerCooldown;
    public override string ModifierName => "Token Cooldown";
    public override string Notification => "Your cooldown is reduced!";
    public override bool Negative => false;
    public override bool RemoveAfterMeeting => false;

    public float CooldownMultiplier { get; private set; } = multiplier;
}
