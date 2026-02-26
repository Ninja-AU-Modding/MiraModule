using HarmonyLib;
using MiraAPI.Modifiers;
using MiraModule.Modifiers;
using MiraModule.Roles.Neutral;
using TownOfUs.Networking;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Patches;

[HarmonyPatch(typeof(MeetingHud))]
public static class AbyssMeetingPatch
{
    /// <summary>
    /// When the meeting starts: snap each victim to their Abyss's position,
    /// remove the modifier (restores HUD/camera/movement), then kill them so
    /// the body spawns exactly where the Abyss was standing.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void Start_Postfix()
    {
        if (AbyssRole.SwallowedPlayers.Count == 0) return;
        if (!AmongUsClient.Instance.AmHost) return;

        var toKill = AbyssRole.SwallowedPlayers.Keys.ToList();
        foreach (var victimId in toKill)
        {
            var victim = MiscUtils.PlayerById(victimId);
            if (victim == null) continue;

            // Find the Abyss who swallowed them
            PlayerControl? abyss = null;
            if (AbyssRole.SwallowedPlayers.TryGetValue(victimId, out var abyssId))
                abyss = MiscUtils.PlayerById(abyssId);

            // Snap victim to Abyss position BEFORE kill so body spawns there
            if (abyss != null)
                victim.NetTransform.SnapTo(abyss.GetTruePosition());

            // Remove modifier — restores their camera/HUD/movement/appearance
            if (victim.HasModifier<SwallowedModifier>())
                victim.RpcRemoveModifier<SwallowedModifier>();

            // Kill with body at Abyss's position (victim is already snapped there)
            var killer = abyss ?? victim;
            killer.RpcSpecialMurder(
                victim,
                createDeadBody: true,
                teleportMurderer: false,
                showKillAnim: false,
                causeOfDeath: "Abyss");
        }

        AbyssRole.SwallowedPlayers.Clear();
    }

    /// <summary>
    /// Hide swallowed players' vote areas during the meeting.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Update))]
    public static void Update_Postfix(MeetingHud __instance)
    {
        if (AbyssRole.SwallowedPlayers.Count == 0 || __instance.playerStates == null) return;

        foreach (var voteArea in __instance.playerStates)
        {
            if (!voteArea.gameObject.active) continue;
            if (AbyssRole.SwallowedPlayers.ContainsKey(voteArea.TargetPlayerId))
                voteArea.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Re-sort vote areas so swallowed-player gaps don't appear.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.SortButtons))]
    public static bool SortButtons_Prefix(MeetingHud __instance)
    {
        if (AbyssRole.SwallowedPlayers.Count == 0) return true;

        var array = __instance.playerStates
            .Where(p => !AbyssRole.SwallowedPlayers.ContainsKey(p.TargetPlayerId))
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

        return false;
    }
}
