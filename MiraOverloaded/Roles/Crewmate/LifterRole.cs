using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Freeplay;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraOverloaded.Assets;
using MiraOverloaded.Buttons.Crewmates;
using MiraOverloaded.Options.Roles.Crewmates;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace MiraOverloaded.Roles.Crewmate;

public sealed class LifterRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    private static readonly Dictionary<byte, int> ExtraVotesById = new();
    private static readonly Dictionary<byte, HashSet<byte>> LiftedBodiesByLifter = new();

    [HideFromIl2Cpp] public int ExtraVotes => GetExtraVotes(Player?.PlayerId ?? byte.MaxValue);

    public string LocaleKey => "Lifter";
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
            TouLocale.GetParsed($"MiraRole{LocaleKey}Lift", "Lift"),
            TouLocale.GetParsed($"MiraRole{LocaleKey}LiftWikiDescription"),
            CrewAssets.LifterLiftSprite),
    ];

    public Color RoleColor => MiraOverloadedColors.Lifter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Lifter,
        GhostRole = RoleTypes.CrewmateGhost,
        FreeplayFolder = TaskAdderPatches.CrewmateName,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (!ExtraVotesById.ContainsKey(player.PlayerId))
        {
            ExtraVotesById[player.PlayerId] = 0;
        }

        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(
                CustomButtonSingleton<LifterLiftButton>.Instance,
                true));
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ExtraVotesById.Remove(targetPlayer.PlayerId);
    }

    public void AddExtraVotes(int amount)
    {
        if (Player == null || amount <= 0) return;
        var current = GetExtraVotes(Player.PlayerId);
        ExtraVotesById[Player.PlayerId] = Math.Clamp(current + amount, 0, 99);
    }

    public static int GetExtraVotes(byte playerId)
    {
        return ExtraVotesById.TryGetValue(playerId, out var value) ? value : 0;
    }

    public static bool CanLiftBody(byte lifterId, byte bodyParentId)
    {
        if (!LiftedBodiesByLifter.TryGetValue(lifterId, out var set))
        {
            return true;
        }

        return !set.Contains(bodyParentId);
    }

    private static void MarkBodyLifted(byte lifterId, byte bodyParentId)
    {
        if (!LiftedBodiesByLifter.TryGetValue(lifterId, out var set))
        {
            set = new HashSet<byte>();
            LiftedBodiesByLifter[lifterId] = set;
        }

        set.Add(bodyParentId);
    }

    public static void ClearAllVotes()
    {
        ExtraVotesById.Clear();
        LiftedBodiesByLifter.Clear();
    }

    [MethodRpc((uint)MiraOverloadedRpc.LifterLiftVote)]
    public static void RpcLiftVote(PlayerControl source, byte targetId)
    {
        if (source.Data.Role is not LifterRole role) return;
        if (source.HasDied()) return;
        var target = MiscUtils.PlayerById(targetId);
        if (target == null) return;
        if (!CanLiftBody(source.PlayerId, targetId)) return;

        MarkBodyLifted(source.PlayerId, targetId);
        role.AddExtraVotes(1);
    }
}
