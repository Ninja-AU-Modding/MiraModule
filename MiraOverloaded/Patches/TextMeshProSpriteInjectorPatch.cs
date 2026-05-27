using HarmonyLib;
using MiraOverloaded.Utilities;
using TMPro;

namespace MiraOverloaded.Patches;

[HarmonyPatch(typeof(TextMeshProUGUI), "Awake")]
public static class TextMeshProSpriteInjectorPatch
{
    public static void Postfix(TextMeshProUGUI __instance)
    {
        SpriteTagRegistry.InjectIntoComponent(__instance);
    }
}
