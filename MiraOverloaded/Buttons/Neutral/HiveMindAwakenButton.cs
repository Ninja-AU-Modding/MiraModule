using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using MiraOverloaded.Assets;
using MiraOverloaded.Events;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Patches;
using MiraOverloaded.Roles.Neutral;
using TownOfUs.Buttons;
using UnityEngine;

namespace MiraOverloaded.Buttons.Neutral;

public sealed class HiveMindAwakenButton : TownOfUsRoleButton<HiveMindRole>
{
    public override string Name => "Awaken";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MiraOverloadedColors.HiveMind;
    public override float Cooldown => 0f;
    public override float InitialCooldown => 0f;
    public override LoadableAsset<Sprite> Sprite => RoleIcons.HiveMind;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    private bool _used;

    public override bool CanUse()
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.HasDied()) return false;
        if (PlayerControl.LocalPlayer.Data.Role is not HiveMindRole) return false;
        return !_used;
    }

    protected override void OnClick()
    {
        if (_used) return;
        _used = true;

        var opts = OptionGroupSingleton<HiveMindOptions>.Instance;
        SoundManager.Instance.PlaySound(MiraOverloadedAudio.HiveMind.LoadAsset(), false, 1f);
        HiveMindEvents.ApplyAwakenDebuffs(PlayerControl.LocalPlayer, opts);

        HiveMindDeathPatch.RpcHiveMindAwaken(PlayerControl.LocalPlayer);
    }
}
