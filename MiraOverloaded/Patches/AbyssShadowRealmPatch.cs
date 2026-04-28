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

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPrefix]
    public static void PlayerControl_FixedUpdate_Prefix(PlayerControl __instance)
    {
        ApplyShadowShift(__instance, 1f);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void PlayerControl_FixedUpdate_Postfix(PlayerControl __instance)
    {
        ApplyShadowShift(__instance, -1f);
    }

    private static void ApplyShadowShift(PlayerControl player, float direction)
    {
        if (!ShouldShift(player)) return;
        player.transform.position += (Vector3)(SwallowedModifier.ShadowOffset * direction);
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