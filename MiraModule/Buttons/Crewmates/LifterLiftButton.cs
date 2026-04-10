using System;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Crewmates;
using MiraModule.Roles.Crewmate;
using TownOfUs.Buttons;
using UnityEngine;

namespace MiraModule.Buttons.Crewmates;

public sealed class LifterLiftButton : TownOfUsRoleButton<LifterRole>
{
    private const float LiftRange = 1.8f;
    public override string Name => TouLocale.GetParsed("MiraRoleLifterLift", "Lift");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Lifter;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<LifterOptions>.Instance.LiftCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => CrewAssets.LifterLiftSprite;

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null)
        {
            return false;
        }

        if (Minigame.Instance)
        {
            return false;
        }

        var body = FindClosestDeadBody(PlayerControl.LocalPlayer, LiftRange);
        return body != null;
    }

    protected override void OnClick()
    {
        var local = PlayerControl.LocalPlayer;
        var body = FindClosestDeadBody(local, LiftRange);
        if (body == null)
        {
            return;
        }

        LifterRole.RpcLiftVote(local, body.ParentId);
        SetTimer(Cooldown);
    }

    private static DeadBody? FindClosestDeadBody(PlayerControl player, float maxDistance)
    {
        if (player == null) return null;

        var bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        DeadBody? closest = null;
        var best = maxDistance;
        var pos = player.GetTruePosition();

        foreach (var body in bodies)
        {
            if (body == null) continue;
            if (!LifterRole.CanLiftBody(player.PlayerId, body.ParentId)) continue;
            var dist = Vector2.Distance(pos, body.transform.position);
            if (dist <= best)
            {
                best = dist;
                closest = body;
            }
        }

        return closest;
    }
}
