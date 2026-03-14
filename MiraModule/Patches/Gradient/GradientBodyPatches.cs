using HarmonyLib;
using MiraAPI.Utilities;

namespace MiraModule.Patches.Gradient;

internal static class GradientShaderIDs
{
    internal const string GradientOffset = "_GradientOffset";
}

[HarmonyPatch(typeof(LongBoiPlayerBody), nameof(LongBoiPlayerBody.Start))]
public static class LongBoiGradientPatch
{
    public static void Postfix(LongBoiPlayerBody __instance)
    {
        __instance.cosmeticLayer.currentBodySprite.BodySprite.material.SetFloat(ShaderID.Get(GradientShaderIDs.GradientOffset), -2f);
        __instance.neckSprite.material.SetFloat(ShaderID.Get(GradientShaderIDs.GradientOffset), -2f);
        __instance.foregroundNeckSprite.material.SetFloat(ShaderID.Get(GradientShaderIDs.GradientOffset), -0.5f);
        __instance.headSprite.material.SetFloat(ShaderID.Get(GradientShaderIDs.GradientOffset), 2f);
    }
}

[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.EnsureInitialized))]
public static class HorseGradientPatch
{
    public static void Postfix(CosmeticsLayer __instance, PlayerBodyTypes bt)
    {
        if (bt is not PlayerBodyTypes.Horse) return;
        __instance.currentBodySprite.BodySprite.material.SetFloat(ShaderID.Get(GradientShaderIDs.GradientOffset), 1.5f);
    }
}