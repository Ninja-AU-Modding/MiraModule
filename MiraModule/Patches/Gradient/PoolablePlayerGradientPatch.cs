using HarmonyLib;
using MiraAPI.Utilities;
using MiraModule.Assets;
using MiraModule.Components;
using UnityEngine;

namespace MiraModule.Patches.Gradient;

[HarmonyPatch(typeof(PoolablePlayer))]
public static class PoolablePlayerGradientPatch
{
    private static void ApplySplit(PoolablePlayer instance, int colorId)
    {
        if (!instance || colorId < 0 || colorId >= Palette.PlayerColors.Length)
        {
            return;
        }

        if (!MiraModuleGradientColors.TryGetSecondary(colorId, out var secondaryMain, out var secondaryShadow))
        {
            return;
        }

        var renderer = instance.cosmetics?.currentBodySprite?.BodySprite;
        if (!renderer || renderer == null) return;
        

        var mat = renderer.material;
        var target = mat.HasProperty(ShaderID.Mask)
            ? MiraModuleGradientAssets.MaskedGradientMaterial.LoadAsset()
            : MiraModuleGradientAssets.GradientMaterial.LoadAsset();

        if (mat.shader != target.shader)
        {
            renderer.material = target;
            mat = renderer.material;
        }

        mat.SetColor(ShaderID.Get("_BodyColor2"), secondaryMain);
        mat.SetColor(ShaderID.Get("_BackColor2"), secondaryShadow);
        mat.SetFloat(ShaderID.Get("_GradientBlend"), 1f);
        mat.SetFloat(ShaderID.Get("_GradientAngle"), 225f);
        mat.SetFloat(ShaderID.Get("_GradientOffset"), 0.4f);

        var grad = renderer.GetComponent<GradientColorComponent>() ?? renderer.gameObject.AddComponent<GradientColorComponent>();
        grad.SetColor(Palette.PlayerColors[colorId], secondaryMain);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromPlayerData))]
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromEitherPlayerDataOrCache))]
    public static void UpdateFromPlayerDataPostfix(PoolablePlayer __instance, NetworkedPlayerInfo pData)
    {
        if (!pData)
        {
            return;
        }

        ApplySplit(__instance, pData.DefaultOutfit.ColorId);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromPlayerOutfit))]
    public static void UpdateFromPlayerOutfitPostfix(PoolablePlayer __instance, NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        ApplySplit(__instance, outfit.ColorId);
    }
}
