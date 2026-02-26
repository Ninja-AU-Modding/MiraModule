using HarmonyLib;
using MiraAPI.Modifiers;
using MiraModule.Modifiers;
using UnityEngine;

namespace MiraModule.Patches;

/// <summary>
/// Keeps swallowed players invisible to everyone else.
/// Mirrors the SpectatorRole pattern from TOU-Mira (EnsureSpecAlwaysInvis).
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Visible), MethodType.Setter)]
[HarmonyPriority(Priority.Last)]
public static class SwallowedVisibilityPatch
{
    public static void Prefix(PlayerControl __instance, ref bool value)
    {
        // If something tries to make a swallowed player visible, block it
        if (value && __instance.HasModifier<SwallowedModifier>())
        {
            value = false;
        }
    }
}
