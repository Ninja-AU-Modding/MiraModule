using HarmonyLib;
using MiraAPI.Modifiers;
using MiraModule.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace MiraModule.Patches;

/// <summary>
/// Blocks all movement for swallowed players, exactly mirroring what Puppeteer does
/// for its controlled victims. The swallowed player's physics simply does nothing —
/// their position stays where they were when gulped.
/// </summary>
[HarmonyPatch]
public static class SwallowedMovementPatch
{
    /// <summary>
    /// Block PlayerPhysics.FixedUpdate for swallowed players so they cannot move.
    /// </summary>
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPrefix]
    public static bool PlayerPhysics_FixedUpdate_Prefix(PlayerPhysics __instance)
    {
        var player = __instance.myPlayer;
        if (player == null || player.Data == null) return true;

        // Skip during ladders/platforms — let vanilla handle those so they don't get stuck
        if (player.onLadder || player.inMovingPlat) return true;
        // Skip during animations so venting/emotes work
        if (player.IsInTargetingAnimState() || player.inVent || player.walkingToVent) return true;

        if (!player.HasModifier<SwallowedModifier>()) return true;

        // Swallowed: zero out movement, do not run normal physics
        AdvancedMovementUtilities.ApplyControlledMovement(__instance, UnityEngine.Vector2.zero, stopIfZero: true);
        return false;
    }

    /// <summary>
    /// Block CustomNetworkTransform.FixedUpdate for swallowed players so their
    /// position doesn't broadcast and cause network jitter.
    /// </summary>
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    [HarmonyPrefix]
    public static bool NetworkTransform_FixedUpdate_Prefix(CustomNetworkTransform __instance)
    {
        if (__instance.isPaused || !__instance.myPlayer) return true;

        var player = __instance.myPlayer;
        if (!player.HasModifier<SwallowedModifier>()) return true;

        // Skip ladder/platform — let those still sync
        if (player.inMovingPlat || player.onLadder) return true;
        if (player.IsInTargetingAnimState() || player.inVent || player.walkingToVent) return true;

        // Suppress network transform updates entirely — frozen in place
        return false;
    }
}
