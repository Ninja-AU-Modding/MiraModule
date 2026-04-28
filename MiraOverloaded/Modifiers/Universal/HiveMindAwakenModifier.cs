using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraOverloaded.Assets;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Patches;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace MiraOverloaded.Modifiers.Universal;

public class HiveMindAwakenModifier : UniversalGameModifier
{
    public override string ModifierName => "Awakening";
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override LoadableAsset<Sprite>? ModifierIcon => RoleIcons.HiveMind;
    public override Color FreeplayFileColor => Color.red;
    public override int GetAssignmentChance() => 0;
    public override int GetAmountPerGame() => 0;

    public float TimeLeft { get; private set; }
    private bool _isCountingDown;

    public override string GetDescription() => $"<color=#FF4444>Awaken within: {Mathf.CeilToInt(TimeLeft)}s</color>";

    public override void OnActivate()
    {
        TimeLeft = OptionGroupSingleton<HiveMindOptions>.Instance.AwakeningWindow;
        _isCountingDown = true;
    }

    public override void FixedUpdate()
    {
        if (!_isCountingDown || Player == null || !Player.AmOwner || Player.HasDied()) return;

        TimeLeft -= Time.fixedDeltaTime;

        if (TimeLeft <= 0f)
        {
            _isCountingDown = false;
            TimeLeft = 0f;

            Player.Die(DeathReason.Kill, true);

            HiveMindDeathPatch.RpcHiveMindTimeout(Player);

            Player.RpcRemoveModifier<HiveMindAwakenModifier>();
        }
    }
}