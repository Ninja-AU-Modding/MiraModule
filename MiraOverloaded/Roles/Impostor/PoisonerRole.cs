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
using Reactor.Utilities;
using TownOfUs;
using UnityEngine;

namespace MiraOverloaded.Roles.Impostor;

public sealed class PoisonerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public static readonly Dictionary<byte, float> PoisonedPlayers = new();


    public string LocaleKey => "Poisoner";
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
        new(TouLocale.GetParsed($"MiraRole{LocaleKey}Poison", "Poison"),
            TouLocale.GetParsed($"MiraRole{LocaleKey}PoisonWikiDescription"),
            ImpostorAssets.NinjaAssassinateSprite),
    ];

    public Color RoleColor => new Color32(0, 50, 0, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Poisoner,
        GhostRole = RoleTypes.ImpostorGhost,
        UseVanillaKillButton = false,
        CanUseSabotage = true,
        CanUseVent = OptionGroupSingleton<PoisonerOptions>.Instance.CanVent,
        FreeplayFolder = TaskAdderPatches.ImpostorName,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            Reactor.Utilities.Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<PoisonerPoison>.Instance,
                !OptionGroupSingleton<PoisonerOptions>.Instance.CanVent));
        }
    }

    public void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;
        var poisonedIds = new List<byte>(PoisonedPlayers.Keys);

        foreach (var id in poisonedIds)
        {
            PoisonedPlayers[id] -= dt;
        }

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
        if (MeetingHud.Instance != null || ExileController.Instance != null) return;

        var toRemove = new List<byte>();
        var toAdd = new Dictionary<byte, float>();

        foreach (var id in poisonedIds)
        {
            var victim = MiscUtils.PlayerById(id);


            if (victim == null || victim.Data.IsDead || victim.Data.Disconnected)
            {
                toRemove.Add(id);
                continue;
            }

            if (PoisonedPlayers[id] <= 0)
            {
                victim.RpcMurderPlayer(victim, true);
                toRemove.Add(id);
                continue;
            }

            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.Data.IsDead || p.Data.Disconnected || p.Data.Role.IsImpostor) continue;
                if (PoisonedPlayers.ContainsKey(p.PlayerId) || toAdd.ContainsKey(p.PlayerId)) continue;

                if (Vector2.Distance(victim.GetTruePosition(), p.GetTruePosition()) < 0.5f)
                {
                    toAdd.Add(p.PlayerId, OptionGroupSingleton<PoisonerOptions>.Instance.PoisonTimer);
                }
            }
        }

        foreach (var id in toRemove) PoisonedPlayers.Remove(id);
        foreach (var kvp in toAdd) RpcPoison(kvp.Key, kvp.Value);
    }

    public bool TryPoison(PlayerControl target)
    {
        if (target == null || target.Data.IsDead || target.Data.Role.IsImpostor) return false;
        RpcPoison(target.PlayerId, OptionGroupSingleton<PoisonerOptions>.Instance.PoisonTimer);
        return true;
    }

    [MethodRpc((uint)MiraOverloadedRpc.PoisonerPoison)]
    public static void RpcPoison(byte targetId, float timer)
    {
        PoisonedPlayers[targetId] = timer;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        PoisonedPlayers.Clear();
    }
}
