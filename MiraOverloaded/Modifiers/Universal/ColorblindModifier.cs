using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraOverloaded.Assets;
using MiraOverloaded.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraOverloaded.Modifiers.Universal;

public sealed class ColorblindModifier(float darkness = 0f) : BaseModifier
{
    public override string ModifierName => "Colorblind";
    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => ModifierIcons.Colorblind;

    private SpriteRenderer? _filterRenderer;
    private SpriteRenderer? _darknessRenderer;
    public float Darkness { get; } = darkness;

    public override void OnActivate()
    {
        if (!Player.AmOwner) return;

        _filterRenderer = ChaosTokensUtils.CreatePostprocessFilter(
            Object.Instantiate(ChaosTokensAssets.ColorblindMaterial.LoadAsset()));

        float darknessAlpha = Mathf.Clamp01(Darkness / 100f);
        if (darknessAlpha > 0f)
        {
            _darknessRenderer = ChaosTokensUtils.CreateScreenOverlay(
                "ColorblindDarknessOverlay",
                new Color(0f, 0f, 0f, darknessAlpha));
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (_filterRenderer != null)
        {
            if (_filterRenderer.material != null)
            {
                Object.Destroy(_filterRenderer.material);
            }

            Object.Destroy(_filterRenderer.gameObject);
            _filterRenderer = null;
        }

        if (_darknessRenderer != null)
        {
            Object.Destroy(_darknessRenderer.gameObject);
            _darknessRenderer = null;
        }
    }
}
