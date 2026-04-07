using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraModule.Modifiers.ChaosTokens;

/// <summary>
/// Simplified Vision modifier for TokenVision effect
/// </summary>
public class VisionModifier(float multiplier) : BaseModifier
{
    public override string ModifierName => "Enhanced Vision";
    public override bool HideOnUi => true;

    public float Multiplier { get; } = multiplier;
    private float _originalLightMod = -1f;

    public override void OnActivate()
    {
        if (Player.AmOwner && _originalLightMod < 0)
        {
            _originalLightMod = GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
            GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod = _originalLightMod * Multiplier;
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner && _originalLightMod >= 0)
        {
            GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod = _originalLightMod;
            _originalLightMod = -1f;
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }
}
