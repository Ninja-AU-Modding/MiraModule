using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Freeplay;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraModule.Assets;
using MiraModule.Buttons.Crewmates;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Roles.Crewmate;

public sealed class CommandSpecialistRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    private static readonly Dictionary<byte, RoleTypes> OriginalRoleById = new();

    public string LocaleKey => "CommandSpecialist";
    public string RoleName => TouLocale.Get($"MiraRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"MiraRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"MiraRole{LocaleKey}WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(
            TouLocale.GetParsed($"MiraRole{LocaleKey}Kill", "Kill"),
            TouLocale.GetParsed($"MiraRole{LocaleKey}KillWikiDescription"),
            CrewAssets.CommandSpecialistKillSprite),
    ];

    public Color RoleColor => MiraModuleColors.CommandSpecialist;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.CommandSpecialist,
        GhostRole = RoleTypes.CrewmateGhost,
        FreeplayFolder = TaskAdderPatches.CrewmateName,
        HideSettings = true,
        RoleHintType = RoleHintType.None
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(
                CustomButtonSingleton<CommandSpecialistKillButton>.Instance,
                true));
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        OriginalRoleById.Remove(targetPlayer.PlayerId);
    }

    public static void ClearAllState()
    {
        OriginalRoleById.Clear();
    }

    public static void RevertAllToOriginal()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var snapshot = OriginalRoleById.ToList();
        OriginalRoleById.Clear();

        foreach (var (playerId, roleType) in snapshot)
        {
            var player = MiscUtils.PlayerById(playerId);
            if (player == null || player.Data == null || player.Data.Role == null)
            {
                continue;
            }

            if (player.Data.Role.Role != (RoleTypes)RoleId.Get<CommandSpecialistRole>())
            {
                continue;
            }

            player.RpcSetRole(roleType, true);
        }
    }

    public static void RecordOriginalRole(PlayerControl player, RoleTypes roleType)
    {
        if (player == null)
        {
            return;
        }

        OriginalRoleById[player.PlayerId] = roleType;
    }

    private static void RevertToOriginal(PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        if (!OriginalRoleById.TryGetValue(player.PlayerId, out var roleType))
        {
            return;
        }

        OriginalRoleById.Remove(player.PlayerId);

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            player.RpcSetRole(roleType, true);
        }
    }

    private static bool IsNeutralOrImpostor(PlayerControl target)
    {
        return target.IsImpostorAligned() || target.IsNeutral();
    }

    [MethodRpc((uint)MiraModuleRpc.CommandSpecialistStrike)]
    public static void RpcStrike(PlayerControl source, byte targetId)
    {
        if (source.Data.Role is not CommandSpecialistRole)
        {
            return;
        }

        if (source.HasDied())
        {
            return;
        }

        var target = MiscUtils.PlayerById(targetId);
        if (target == null || target.HasDied())
        {
            return;
        }

        if (target.HasModifier<FirstDeadShield>() || target.HasModifier<BaseShieldModifier>())
        {
            return;
        }

        var isEvil = IsNeutralOrImpostor(target);

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            if (isEvil)
            {
                source.RpcCustomMurder(target, MeetingCheck.OutsideMeeting);
            }
            else
            {
                source.RpcCustomMurder(source, MeetingCheck.OutsideMeeting);
            }
        }

        if (isEvil)
        {
            RevertToOriginal(source);
        }
    }
}
