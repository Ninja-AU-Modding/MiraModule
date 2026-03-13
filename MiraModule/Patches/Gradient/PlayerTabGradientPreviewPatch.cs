using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Utilities;
using MiraModule.Assets;
using MiraModule.Modules.Rainbow;
using UnityEngine;

namespace MiraModule.Patches.Gradient;

[HarmonyPatch(typeof(PlayerTab))]
public static class PlayerTabGradientPreviewPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerTab.OnEnable))]
    public static void OnEnablePostfix(PlayerTab __instance)
    {
        ApplyToAllChips(__instance);
    }
    
    private static readonly Dictionary<(System.Type, string), FieldInfo?> s_fieldCache = new();


    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerTab.UpdateAvailableColors))]
    public static void UpdateAvailableColorsPostfix(PlayerTab __instance)
    {
        ApplyToAllChips(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerTab.Update))]
    public static void UpdatePostfix(PlayerTab __instance)
    {
        ApplyToAllChips(__instance);
    }

    private static void ApplyToAllChips(PlayerTab tab)
    {
        foreach (var chipObj in GetColorChips(tab))
        {
            if (chipObj is not Component chip)
            {
                continue;
            }

            var colorId = GetColorId(chipObj);
            if (colorId < 0 || colorId >= Palette.PlayerColors.Length)
            {
                continue;
            }

            var inner = GetFieldValue(chipObj, "Inner");
            ApplyToChip(chip, inner, colorId);
        }
    }

    private static void ApplyToChip(Component chip, object? inner, int colorId)
    {
        // Rainbow preview (only for MiraModule custom rainbows).
        if (MiraModuleGradientColors.TryGetRainbowName(colorId, out var rainbowDef))
        {
            var rainbow = CustomRainbowUtils.GetColor(rainbowDef.Speed, rainbowDef.Colors);
            SetInnerSpriteColor(inner, rainbow);
            SetAllSpriteRendererColors(chip, inner, rainbow);
            return;
        }

        // Gradient split preview.
        if (!MiraModuleGradientColors.TryGetSecondary(colorId, out var secondaryMain, out var secondaryShadow))
        {
            return;
        }

        foreach (var renderer in GetAllSpriteRenderers(chip, inner))
        {
            if (!renderer)
            {
                continue;
            }

            var mat = renderer.material;
            var target = mat.HasProperty(ShaderID.Mask)
                ? MiraModuleGradientAssets.MaskedGradientMaterial.LoadAsset()
                : MiraModuleGradientAssets.GradientMaterial.LoadAsset();

            if (mat.shader != target.shader)
            {
                renderer.material = target;
                mat = renderer.material;
            }

            mat.SetFloat(ShaderID.Get("_GradientBlend"), 1f);
            mat.SetFloat(ShaderID.Get("_GradientAngle"), 225f);
            mat.SetFloat(ShaderID.Get("_GradientOffset"), 0.4f);

            PlayerMaterial.SetColors(colorId, mat);
            mat.SetColor(ShaderID.Get("_BodyColor2"), secondaryMain);
            mat.SetColor(ShaderID.Get("_BackColor2"), secondaryShadow);
        }

    }
    
    private static IEnumerable<object> GetColorChips(PlayerTab tab)
    {
        var value = GetFieldValue(tab, "ColorChips") ?? GetFieldValue(tab, "colorChips");
        if (value == null)
        {
            yield break;
        }

        if (value is Il2CppReferenceArray<ColorChip> il2cppArray)
        {
            foreach (var chip in il2cppArray)
            {
                if (chip != null)
                {
                    yield return chip;
                }
            }
            yield break;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }
    }

    private static int GetColorId(object chip)
    {
        var type = chip.GetType();
        var prop = AccessTools.Property(type, "ColorId");
        if (prop != null)
        {
            return (int)prop.GetValue(chip);
        }

        var field = AccessTools.Field(type, "ColorId") ?? AccessTools.Field(type, "colorId");
        if (field != null)
        {
            return (int)field.GetValue(chip);
        }

        return -1;
    }

    private static object? GetFieldValue(object obj, string name)
    {
        if (obj == null) return null;

        var type = obj.GetType();
        var key = (type, name);

        if (!s_fieldCache.TryGetValue(key, out var field))
        {
            field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_fieldCache[key] = field;
        }

        if (field != null)
        {
            return field.GetValue(obj);
        }

        // Fallback to property access if a field wasn't found.
        var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
        {
            return prop.GetValue(obj);
        }

        return null;
    }

    private static void SetInnerSpriteColor(object? inner, Color color)
    {
        if (inner == null)
        {
            return;
        }

        var prop = AccessTools.Property(inner.GetType(), "SpriteColor");
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(inner, color);
        }
    }

    private static void SetAllSpriteRendererColors(Component chip, object? inner, Color color)
    {
        foreach (var renderer in GetAllSpriteRenderers(chip, inner))
        {
            if (renderer)
            {
                renderer.color = color;
            }
        }
    }

    private static IEnumerable<SpriteRenderer> GetAllSpriteRenderers(Component chip, object? inner)
    {
        if (chip != null)
        {
            var sr = chip.GetComponent<SpriteRenderer>();
            if (sr) yield return sr;
        }

        if (inner is Component innerComp)
        {
            var sr = innerComp.GetComponent<SpriteRenderer>();
            if (sr) yield return sr;
        }

        if (inner != null)
        {
            foreach (var field in inner.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.FieldType == typeof(SpriteRenderer))
                {
                    if (field.GetValue(inner) is SpriteRenderer sr && sr)
                    {
                        yield return sr;
                    }
                }
            }
        }
    }
}
