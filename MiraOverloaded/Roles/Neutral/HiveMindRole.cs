using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraModule.Assets;
using MiraModule.Buttons.Neutral;
using MiraModule.Options.Roles.Neutral;
using Reactor.Utilities;
using System.Globalization;
using System.Text;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace MiraModule.Roles.Neutral;

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
    public Color RoleColor => MiraModuleColors.HiveMind;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<HiveMindOptions>.Instance.CanVent,
        IntroSound = MiraModuleAudio.HiveMind,
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
        var kill = CustomButtonSingleton<SheriffShootButton>.Instance;
        var canVent = OptionGroupSingleton<HiveMindOptions>.Instance.CanVent || LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.OffsetButtonsToggle.Value;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(kill, !canVent));
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.HiveMind);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
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