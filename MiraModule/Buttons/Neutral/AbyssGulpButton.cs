using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Neutral;
using MiraModule.Roles.Neutral;
using TownOfUs.Buttons;
using TownOfUs.Networking;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Buttons.Neutral;

public sealed class AbyssGulpButton : TownOfUsKillRoleButton<AbyssRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("ExampleRoleAbyssGulp", "Gulp");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Abyss;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<AbyssOptions>.Instance.GulpCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => ExampleNeutAssets.AbyssGulpSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(
            MiscUtils.CoMoveButtonIndex(this, !OptionGroupSingleton<AbyssOptions>.Instance.CanVent));
    }

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Abyss Gulp: Target is null");
            return;
        }

        // Register the target as swallowed before the murder
        AbyssRole.SwallowedPlayers[Target.PlayerId] = PlayerControl.LocalPlayer.PlayerId;

        // Kill with no body created and no teleport, so nothing visible happens on the map
        PlayerControl.LocalPlayer.RpcSpecialMurder(
            Target,
            createDeadBody: false,
            teleportMurderer: false,
            showKillAnim: false,
            causeOfDeath: "Abyss");
    }
}
