using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Options.Roles.Neutral;
using MiraModule.Roles.Neutral;
using Reactor.Networking.Attributes;
using TownOfUs.Buttons;
using TownOfUs.Options.Modifiers.Alliance;
using UnityEngine;

namespace MiraModule.Buttons.Neutral;

public sealed class BaiterSpawnButton : TownOfUsRoleButton<BaiterRole>
{
    public override string Name => TouLocale.GetParsed("MiraRoleBaiterSpawn", "Spawn Bait");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => MiraModuleColors.Baiter;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<BaiterOptions>.Instance.SpawnCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => NeutAssets.BaiterSpawnSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Reactor.Utilities.Coroutines.Start(
            MiscUtils.CoMoveButtonIndex(this, !OptionGroupSingleton<BaiterOptions>.Instance.CanVent));
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        var pos = player.GetTruePosition();


        var players = PlayerControl.AllPlayerControls.ToArray();
        var target = players[UnityEngine.Random.Range(0, players.Length)];


        BaiterRole.RpcSpawnBait(player, target.PlayerId, pos.x, pos.y);
    }
}
