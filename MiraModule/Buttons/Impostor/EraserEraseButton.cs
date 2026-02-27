using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Impostor;
using MiraModule.Roles.Impostor;
using TownOfUs.Buttons;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Buttons.Impostor;

public sealed class EraserEraseButton : TownOfUsKillRoleButton<EraserRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("MiraRoleEraserErase", "Erase");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Eraser;

    public override float Cooldown
    {
        get
        {
            var opts = OptionGroupSingleton<EraserOptions>.Instance;
            var baseCD = opts.EraseCooldown + MapCooldown;
            var increase = opts.CooldownIncrease * EraserRole.ErasedPlayerIds.Count;
            return Math.Clamp(baseCD + increase, 5f, 120f);
        }
    }

    public override LoadableAsset<Sprite> Sprite => ImpostorAssets.EraserEraseSprite;

    public override PlayerControl? GetTarget()
    {
        var opts = OptionGroupSingleton<EraserOptions>.Instance;

        bool IsValidTarget(PlayerControl p)
        {
            // Never erase someone already queued for erase
            if (EraserRole.PendingErases.ContainsKey(p.PlayerId)) return false;

            // By default, don't allow erasing other Impostors unless the option is on
            if (!opts.CanEraseImpostors && p.Data.Role.IsImpostor) return false;

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
            Error("Eraser Erase: Target is null");
            return;
        }

        // Queue the erase — it will resolve at the next meeting start
        EraserRole.PendingErases[Target.PlayerId] = PlayerControl.LocalPlayer.PlayerId;

        // Trigger the cooldown immediately
        SetTimer(Cooldown);

        Info($"Eraser queued erase on {Target.Data.PlayerName} (id={Target.PlayerId})");
    }
}
