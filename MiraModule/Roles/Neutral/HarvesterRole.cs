using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraModule.Assets;
using MiraModule.Buttons.Neutral;
using MiraModule.Modifiers.Neutral;
using MiraModule.Options.Roles.Neutral;
using Reactor.Utilities;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace MiraModule.Roles.Neutral;

public sealed class HarvesterRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string LocaleKey => "Harvester";
    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"MiraRole{LocaleKey}WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>();
        }
    }

    public Color RoleColor => MiraModuleColors.Harvester;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<HarvesterOptions>.Instance.CanVent,
        IntroSound = TouAudio.GlitchSound,
        Icon = RoleIcons.Harvester,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => OptionGroupSingleton<HarvesterOptions>.Instance.ImpostorVision;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MiraModuleColors.Harvester);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public void OffsetButtons()
    {
        var canVent = OptionGroupSingleton<HarvesterOptions>.Instance.CanVent ||
                      LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.OffsetButtonsToggle.Value;
        var kill = CustomButtonSingleton<HarvesterKillButton>.Instance;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(kill, !canVent));
    }

    public static void TryHarvestRole(PlayerControl harvesterPlayer, PlayerControl victim, ushort? roleIdOverride = null)
    {
        if (harvesterPlayer == null || victim == null || victim.Data == null || victim.Data.Role == null)
        {
            return;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var roleId = roleIdOverride ?? (ushort)victim.Data.Role.Role;
        var role = roleIdOverride.HasValue
            ? RoleManager.Instance.GetRole((RoleTypes)roleId)
            : victim.Data.Role;

        if (role == null || !IsTownOfUsRole(role) || role is HarvesterRole)
        {
            return;
        }

        if (harvesterPlayer.TryGetModifier<HarvesterCacheModifier>(out _))
        {
            harvesterPlayer.RpcRemoveModifier<HarvesterCacheModifier>();
        }
        harvesterPlayer.RpcAddModifier<HarvesterCacheModifier>(roleId);
    }

    public static void ClearHarvestedRole(PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        if (player.TryGetModifier<HarvesterCacheModifier>(out _))
        {
            player.RpcRemoveModifier<HarvesterCacheModifier>();
        }
    }

    public bool TryConsumeHarvestedRole()
    {
        if (Player == null || !Player.HasModifier<HarvesterCacheModifier>())
        {
            return false;
        }

        ClearHarvestedRole(Player);
        return true;
    }

    public bool WinConditionMet()
    {
        var harvesterCount = CustomRoleUtils.GetActiveRolesOfType<HarvesterRole>().Count(x => !x.Player.HasDied());
        if (MiscUtils.KillersAliveCount > harvesterCount)
        {
            return false;
        }

        return harvesterCount >= Helpers.GetAlivePlayers().Count - harvesterCount;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    internal static bool IsTownOfUsRole(RoleBehaviour role)
    {
        var ns = role.GetType().Namespace;
        return ns != null && ns.StartsWith("TownOfUs.Roles", StringComparison.Ordinal);
    }
}
