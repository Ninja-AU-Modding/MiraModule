using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
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
using System.Linq;
using System.Text;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace MiraOverloaded.Roles.Neutral;

public sealed class HiveMindRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string LocaleKey => "HiveMind";
    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => MiraOverloadedLocale.GetString($"MiraRole{LocaleKey}TabDescription");
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
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Kill", "Kill"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}KillWikiDescription"),
                    NeutAssets.HiveMindAwakenSprite),
                new(TouLocale.GetParsed($"MiraRole{LocaleKey}Awaken", "Awaken"),
                    TouLocale.GetParsed($"MiraRole{LocaleKey}AwakenWikiDescription"),
                    NeutAssets.HiveMindAwakenSprite),
            };
        }
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
        if (Player.HasDied()) return false;
        var alivePlayers = Helpers.GetAlivePlayers();
        var aliveHive = alivePlayers.Count(p => p?.Data?.Role is HiveMindRole);
        if (aliveHive == 0) return false;
        var killersAlive = MiscUtils.KillersAliveCount;
        return alivePlayers.Count <= killersAlive && killersAlive == aliveHive;
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
