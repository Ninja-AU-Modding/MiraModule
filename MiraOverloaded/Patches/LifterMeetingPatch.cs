using System.Linq;
using HarmonyLib;
using MiraOverloaded.Roles.Crewmate;
using TMPro;
using UnityEngine;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class LifterMeetingPatch
{
    private const string VotesTextName = "LifterVotesText_TMP";

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    public static void UpdatePatch(MeetingHud __instance)
    {
        if (__instance.playerStates == null) return;
        var local = PlayerControl.LocalPlayer;
        if (local == null) return;

        var area = __instance.playerStates.FirstOrDefault(x => x != null && x.TargetPlayerId == local.PlayerId);
        if (area == null) return;

        var text = EnsureVotesText(area);
        if (text == null) return;

        if (local.Data?.Role is not LifterRole lifter)
        {
            text.enabled = false;
            return;
        }

        var totalVotes = 1 + lifter.ExtraVotes;
        text.text = $"Votes: {totalVotes}";
        text.color = MiraOverloadedColors.Lifter;
        text.enabled = true;
    }

    private static TextMeshPro? EnsureVotesText(PlayerVoteArea area)
    {
        var existing = area.transform.Find(VotesTextName);
        if (existing != null)
        {
            return existing.GetComponent<TextMeshPro>();
        }

        if (area.NameText == null) return null;

        var go = new GameObject(VotesTextName);
        go.transform.SetParent(area.NameText.transform, false);
        go.transform.localPosition = new Vector3(0f, -0.18f, 0f);

        var text = go.AddComponent<TextMeshPro>();
        text.fontSize = 1.6f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = MiraOverloadedColors.Lifter;
        text.enabled = false;
        return text;
    }
}
