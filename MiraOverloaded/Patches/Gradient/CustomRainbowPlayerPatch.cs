using HarmonyLib;
using MiraOverloaded.Modules.Rainbow;
using UnityEngine;

namespace MiraOverloaded.Patches.Gradient;

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(int), typeof(Renderer))]
public static class CustomRainbowPlayerPatch
{
    public static bool Prefix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer rend)
    {
        if (colorId < 0 || colorId >= Palette.ColorNames.Length)
        {
            return true;
        }

        var name = Palette.ColorNames[colorId];
        if (!MiraOverloadedColorRegistry.TryGetRainbow(name, out var def))
        {
            var existing = rend.gameObject.GetComponent<CustomRainbowRenderer>();
            if (existing)
            {
                UnityEngine.Object.Destroy(existing);
            }
            return true;
        }

        var comp = rend.gameObject.GetComponent<CustomRainbowRenderer>();
        if (comp != null && comp.ColorId != colorId)
        {
            UnityEngine.Object.Destroy(comp);
            comp = null;
        }

        if (comp == null)
        {
            comp = rend.gameObject.AddComponent<CustomRainbowRenderer>();
            comp.Initialize(colorId, rend, def);
        }

        return false;
    }
}

// Gradient handling is in PlayerMaterialGradientPatch (separate from rainbow).
