using HarmonyLib;
using MiraModule.Modules.Rainbow;
using UnityEngine;

namespace MiraModule.Patches.Gradient;

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(int), typeof(Renderer))]
public static class CustomRainbowMaterialPatch
{
    public static void Postfix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer renderer)
    {
        if (colorId < 0 || colorId >= Palette.ColorNames.Length)
        {
            return;
        }

        var name = Palette.ColorNames[colorId];
        if (!MiraModuleColorRegistry.TryGetRainbow(name, out var def))
        {
            var existing = renderer.gameObject.GetComponent<CustomRainbowRenderer>();
            if (existing)
            {
                UnityEngine.Object.Destroy(existing);
            }
            return;
        }

        var comp = renderer.gameObject.GetComponent<CustomRainbowRenderer>();
        if (!comp || comp.ColorId != colorId)
        {
            comp = renderer.gameObject.AddComponent<CustomRainbowRenderer>();
            comp.Initialize(colorId, renderer, def);
        }
    }
}
