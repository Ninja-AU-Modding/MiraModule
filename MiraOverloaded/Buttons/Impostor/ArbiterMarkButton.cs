using System;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraOverloaded.Assets;
using MiraOverloaded.Options.Roles.Impostor;
using MiraOverloaded.Roles.Impostor;
using TownOfUs.Buttons;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraOverloaded.Buttons.Impostor;

public sealed class ArbiterMarkButton : TownOfUsRoleButton<ArbiterRole>
{
    public override string Name => TouLocale.GetParsed("MiraRoleArbiterMark", "Mark");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraOverloadedColors.Arbiter;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<ArbiterOptions>.Instance.TargetCooldown + MapCooldown, 0f, 120f);
    public override LoadableAsset<Sprite> Sprite => ImpostorAssets.ArbiterMarkSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(
            MiscUtils.CoMoveButtonIndex(this, !OptionGroupSingleton<ArbiterOptions>.Instance.CanVent));
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null)
        {
            return false;
        }

        var opts = OptionGroupSingleton<ArbiterOptions>.Instance;
        if (Role.TargetId != byte.MaxValue && !opts.CanChangeTarget)
        {
            return false;
        }

        return !Minigame.Instance;
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        if (Minigame.Instance || Role == null)
        {
            return;
        }

        var opts = OptionGroupSingleton<ArbiterOptions>.Instance;
        if (Role.TargetId != byte.MaxValue && !opts.CanChangeTarget)
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

                ArbiterRole.RpcSetTarget(PlayerControl.LocalPlayer, plr.PlayerId);
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
