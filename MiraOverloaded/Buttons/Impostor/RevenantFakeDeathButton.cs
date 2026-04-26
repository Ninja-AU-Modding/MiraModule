using System;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraOverloaded.Assets;
using MiraOverloaded.Options.Roles.Impostor;
using MiraOverloaded.Roles.Impostor;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraOverloaded.Buttons.Impostor;

public sealed class RevenantFakeDeathButton : TownOfUsRoleButton<RevenantRole>
{
    public override string Name => string.Empty;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => new Color32(100, 0, 0, 255);
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<RevenantOptions>.Instance.FakeDeathCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => ImpostorAssets.RevenantFakeDeathSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(
            MiscUtils.CoMoveButtonIndex(this, !OptionGroupSingleton<RevenantOptions>.Instance.CanVent));
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null) return false;
        return !Role.Player.Data.IsDead && !Role.HasFakedDeath;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is RevenantRole rev && !rev.Player.Data.IsDead && !rev.HasFakedDeath;
    }

    protected override void OnClick()
    {
        if (Role == null) return;
        if (Role.TryFakeDeath())
        {
            SetTimer(Cooldown);
        }
    }
}
