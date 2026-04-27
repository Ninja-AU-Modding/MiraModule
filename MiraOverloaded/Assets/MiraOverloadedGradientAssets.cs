using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraOverloaded.Assets;

public static class MiraOverloadedGradientAssets
{
    private static readonly AssetBundle Bundle = AssetBundleManager.Load("MiraOverloaded-assets");

    public static readonly LoadableAsset<Material> GradientMaterial =
        new LoadableBundleAsset<Material>("GradientPlayerMaterial", Bundle);

    public static readonly LoadableAsset<Material> MaskedGradientMaterial =
        new LoadableBundleAsset<Material>("MaskedGradientMaterial", Bundle);

    public static void DumpBundleInfo()
    {
        try
        {
            Info("[MiraOverloaded] Gradient bundle loaded. Assets:");
            foreach (var name in Bundle.GetAllAssetNames())
            {
                Info($"[MiraOverloaded]  - {name}");
            }

            var grad = GradientMaterial.LoadAsset();
            var masked = MaskedGradientMaterial.LoadAsset();

            if (grad != null)
            {
                Info($"[MiraOverloaded] Gradient material: {grad.name}, shader: {grad.shader.name}");
                DumpShaderProps(grad.shader);
            }
            else
            {
                Info("[MiraOverloaded] Gradient material is NULL.");
            }

            if (masked != null)
            {
                Info($"[MiraOverloaded] Masked gradient material: {masked.name}, shader: {masked.shader.name}");
                DumpShaderProps(masked.shader);
            }
            else
            {
                Info("[MiraOverloaded] Masked gradient material is NULL.");
            }
        }
        catch (System.Exception ex)
        {
            Error($"[MiraOverloaded] DumpBundleInfo failed: {ex}");
        }
    }

    private static void DumpShaderProps(Shader shader)
    {
        if (shader == null)
        {
            return;
        }

        var count = shader.GetPropertyCount();
        Info($"[MiraOverloaded] Shader props ({count}):");
        for (var i = 0; i < count; i++)
        {
            var propName = shader.GetPropertyName(i);
            Info($"[MiraOverloaded]   {i}: {propName}");
        }
    }
}
