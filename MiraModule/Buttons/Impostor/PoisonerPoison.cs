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

public sealed class PoisonerPoison : TownOfUsKillRoleButton<PoisonerRole, PlayerControl>
{
    public override string Name => string.Empty;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => new Color32(0, 50, 0, 255);
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<PoisonerOptions>.Instance.KillCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => ImpostorAssets.NinjaAssassinateSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(
            MiscUtils.CoMoveButtonIndex(this, !OptionGroupSingleton<PoisonerOptions>.Instance.CanVent));
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null)
        {
            return false;
        }

        return !Role.Player.Data.IsDead && !Role.Player.inVent;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false);
    }

    protected override void OnClick()
    {
        if (Role == null || Target == null)
        {
            return;
        }

        if (Role.TryPoison(Target))
        {
            SetTimer(Cooldown);
        }
    }
}
