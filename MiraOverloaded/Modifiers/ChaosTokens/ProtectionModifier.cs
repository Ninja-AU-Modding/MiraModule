using MiraAPI.Modifiers;
using TownOfUs.Modifiers;

namespace MiraOverloaded.Modifiers.ChaosTokens;

public sealed class ProtectionModifier(float duration) : BaseShieldModifier
{
    public override string ModifierName => "Protection";
    public override bool HideOnUi => true;

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override void OnActivate()
    {
        StartTimer();
        TimeRemaining = duration;
    }
}
