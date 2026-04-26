using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraOverloaded.Options.Roles.Impostor;
using MiraOverloaded.Roles.Impostor;
using UnityEngine;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class PoisonerPatches
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    public static void MeetingStartPatch()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

        var action = OptionGroupSingleton<PoisonerOptions>.Instance.MeetingAction;
        var poisonedPlayers = new List<byte>(PoisonerRole.PoisonedPlayers.Keys);

        if (action == PoisonerMeetingAction.SavesVictims)
        {
            PoisonerRole.PoisonedPlayers.Clear();
        }
        else if (action == PoisonerMeetingAction.KillsAllVictims)
        {
            foreach (var id in poisonedPlayers)
            {
                var p = MiscUtils.PlayerById(id);
                p?.RpcMurderPlayer(p, true);
            }
            PoisonerRole.PoisonedPlayers.Clear();
        }
        else if (action == PoisonerMeetingAction.KillsFirstVictim)
        {
            if (poisonedPlayers.Count > 0)
            {
                var id = poisonedPlayers[0];
                var p = MiscUtils.PlayerById(id);
                p?.RpcMurderPlayer(p, true);
            }
            PoisonerRole.PoisonedPlayers.Clear();
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudUpdatePatch()
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;
        
        bool canSeePoison = PlayerControl.LocalPlayer.Data.Role.IsImpostor;
        if (!canSeePoison) return;

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.Data.IsDead) continue;
            
            if (PoisonerRole.PoisonedPlayers.ContainsKey(p.PlayerId))
            {
                if (p.cosmetics.nameText != null)
                {
                    p.cosmetics.nameText.color = new Color32(0, 100, 0, 255);
                }
            }
        }
        
        if (MeetingHud.Instance != null)
        {
            foreach (var state in MeetingHud.Instance.playerStates)
            {
                if (PoisonerRole.PoisonedPlayers.ContainsKey(state.TargetPlayerId))
                {
                    state.NameText.color = new Color32(0, 100, 0, 255);
                }
            }
        }
    }
}
