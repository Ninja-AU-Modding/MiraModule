using HarmonyLib;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
public static class LogoPatch
{
    [HarmonyPatch(typeof(TouAssets), nameof(TouAssets.Banner), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool Prefix(ref LoadableAsset<Sprite> __result)
    {
        __result = Assets.Assets.Banner;
        return false;
    }
}