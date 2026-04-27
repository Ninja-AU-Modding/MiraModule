using System.Collections.Generic;
using MiraAPI.Colors;
using MiraAPI.Utilities;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace MiraOverloaded;

public static class MiraOverloadedColorRegistry
{
    private static bool _initialized;
    private static readonly List<CustomColor> CustomColorsInternal = [];
    private static readonly Dictionary<StringNames, (Color32 SecondaryMain, Color32 SecondaryShadow, bool IsGradient)> GradientMap = [];
    private static readonly Dictionary<StringNames, MiraOverloadedRainbowDef> RainbowMap = [];

    public static IReadOnlyList<CustomColor> CustomColors
    {
        get
        {
            EnsureInitialized();
            return CustomColorsInternal;
        }
    }

    public static bool TryGetSecondary(StringNames name, out Color32 secondaryMain, out Color32 secondaryShadow)
    {
        EnsureInitialized();
        if (GradientMap.TryGetValue(name, out var info) && info.IsGradient)
        {
            secondaryMain = info.SecondaryMain;
            secondaryShadow = info.SecondaryShadow;
            return true;
        }

        secondaryMain = default;
        secondaryShadow = default;
        return false;
    }

    public static bool TryGetRainbow(StringNames name, out MiraOverloadedRainbowDef def)
    {
        EnsureInitialized();
        return RainbowMap.TryGetValue(name, out def);
    }

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        CustomColorsInternal.Clear();
        GradientMap.Clear();
        RainbowMap.Clear();

        var nameMap = new Dictionary<string, StringNames>();

        foreach (var def in MiraOverloadedPlayerColors.ColorDefs)
        {
            if (def.Colors.Length == 0)
            {
                continue;
            }

            var main = def.Colors[0];
            var isGradient = def.Blend == MiraOverloadedColorBlend.Gradient && def.Colors.Length > 1;
            var shadow = main.GetShadowColor(60);

            var custom = new CustomColor(def.Name, main, shadow)
            {
                ColorBrightness = def.Brightness
            };

            CustomColorsInternal.Add(custom);
            nameMap[def.Name] = custom.Name;

            var secondaryMain = isGradient ? def.Colors[^1] : main;
            var secondaryShadow = isGradient ? secondaryMain.GetShadowColor(40) : shadow;

            GradientMap[custom.Name] = (secondaryMain, secondaryShadow, isGradient);
        }

        foreach (var def in MiraOverloadedPlayerColors.RainbowDefs)
        {
            if (def.Colors.Length < 2)
            {
                continue;
            }

            if (nameMap.TryGetValue(def.Name, out var existing))
            {
                RainbowMap[existing] = def;
            }
            else
            {
                var name = CustomStringName.CreateAndRegister(def.Name);
                RainbowMap[name] = def;
            }
        }
    }
}
