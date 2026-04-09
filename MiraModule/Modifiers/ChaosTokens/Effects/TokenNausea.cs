using UnityEngine;
using MiraModule.Utilities;
using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game;

namespace MiraModule.Modifiers.ChaosTokens.Effects;

public class TokenNausea : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.Nausea;
    public override string ModifierName => "Token Nausea";
    public override string Notification => "You are nauseous!";
    public override bool Negative => true;
    public override bool RemoveOnDeath => true;

    private SpriteRenderer? _renderer;

    public override void OnActivate()
    {
        base.OnActivate();
        if (!Player.AmOwner) return;

        _renderer = ChaosTokensUtils.CreatePostprocessFilter(ChaosTokensAssets.NauseaMaterial.LoadAsset());
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
