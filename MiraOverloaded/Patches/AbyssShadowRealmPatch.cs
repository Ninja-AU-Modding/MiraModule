using HarmonyLib;
using MiraAPI.Utilities;
using MiraOverloaded.Modifiers;
using MiraOverloaded.Roles.Neutral;
using UnityEngine;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class AbyssShadowRealmPatch
{
    /// <summary>
    /// For the Abyss player, we want their internal 'transform.position' to be shifted
    /// to the Shadow Realm (Void) so that proximity chat (CrewLink) isolates them
    /// with their victims.
    ///
    /// But we must compensate so:
    /// 1. Networking still reports their normal map position.
    /// 2. Visuals still render them on the map.
    /// 3. Colliders stay on the map.
    /// </summary>
    
    [HarmonyPatch(typeof(PlayerControl), "FixedUpdate")]
    [HarmonyPrefix]
    public static void PlayerControl_FixedUpdate_Prefix(PlayerControl __instance)
    {
        if (!ShouldShift(__instance)) return;
        __instance.transform.position += (Vector3)SwallowedModifier.ShadowOffset;
    }

    [HarmonyPatch(typeof(PlayerControl), "FixedUpdate")]
    [HarmonyPostfix]
    public static void PlayerControl_FixedUpdate_Postfix(PlayerControl __instance)
    {
        if (!ShouldShift(__instance)) return;
        __instance.transform.position -= (Vector3)SwallowedModifier.ShadowOffset;
    }

    [HarmonyPatch(typeof(PlayerControl), "Update")]
    [HarmonyPrefix]
    public static void PlayerControl_Update_Prefix(PlayerControl __instance)
    {
        if (!ShouldShift(__instance)) return;
        __instance.transform.position += (Vector3)SwallowedModifier.ShadowOffset;
    }

    [HarmonyPatch(typeof(PlayerControl), "Update")]
    [HarmonyPostfix]
    public static void PlayerControl_Update_Postfix(PlayerControl __instance)
    {
        if (!ShouldShift(__instance)) return;
        __instance.transform.position -= (Vector3)SwallowedModifier.ShadowOffset;
    }

    private static bool ShouldShift(PlayerControl player)
    {
        if (player == null || player.Data == null) return false;

        // Shift if this is the local Abyss player
        if (player.AmOwner && player.IsRole<AbyssRole>()) return true;

        // Shift if the local player is swallowed BY this player
        if (PlayerControl.LocalPlayer != null && 
            AbyssRole.SwallowedPlayers.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var abyssId) &&
            abyssId == player.PlayerId)
        {
            return true;
        }

        return false;
    }
}
