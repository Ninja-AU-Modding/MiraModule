using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Crewmates;
using MiraModule.Roles.Crewmate;
using TownOfUs.Buttons;
using TownOfUs.Modules.Localization;
using UnityEngine;

namespace MiraModule.Buttons.Crewmates;

public sealed class PhasewalkerPhaseButton : TownOfUsRoleButton<PhasewalkerRole>
{
    public override string Name => TouLocale.GetParsed("MiraRolePhasewalkerPhase", "Phase");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Phasewalker;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<PhasewalkerOptions>.Instance.PhaseCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration =>
        OptionGroupSingleton<PhasewalkerOptions>.Instance.PhaseDuration;

    public override LoadableAsset<Sprite> Sprite => RoleIcons.Phasewalker;

    protected override void OnClick()
    {
        if (Role == null) return;

        if (EffectActive)
        {
            // Manual early deactivation
            Role.EndPhase();
        }
        else
        {
            Role.BeginPhase();
        }
    }

    public override void OnEffectEnd()
    {
        Role?.EndPhase();
    }

    public override bool CanUse()
    {
        if (MeetingHud.Instance) return false;
        if (HudManager.Instance.Chat.IsOpenOrOpening) return false;

        return (Timer <= 0f && !EffectActive) || EffectActive;
    }
}
