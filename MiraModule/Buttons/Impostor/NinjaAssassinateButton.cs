using System;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Impostor;
using MiraModule.Roles.Impostor;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Buttons.Impostor;

public sealed class NinjaAssassinateButton : TownOfUsRoleButton<NinjaRole>
{
    public override string Name => string.Empty;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Ninja;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<NinjaOptions>.Instance.AssassinateCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => ImpostorAssets.NinjaAssassinateSprite;

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

        return Role.CanAssassinate();
    }

    protected override void OnClick()
    {
        if (Role == null)
        {
            return;
        }

        if (Role.TryAssassinate())
        {
            SetTimer(Cooldown);
        }
    }
}
