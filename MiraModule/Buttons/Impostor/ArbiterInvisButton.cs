using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Roles.Impostor;
using TownOfUs.Buttons;
using TownOfUs.Modules.Localization;
using UnityEngine;

namespace MiraModule.Buttons.Impostor;

public sealed class ArbiterInvisButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("MiraRoleArbiterInvisibility", "Invisibility");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => MiraModuleColors.Arbiter;
    public override float Cooldown => 0f;
    public override float InitialCooldown => 0f;
    public override LoadableAsset<Sprite> Sprite => ImpostorAssets.ArbiterInvisSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ZeroIsInfinite { get; set; } = false;
    public override int MaxUses => -1;

    public override bool Enabled(RoleBehaviour? role)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data.IsDead)
        {
            return false;
        }

        return PlayerControl.LocalPlayer.Data.Role is ArbiterRole arbiter && arbiter.InvisUses > 0;
    }

    public override bool CanUse()
    {
        if (!base.CanUse())
        {
            return false;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null || Minigame.Instance)
        {
            return false;
        }

        return PlayerControl.LocalPlayer.Data.Role is ArbiterRole;
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not ArbiterRole arbiter)
        {
            return;
        }

        arbiter.TryUseInvisibility();
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
        Timer = Cooldown;
    }
}
