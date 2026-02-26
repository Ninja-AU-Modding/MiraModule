using HarmonyLib;
using MiraModule.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Patches;

/// <summary>
/// Handles Pelican's unique meeting behaviour:
/// 1. When a meeting starts, swallowed players' deaths are committed (list cleared so they show as normally dead).
/// 2. Until the meeting starts their vote area is hidden so they don't appear in the dead list mid-round.
/// </summary>
[HarmonyPatch(typeof(MeetingHud))]
public static class PelicanMeetingPatch
{
    /// <summary>
    /// When the meeting HUD starts, clear the swallowed list.
    /// The players were already killed without a body — clearing here means they will now
    /// show as dead normally in the vote area like any other corpse.
    /// Every client runs this independently on their own local dictionary.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void Start_Postfix()
    {
        PelicanRole.SwallowedPlayers.Clear();
    }

    /// <summary>
    /// Hide swallowed players' vote areas during the round before a meeting is called.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Update))]
    public static void Update_Postfix(MeetingHud __instance)
    {
        if (PelicanRole.SwallowedPlayers.Count == 0 || __instance.playerStates == null)
        {
            return;
        }

        foreach (var voteArea in __instance.playerStates)
        {
            if (!voteArea.gameObject.active)
            {
                continue;
            }

            if (PelicanRole.SwallowedPlayers.ContainsKey(voteArea.TargetPlayerId))
            {
                voteArea.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Re-sort vote areas so hidden swallowed-player slots don't leave gaps.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.SortButtons))]
    public static bool SortButtons_Prefix(MeetingHud __instance)
    {
        if (PelicanRole.SwallowedPlayers.Count == 0)
        {
            return true; // Use default sorting when no one is swallowed
        }

        var array = __instance.playerStates
            .Where(p => !PelicanRole.SwallowedPlayers.ContainsKey(p.TargetPlayerId))
            .OrderBy(p => p.AmDead ? 50 : 0)
            .ThenBy(p => p.TargetPlayerId)
            .ToArray();

        for (var i = 0; i < array.Length; i++)
        {
            var col = i % 3;
            var row = i / 3;
            array[i].transform.localPosition = __instance.VoteOrigin
                + new Vector3(
                    __instance.VoteButtonOffsets.x * col,
                    __instance.VoteButtonOffsets.y * row,
                    -0.9f - row * 0.01f);
        }

        return false; // Skip default sorting
    }
}
