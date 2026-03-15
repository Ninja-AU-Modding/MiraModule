using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Roles.Neutral;
using TownOfUs.Buttons;
using TownOfUs.Options.Modifiers.Alliance;
using UnityEngine;

namespace MiraModule.Buttons.Neutral;

public sealed class HarvesterAbilityButton : TownOfUsKillRoleButton<HarvesterRole, PlayerControl>
{
    public override string Name => Role?.CurrentAbilityName ?? TouLocale.GetParsed("MiraRoleHarvesterHarvest", "Harvest");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Harvester;
    public override float Cooldown => 0f;
    public override LoadableAsset<Sprite> Sprite => Role?.CurrentAbilityIcon ?? NeutAssets.HarvesterHarvestSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, false));
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null)
        {
            return false;
        }

        return Role.HasStolenAbility && !Minigame.Instance;
    }

    public override PlayerControl? GetTarget()
    {
        if (Role == null || !Role.HasStolenAbility)
        {
            return null;
        }

        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false,
                x => !x.IsLover() && x.PlayerId != PlayerControl.LocalPlayer.PlayerId);
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false,
            x => x.PlayerId != PlayerControl.LocalPlayer.PlayerId);
    }

    protected override void OnClick()
    {
        if (Role == null)
        {
            return;
        }

        var target = Target;
        Role.UseStolenAbility(target);
    }
}
