using HarmonyLib;
using MiraOverloaded.Components;
using MiraOverloaded.Assets;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraOverloaded.Patches.Gradient;

public static class PlayerMaterialGradientUtils
{
    public static void Apply(int colorId, Renderer renderer)
    {
        if (renderer.GetComponentInParent<HatParent>() && !renderer.GetComponentInParent<CosmeticsLayer>())
        {
            return;
        }

        if (colorId < 0 || colorId >= Palette.PlayerColors.Length)
        {
            return;
        }

        var primary = Palette.PlayerColors[colorId];
        var secondary = primary;
        var isGradient = MiraOverloadedGradientColors.TryGetSecondary(colorId, out var secondaryMain, out _);
        if (isGradient)
        {
            secondary = secondaryMain;
        }

        if (isGradient)
        {
            var mat = renderer.material;
            var target = mat.HasProperty(ShaderID.Mask)
                ? MiraOverloadedGradientAssets.MaskedGradientMaterial.LoadAsset()
                : MiraOverloadedGradientAssets.GradientMaterial.LoadAsset();

            if (mat.shader != target.shader)
            {
                renderer.material = target;
                mat = renderer.material;
            }

            
            mat.SetFloat(ShaderID.Get("_GradientBlend"), 1f);
            mat.SetFloat(ShaderID.Get("_GradientAngle"), 225f);
            mat.SetFloat(ShaderID.Get("_GradientOffset"), 0.4f);
        }

        var grad = renderer.GetComponent<GradientColorComponent>() ?? renderer.gameObject.AddComponent<GradientColorComponent>();
        grad.SetColor(primary, secondary);
    }

    public static void Apply(Color color, Renderer renderer)
    {
        if (renderer.GetComponentInParent<HatParent>() && !renderer.GetComponentInParent<CosmeticsLayer>())
        {
            return;
        }

        var c32 = (Color32)color;
        var grad = renderer.GetComponent<GradientColorComponent>() ?? renderer.gameObject.AddComponent<GradientColorComponent>();

        // If this came from a palette color, recover the id so gradients still work
        // when the Color overload is used (common in UI previews).
        var foundId = -1;
        for (var i = 0; i < Palette.PlayerColors.Length; i++)
        {
            if (Palette.PlayerColors[i].Equals(c32))
            {
                foundId = i;
                break;
            }
        }

        if (foundId >= 0 && MiraOverloadedGradientColors.TryGetSecondary(foundId, out var secondaryMain, out _))
        {
            var mat = renderer.material;
            var target = mat.HasProperty(ShaderID.Mask)
                ? MiraOverloadedGradientAssets.MaskedGradientMaterial.LoadAsset()
                : MiraOverloadedGradientAssets.GradientMaterial.LoadAsset();

            if (mat.shader != target.shader)
            {
                renderer.material = target;
                mat = renderer.material;
            }

            mat.SetFloat(ShaderID.Get("_GradientBlend"), 1f);
            mat.SetFloat(ShaderID.Get("_GradientAngle"), 225f);
            mat.SetFloat(ShaderID.Get("_GradientOffset"), 0.4f);

            grad.SetColor(Palette.PlayerColors[foundId], secondaryMain);
            return;
        }

        grad.SetColor(c32, c32);
    }
}

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(int), typeof(Renderer))]
public static class PlayerMaterialGradientPatch
{
    public static void Postfix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer renderer)
    {
        PlayerMaterialGradientUtils.Apply(colorId, renderer);
    }
}

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(Color), typeof(Renderer))]
public static class PlayerMaterialGradientPatch2
{
    public static void Postfix([HarmonyArgument(0)] Color color, [HarmonyArgument(1)] Renderer renderer)
    {
        PlayerMaterialGradientUtils.Apply(color, renderer);
    }
}

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(int), typeof(SpriteRenderer))]
public static class PlayerMaterialGradientPatchSprite
{
    public static void Postfix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] SpriteRenderer renderer)
    {
        PlayerMaterialGradientUtils.Apply(colorId, renderer);
    }
}

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(Color), typeof(SpriteRenderer))]
public static class PlayerMaterialGradientPatchSprite2
{
    public static void Postfix([HarmonyArgument(0)] Color color, [HarmonyArgument(1)] SpriteRenderer renderer)
    {
        PlayerMaterialGradientUtils.Apply(color, renderer);
    }
}
