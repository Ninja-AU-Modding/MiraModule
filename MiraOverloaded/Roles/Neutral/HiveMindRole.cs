using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraOverloaded.Assets;
using MiraOverloaded.Buttons.Neutral;
using MiraOverloaded.Events;
using MiraOverloaded.Options.Roles.Neutral;
using Reactor.Utilities;
using System.Globalization;
using System.Text;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace MiraOverloaded.Roles.Neutral;

public sealed class HiveMindRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string LocaleKey => "HiveMind";
    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}TabDescription");
    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"MiraRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public Color RoleColor => MiraOverloadedColors.HiveMind;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<HiveMindOptions>.Instance.CanVent,
        IntroSound = MiraOverloadedAudio.HiveMind,
        Icon = RoleIcons.HiveMind,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => OptionGroupSingleton<HiveMindOptions>.Instance.ImposterVision;

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        var aliveCount = Helpers.GetAlivePlayers().Count;
        var killersAlive = MiscUtils.KillersAliveCount;

        return aliveCount <= killersAlive && killersAlive == 1;
    }

    public void OffsetButtons()
    {
        var kill = CustomButtonSingleton<HiveMindKillButton>.Instance;
        var awaken = CustomButtonSingleton<HiveMindAwakenButton>.Instance;

        var canVent = OptionGroupSingleton<HiveMindOptions>.Instance.CanVent || LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.OffsetButtonsToggle.Value;

        if (kill != null)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(kill, !canVent));
        }

        if (awaken != null)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(awaken, !canVent));
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            HiveMindEvents.ResetTimedDebuffs(Player);
            HiveMindEvents.SetSharedCooldown(Player, Player.killTimer > 0f ? Player.killTimer : OptionGroupSingleton<HiveMindOptions>.Instance.KillCooldown);
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraOverloadedColors.HiveMind);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HiveMindEvents.ResetTimedDebuffs(Player);
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }
}
