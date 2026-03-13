using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Modifiers.Universal;
using TownOfUs.Buttons;
using TownOfUs.Modules.Localization;
using UnityEngine;

namespace MiraModule.Buttons.Universal;

/// <summary>
/// The dice button for the Chaos Tokens modifier.
/// Visible only when the local player has ChaosTokenModifier and is alive.
/// Uses count mirrors the token count — managed by GainTokens/TrySpendToken.
/// </summary>
public sealed class ChaosTokenButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("MiraModifierChaosTokenRoll", "Roll");
    public override float Cooldown => 5f;
    public override float InitialCooldown => 0f;
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override LoadableAsset<Sprite> Sprite => ModifierIcons.ChaosToken;
    public override Color TextOutlineColor => ChaosTokenModifier.ModColor;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    // ZeroIsInfinite = false: 0 shows as "0" (disabled), not infinite.
    public override bool ZeroIsInfinite { get; set; } = false;
    // MaxUses = -1 with ZeroIsInfinite=false = no hard cap; SetUses() drives the counter display.
    public override int MaxUses => -1;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               !PlayerControl.LocalPlayer.Data.IsDead &&
               PlayerControl.LocalPlayer.HasModifier<ChaosTokenModifier>();
    }

    public override bool CanUse()
    {
        if (!base.CanUse()) return false;
        if (!PlayerControl.LocalPlayer.TryGetModifier<ChaosTokenModifier>(out var mod)) return false;
        return mod.Tokens > 0;
    }

    public override bool CanClick() => CanUse() && Timer <= 0f;

    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.TryGetModifier<ChaosTokenModifier>(out var mod)) return;
        if (!mod.TrySpendToken()) return;
        mod.RollAndApply();
    }

    // Override so we don't decrement UsesLeft through TownOfUsButton's ClickHandler
    // (tokens are managed by TrySpendToken instead).
    public override void ClickHandler()
    {
        if (!CanClick()) return;
        OnClick();
        Timer = Cooldown; // Start 5s cooldown
    }
}
