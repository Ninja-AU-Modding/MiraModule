using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Neutral;
using MiraModule.Roles.Neutral;
using TownOfUs.Buttons;
using TownOfUs.Modules.Localization;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Buttons.Neutral;

public sealed class ShifterShiftButton : TownOfUsKillRoleButton<ShifterRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("MiraRoleShifterShift", "Shift");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Shifter;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<ShifterOptions>.Instance.ShiftCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => NeutAssets.ShifterShiftSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, false));
    }

    public override PlayerControl? GetTarget()
    {
        var opts = OptionGroupSingleton<ShifterOptions>.Instance;

        bool IsValidTarget(PlayerControl p)
        {
            // Can't re-target someone already pending
            if (ShifterRole.PendingShifts.ContainsValue(p.PlayerId)) return false;
            // Dead players only if option allows
            if (p.Data.IsDead && !opts.CanShiftDeadPlayers) return false;
            return true;
        }

        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false,
                x => !x.IsLover() && IsValidTarget(x));
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, IsValidTarget);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Shifter Shift: Target is null");
            return;
        }

        // Replace any previous target — Shifter can only have one pending shift at a time
        ShifterRole.PendingShifts[PlayerControl.LocalPlayer.PlayerId] = Target.PlayerId;

        Info($"[Shifter] Queued shift: {PlayerControl.LocalPlayer.Data.PlayerName} → {Target.Data.PlayerName}");
    }
}
