using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using MiraOverloaded.Assets;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Roles.Neutral;
using TownOfUs.Buttons;
using TownOfUs.Options.Modifiers.Alliance;
using UnityEngine;
using TownOfUs.Networking;

namespace MiraOverloaded.Buttons.Neutral;

public sealed class HiveMindKillButton : TownOfUsKillRoleButton<HiveMindRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => MiraOverloadedColors.HiveMind;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<HiveMindOptions>.Instance.KillCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => NeutAssets.HiveMindAwakenSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(
            MiscUtils.CoMoveButtonIndex(this, !OptionGroupSingleton<HiveMindOptions>.Instance.CanVent));
    }

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
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
            Error("Hive Mind Kill: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcSpecialMurder(Target, causeOfDeath: "HiveMind");
    }
}
