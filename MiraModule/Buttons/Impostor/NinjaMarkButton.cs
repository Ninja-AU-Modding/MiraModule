using System;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Impostor;
using MiraModule.Roles.Impostor;
using TownOfUs.Buttons;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Buttons.Impostor;

public sealed class NinjaMarkButton : TownOfUsRoleButton<NinjaRole>
{
    public override string Name => TouLocale.GetParsed("MiraRoleNinjaMark", "Mark");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Ninja;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<NinjaOptions>.Instance.MarkCooldown + MapCooldown, 0f, 120f);
    public override LoadableAsset<Sprite> Sprite => ImpostorAssets.NinjaMarkSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(
            MiscUtils.CoMoveButtonIndex(this, !OptionGroupSingleton<NinjaOptions>.Instance.CanVent));
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null)
        {
            return false;
        }

        return !Minigame.Instance;
    }

    protected override void OnClick()
    {
        if (Minigame.Instance || Role == null)
        {
            return;
        }

        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        playerMenu.Begin(
            plr => plr != null &&
                   plr.Data != null &&
                   !plr.Data.IsDead &&
                   !plr.Data.Disconnected &&
                   plr.PlayerId != PlayerControl.LocalPlayer.PlayerId,
            plr =>
            {
                playerMenu.ForceClose();
                if (plr == null)
                {
                    return;
                }

                NinjaRole.RpcSetTarget(PlayerControl.LocalPlayer, plr.PlayerId);
                SetTimer(Cooldown);
            }
        );

        foreach (var panel in playerMenu.potentialVictims)
        {
            panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
            if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
            {
                panel.NameText.color = Color.white;
            }
        }
    }
}
