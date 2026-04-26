using HarmonyLib;

namespace MiraModule.Patches;

[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
public static class DistorterTargetPatch
{
    public static void Postfix(ImpostorRole __instance, NetworkedPlayerInfo target, ref bool __result)
    {
        // Will be implemented later
    }
}
