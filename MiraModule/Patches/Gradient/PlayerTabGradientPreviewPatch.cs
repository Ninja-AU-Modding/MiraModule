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
    private static readonly Dictionary<(System.Type, string), FieldInfo?> s_fieldCache = new();

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerTab.OnEnable))]
    public static void OnEnablePostfix(PlayerTab __instance) => ApplyToAllChips(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerTab.UpdateAvailableColors))]
    public static void UpdateAvailableColorsPostfix(PlayerTab __instance) => ApplyToAllChips(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerTab.Update))]
    public static void UpdatePostfix(PlayerTab __instance) => ApplyToAllChips(__instance);

    private static void ApplyToAllChips(PlayerTab tab)
    {
        foreach (var chipObj in GetColorChips(tab))
        {
            if (chipObj is not Component chip) continue;

            var colorId = GetColorId(chipObj);
            if (colorId < 0 || colorId >= Palette.PlayerColors.Length) continue;

            ApplyToChip(chip, GetFieldValue(chipObj, "Inner"), colorId);
        }
    }

    private static void ApplyToChip(Component chip, object? inner, int colorId)
    {
        if (MiraModuleGradientColors.TryGetRainbowName(colorId, out var rainbowDef))
        {
            var rainbow = CustomRainbowUtils.GetColor(rainbowDef.Speed, rainbowDef.Colors);
            SetInnerSpriteColor(inner, rainbow);
            SetAllSpriteRendererColors(chip, inner, rainbow);
            return;
        }

        if (!MiraModuleGradientColors.TryGetSecondary(colorId, out var secondaryMain, out var secondaryShadow)) return;

        foreach (var renderer in GetAllSpriteRenderers(chip, inner).Where(r => r))
        {
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
        if (value == null) yield break;

        if (value is Il2CppReferenceArray<ColorChip> il2cppArray)
        {
            foreach (var chip in il2cppArray.Where(c => c != null))
                yield return chip;
            yield break;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable.Cast<object?>().Where(i => i != null))
                yield return item!;
        }
    }

    private static int GetColorId(object chip)
    {
        var type = chip.GetType();

        var prop = AccessTools.Property(type, "ColorId");
        if (prop != null) return (int)(prop.GetValue(chip) ?? -1);

        var field = AccessTools.Field(type, "ColorId") ?? AccessTools.Field(type, "colorId");
        if (field != null) return (int)(field.GetValue(chip) ?? -1);

        return -1;
    }

#pragma warning disable S3011 // Needed for IL2CPP interop, game fields are not always public
    private static object? GetFieldValue(object? obj, string name)
    {
        if (obj == null) return null;

        var type = obj.GetType();
        var key = (type, name);

        if (!s_fieldCache.TryGetValue(key, out var field))
        {
            field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_fieldCache[key] = field;
        }

        if (field != null) return field.GetValue(obj);

        var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return prop?.GetValue(obj);
    }
#pragma warning restore S3011

    private static void SetInnerSpriteColor(object? inner, Color color)
    {
        if (inner == null) return;
        var prop = AccessTools.Property(inner.GetType(), "SpriteColor");
        if (prop?.CanWrite == true) prop.SetValue(inner, color);
    }

    private static void SetAllSpriteRendererColors(Component chip, object? inner, Color color)
    {
        foreach (var renderer in GetAllSpriteRenderers(chip, inner).Where(r => r))
            renderer.color = color;
    }

#pragma warning disable S3011
    private static IEnumerable<SpriteRenderer> GetAllSpriteRenderers(Component chip, object? inner)
    {
        var chipSr = chip?.GetComponent<SpriteRenderer>();
        if (chipSr) yield return chipSr!;

        if (inner is Component innerComp)
        {
            var innerSr = innerComp.GetComponent<SpriteRenderer>();
            if (innerSr) yield return innerSr;
        }

        if (inner == null) yield break;

        foreach (var sr in inner.GetType()
                     .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(f => f.FieldType == typeof(SpriteRenderer))
                     .Select(f => f.GetValue(inner) as SpriteRenderer)
                     .Where(sr => sr))
        {
            yield return sr!;
        }
    }
#pragma warning restore S3011
}