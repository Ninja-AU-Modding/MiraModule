using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraModule.Assets;

public static class MiraModuleGradientAssets
{
    private static readonly AssetBundle Bundle = AssetBundleManager.Load("miramodule-assets");

    public static readonly LoadableAsset<Material> GradientMaterial =
        new LoadableBundleAsset<Material>("GradientPlayerMaterial", Bundle);

    public static readonly LoadableAsset<Material> MaskedGradientMaterial =
        new LoadableBundleAsset<Material>("MaskedGradientMaterial", Bundle);

    public static void DumpBundleInfo()
    {
        try
        {
            Info("[MiraModule] Gradient bundle loaded. Assets:");
            foreach (var name in Bundle.GetAllAssetNames())
            {
                Info($"[MiraModule]  - {name}");
            }

            var grad = GradientMaterial.LoadAsset();
            var masked = MaskedGradientMaterial.LoadAsset();

            if (grad != null)
            {
                Info($"[MiraModule] Gradient material: {grad.name}, shader: {grad.shader.name}");
                DumpShaderProps(grad.shader);
            }
            else
            {
                Info("[MiraModule] Gradient material is NULL.");
            }

            if (masked != null)
            {
                Info($"[MiraModule] Masked gradient material: {masked.name}, shader: {masked.shader.name}");
                DumpShaderProps(masked.shader);
            }
            else
            {
                Info("[MiraModule] Masked gradient material is NULL.");
            }
        }
        catch (System.Exception ex)
        {
            Error($"[MiraModule] DumpBundleInfo failed: {ex}");
        }
    }

    private static void DumpShaderProps(Shader shader)
    {
        if (shader == null)
        {
            return;
        }

        var count = shader.GetPropertyCount();
        Info($"[MiraModule] Shader props ({count}):");
        for (var i = 0; i < count; i++)
        {
            var propName = shader.GetPropertyName(i);
            Info($"[MiraModule]   {i}: {propName}");
        }
    }
}
