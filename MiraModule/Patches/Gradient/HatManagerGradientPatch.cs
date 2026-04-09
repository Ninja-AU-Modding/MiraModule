using HarmonyLib;
using MiraAPI.Utilities;
using MiraModule.Assets;

namespace MiraModule.Patches.Gradient;

[HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
public static class HatManagerGradientPatch
{
    public static void Postfix(HatManager __instance)
    {
        var mat1 = __instance.PlayerMaterial = MiraModuleGradientAssets.GradientMaterial.LoadAsset();
        var mat2 = __instance.MaskedPlayerMaterial = MiraModuleGradientAssets.MaskedGradientMaterial.LoadAsset();

        mat1.SetFloat(ShaderID.Get("_GradientBlend"), 1f);
        mat2.SetFloat(ShaderID.Get("_GradientBlend"), 1f);

        mat1.SetFloat(ShaderID.Get("_GradientAngle"), 225f);
        mat2.SetFloat(ShaderID.Get("_GradientAngle"), 225f);

        mat1.SetFloat(ShaderID.Get("_GradientOffset"), 0.4f);
        mat2.SetFloat(ShaderID.Get("_GradientOffset"), 0.4f);
    }
}
