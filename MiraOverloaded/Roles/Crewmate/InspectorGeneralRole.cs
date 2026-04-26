using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Freeplay;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraOverloaded.Assets;
using MiraOverloaded.Buttons.Crewmates;
using MiraOverloaded.Options.Roles.Crewmates;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraOverloaded.Roles.Crewmate;

public sealed class InspectorGeneralRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    private static readonly Dictionary<byte, int> ConvertsRemainingById = new();
    private static readonly HashSet<byte> ConversionsLocked = new();

    [HideFromIl2Cpp] public int ConvertsRemaining => GetConvertsRemaining(Player?.PlayerId ?? byte.MaxValue);
    [HideFromIl2Cpp] public bool CanConvert => !IsConversionLocked(Player?.PlayerId ?? byte.MaxValue) && ConvertsRemaining > 0;

    public string LocaleKey => "InspectorGeneral";
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
            TouLocale.GetParsed($"MiraRole{LocaleKey}Convert", "Convert"),
            TouLocale.GetParsed($"MiraRole{LocaleKey}ConvertWikiDescription"),
            CrewAssets.InspectorGeneralConvertSprite),
        new(
            TouLocale.GetParsed($"MiraRole{LocaleKey}Kill", "Kill"),
            TouLocale.GetParsed($"MiraRole{LocaleKey}KillWikiDescription"),
            CrewAssets.InspectorGeneralKillSprite),
    ];

    public Color RoleColor => MiraOverloadedColors.InspectorGeneral;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.InspectorGeneral,
        GhostRole = RoleTypes.CrewmateGhost,
        FreeplayFolder = TaskAdderPatches.CrewmateName,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ConvertsRemainingById[player.PlayerId] =
            Math.Clamp((int)OptionGroupSingleton<InspectorGeneralOptions>.Instance.MaxConversions, 0, 99);
        ConversionsLocked.Remove(player.PlayerId);

        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(
                CustomButtonSingleton<InspectorGeneralConvertButton>.Instance,
                true));
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(
                CustomButtonSingleton<InspectorGeneralKillButton>.Instance,
                false));
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ConvertsRemainingById.Remove(targetPlayer.PlayerId);
        ConversionsLocked.Remove(targetPlayer.PlayerId);
    }

    public static void ClearAllState()
    {
        ConvertsRemainingById.Clear();
        ConversionsLocked.Clear();
    }

    private static int GetConvertsRemaining(byte playerId)
    {
        return ConvertsRemainingById.TryGetValue(playerId, out var value) ? value : 0;
    }

    private static bool IsConversionLocked(byte playerId)
    {
        return ConversionsLocked.Contains(playerId);
    }

    private static void LockConversions(byte playerId)
    {
        ConversionsLocked.Add(playerId);
    }

    private static bool TryConsumeConvert(byte playerId)
    {
        if (IsConversionLocked(playerId))
        {
            return false;
        }

        var remaining = GetConvertsRemaining(playerId);
        if (remaining <= 0)
        {
            return false;
        }

        ConvertsRemainingById[playerId] = remaining - 1;
        return true;
    }

    private static bool IsNeutralOrImpostor(PlayerControl target)
    {
        return target.IsImpostorAligned() || target.IsNeutral();
    }

    [MethodRpc((uint)MiraOverloadedRpc.InspectorGeneralConvert)]
    public static void RpcConvert(PlayerControl source, byte targetId)
    {
        if (source.Data.Role is not InspectorGeneralRole)
        {
            return;
        }

        if (source.HasDied())
        {
            return;
        }

        if (IsConversionLocked(source.PlayerId))
        {
            return;
        }

        var target = MiscUtils.PlayerById(targetId);
        if (target == null || target.HasDied())
        {
            return;
        }

        if (target.IsRole<InspectorGeneralRole>() || target.IsRole<CommandSpecialistRole>())
        {
            return;
        }

        if (IsNeutralOrImpostor(target))
        {
            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            {
                source.RpcCustomMurder(target, MeetingCheck.OutsideMeeting);
            }

            return;
        }

        if (!TryConsumeConvert(source.PlayerId))
        {
            return;
        }

        CommandSpecialistRole.RecordOriginalRole(target, target.Data.Role.Role);

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            target.RpcSetRole((RoleTypes)RoleId.Get<CommandSpecialistRole>());
        }
    }

    [MethodRpc((uint)MiraOverloadedRpc.InspectorGeneralStrike)]
    public static void RpcStrike(PlayerControl source, byte targetId)
    {
        if (source.Data.Role is not InspectorGeneralRole)
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
            LockConversions(source.PlayerId);
        }
        else
        {
            CommandSpecialistRole.RevertAllToOriginal();
        }
    }
}
