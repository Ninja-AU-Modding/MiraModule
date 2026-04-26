using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraOverloaded.Modifiers.ChaosTokens;

/// <summary>
/// Simplified Invisible modifier for TokenInvisible effect
/// </summary
public class InvisibleModifier(float duration) : BaseModifier
{
    public override string ModifierName => "Invisible";
    public override bool HideOnUi => true;

    public float TimeRemaining { get; private set; } = duration;

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            Player.cosmetics.currentBodySprite.BodySprite.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            Player.cosmetics.currentBodySprite.BodySprite.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public override void FixedUpdate()
    {
        TimeRemaining -= Time.fixedDeltaTime;
        if (TimeRemaining <= 0)
        {
            Player.RemoveModifier(this);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }
}
