using HarmonyLib;
using MiraAPI.Utilities;
using MiraOverloaded.Assets;

namespace MiraOverloaded.Patches.Gradient;

[HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
public static class HatManagerGradientPatch
{
    public static void Postfix(HatManager __instance)
    {
        var mat1 = __instance.PlayerMaterial = MiraOverloadedGradientAssets.GradientMaterial.LoadAsset();
        var mat2 = __instance.MaskedPlayerMaterial = MiraOverloadedGradientAssets.MaskedGradientMaterial.LoadAsset();

        mat1.SetFloat(ShaderID.Get("_GradientBlend"), 1f);
        mat2.SetFloat(ShaderID.Get("_GradientBlend"), 1f);

        mat1.SetFloat(ShaderID.Get("_GradientAngle"), 225f);
        mat2.SetFloat(ShaderID.Get("_GradientAngle"), 225f);

        mat1.SetFloat(ShaderID.Get("_GradientOffset"), 0.4f);
        mat2.SetFloat(ShaderID.Get("_GradientOffset"), 0.4f);
    }
}
