using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Freeplay;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraOverloaded.Assets;
using MiraOverloaded.Buttons.Impostor;
using MiraOverloaded.Options.Roles.Impostor;
using Reactor.Networking.Attributes;
using TownOfUs;
using UnityEngine;

namespace MiraOverloaded.Roles.Impostor;

public sealed class RevenantRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public float GhostKillsRemaining { get; set; } = 0f;
    [HideFromIl2Cpp] public bool HasFakedDeath { get; set; } = false;

    public string LocaleKey => "Revenant";
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
        new(TouLocale.GetParsed($"MiraRole{LocaleKey}FakeDeath", "Fake Death"),
            TouLocale.GetParsed($"MiraRole{LocaleKey}FakeDeathWikiDescription"),
            ImpostorAssets.RevenantFakeDeathSprite),
    ];

    public Color RoleColor => new Color32(100, 0, 0, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Revenant,
        GhostRole = RoleTypes.ImpostorGhost,
        UseVanillaKillButton = true,
        CanUseSabotage = true,
        CanUseVent = OptionGroupSingleton<RevenantOptions>.Instance.CanVent,
        FreeplayFolder = TaskAdderPatches.ImpostorName,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        GhostKillsRemaining = OptionGroupSingleton<RevenantOptions>.Instance.GhostKills;
        HasFakedDeath = false;

        if (Player.AmOwner)
        {
            Reactor.Utilities.Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<RevenantFakeDeathButton>.Instance,
                !OptionGroupSingleton<RevenantOptions>.Instance.CanVent));
        }
    }

    public bool TryFakeDeath()
    {
        if (HasFakedDeath || Player.Data.IsDead) return false;
        RpcFakeDeath(Player.PlayerId);
        return true;
    }

    public bool TryGhostKill(PlayerControl target)
    {
        if (!Player.Data.IsDead || GhostKillsRemaining <= 0 || target == null || target.Data.IsDead || target.Data.Role.IsImpostor) return false;
        RpcGhostKill(Player.PlayerId, target.PlayerId);
        return true;
    }

    [MethodRpc((uint)MiraOverloadedRpc.RevenantFakeDeath)]
    public static void RpcFakeDeath(byte playerId)
    {
        var player = MiscUtils.PlayerById(playerId);
        if (player == null) return;

        var role = player.GetRole<RevenantRole>();
        if (role != null) role.HasFakedDeath = true;

        if (AmongUsClient.Instance.AmHost)
        {
            player.RpcMurderPlayer(player, true);
        }
    }

    [MethodRpc((uint)MiraOverloadedRpc.RevenantGhostKill)]
    public static void RpcGhostKill(byte sourceId, byte targetId)
    {
        var source = MiscUtils.PlayerById(sourceId);
        var target = MiscUtils.PlayerById(targetId);
        if (source == null || target == null) return;

        var role = source.GetRole<RevenantRole>();
        if (role != null)
        {
            role.GhostKillsRemaining--;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            source.RpcMurderPlayer(target, true);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        HasFakedDeath = false;
        GhostKillsRemaining = 0;
    }
}
