using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using MiraModule.Assets;
using MiraModule.Buttons.Neutral;
using MiraModule.Options.Roles.Neutral;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Roles.Neutral;

public sealed class SentinelRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string LocaleKey => "Sentinel";
    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"MiraRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Explode", "Explode"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}ExplodeWikiDescription"),
                    NeutAssets.SentinelExplodeSprite),
            };
        }
    }

    public Color RoleColor => MiraModuleColors.Sentinel;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<SentinelOptions>.Instance.CanVent,
        IntroSound = TouAudio.GlitchSound,
        Icon = RoleIcons.Sentinel,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => OptionGroupSingleton<SentinelOptions>.Instance.ImpostorVision;

    public bool WinConditionMet()
    {
        var glitchCount = CustomRoleUtils.GetActiveRolesOfType<SentinelRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > glitchCount)
        {
            return false;
        }

        return glitchCount >= Helpers.GetAlivePlayers().Count - glitchCount;
    }

    public void OffsetButtons()
    {
        var canVent = OptionGroupSingleton<SentinelOptions>.Instance.CanVent || LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.OffsetButtonsToggle.Value;
        var douse = CustomButtonSingleton<SentinelExplodeButton>.Instance;
        var ignite = CustomButtonSingleton<SentinelKillButton>.Instance;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(douse, !canVent));
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(ignite, !canVent));
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            HudManager.Instance.ImpostorVentButton.graphic.sprite = NeutAssets.SentinelVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Sentinel);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);

            var button = CustomButtonSingleton<SentinelExplodeButton>.Instance;

            if (button.Explode != null)
            {
                button.Explode.Clear();
                button.Explode = null;
            }
        }
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

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
