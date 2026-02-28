using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Modifiers.Universal;
using UnityEngine;

namespace MiraModule.Buttons.Universal;

/// <summary>
/// The dice button for the Chaos Tokens modifier.
/// Shows on every player that has ChaosTokenModifier.
/// Uses = remaining token count. No cooldown — spending a token IS the action.
/// </summary>
public sealed class ChaosTokenButton : CustomActionButton
{
    public override string Name  => "Roll";
    public override float Cooldown => 0f;
    public override LoadableAsset<Sprite> Sprite => ModifierIcons.ChaosToken;
    public override Color TextOutlineColor => ChaosTokenModifier.ModColor;

    // Uses display mirrors the token count — set externally via SetUses()
    public override int MaxUses       => ZeroIsInfinite ? 0 : -1;
    public override bool ZeroIsInfinite { get; set; } = false;

    public override bool Enabled(RoleBehaviour? role) =>
        PlayerControl.LocalPlayer != null &&
        !PlayerControl.LocalPlayer.HasDied() &&
        PlayerControl.LocalPlayer.HasModifier<ChaosTokenModifier>();

    public override bool CanUse()
    {
        if (!PlayerControl.LocalPlayer.TryGetModifier<ChaosTokenModifier>(out var mod)) return false;
        return mod.Tokens > 0 && !PlayerControl.LocalPlayer.HasDied();
    }

    public override bool CanClick() => CanUse();

    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.TryGetModifier<ChaosTokenModifier>(out var mod)) return;
        if (!mod.TrySpendToken()) return;
        mod.RollAndApply();
    }

    // Override ClickHandler so we skip the built-in uses decrement
    // (tokens are decremented inside TrySpendToken instead)
    public override void ClickHandler()
    {
        if (!CanClick()) return;
        OnClick();
        // No cooldown, no built-in use decrement — token state owns all of this
        Timer = 0f;
    }
}
