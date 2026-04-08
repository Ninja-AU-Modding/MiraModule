using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraModule.Options.Roles.Impostor;
using MiraModule.Roles.Impostor;
using UnityEngine;

namespace MiraModule.Patches;

[HarmonyPatch]
public static class RevenantPatches
{
    [HarmonyPatch(typeof(LogicGameFlow), nameof(LogicGameFlow.CheckEndCriteria))]
    [HarmonyPrefix]
    public static bool CheckEndCriteriaPrefix(LogicGameFlow __instance)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return true;

        var revenantGhosts = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p.Data.IsDead && p.IsRole<RevenantRole>())
            .Select(p => p.GetRole<RevenantRole>())
            .Where(r => r != null && r.GhostKillsRemaining > 0)
            .ToList();

        if (revenantGhosts.Count > 0)
        {
            var numCrew = PlayerControl.AllPlayerControls.ToArray().Count(p => !p.Data.IsDead && !p.Data.Role.IsImpostor);
            var numImps = PlayerControl.AllPlayerControls.ToArray().Count(p => !p.Data.IsDead && p.Data.Role.IsImpostor);

            if (numImps == 0 && numCrew > 0)
            {
                return false;
            }
        }

        return true;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void HudManagerUpdatePostfix(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || !PlayerControl.LocalPlayer.Data.IsDead) return;

        var role = PlayerControl.LocalPlayer.GetRole<RevenantRole>();
        if (role == null || role.GhostKillsRemaining <= 0) return;

        var killButton = __instance.KillButton;
        if (killButton == null) return;

        killButton.gameObject.SetActive(true);
        killButton.enabled = true;

        if (killButton.graphic != null)
        {
            killButton.graphic.color = Palette.EnabledColor;
            killButton.graphic.material.SetFloat("_Desat", 0f);
        }

        killButton.transform.localPosition = new Vector3(0.6f, 1.2f, 0);

        var killDistanceIndex = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance);
        float killDistance = 2.5f;
        if (killDistanceIndex == 0) killDistance = 1.0f;
        else if (killDistanceIndex == 2) killDistance = 5.0f;

        var target = PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, killDistance);
        killButton.SetTarget(target);
    }

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPrefix]
    public static bool KillButtonDoClickPrefix(KillButton __instance)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || !PlayerControl.LocalPlayer.Data.IsDead) return true;

        var role = PlayerControl.LocalPlayer.GetRole<RevenantRole>();
        if (role == null || role.GhostKillsRemaining <= 0) return true;

        if (__instance.isCoolingDown) return false;

        if (__instance.currentTarget != null)
        {
            RevenantRole.RpcGhostKill(PlayerControl.LocalPlayer.PlayerId, __instance.currentTarget.PlayerId);

            var cooldown = OptionGroupSingleton<RevenantOptions>.Instance.GhostKillCooldown;
            var maxCooldown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);

            PlayerControl.LocalPlayer.killTimer = cooldown;
            __instance.SetCoolDown(cooldown, maxCooldown);
        }

        return false;
    }
}
