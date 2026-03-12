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

public sealed class NinjaMarkButton : TownOfUsKillRoleButton<NinjaRole, PlayerControl>
{
    public override string Name => string.Empty;
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

        if (Role.MarkedTargetId != byte.MaxValue)
        {
            return false;
        }

        return !Minigame.Instance;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is NinjaRole ninja && ninja.MarkedTargetId == byte.MaxValue;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false);
    }

    protected override void OnClick()
    {
        if (Minigame.Instance || Role == null)
        {
            return;
        }

        if (Target == null)
        {
            return;
        }

        NinjaRole.RpcSetTarget(PlayerControl.LocalPlayer, Target.PlayerId);
        SetTimer(Cooldown);
    }
}
