using System.Linq;
using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraOverloaded.Assets;
using MiraOverloaded.Roles.Neutral;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class BaiterPatches
{
    [HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
    [HarmonyPostfix]
    public static void MeetingIntroAnimationPatch(MeetingIntroAnimation __instance)
    {
        if (!BaiterRole.BaitReportedIntroFlag) return;
        if (__instance.ProtectedRecently == null) return;


        __instance.ProtectedRecently.SetActive(true);


        var textObj = __instance.ProtectedRecently.transform.FindChild("ProtectedText_TMP");
        if (textObj != null)
        {
            var textTMP = textObj.GetComponent<TMPro.TextMeshPro>();
            if (textTMP != null)
            {
                textTMP.text = "You've reported a Bait Body!";
            }
        }

        var iconObj = __instance.ProtectedRecently.transform.FindChild("UI_ProtectionIcon");
        if (iconObj != null)
        {
            var iconSprite = iconObj.GetComponent<SpriteRenderer>();
            if (iconSprite != null)
            {
                iconSprite.sprite = NeutAssets.BaiterSpawnSprite.LoadAsset();
            }
        }

        if (HudManager.Instance != null)
        {
            var color = ColorUtility.ToHtmlStringRGBA(MiraOverloadedColors.Baiter);
            var title = $"<color=#{color}>Baiter</color>";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, "A Bait Body was reported!", false, true);
        }

        BaiterRole.BaitReportedIntroFlag = false;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        DeadBody? reportedBait = null;
        foreach (var body in BaiterRole.BaitBodies)
        {
            if (body == null) continue;


            if (Vector2.Distance(__instance.GetTruePosition(), body.transform.position) < 1.5f)
            {
                reportedBait = body;
                break;
            }
        }

        if (reportedBait == null) return true;



        BaiterRole.BaitReportedIntroFlag = true;

        BaiterRole.RpcReportBait(__instance, reportedBait.ParentId);

        return false;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance)
    {
        if (!BaiterRole.DiedThisRound.Contains(__instance.PlayerId))
        {
            BaiterRole.DiedThisRound.Add(__instance.PlayerId);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    [HarmonyPostfix]
    public static void Postfix()
    {
        BaiterRole.DiedThisRound.Clear();
        BaiterRole.BaitReportedIntroFlag = false;
    }
}
