using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraOverloaded.Modifiers.ChaosTokens;
using MiraOverloaded.Options.Modifiers;
using MiraOverloaded.Utilities;
using TownOfUs.Buttons;
using UnityEngine;

using MiraOverloaded.Networking;

namespace MiraOverloaded.Buttons;

public class ChaosTokenRollButton : TownOfUsButton
{
    public override string Name => "Roll";
    public override float Cooldown => OptionGroupSingleton<ChaosTokensOptions>.Instance.RollCooldown;
    public override float InitialCooldown => OptionGroupSingleton<ChaosTokensOptions>.Instance.InitialRollCooldown;
    public override LoadableAsset<Sprite> Sprite => ChaosTokensAssets.DiceButton;
    public override Color TextOutlineColor => MiraOverloadedColors.ChaosTokens;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcChaosTokenRoll();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data.IsDead)
        {
            return false;
        }

        if (!PlayerControl.LocalPlayer.TryGetModifier<ChaosTokenModifier>(out var chaosToken))
        {
            return false;
        }

        return chaosToken.Tokens > 0;
    }
}
