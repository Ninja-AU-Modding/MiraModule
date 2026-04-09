using UnityEngine;
using System.Collections.Generic;
using MiraModule.Utilities;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;

namespace MiraModule.Modifiers.ChaosTokens.Effects;

public class TokenColorblind : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.Colorblind;
    public override string ModifierName => "Token Colorblind";
    public override string Notification => "You are colorblind!";
    public override bool Negative => true;
    public override bool RemoveOnDeath => true;

    private SpriteRenderer? _renderer;

    public override void OnActivate()
    {
        base.OnActivate();
        if (!Player.AmOwner) return;

        _renderer = ChaosTokensUtils.CreatePostprocessFilter(ChaosTokensAssets.ColorblindMaterial.LoadAsset());
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        if (_renderer != null)
        {
            UnityEngine.Object.Destroy(_renderer.gameObject);
        }
    }
}
