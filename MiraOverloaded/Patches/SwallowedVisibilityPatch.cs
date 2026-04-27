using HarmonyLib;
using MiraAPI.Modifiers;
using MiraOverloaded.Modifiers;

namespace MiraOverloaded.Patches;

/// <summary>
/// Keeps swallowed players invisible to everyone else.
/// Allows visibility to be restored when the modifier is being deactivated (ReleasingPlayers).
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Visible), MethodType.Setter)]
[HarmonyPriority(Priority.Last)]
public static class SwallowedVisibilityPatch
{
    public static void Prefix(PlayerControl __instance, ref bool value)
    {
        if (!value) return; // already going invisible — don't interfere

        // Allow visibility if we're explicitly releasing this player
        if (SwallowedModifier.ReleasingPlayers.Contains(__instance.PlayerId)) return;

        // Block visibility while the modifier is still active
        if (__instance.HasModifier<SwallowedModifier>())
        {
            value = false;
        }
    }
}
