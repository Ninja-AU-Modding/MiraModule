using MiraOverloaded.Modifiers.Universal;

namespace MiraOverloaded.Modifiers.ChaosTokens.Effects;

public sealed class TokenColorblind(float darkness = 0f) : TokenEffect<ColorblindModifier>
{
    public override ChaosEffects Effect => ChaosEffects.Colorblind;
    public override string ModifierName => "Token Colorblind";
    public override string Notification => "You are colorblind!";
    public override bool Negative => true;
    public override bool RemoveOnDeath => true;
    public override bool LinkToAditional => true;

    public override void OnActivate()
    {
        Args = [darkness];
        base.OnActivate();
    }
}
