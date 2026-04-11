using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Crewmates;
using MiraModule.Roles.Crewmate;
using Reactor.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Buttons.Crewmates;

public sealed class InspectorGeneralConvertButton : TownOfUsKillRoleButton<InspectorGeneralRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("MiraRoleInspectorGeneralConvert", "Convert");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraModuleColors.InspectorGeneral;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<InspectorGeneralOptions>.Instance.ConvertCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => CrewAssets.InspectorGeneralConvertSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, true));
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role != null && Role.CanConvert;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false,
            p => p != null &&
                 p != PlayerControl.LocalPlayer &&
                 !p.IsRole<InspectorGeneralRole>() &&
                 !p.IsRole<CommandSpecialistRole>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Inspector General Convert: Target is null");
            return;
        }

        InspectorGeneralRole.RpcConvert(PlayerControl.LocalPlayer, Target.PlayerId);
    }
}
