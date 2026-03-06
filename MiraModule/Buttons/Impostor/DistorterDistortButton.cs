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

public sealed class DistorterDistortButton : TownOfUsRoleButton<DistorterRole>
{
    public override string Name => TouLocale.GetParsed("MiraRoleDistorterDistort", "Distort");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Distorter;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<DistorterOptions>.Instance.DistortCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => ImpostorAssets.DistorterDistortSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(
            MiscUtils.CoMoveButtonIndex(this, !OptionGroupSingleton<DistorterOptions>.Instance.CanVent));
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null)
        {
            return false;
        }

        return !Role.IsAbilityBusy && !Role.PostInvisibilityActive && !Minigame.Instance;
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

        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        playerMenu.Begin(
            plr => Role.CanStartDistortOn(plr),
            plr =>
            {
                playerMenu.ForceClose();
                if (plr == null)
                {
                    return;
                }

                DistorterRole.RpcStartDistort(PlayerControl.LocalPlayer, plr.PlayerId);
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



