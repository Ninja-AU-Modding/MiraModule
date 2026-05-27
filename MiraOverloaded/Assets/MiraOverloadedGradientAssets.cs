using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraOverloaded.Assets;

public static class MiraOverloadedGradientAssets
{
    private static readonly AssetBundle? Bundle = TryLoadBundle();

    public static readonly LoadableAsset<Material>? GradientMaterial =
        Bundle != null ? new LoadableBundleAsset<Material>("GradientPlayerMaterial", Bundle) : null;

    public static readonly LoadableAsset<Material>? MaskedGradientMaterial =
        Bundle != null ? new LoadableBundleAsset<Material>("MaskedGradientMaterial", Bundle) : null;

    public static bool IsLoaded => Bundle != null;

    private static AssetBundle? TryLoadBundle()
    {
        try
        {
            return AssetBundleManager.Load("MiraOverloaded-assets");
        }
        catch (System.Exception ex)
        {
            Warning($"[MiraOverloaded] Gradient asset bundle not found, gradient colors disabled: {ex.Message}");
            return null;
        }
    }

    public static void DumpBundleInfo()
    {
        if (Bundle == null)
        {
            Warning("[MiraOverloaded] Gradient asset bundle is not loaded.");
            return;
        }

        try
        {
            Info("[MiraOverloaded] Gradient bundle loaded. Assets:");
            foreach (var name in Bundle.GetAllAssetNames())
            {
                Info($"[MiraOverloaded]  - {name}");
            }

            var grad = GradientMaterial?.LoadAsset();
            var masked = MaskedGradientMaterial?.LoadAsset();

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
